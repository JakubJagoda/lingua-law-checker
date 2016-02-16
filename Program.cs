using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguaLawChecker
{
    class Program
    {
        private static Fetcher fetcher = new Fetcher();
        private static HeapsLawExperiment heapsLawExperiment = new HeapsLawExperiment();
        static void Main(string[] args)
        {
            Task.Run(Program.MainAsync).Wait();
        }

        static async Task MainAsync()
        {
            const int ARTICLES_COUNT = 5;
            Console.WriteLine("Fetching {0} articles from {1} Wikipedia...", ARTICLES_COUNT, Language.EN.ToString());
            IEnumerable<Tuple<string, string>> titlesAndArticlesPairs = await fetcher.GetNRandomArticlesForLanguage(Language.EN, ARTICLES_COUNT);

            try
            {
                IEnumerable<ExperimentResult> results = heapsLawExperiment.Perform(titlesAndArticlesPairs);
                foreach (ExperimentResult result in results)
                {
                    Console.WriteLine("{0}: {1} letters, {2} words", result.ArticleTitle, result.ArticleLength, result.WordCount);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }

    public enum Language { EN }
}
