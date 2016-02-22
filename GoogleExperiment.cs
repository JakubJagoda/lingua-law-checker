using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LinguaLawChecker
{
    class GoogleExperiment : Experiment
    {
        private static string GOOGLE_TRENDS_URL = "https://www.google.com/trends/hottrends/hotItems";
        private static Dictionary<Language, string> GOOGLE_LANGUAGE_CODES = new Dictionary<Language, string>
        {
            { Language.EN, "p1" },
            { Language.ES, "p26" },
            { Language.DE, "p15" },
            { Language.IT, "p27" },
            { Language.PL, "p31" },
            { Language.FR, "p16" },
            { Language.SE, "p42" },
            { Language.CZ, "p43" },
            { Language.SK, "p39" },
            { Language.HU, "p45" }
        };
        private static int MIN_WORD_LENGTH = 3;

        public override string GetSerializedResults(IEnumerable<ExperimentResult> results)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ExperimentResult>> PerformAsync(IEnumerable<Article> titlesAndArticlesPairs, Language lang)
        {
            IEnumerable<string> wordsFromTrendingSearches = await this.GetWordsFromTrendingSearchesForLanguage(lang);

            IEnumerable<ExperimentResult> results = titlesAndArticlesPairs.Select(input =>
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

                string[] distinctWordsFromArticle = GoogleExperiment.GetWords(input.Content).ToArray();

                int matchedTrendingSearches = wordsFromTrendingSearches.Count(word => Array.FindIndex(distinctWordsFromArticle, w => w.Equals(word, StringComparison.InvariantCultureIgnoreCase)) >= 0);

                ExperimentResult result = new ExperimentResult
                {
                    ArticleTitle = input.Title,
                    ArticleContents = input.Content,
                    ArticleLength = input.Content.Length,
                    WordCount = distinctWordsFromArticle.Length,
                    MatchedWordsFromSearchTrends = matchedTrendingSearches
                };

                return result;
            });

            //int allMatched = results.Aggregate()

            return results;
        }

        private async Task<IEnumerable<string>> GetTrendingSearchesForLanguage(Language lang)
        {
            string data = String.Join("&", new Dictionary<string, string>()
            {
                { "ajax", "1" },
                { "htd", "" },
                { "htv", "l" },
                { "pn", GoogleExperiment.GOOGLE_LANGUAGE_CODES[lang] }
            }.Select(kvp => kvp.Key + "=" + kvp.Value));

            JObject parsed;

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(GoogleExperiment.GOOGLE_TRENDS_URL, new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded"));
                var json = await response.Content.ReadAsStringAsync();
                parsed = JObject.Parse(json);
            }

            IEnumerable<string> trendingSearches = parsed["trendsByDateList"].Aggregate(new List<string>(), (prev, next) =>
            {
                IEnumerable<string> words = next["trendsList"].Select(a => a["title"].ToString());
                prev.AddRange(words);
                return prev;
            }).ToArray();

            return trendingSearches;
        }

        private async Task<IEnumerable<string>> GetWordsFromTrendingSearchesForLanguage(Language lang)
        {
            IEnumerable<string> trendingSearches = await this.GetTrendingSearchesForLanguage(lang);
            IEnumerable<string> wordsFromTrendingSearches = trendingSearches.Aggregate(new List<string>(), (prev, next) =>
            {
                string[] words = next.Split(' ');
                prev.AddRange(words);

                return prev;
            });

            return wordsFromTrendingSearches
                .Where(word => word.All(Char.IsLetter) && word.Length > GoogleExperiment.MIN_WORD_LENGTH)
                .Distinct();
        }
    }
}
