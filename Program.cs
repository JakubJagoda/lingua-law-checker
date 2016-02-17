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
        private static PunctuationExperiment punctuationExperiment = new PunctuationExperiment();
        static void Main(string[] args)
        {
            Task.Run(Program.MainAsync).Wait();
        }

        static async Task MainAsync()
        {
            const int ARTICLES_COUNT = 5;

            foreach (Language language in Enum.GetValues(typeof(Language)))
            {
                Console.WriteLine("Fetching {0} articles from {1} Wikipedia...", ARTICLES_COUNT, language);
                IEnumerable<Tuple<string, string>> titlesAndArticlesPairs = await fetcher.GetNRandomArticlesForLanguage(language, ARTICLES_COUNT);
                Console.WriteLine("Fetched, starting experiments...");

                try
                {
                    IEnumerable<ExperimentResult> results = heapsLawExperiment.Perform(titlesAndArticlesPairs, language);
                    string serializedResults = heapsLawExperiment.GetSerializedResults(results);
                    File.WriteAllText(String.Format("./{0}_HeapsResults.csv", language), serializedResults);

                    List<int> zipfsResults = ZipfsLawExperiment.Perform(titlesAndArticlesPairs);
                    string serializedZipfsResults = ZipfsLawExperiment.GetSerializedResults(zipfsResults);
                    File.WriteAllText(String.Format("./{0}_ZipfsResults.csv", language), serializedZipfsResults);

                    IEnumerable<ExperimentResult> punctuationResults = punctuationExperiment.Perform(titlesAndArticlesPairs, language);
                    string serializedPunctuationResults = punctuationExperiment.GetSerializedResults(punctuationResults);
                    File.WriteAllText(String.Format("./{0}_PunctuationResults.csv", language), serializedPunctuationResults);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("Done. Press any key.");
            Console.ReadLine();
        }
    }

    public enum Language { EN, ES, DE, IT, PL, FR, SE, CZ, SK, HU }
}
