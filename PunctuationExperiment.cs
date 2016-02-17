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
        public override IEnumerable<ExperimentResult> Perform(IEnumerable<Tuple<string, string>> pairs, Language lang)
        {
            Regex regex = new Regex(@"[\p{P}]");

            return pairs.Select(pair => {
                int punctuation = pair.Item2.ToCharArray()
                    .Where(character => regex.IsMatch(character.ToString()))
                    .Count();

                ExperimentResult result = new ExperimentResult();
                result.ArticleLength = pair.Item2.Length;
                result.Punctuation = punctuation;
                result.ArticleContents = pair.Item2;

                return result;
            });
        }
        public override string GetSerializedResults(IEnumerable<ExperimentResult> results)
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
