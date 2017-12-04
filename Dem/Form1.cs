using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dem
{

    public partial class Form1 : Form
    {
        private static Logger log = new Logger(typeof(Form1));
        public Form1()
        {
            InitializeComponent();
        }

        private static readonly string conn = ConfigurationManager.ConnectionStrings["conn"].ToString();
        static CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CancellationToken      token = tokenSource.Token;
        AutoResetEvent are = new AutoResetEvent(true);
        private bool isCancle = false;

        private string[] Names =
        {
            "赵", "钱", "孙", "李", "周", "吴", "郑", "王", "冯", "陈", "褚", "卫"
                ,"蒋",
            " 沈 ", "韩", " 杨", " 朱", " 秦", " 尤", " 许", "何", "吕", " 施", "张"
               , "孔",
            " 曹 ", "严", " 华", " 金", " 魏", " 陶", " 姜", "戚", "谢", " 邹", "喻"
                ,"柏",
            " 水 ", "窦", " 章", " 云", " 苏", " 潘", " 葛", "奚", "范", " 彭", "郎"
               , "鲁",
            " 韦 ", "昌", " 马", " 苗", " 凤", " 花", " 方", "俞", "任", " 袁", "柳"
               , "酆",
            " 鲍 ", "史", " 唐", " 费", " 廉", " 岑", " 薛", "雷", "贺", " 倪", "汤"
               , "滕",
            " 殷 ", "罗", " 毕", " 郝", " 邬", " 安", " 常", "乐", "于", " 时", "傅"
               , "皮",
            " 卞 ", "齐", " 康", " 伍", " 余", " 元", " 卜", "顾", "孟", " 平", "黄"
               , "和",
            " 穆 ", "萧", " 尹", " 姚", " 邵", " 湛", " 汪", "祁", "毛", " 禹", "狄"
               , "米",
            " 贝 ", "明", " 臧", " 计", " 伏", " 成", " 戴", "谈", "宋", " 茅", "庞"
              ,  "熊",
            " 纪 ", "舒", " 屈", " 项", " 祝", " 董", " 梁", "杜", "阮", " 蓝", "闵"
        };
        private void button1_Click(object sender, EventArgs e)
        {

            if (button1.Text.Equals("插入数据"))
            {
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                button1.Text = "停止插入";
                ThreadPool.QueueUserWorkItem(p =>
                {
                    
                    List<Task> tasks = new List<Task>();
                    TaskFactory factory = new TaskFactory();
                    for (int i = 0; i < 5; i++)
                    {
                        Task task = factory.StartNew(() =>
                        {
                            while (true)
                            {
                                token.ThrowIfCancellationRequested();
                                Student stu = new Student()
                                {
                                    Name = Names[new Random().Next() % Names.Count()] +
                                           Names[new Random().Next(DateTime.Now.Millisecond) % Names.Count()],
                                    Age = new Random().Next() % 100
                                };
                                try
                                {
                                    SqlHelper.Insert<Student>(stu, typeof(Student).Name);
                                    IndexHelper.CreateInstance().InsertIndex(stu);
                                }
                                catch (Exception exception)
                                {
                                    log.Error("Error:", exception);

                                }
                                
                               
                            }
                        });

                        tasks.Add(task);
                    }
                    try
                    {
                        Task.WaitAll(tasks.ToArray(), token);
                      
                    }
                    catch  
                    {
                      
                    }
               
                    
                });
            }
            else
            {
                button1.Text = "插入数据";
                tokenSource.Cancel();
                IndexHelper.CreateInstance().MergeIndex();

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Index\\";
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists) info.Create();

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {

        }
    }
}
