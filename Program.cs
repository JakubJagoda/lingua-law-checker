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
        private static GoogleExperiment googleExperiment = new GoogleExperiment();

        static void Main(string[] args)
        {
            Task.Run(() => Program.MainAsync(args)).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            const int ARTICLES_COUNT = 10;

            bool forceFetching = args.Contains("-f");

            foreach (Language language in Enum.GetValues(typeof(Language)))
            {
                Console.WriteLine("Getting {0} articles for language {1}...", ARTICLES_COUNT, language);
                IEnumerable<Article> articles = await fetcher.GetNArticlesForLanguage(language, ARTICLES_COUNT, forceFetching);

                Console.WriteLine("Fetched, starting experiments...");

                try
                {
                    IEnumerable<ExperimentResult> results = heapsLawExperiment.Perform(articles, language);
                    string serializedResults = heapsLawExperiment.GetSerializedResults(results);
                    File.WriteAllText(String.Format("./{0}_HeapsResults.csv", language), serializedResults);

                    List<int> zipfsResults = ZipfsLawExperiment.Perform(articles);
                    string serializedZipfsResults = ZipfsLawExperiment.GetSerializedResults(zipfsResults);
                    File.WriteAllText(String.Format("./{0}_ZipfsResults.csv", language), serializedZipfsResults);

                    IEnumerable<ExperimentResult> punctuationResults = punctuationExperiment.Perform(articles, language);
                    string serializedPunctuationResults = punctuationExperiment.GetSerializedResults(punctuationResults);
                    File.WriteAllText(String.Format("./{0}_PunctuationResults.csv", language), serializedPunctuationResults);

                    //IEnumerable<ExperimentResult> results = await googleExperiment.PerformAsync(articles, language);
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
