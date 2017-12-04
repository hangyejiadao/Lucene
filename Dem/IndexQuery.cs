using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;
namespace Dem
{
   public class IndexQuery
    {
        private Logger logger = new Logger(typeof(IndexQuery));
        public List<Student> QueryIndex(string queryString)
        {
            IndexSearcher searcher = null;
            List<Student> ciList = null;
            try
            {
                ciList = new List<Student>();
                Directory dir = FSDirectory.Open(IndexHelper.Path);
                searcher = new IndexSearcher(dir);
                Analyzer analyzer = new PanGuAnalyzer();
                //这里配置搜索条件
                QueryParser parser = new QueryParser(Version.LUCENE_30, "title", analyzer);
                Query query = parser.Parse(queryString);
                TopDocs docs = searcher.Search(query, (Filter)null, 10000);
                foreach (ScoreDoc sd in docs.ScoreDocs)
                {
                    Document doc = searcher.Doc(sd.Doc);
                    ciList.Add(DocumentToCommodityInfo(doc));
                }
                return ciList;
            }
            catch (Exception e)
            {
                logger.Error("Error:", e);
            }
            finally
            {
                if (searcher != null)
                {
                    searcher.Close();
                }
            }
            return ciList;
        }

        private Student DocumentToCommodityInfo(Document doc)
        {
            return new Student()
            {
                ID = int.Parse(doc.Get("ID")),
                Name = doc.Get("Name"),
                Age = int.Parse(doc.Get("Age"))  
            };
        }
 
    }
}
