using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinguaLawChecker
{
    class PunctuationExperiment : Experiment
    {
        public IEnumerable<ExperimentResult> Perform(IEnumerable<Article> pairs, Language lang)
        {
            Regex regex = new Regex(@"[\p{P}]");

            return pairs.Select(pair => {
                int punctuation = pair.Content.ToCharArray()
                    .Where(character => regex.IsMatch(character.ToString()))
                    .Count();

                ExperimentResult result = new ExperimentResult();
                result.ArticleLength = pair.Content.Length;
                result.Punctuation = punctuation;
                result.ArticleContents = pair.Content;

                return result;
            }).OrderBy(result => result.ArticleLength);
        }
        public string GetSerializedResults(IEnumerable<ExperimentResult> results)
        {
            StringBuilder csv = new StringBuilder();

            foreach (ExperimentResult result in results)
            {
                float percentage = (float) result.Punctuation / (float) result.ArticleLength;
                csv.AppendLine(String.Format("{0},{1},{2}", result.ArticleLength, result.Punctuation, percentage.ToString("0.000", CultureInfo.GetCultureInfo("en-US"))));
            }

            return csv.ToString();
        }
    }
}
