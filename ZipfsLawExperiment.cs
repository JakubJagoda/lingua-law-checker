using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinguaLawChecker
{
    class ZipfsLawExperiment
    {
        public static List<int> Perform(IEnumerable<Article> articles)
        {
            Regex regex = new Regex("[^a-zA-Z]");

            List<string> words = articles
                .Select(pair => pair.Content)
                .Select(article => article.Split(' '))
                .Select(i => i.ToList())
                .Aggregate(new List<string>(), (list, next) => list.Concat(next).ToList())
                .Select(word => word.ToLower())
                .Select(word => regex.Replace(word, ""))
                .Where(word => word.Length > 0)
                .ToList();

            Dictionary<string, int> index = new Dictionary<string, int>();
            words.ForEach(word => {
                int count = index.ContainsKey(word) ? index[word] : 0;
                index[word] = ++count;
            });

            List<int> values = index.Values.ToList();
            values.Sort();
            values.Reverse();

            return values;
        }

        public static string GetSerializedResults(List<int> results)
        {
            StringBuilder csv = new StringBuilder();
            results.ForEach(value => csv.AppendLine(value.ToString()));
            return csv.ToString();
        }
    }
}
