using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Collections;

namespace WDS.Utilities
{
    /// <summary>
    /// SQL Server Helper
    /// </summary>
    public class SqliteHelper : IDisposable
    {
        private SQLiteConnection sqlconnection = null;
        private SQLiteCommand sqlcommand = null;
        private SQLiteDataReader sqldatareader = null;
        private SQLiteDataAdapter sqldataadapter = null;
        private string stringConnectionString;
        private string stringConnectionNameDefault = "SqliteHelper";

        #region SqliteHelper Constructors
        /// <summary>
        /// Default Connection Name (SqlHelperForSqlite)
        /// Without Transaction
        /// </summary>
        public SqliteHelper()
        {
            InitialComponents(stringConnectionNameDefault);
        }
        /// <summary>
        /// Use Asigned Connection Name
        /// Without Transaction
        /// </summary>
        /// <param name="stringConnectionName"></param>
        public SqliteHelper(string stringConnectionName)
        {
            InitialComponents(stringConnectionName);
        }
        /// <summary>
        /// Init SQL Components
        /// </summary>
        /// <param name="stringConnectionName"></param>
        private void InitialComponents(string stringConnectionName)
        {
            stringConnectionString = ConfigurationManager.ConnectionStrings[stringConnectionName].ToString();
            sqlconnection = new SQLiteConnection(stringConnectionString);
            sqlcommand = new SQLiteCommand();

            sqlconnection.Open();
            sqlcommand.CommandType = CommandType.Text;
            sqlcommand.Connection = sqlconnection;
        }
        #endregion

        #region ExecNonQuery
        /// <summary>
        /// ExecNonQuery Without SqlParameters
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public int ExecNonQuery(string stringCommandText)
        {
            return ExecNonQuery(stringCommandText, null);
        }
        /// <summary>
        /// ExecNonQuery
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public int ExecNonQuery(string stringCommandText, SQLiteParameter[] sqlparameters)
        {
            SetCommand(stringCommandText, sqlparameters);
            return sqlcommand.ExecuteNonQuery();
        }
        #endregion

        #region GetDataTable , GetDataSet
        /// <summary>
        /// Get DataTable Without SqlParameters
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string stringCommandText)
        {
            return GetDataSet(stringCommandText).Tables[0];
        }
        /// <summary>
        /// Get DataTable
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string stringCommandText, SQLiteParameter[] sqlparameters)
        {
            return GetDataSet(stringCommandText, sqlparameters).Tables[0];
        }
        /// <summary>
        /// Get DataSet Without SqlParameters
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string stringCommandText)
        {
            return GetDataSet(stringCommandText, null);
        }
        /// <summary>
        /// Get DataSet
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string stringCommandText, SQLiteParameter[] sqlparameters)
        {
            SetCommand(stringCommandText, sqlparameters);
            DataSet dataset = new DataSet();
            sqldataadapter = new SQLiteDataAdapter();
            sqldataadapter.SelectCommand = sqlcommand;
            sqldataadapter.Fill(dataset);
            sqldataadapter.Dispose();
            sqldataadapter = null;
            return dataset;
        }
        #endregion

        /// <summary>
        /// Get Custom Type Data Object
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public T GetDataObject<T>(string stringCommandText)
        {
            return GetDataObject<T>(stringCommandText, null);
        }

        /// <summary>
        /// Get Custom Type Data Object
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public T GetDataObject<T>(string stringCommandText, SQLiteParameter[] sqlparameters)
        {
            object objectOfType = null;
            SQLiteDataReader sqldatareader = ExecReader(stringCommandText, sqlparameters);
            try
            {
                if (sqldatareader.HasRows)
                {
                    ArrayList arraylistName = new ArrayList();
                    for (int i = 0; i < sqldatareader.FieldCount; i++)
                    {
                        string stringColumnName = sqldatareader.GetName(i);
                        foreach (PropertyInfo propertyinfo in typeof(T).GetProperties())
                        {
                            //System.Reflection.PropertyAttributes patts = att.Attributes;
                            if (propertyinfo.Name.ToLower() == stringColumnName.ToLower())
                            {
                                arraylistName.Add(propertyinfo.Name);
                                break;
                            }
                        }
                    }
                    if (arraylistName.Count > 0)
                    {
                        if (sqldatareader.Read())
                        {
                            objectOfType = Activator.CreateInstance(typeof(T));
                            foreach (string stringColumnName in arraylistName)
                            {
                                object obj = sqldatareader[stringColumnName];
                                if (obj == DBNull.Value) obj = null;
                                PropertyInfo propertyinfo = objectOfType.GetType().GetProperty(stringColumnName, BindingFlags.Public | BindingFlags.Instance);
                                if (null != propertyinfo && propertyinfo.CanWrite)
                                {
                                    propertyinfo.SetValue(objectOfType, obj, null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try
                {
                    sqldatareader.Close();
                }
                catch { }
            }
            return (T)objectOfType;
        }

        #region GetDataList
        public List<T> GetDataList<T>(string stringCommandText)
        {
            return GetDataList<T>(stringCommandText, null);
        }

        /// <summary>
        /// Get Custom Type Data Object List
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public List<T> GetDataList<T>(string stringCommandText, SQLiteParameter[] sqlparameters)
        {
            var list = new List<T>();
            SQLiteDataReader sqldatareader = ExecReader(stringCommandText, sqlparameters);
            try
            {
                if (sqldatareader.HasRows)
                {
                    if (typeof(T).ToString().StartsWith("System."))
                    {
                        while (sqldatareader.Read())
                        {
                            list.Add((T)sqldatareader[0]);
                        }
                    }
                    else
                    {
                        ArrayList arraylistName = new ArrayList();
                        for (int i = 0; i < sqldatareader.FieldCount; i++)
                        {
                            string stringColumnName = sqldatareader.GetName(i);
                            foreach (PropertyInfo propertyinfo in typeof(T).GetProperties())
                            {
                                //System.Reflection.PropertyAttributes patts = att.Attributes;
                                if (propertyinfo.Name.ToLower() == stringColumnName.ToLower())
                                {
                                    arraylistName.Add(propertyinfo.Name);
                                    break;
                                }
                            }
                        }
                        if (arraylistName.Count > 0)
                        {
                            while (sqldatareader.Read())
                            {
                                object objectOfType = Activator.CreateInstance(typeof(T));
                                foreach (string stringColumnName in arraylistName)
                                {
                                    object obj = sqldatareader[stringColumnName];
                                    if (obj == DBNull.Value) obj = null;
                                    PropertyInfo propertyinfo = objectOfType.GetType().GetProperty(stringColumnName, BindingFlags.Public | BindingFlags.Instance);
                                    if (null != propertyinfo && propertyinfo.CanWrite)
                                    {
                                        propertyinfo.SetValue(objectOfType, obj, null);
                                    }
                                }
                                list.Add((T)objectOfType);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try
                {
                    sqldatareader.Close();
                }
                catch { }
            }
            return list;
        }
        #endregion

        #region ExecReader
        /// <summary>
        /// Execute Reader (Get SqlDataReader Without SqlParameters)
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public SQLiteDataReader ExecReader(string stringCommandText)
        {
            return ExecReader(stringCommandText, null);
        }
        /// <summary>
        /// Execute Reader
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public SQLiteDataReader ExecReader(string stringCommandText, SQLiteParameter[] sqlparameters)
        {
            SetCommand(stringCommandText, sqlparameters);
            sqldatareader = sqlcommand.ExecuteReader();
            return sqldatareader;
        }
        #endregion

        #region ExecScalar
        /// <summary>
        /// Execute Scalar, Get Single (first row, first column) Value Without SqlParameters
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public object ExecScalar(string stringCommandText)
        {
            return ExecScalar(stringCommandText, null);
        }
        /// <summary>
        /// Execute Scalar, Get Single (first row, first column) Value
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public object ExecScalar(string stringCommandText, SQLiteParameter[] sqlparameters)
        {
            SetCommand(stringCommandText, sqlparameters);
            return sqlcommand.ExecuteScalar();
        }
        #endregion


        /// <summary>
        /// Setup Sql Command
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        private void SetCommand(string stringCommandText, SQLiteParameter[] sqlparameters)
        {
            if (sqlcommand.Parameters != null)
            {
                sqlcommand.Parameters.Clear();
            }
            sqlcommand.CommandText = stringCommandText;
            if (sqlparameters != null)
            {
                foreach (SQLiteParameter sqlparameter in sqlparameters)
                {
                    if (sqlparameter != null)
                    {
                        if (sqlparameter.Value == null)
                        {
                            sqlparameter.Value = DBNull.Value;
                        }
                        sqlcommand.Parameters.Add(sqlparameter);
                    }
                }
            }
        }

        /// <summary>
        /// Dispose Resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                sqlcommand.Dispose();
                try
                {
                    sqldatareader.Close();
                    sqldatareader.Dispose();
                }
                catch { }
                try
                {
                    sqlconnection.Close();
                    sqlconnection.Dispose();
                }
                catch { }
            }
            catch { }
        }

        /// <summary>
        /// Convert Column Value to String
        /// </summary>
        /// <param name="objectValue"></param>
        /// <returns></returns>
        public string ColumnToString(object objectValue)
        {
            if (objectValue == null)
            {
                return null;
            }
            else if (objectValue == DBNull.Value)
            {
                return "[NULL]";
            }
            else if (objectValue.GetType() == typeof(DateTime))
            {
                return DateTime.Parse(objectValue.ToString()).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                return objectValue.ToString().Trim();
            }
        }
    }
}
