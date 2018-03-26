using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace A
{
    public partial class Form1 : Form
    {
        



        public Form1()
        {
            InitializeComponent();
        }




        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            IndexWriter writer = null;
            Analyzer analyzer = new PanGuAnalyzer();
            Lucene.Net.Store.Directory dir = FSDirectory.Open(new System.IO.DirectoryInfo("IndexDir"));
            try
            {
                bool isCreate = !IndexReader.IndexExists(dir);
                writer = new IndexWriter(dir, analyzer, isCreate, IndexWriter.MaxFieldLength.UNLIMITED);
                for (int i = 1; i <= 5; i++)
                {
                    Document doc = new Document();
                    string path = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName +@"\Data\Test\" + i + ".txt";
                    string text = File.ReadAllText(path, Encoding.Default);
                    //Field.Store.YES:表示是否存储原值。只有当Field.Store.YES在后面才能用doc.Get("number")取出值来.Field.Index. NOT_ANALYZED:不进行分词保存
                    doc.Add(new Field("number", i.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    // Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS:不仅保存分词还保存分词的距离。
                    doc.Add(new Field("body", text, Field.Store.YES, Field.Index.ANALYZED,  Field.TermVector.WITH_POSITIONS_OFFSETS));
                    writer.AddDocument(doc);
                }
                writer.Optimize();
                sw.Stop();
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
                if (dir != null)
                {
                    dir.Dispose();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtSearch.Text))
                MessageBox.Show("请输入搜索的文本");
            StringBuilder sb = new StringBuilder();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //索引库目录
            Lucene.Net.Store.Directory dir = FSDirectory.Open(new System.IO.DirectoryInfo("IndexDir"), new NoLockFactory());
            IndexReader reader = IndexReader.Open(dir, true);
            IndexSearcher search = null;
            try
            {
                search = new IndexSearcher(reader);
                QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "body", new PanGuAnalyzer());
                Query query = parser.Parse(LuceneHelper.GetKeyWordSplid(this.txtSearch.Text));
                //执行搜索，获取查询结果集对象  
                TopDocs ts = search.Search(query, null, 1000);
                ///获取命中的文档信息对象  
                ScoreDoc[] docs = ts.ScoreDocs;
                sw.Stop();
                this.listBox1.Items.Clear();
                for (int i = 0; i < docs.Length; i++)
                {
                    int docId = docs[i].Doc;
                    Document doc = search.Doc(docId);
                    this.listBox1.Items.Add(doc.Get("number") + "\r\n");
                    this.listBox1.Items.Add(doc.Get("body") + "\r\n");
                    this.listBox1.Items.Add("------------------------\r\n");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (search != null)
                    search.Dispose();
                if (dir != null)
                    dir.Dispose();
            }
            this.label1.Text = "搜索用时:"+    sw.ElapsedMilliseconds + "毫秒";
        }
    }
}
