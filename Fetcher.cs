using CsQuery;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<IEnumerable<Article>> GetNArticlesForLanguage(Language lang, int n, bool forceFetching = false)
        {
            if (forceFetching)
            {
                return await this.GetNRandomArticlesFromWikiForLanguage(lang, n);
            } else
            {
                try
                {
                    return this.GetCachedArticlesForLanguage(lang);
                } catch(Exception)
                {
                    return await this.GetNRandomArticlesFromWikiForLanguage(lang, n);
                }
            }
        }

        private async Task<IEnumerable<Article>> GetNRandomArticlesFromWikiForLanguage(Language language, int n)
        {
            IEnumerable<string> titles = await this.GetNRandomPageTitlesForLanguage(n, language);
            IEnumerable<string> contents = await Task.WhenAll(titles.Select(title => this.GetArticleContents(title, language)));
            IEnumerable<Article> articles = titles.Zip(contents, (title, content) => new Article {
                Title = title,
                Content = content
            });

            this.SaveCachedArticlesForLanguage(language, articles);
            return articles;
        }

        private IEnumerable<Article> GetCachedArticlesForLanguage(Language lang)
        {
            string cached = File.ReadAllText(String.Format("./{0}_CachedData.json", lang));
            return JsonConvert.DeserializeObject<IEnumerable<Article>>(cached);
        }

        private void SaveCachedArticlesForLanguage(Language lang, IEnumerable<Article> articles)
        {
            string output = JsonConvert.SerializeObject(articles);
            File.WriteAllText(String.Format("./{0}_CachedData.json", lang), output);
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
