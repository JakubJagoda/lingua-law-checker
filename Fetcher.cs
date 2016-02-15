using CsQuery;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LinguaLawChecker
{
    class Fetcher
    {
        private const string WIKI_ARTICLE_API_URL_TPL = "https://{0}.wikipedia.org/w/api.php?action=parse&page={1}&format=json";
        private const string WIKI_RANDOM_PAGE_TITLES_URL_TPL = "https://{0}.wikipedia.org/w/api.php?action=query&list=random&rnlimit={1}&rnnamespace=0&format=json";
        private const int ARTICLES_COUNT = 10;

        public async Task<IEnumerable<Tuple<string, string>>> GetNRandomArticlesForLanguage(Language language, int n = Fetcher.ARTICLES_COUNT)
        {
            IEnumerable<string> titles = await this.GetNRandomPageTitlesForLanguage(n, language);
            IEnumerable<string> articles = await Task.WhenAll(titles.Select(title => this.GetArticleContents(title, language)));
            IEnumerable<Tuple<string, string>> pairs = titles.Zip(articles, (title, article) => Tuple.Create(title, article));
            return pairs;
        }

        private async Task<IEnumerable<string>> GetNRandomPageTitlesForLanguage(int n, Language language)
        {
            string url = String.Format(Fetcher.WIKI_RANDOM_PAGE_TITLES_URL_TPL, language.ToString().ToLower(), n);
            JObject json = await this.GetJSONFromURL(url);
            IEnumerable<string> titles = from page in json["query"]["random"]
                                   select page["title"].Value<string>();

            return titles;
        }

        private async Task<string> GetArticleContents(string articleTitle, Language language)
        {
            string url = String.Format(Fetcher.WIKI_ARTICLE_API_URL_TPL, language.ToString().ToLower(), articleTitle);
            JObject json = await this.GetJSONFromURL(url);
            string html = json["parse"]["text"]["*"].ToString();

            return this.GetArticleTextFromHtml(html);
        }

        private async Task<JObject> GetJSONFromURL(string url)
        {
            JObject parsed;
            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync(url);
                parsed = JObject.Parse(json);
            }

            return parsed;
        }

        private string GetArticleTextFromHtml(string html)
        {
            CQ dom = html;
            dom["sup.reference, .noprint"].Remove(); //removes citations and [citation needed] (and maybe some other nonprintable stuff)
            CQ paragraphs = dom["p"];
            return paragraphs.Text();
        }
    }
}
