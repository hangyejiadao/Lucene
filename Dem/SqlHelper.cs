using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder; 
using System.Windows.Forms;
 
 
namespace Dem
{
    public class SqlHelper
    {
        private static Logger logger = new Logger(typeof(SqlHelper));
        private static readonly  string _Constr = ConfigurationManager.ConnectionStrings["conn"].ToString();
        public static string sqlInfo = string.Empty;

        public static void GetInfo(string sql)
        {
            using (SqlConnection con = new SqlConnection(_Constr))
            {
                con.Open();
                con.InfoMessage += new SqlInfoMessageEventHandler(OnReceivingInfoMessage); ;
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
        private static void OnReceivingInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            var action = new Action(() =>
            {
                sqlInfo = e.ToString();
            });
            action.BeginInvoke(null, null);
        }




        public static void ExecuteNonQuery(string sql)
        {
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(_Constr))
                {
                    sqlConn.Open();
                    SqlCommand cmd = new SqlCommand(sql, sqlConn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                logger.Error("Error:", e);
            }

        }
        /// <summary>
        /// 通过事物
        /// </summary>
        /// <param name="sql"></param>
        public static void ExecuteNonQueryWidthTrans(string sql)
        {
            SqlTransaction trans = null;
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(_Constr))
                {
                    sqlConn.Open();
                    trans = sqlConn.BeginTransaction();
                    SqlCommand cmd = new SqlCommand(sql, sqlConn, trans);
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                if (trans != null && trans.Connection != null)
                {
                    trans.Rollback();
                    logger.Error("Error:", e);
                    throw e;
                }

            }

        }

        public static List<T> QueryList<T>(string sql) where T : new()
        {
            using (SqlConnection sqlCon = new SqlConnection(_Constr))
            {
                sqlCon.Open();
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                return TransList<T>(cmd.ExecuteReader());
            }
        }

        public static void Insert<T>(T model, string tableName) where T : new()
        {
            string sql = GetInsertSql<T>(model, tableName);
            ExecuteNonQuery(sql);
        }

        public static void InsertList<T>(List<T> list, string tableName) where T : new()
        {
            string sql = string.Join(" ", list.Select(t => GetInsertSql<T>(t, tableName)));
            ExecuteNonQuery(sql);
        }

        #region private

        private static string GetInsertSql<T>(T model, string tableName)
        {
            StringBuilder strSql = new StringBuilder();
            StringBuilder strFields = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();
            Type type = model.GetType();
            var properties = type.GetProperties();
            foreach (PropertyInfo p in properties)
            {
                string name = p.Name;
                if (!name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    strFields.AppendFormat("[{0}],", name);
                    string sValue = null;
                    object oValue = p.GetValue(model);
                    if (oValue != null)
                    {
                        sValue = oValue.ToString().Replace("'", "");
                    }
                    sbValues.AppendFormat("'{0}',", sValue);
                }
            }
            strSql.AppendFormat("INSERT INTO {0} ({1}) values ({2}); ", tableName, strFields.ToString().TrimEnd(','), sbValues.ToString().TrimEnd(','));
            return strSql.ToString();

        }

        private static string GetListInsertSql<T>(List<T> tList, string tableName)
        {
            Type type = typeof(T);
            StringBuilder strSql = new StringBuilder();
            StringBuilder strValues = new StringBuilder();
            try
            {
                foreach (T t in tList)
                {
                    strValues.AppendFormat("(");
                    StringBuilder tem = new StringBuilder();
                    foreach (var pInfo in type.GetProperties())
                    {
                        string sValue = null;
                        object oValue = pInfo.GetValue(t);
                        if (oValue != null)
                        {
                            sValue = oValue.ToString().Replace("'", "");
                        }
                        tem.AppendFormat("'{0}',", sValue);
                    }
                    strValues.AppendFormat(tem.ToString().TrimEnd(',')).AppendFormat("),");
                }
            }
            catch (Exception e)
            {
                logger.Error("Error:", e);
            }
            return string.Format("INSERT INTO {0} ({1}) VALUES {2}", tableName, GetFieldStr(type), strValues.ToString().TrimEnd(',').ToString());
        }
        private static string GetFieldStr(Type t)
        {
            StringBuilder sbStr = new StringBuilder();
            foreach (PropertyInfo p in t.GetProperties())
            {
                string name = p.Name;
                if (name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    sbStr.AppendFormat("[{0}],", name);
                }
            }
            return sbStr.ToString().TrimEnd(',');
        }
        private static List<T> TransList<T>(SqlDataReader reader) where T : new()
        {
            List<T> tList = new List<T>();
            Type type = typeof(T);
            var properties = type.GetProperties();
            if (reader.Read())
            {
                do
                {
                    T t = new T();
                    foreach (PropertyInfo p in properties)
                    {
                        p.SetValue(t, Convert.ChangeType(reader[p.Name], p.PropertyType));
                    }
                    tList.Add(t);
                } while (reader.Read());
            }
            return tList;
        }
        private static T TransModel<T>(SqlDataReader reader) where T : new()
        {
            T t = new T();
            if (reader.Read())
            {
                do
                {
                    Type type = t.GetType();
                    var properties = type.GetProperties();
                    foreach (PropertyInfo p in properties)
                    {
                        p.SetValue(t, Convert.ChangeType(reader[p.Name], p.PropertyType));
                    }
                } while (reader.Read());
            }
            return t;
        }

        #endregion



    }
}