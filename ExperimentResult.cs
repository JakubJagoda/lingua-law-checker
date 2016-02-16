using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguaLawChecker
{
    class ExperimentResult
    {
        public string ArticleTitle { get; set; }
        public string ArticleContents { get; set; }
        public int ArticleLength { get; set; }
        public int WordCount { get; set; }
        public Language Language { get; set; }
    }
}
