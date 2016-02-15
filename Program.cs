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
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                IEnumerable<Tuple<string, string>> titlesAndArticlesPairs = await fetcher.GetNRandomArticlesForLanguage(Language.EN);
                foreach (Tuple<string, string> titleArticlePair in titlesAndArticlesPairs)
                {
                    Console.WriteLine("{0}: {1}", titleArticlePair.Item1, titleArticlePair.Item2);
                }

                Console.ReadLine();
            }).Wait();
        }
    }

    public enum Language { EN }
}
