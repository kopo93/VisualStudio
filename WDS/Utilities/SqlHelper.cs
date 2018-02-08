using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;


namespace WDS.Utilities
{
    public class SqlHelper : IDisposable
    {
        private SqlConnection sqlconnection = null;
        private SqlTransaction sqltransaction = null;
        private SqlCommand sqlcommand = null;
        private SqlDataReader sqldatareader = null;
        private SqlDataAdapter sqldataadapter = null;
        private string stringConnectionNameDefault = "SqlHelper";
        private string stringConnectionNameReplicate = "SqlHelperReplicate";
        

        #region SqlHelper Constructors
        /// <summary>
        /// Default Connection Name (SqlHelper)
        /// Without Transaction
        /// </summary>
        public SqlHelper(string IsReplicate = null)
        {
            if (IsReplicate == null)
            {
                InitialComponents(stringConnectionNameDefault);
            }
            else if (IsReplicate == "SqlHelperMapReplicate")
            {
                InitialComponents("SqlHelperMapReplicate");
            }
            else
            {

                InitialComponents(stringConnectionNameReplicate);
            }
        }
        /// <summary>
        /// Use Asigned Connection Name
        /// Without Transaction
        /// </summary>
        /// <param name="stringConnectionName"></param>
        public SqlHelper( string stringConnectionName,string IsReplicate = null)
        {
            if (IsReplicate == null)
            {
                InitialComponents(stringConnectionName);
            }
            else
            {
                InitialComponents(stringConnectionName);
            }
        }
        /// <summary>
        /// Init SQL Components
        /// </summary>
        /// <param name="stringConnectionName"></param>
        private void InitialComponents(string stringConnectionName)
        {
            
            sqlconnection = new SqlConnection(ConfigurationManager.ConnectionStrings[stringConnectionName].ToString());
            sqlcommand = new SqlCommand();
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
        public int ExecNonQuery(string stringCommandText, SqlParameter[] sqlparameters)
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
        public DataTable GetDataTable(string stringCommandText, SqlParameter[] sqlparameters)
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
        public DataSet GetDataSet(string stringCommandText, SqlParameter[] sqlparameters)
        {
            SetCommand(stringCommandText, sqlparameters);
            DataSet dataset = new DataSet();
            sqldataadapter = new SqlDataAdapter();
            sqldataadapter.SelectCommand = sqlcommand;
            sqldataadapter.Fill(dataset);
            sqldataadapter.Dispose();
            sqldataadapter = null;
            return dataset;
        }
        #endregion

        #region GetDataObject
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
        public T GetDataObject<T>(string stringCommandText, SqlParameter[] sqlparameters)
        {
            object objectOfType = null;
            SqlDataReader sqldatareader = ExecReader(stringCommandText, sqlparameters);
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
                                    //propertyinfo.SetValue(objectOfType, obj, null);
                                    // Due to boolean is stored as TINYINT(1/0) in MySQL , when using setValue we need to convert into bool
                                    if (propertyinfo.PropertyType == typeof(bool?) && obj != null)
                                    {
                                        propertyinfo.SetValue(objectOfType, Convert.ToBoolean(obj), null);
                                    }
                                    else if (propertyinfo.PropertyType == typeof(bool) && obj != null)
                                    {
                                        propertyinfo.SetValue(objectOfType, Convert.ToBoolean(obj), null);
                                    }
                                    else
                                    {
                                        if (obj != null && obj.GetType() == typeof(bool))
                                        {
                                            propertyinfo.SetValue(objectOfType, Convert.ToBoolean(obj) == true ? "1" : "0", null);
                                        }
                                        else 
                                        {
                                            propertyinfo.SetValue(objectOfType, obj, null);
                                        }
                                      
                                    }
                                }
                                //if (null != propertyinfo && propertyinfo.CanWrite)
                                //{
                                //    propertyinfo.SetValue(objectOfType, obj, null);
                                //}
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
        #endregion

        #region GetDataList
        /// <summary>
        /// Get Custom Type Data Object List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
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
        public List<T> GetDataList<T>(string stringCommandText, SqlParameter[] sqlparameters)
        {
            var list = new List<T>();
            SqlDataReader sqldatareader = ExecReader(stringCommandText, sqlparameters);
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
                                        //propertyinfo.SetValue(objectOfType, obj, null);
                                        // Due to boolean is stored as TINYINT(1/0) in MySQL , when using setValue we need to convert into bool
                                        if (propertyinfo.PropertyType == typeof(bool?) && obj != null)
                                        {
                                            propertyinfo.SetValue(objectOfType, Convert.ToBoolean(obj), null);
                                        }
                                        else if (propertyinfo.PropertyType == typeof(bool) && obj != null)
                                        {
                                            propertyinfo.SetValue(objectOfType, Convert.ToBoolean(obj), null);
                                        }
                                        else
                                        {
                                            propertyinfo.SetValue(objectOfType, obj, null);
                                        }
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


        /// <summary>
        /// Get Stored ProcudureCustom Type Data Object List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public List<T> GetSP_DataList<T>(string stringCommandText)
        {
            return GetDataList<T>(stringCommandText, null);
        }


        /// <summary>
        /// Function For Get Store Procedures Data 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public List<T> GetSP_DataList<T>(string stringCommandText, SqlParameter[] sqlparameters)
        {
            var list = new List<T>();

            sqlcommand.CommandType = CommandType.StoredProcedure;
            if (sqlparameters != null)
            {
                foreach (SqlParameter sqlparameter in sqlparameters)
                {
                    if (sqlparameter != null)
                    {
                        if (sqlparameter.Value == null)
                        {
                            sqlparameter.Value = DBNull.Value;
                        }

                        sqlcommand.Parameters.Add(new SqlParameter(sqlparameter.ParameterName, sqlparameter.Value));
                    }
                }
            }
            SqlDataReader sqldatareader = ExecReader(stringCommandText, sqlparameters);

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
                                        // Due to boolean is stored as TINYINT(1/0) in MySQL , when using setValue we need to convert into bool
                                        if (propertyinfo.PropertyType == typeof(bool?) && obj != null)
                                        {
                                            propertyinfo.SetValue(objectOfType, Convert.ToBoolean(obj), null);
                                        }
                                        else if (propertyinfo.PropertyType == typeof(bool) && obj != null)
                                        {
                                            propertyinfo.SetValue(objectOfType, Convert.ToBoolean(obj), null);
                                        }
                                        //else if (propertyinfo.PropertyType == typeof(Double) && obj != null)
                                        //{
                                        //    propertyinfo.SetValue(objectOfType, Convert.ToDecimal(obj), null);

                                        //}
                                        else
                                        {
                                            propertyinfo.SetValue(objectOfType, obj, null);
                                        }
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
                    sqlcommand.CommandType = CommandType.Text;
                }
                catch { }
            }

            return list;

        }
        #endregion

        #region GetKeyValueList
        /// <summary>
        /// Get KeyValuePair<TKey, TValue> List
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public List<KeyValuePair<TKey, TValue>> GetKeyValueList<TKey, TValue>(string stringCommandText)
        {
            return GetKeyValueList<TKey, TValue>(stringCommandText, null);
        }


        /// <summary>
        /// Get KeyValuePair<TKey, TValue> List
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public List<KeyValuePair<TKey, TValue>> GetKeyValueList<TKey, TValue>(string stringCommandText, SqlParameter[] sqlparameters)
        {
            var list = new List<KeyValuePair<TKey, TValue>>();
            SqlDataReader sqldatareader = ExecReader(stringCommandText, sqlparameters);
            try
            {
                if (sqldatareader.HasRows)
                {
                    if (typeof(TKey).ToString().StartsWith("System."))
                    {
                        if (typeof(TValue).ToString().StartsWith("System."))
                        {
                            while (sqldatareader.Read())
                            {
                                list.Add(new KeyValuePair<TKey, TValue>((TKey)sqldatareader[0], (TValue)sqldatareader[1]));
                            }
                        }
                        else
                        {
                            ArrayList arraylistName = new ArrayList();
                            for (int i = 0; i < sqldatareader.FieldCount; i++)
                            {
                                string stringColumnName = sqldatareader.GetName(i);
                                foreach (PropertyInfo propertyinfo in typeof(TValue).GetProperties())
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
                                    object objectOfType = Activator.CreateInstance(typeof(TValue));
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
                                    list.Add(new KeyValuePair<TKey, TValue>((TKey)sqldatareader[0], (TValue)objectOfType));
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Dictionary in SqlHelper is not allow TKey with complex object type!");
                    }
                }
            }
            catch (ArgumentException)
            {
                throw new ApplicationException("Dictionary key duplicate!");
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

        #region GetDictionary
        /// <summary>
        /// Get Dictionary<TKey, TValue> List
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string stringCommandText)
        {
            return GetDictionary<TKey, TValue>(stringCommandText, null);
        }


        /// <summary>
        /// Get Dictionary<TKey, TValue> List
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string stringCommandText, SqlParameter[] sqlparameters)
        {
            var dictionary = new Dictionary<TKey, TValue>();
            SqlDataReader sqldatareader = ExecReader(stringCommandText, sqlparameters);
            try
            {
                if (sqldatareader.HasRows)
                {
                    if (typeof(TKey).ToString().StartsWith("System."))
                    {
                        if (typeof(TValue).ToString().StartsWith("System."))
                        {
                            while (sqldatareader.Read())
                            {
                                dictionary.Add((TKey)sqldatareader[0], (TValue)sqldatareader[1]);
                            }
                        }
                        else
                        {
                            ArrayList arraylistName = new ArrayList();
                            for (int i = 0; i < sqldatareader.FieldCount; i++)
                            {
                                string stringColumnName = sqldatareader.GetName(i);
                                foreach (PropertyInfo propertyinfo in typeof(TValue).GetProperties())
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
                                    object objectOfType = Activator.CreateInstance(typeof(TValue));
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
                                    dictionary.Add((TKey)sqldatareader[0], (TValue)objectOfType);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Dictionary in SqlHelper is not allow TKey with complex object type!");
                    }
                }
            }
            catch (ArgumentException)
            {
                throw new ApplicationException("Dictionary key duplicate!");
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
            return dictionary;
        }
        #endregion

        #region ExecReader
        /// <summary>
        /// Execute Reader (Get SqlDataReader Without SqlParameters)
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <returns></returns>
        public SqlDataReader ExecReader(string stringCommandText)
        {
            return ExecReader(stringCommandText, null);
        }
        /// <summary>
        /// Execute Reader
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        /// <returns></returns>
        public SqlDataReader ExecReader(string stringCommandText, SqlParameter[] sqlparameters)
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
        public object ExecScalar(string stringCommandText, SqlParameter[] sqlparameters)
        {
            SetCommand(stringCommandText, sqlparameters);
            return sqlcommand.ExecuteScalar();
        }
        #endregion

        #region Transaction Actions (Roolback , Commit)
        public void BeginTransaction()
        {
            sqltransaction = sqlconnection.BeginTransaction();
            sqlcommand.Transaction = sqltransaction;
        }
        public void Rollback()
        {
            if (sqltransaction != null)
            {
                sqltransaction.Rollback();
            }
        }

        public void Commit()
        {
            if (sqltransaction != null)
            {
                sqltransaction.Commit();
            }
        }
        #endregion

        /// <summary>
        /// Setup Sql Command
        /// </summary>
        /// <param name="stringCommandText"></param>
        /// <param name="sqlparameters"></param>
        private void SetCommand(string stringCommandText, SqlParameter[] sqlparameters)
        {
            if (sqlcommand.Parameters != null)
            {
                sqlcommand.Parameters.Clear();
            }
            sqlcommand.CommandText = stringCommandText;
            if (sqlparameters != null)
            {
                foreach (SqlParameter sqlparameter in sqlparameters)
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
                //Dispose Transaction
                if (sqltransaction != null)
                {
                    sqltransaction.Dispose();
                }
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
            return ColumnToString(objectValue, null);
        }
        /// <summary>
        /// Convert Column Value to String
        /// </summary>
        /// <param name="objectValue"></param>
        /// <param name="dbnullValue"></param>
        /// <returns></returns>
        public string ColumnToString(object objectValue, string dbnullValue)
        {
            if (objectValue == null)
            {
                return null;
            }
            else if (objectValue == DBNull.Value)
            {
                return dbnullValue;
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
