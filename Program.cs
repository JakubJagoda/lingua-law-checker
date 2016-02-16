using System;
using System.Collections.Generic;
using System.IO;
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
            Console.WriteLine("Fetched, starting experiments...");

            try
            {
                IEnumerable<ExperimentResult> results = heapsLawExperiment.Perform(titlesAndArticlesPairs, Language.EN);
                string serializedResults = heapsLawExperiment.GetSerializedResults(results);

                File.WriteAllText(String.Format("./{0}_HeapsResults.csv", Language.EN), serializedResults);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Done. Press any key.");
            Console.ReadLine();
        }
    }

    public enum Language { EN }
}
