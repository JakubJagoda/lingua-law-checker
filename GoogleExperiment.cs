using CsQuery;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LinguaLawChecker
{
    class GoogleExperiment : Experiment
    {
        private const string GOOGLE_TRENDS_URL = "https://www.google.com/trends/hottrends/hotItems";
        private const string WIKIPEDIA_SEARCH_URL = "https://{0}.wikipedia.org/w/api.php?action=opensearch&search={1}&format=json&namespace=0";
        private const string WIKI_ARTICLE_API_URL_TPL = "https://{0}.wikipedia.org/w/api.php?action=parse&page={1}&format=json&redirects";
        private static Dictionary<Language, string> GOOGLE_LANGUAGE_CODES = new Dictionary<Language, string>
        {
            { Language.EN, "p1" },
            { Language.ES, "p26" },
            { Language.DE, "p15" },
            { Language.IT, "p27" },
            { Language.PL, "p31" },
            { Language.FR, "p16" },
            { Language.SV, "p42" },
            { Language.CZ, "p43" },
            { Language.SK, "p39" },
            { Language.HU, "p45" }
        };
        private static Dictionary<Language, string> DISAMBIGUATION_CATEGORIES = new Dictionary<Language, string>
        {
            { Language.EN, "Disambiguation_pages" },
            { Language.ES, "Wikipedia:Desambiguación" },
            { Language.DE, "Begriffsklärung" },
            { Language.IT, "Pagine_di_disambiguazione" },
            { Language.PL, "Strony_ujednoznaczniające" },
            { Language.FR, "Homonymie" },
            { Language.SV, "Förgreningssidor" },
            { Language.CZ, "Rozcestníky" },
            { Language.SK, "Rozlišovacie_stránky" },
            { Language.HU, "Egyértelműsítő_lapok" }
        };

        public string GetSerializedResults(IEnumerable<GoogleExperimentResult> results)
        {
            StringBuilder csv = new StringBuilder();

            foreach (GoogleExperimentResult result in results)
            {
                csv.AppendLine(String.Format(new CultureInfo("en-US"), "{0},{1}", result.Traffic, result.AverageWikiArticleWordCount));
            }

            return csv.ToString();
        }

        public async Task<IEnumerable<GoogleExperimentResult>> PerformAsync(IEnumerable<Article> articles, Language lang)
        {
            IEnumerable<TrendingSearch> trendingSearches = await this.GetTrendingSearchesForLanguage(lang);
            IEnumerable<Article> articlesForTrendingSearches = await Task.WhenAll(trendingSearches.Select(trendingSearch => this.ArticlesForTrendingSearches(trendingSearch)));

            IEnumerable<ExperimentResult> results = trendingSearches.Zip(articlesForTrendingSearches, (trendingSearch, article) =>
            {
                ExperimentResult result = new ExperimentResult
                {
                    ArticleTitle = article.Title,
                    ArticleContents = article.Content,
                    ArticleLength = article.Content.Length,
                    WordCount = GoogleExperiment.GetWords(article.Content, false).Count(),
                    RelatedSearchTermTraffic = trendingSearch.Traffic
                };

                return result;
            });

            return this.GetAverageWordsCountForTermsWithSameTraffic(results).OrderBy(r => r.Traffic);
        }

        private async Task<IEnumerable<TrendingSearch>> GetTrendingSearchesForLanguage(Language lang)
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

            IEnumerable<TrendingSearch> trendingSearches = parsed["trendsByDateList"].Aggregate(new List<TrendingSearch>(), (prev, next) =>
            {
                IEnumerable<TrendingSearch> partialTrendingSearches = next["trendsList"].Select(a => new TrendingSearch
                {
                    Term = a["title"].ToString(),
                    Traffic = int.Parse(a["trafficBucketLowerBound"].ToString()),
                    Language = lang
                });

                prev.AddRange(partialTrendingSearches);
                return prev;
            }).ToArray().OrderByDescending(trendingSearch => trendingSearch.Traffic);

            return trendingSearches;
        }

        private async Task<Article> ArticlesForTrendingSearches(TrendingSearch trendingSearch)
        {
            string articleTitle = await this.GetArticleTitleForSearchTerm(trendingSearch.Term, trendingSearch.Language);

            if (articleTitle.Equals(""))
            {
                return new Article()
                {
                    Title = "",
                    Content = ""
                };
            }

            string articleContents = await this.GetArticleContents(articleTitle, trendingSearch.Language);
            return new Article()
            {
                Title = articleTitle,
                Content = articleContents
            };
        }

        private async Task<string> GetArticleTitleForSearchTerm(string searchTerm, Language language)
        {
            string wikiSearchUrl = String.Format(GoogleExperiment.WIKIPEDIA_SEARCH_URL, language.ToString(), searchTerm);
            string articleUrl, articleTitle;
            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync(wikiSearchUrl);
                JArray parsed = JArray.Parse(json);

                if (parsed[1].Count() == 0)
                {
                    return "";
                }

                articleTitle = parsed[1][0].ToString();
                //articleUrl = parsed[3][0].ToString();
            }

            return articleTitle;
        }

        private async Task<string> GetArticleContents(string articleTitle, Language lang)
        {
            string url = String.Format(GoogleExperiment.WIKI_ARTICLE_API_URL_TPL, lang, articleTitle);
            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync(url);
                JObject parsed = JObject.Parse(json);

                if (parsed["error"] != null)
                {
                    return "";
                }

                //check if this is not a disambiguation page....
                var categories = parsed["parse"]["categories"].ToArray();
                bool isDisambiguationPage = categories.Any(category => category["*"].ToString() == GoogleExperiment.DISAMBIGUATION_CATEGORIES[lang]);

                //and if so - return empty
                if (isDisambiguationPage)
                {
                    return "";
                }

                string html = parsed["parse"]["text"]["*"].ToString();
                return this.GetArticleTextFromHtml(html);
            }
        }

        private string GetArticleTextFromHtml(string html)
        {
            CQ dom = html;
            dom["sup.reference, .noprint"].Remove(); //removes citations and [citation needed] (and maybe some other nonprintable stuff)
            CQ paragraphs = dom["p"];
            return paragraphs.Text();
        }

        private IEnumerable<GoogleExperimentResult> GetAverageWordsCountForTermsWithSameTraffic(IEnumerable<ExperimentResult> input)
        {
            var dict = new Dictionary<int, List<ExperimentResult>>();
            foreach (var i in input)
            {
                int key = i.RelatedSearchTermTraffic;

                if (!dict.ContainsKey(key))
                {
                    dict[key] = new List<ExperimentResult>();
                }

                dict[key].Add(i);
            }

            return dict.Aggregate(new List<GoogleExperimentResult>(), (prev, trafficToExperimentResultsList) =>
            {
                float averageWordCount = (float)trafficToExperimentResultsList.Value.Average(experimentResult => experimentResult.WordCount);
                prev.Add(new GoogleExperimentResult()
                {
                    AverageWikiArticleWordCount = averageWordCount,
                    Traffic = trafficToExperimentResultsList.Key
                });

                return prev;
            });
        }
    }
}
