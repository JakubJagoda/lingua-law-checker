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
        
        public IEnumerable<ExperimentResult> Perform(IEnumerable<Article> titlesAndArticlesPairs, Language lang)
        {
            IEnumerable<ExperimentResult> results = titlesAndArticlesPairs.Select(input =>
            {
                string[] words = HeapsLawExperiment.GetWords(input.Content).ToArray();
                ExperimentResult result = new ExperimentResult {
                    ArticleTitle = input.Title,
                    ArticleContents = input.Content,
                    ArticleLength = input.Content.Length,
                    WordCount = words.Length
                };
                
                return result;
            });

            return results.OrderBy(result => result.ArticleLength);
        }

        public string GetSerializedResults(IEnumerable<ExperimentResult> results)
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
