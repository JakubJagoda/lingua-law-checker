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
        
        public override IEnumerable<ExperimentResult> Perform(IEnumerable<Tuple<string, string>> titlesAndArticlesPairs, params int[] arguments)
        {
            IEnumerable<ExperimentResult> results = titlesAndArticlesPairs.Select(input =>
            {
                ExperimentResult result = new ExperimentResult();
                string[] words = HeapsLawExperiment.GetWords(input.Item2).ToArray();
                result.ArticleTitle = input.Item1;
                result.ArticleContents = input.Item2;
                result.ArticleLength = input.Item2.Length;
                result.WordCount = words.Length;

                return result;
            });

            return results;
        }
    }
}
