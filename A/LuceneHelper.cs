using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Analysis.Tokenattributes;

namespace A
{
    public class LuceneHelper
    {
        public static string GetKeyWordSplid(string keywords)
        {
            StringBuilder sb = new StringBuilder();
            Analyzer analyzer = new PanGuAnalyzer();
            TokenStream stream = analyzer.TokenStream(keywords, new StringReader(keywords));
            ITermAttribute ita = null;
            bool hasNext = stream.IncrementToken();
            while (hasNext)
            {
                ita = stream.GetAttribute<ITermAttribute>();
                sb.Append(ita.Term + " ");
                hasNext = stream.IncrementToken();
            }
            return sb.ToString();
        }
    }
}
