using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinguaLawChecker
{
    abstract class Experiment
    {
        protected static IEnumerable<string> GetWords(string input)
        {
            return Experiment.GetAllTokens(input)
                .Where(w => w.All(Char.IsLetter))
                .Distinct();
        }

        static IEnumerable<string> GetAllTokens(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select Experiment.TrimSuffix(m.Value);

            return words;
        }

        static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }

        public abstract IEnumerable<ExperimentResult> Perform(IEnumerable<Tuple<string, string>> inputs, params int[] arguments);
    }
}
