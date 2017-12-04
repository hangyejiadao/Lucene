using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using LuceneIO=Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Dem
{
    public class IndexHelper
    {
        private static IndexHelper instance;
        public static IndexHelper CreateInstance()
        {
            Object obj = new Object();
            lock (obj)
            {
                if (instance == null)
                {
                    lock (obj)
                    {
                        instance = new IndexHelper();
                    } 
                }
            } 
            return instance;
        }
        private Logger loger = new Logger(typeof(IndexHelper));
        private static readonly string path = AppDomain.CurrentDomain.BaseDirectory + "/Index/";
        private Document ParseToDoc(Student stu)
        {
            Document doc = new Document();
            doc.Add(new Field("ID", stu.ID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Name", stu.Name.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
            doc.Add(new Field("Age", stu.Age.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            return doc;
        }

        private Logger log = new Logger(typeof(Logger));
        public static string Path = AppDomain.CurrentDomain.BaseDirectory + "/Path";


        /// <summary>
        /// 根据一个实例创建索引创建索引
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="t"></param>
        private void CreateIndex(IndexWriter writer, Student t)
        {
            try
            {
                writer.AddDocument(ParseToDoc(t));
            }
            catch (Exception e)
            {
                log.Error("Error:", e);
            }
        }



        /// <summary>
        /// 增加索引
        /// </summary>
        /// <param name="ci"></param>
        public void InsertIndex(Student ci)
        {
            IndexWriter writer = null;
            try
            {
                if (ci == null)
                {
                    return;
                }

                DirectoryInfo dirInfo = Directory.CreateDirectory(Path);

                bool isCreate = dirInfo.GetFiles().Count() == 0;
                Lucene.Net.Store.Directory directory = Lucene.Net.Store.FSDirectory.Open(dirInfo);
                writer = new IndexWriter(directory, CreateAnalyzerWrapper(), isCreate, IndexWriter.MaxFieldLength.UNLIMITED);
                writer.MergeFactor = 100;
                writer.UseCompoundFile = true;
                CreateIndex(writer, ci);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private Analyzer CreateAnalyzerWrapper()
        {
            Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            PerFieldAnalyzerWrapper analyzerWrapper = new PerFieldAnalyzerWrapper(analyzer);
            analyzerWrapper.AddAnalyzer("Name", new PanGuAnalyzer());
            analyzerWrapper.AddAnalyzer("Age", new PanGuAnalyzer());
            return analyzerWrapper;
        }
        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="ci"></param>
        public void DeleteIndex(Student ci)
        {
            IndexReader reader = null;
            try
            {
                if (ci == null)
                {
                    return;
                }
                Analyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);
                string rootIndexPath = Path;
                DirectoryInfo dirInfo = Directory.CreateDirectory(rootIndexPath);
                Lucene.Net.Store.Directory directory = Lucene.Net.Store.FSDirectory.Open(dirInfo);
                reader = IndexReader.Open(directory, false);
                reader.DeleteDocuments(new Term("ID", ci.ID.ToString()));
            }
            catch (Exception e)
            {
                log.Error("DeleteIndex异常", e);

            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }

            }
        }
        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="ci"></param>
        public void UpdateIndex(Student ci)
        {
            IndexWriter writer = null;
            try
            {
                if (ci == null)
                    return;
                string rootIndexPath = Path;
                DirectoryInfo dirInfo = Directory.CreateDirectory(rootIndexPath);
                bool isCreate = dirInfo.GetFiles().Count() == 0;
                LuceneIO.Directory directory = LuceneIO.FSDirectory.Open(dirInfo);
                writer = new IndexWriter(directory, CreateAnalyzerWrapper(), isCreate,
                    IndexWriter.MaxFieldLength.LIMITED);
                writer.MergeFactor = 100;
                writer.UseCompoundFile = true;
                writer.UpdateDocument(new Term("ID", ci.ID.ToString()), ParseToDoc(ci));
            }
            catch (Exception e)
            {
                loger.Error("Error:", e);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// 将索引合并到上级目录
        /// </summary>
        /// <param name="childDirs">子文件夹名</param>
        public void MergeIndex(   )
        {
            IndexWriter writer = null;
            try
            {
               
                Analyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);
                string rootPath = Path;
                DirectoryInfo dirInfo = Directory.CreateDirectory(rootPath);
                LuceneIO.Directory directory = LuceneIO.FSDirectory.Open(dirInfo);
                writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);
                //LuceneIO.Directory[] dirNo = childDirs
                //    .Select(dir => LuceneIO.FSDirectory.Open(
                //        Directory.CreateDirectory(string.Format("{0}\\{1}", rootPath, dir)))).ToArray();

                writer.MergeFactor = 100;
                writer.UseCompoundFile = true;//创建符合文件 减少索引文件数量
                writer.AddIndexesNoOptimize(directory);
                Console.WriteLine("Over");
            }
            catch (Exception e)
            {
                log.Error("Error:", e);
            }
            finally
            {
                try
                {
                    if (writer != null)
                    {
                        writer.Optimize();
                        writer.Close();
                    }
                }
                finally
                {
                    Console.WriteLine("Over1");
                } 
            }
        }
    }
}
