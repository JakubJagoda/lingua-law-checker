using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinguaLawChecker
{
    class HeapsLawExperiment : Experiment
    {
        
        public override IEnumerable<ExperimentResult> Perform(IEnumerable<Tuple<string, string>> titlesAndArticlesPairs, Language lang)
        {
            IEnumerable<ExperimentResult> results = titlesAndArticlesPairs.Select(input =>
            {
                string[] words = HeapsLawExperiment.GetWords(input.Item2).ToArray();
                ExperimentResult result = new ExperimentResult {
                    ArticleTitle = input.Item1,
                    ArticleContents = input.Item2,
                    ArticleLength = input.Item2.Length,
                    WordCount = words.Length
                };
                
                return result;
            });

            return results.OrderBy(result => result.WordCount);
        }

        public override string GetSerializedResults(IEnumerable<ExperimentResult> results)
        {
            StringBuilder csv = new StringBuilder();

            foreach (ExperimentResult result in results)
            {
                csv.AppendLine(String.Format("{0},{1}", result.ArticleLength, result.WordCount));
            }

            return csv.ToString();
        }
    }
}
