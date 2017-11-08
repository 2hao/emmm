﻿/* Copyright 2009 无忧lwz0721@gmail.com
 * @author 无忧lwz0721@gmail.com
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Data;
using System.Data.OracleClient;
using FireWorkflow.Net.Engine.Persistence;

namespace FireWorkflow.Net.Persistence.OracleDAL
{
    /// <summary>
    /// 辅助执行查询Oracle数据库
    /// </summary>
    public abstract class OracleHelper
    {
        /// <summary>引发异常</summary>
        /// <param name="log">异常消息</param>
        /// <param name="commandParameters">OracleParameter对象</param>
        public static void Log(string log, params OracleParameter[] commandParameters)
        {
            StringBuilder sb = new StringBuilder();
            if (commandParameters != null)
            {
                int s = commandParameters.Length;
                for (int i = 0; i < s; i++)
                {
                    sb.Append(":");
                    sb.Append(i + 1);
                    sb.Append("=");
                    if (commandParameters[i] != null && commandParameters[i].Value != null)
                        sb.Append(commandParameters[i].Value.ToString());
                    else
                        sb.Append("null");
                    sb.Append("; ");
                }
            }
            if (sb.Length > 0) throw new Exception(log + sb.ToString());
            else throw new Exception(log);
        }
        //Create a hashtable for the parameter cached
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>创建OracleParameter对象</summary>
        /// <remarks>
        /// e.g.:  
        ///     params OracleParameter[] selectParms = { 
        ///         OracleHelper.NewOracleParameter(":smn01", OracleType.NVarchar22, smn01)
        ///     };
        /// </remarks>
        /// <param name="parameterName">要映射的参数的名称。</param>
        /// <param name="dbType">SqlDbType 值之一。</param>
        /// <param name="value">一个 Object，它是 OracleParameter 的值。</param>
        public static OracleParameter NewOracleParameter(string parameterName, OracleType dbType, object value)
        {
            return NewOracleParameter(parameterName, dbType, 0, ParameterDirection.Input, value);
        }

        /// <summary>创建OracleParameter对象</summary>
        /// <remarks>
        /// e.g.:  
        ///     params OracleParameter[] selectParms = { 
        ///         OracleHelper.NewOracleParameter(":smn01", OracleType.NVarchar22, 20, smn01)
        ///     };
        /// </remarks>
        /// <param name="parameterName">要映射的参数的名称。</param>
        /// <param name="dbType">SqlDbType 值之一。</param>
        /// <param name="size">参数的长度。</param>
        /// <param name="value">一个 Object，它是 OracleParameter 的值。</param>
        public static OracleParameter NewOracleParameter(string parameterName, OracleType dbType, int size, object value)
        {
            return NewOracleParameter(parameterName, dbType, size, ParameterDirection.Input, value);
        }
        /// <summary>创建OracleParameter对象</summary>
        /// <remarks>
        /// e.g.:  
        ///     params OracleParameter[] selectParms = { 
        ///         OracleHelper.NewOracleParameter(":smn01", OracleType.NVarchar2, 20, ParameterDirection.Output, smn01)
        ///     };
        /// </remarks>
        /// <param name="parameterName">要映射的参数的名称。</param>
        /// <param name="dbType">SqlDbType 值之一。</param>
        /// <param name="size">参数的长度。</param>
        /// <param name="direction">ParameterDirection 值之一。</param>
        /// <param name="value">一个 Object，它是 OracleParameter 的值。</param>
        public static OracleParameter NewOracleParameter(string parameterName, OracleType dbType, int size, ParameterDirection direction, object value)
        {
            //if (dbType == OracleType.DateTime)
            //{
            //    if ((DateTime)value < DateTime.Parse("1/1/1753 12:00:00")) value = DateTime.Parse("1/1/1753 12:00:00");
            //}
            OracleParameter sp;
            if (size == 0) sp = new OracleParameter(parameterName, dbType);
            else sp = new OracleParameter(parameterName, dbType, size);
            sp.Direction = direction;
            if (value == null) sp.Value = DBNull.Value;
            else sp.Value = value;
            return sp;
        }

        public static OracleTransaction GetOracleTransaction(string connectionString)
        {
            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();
            return  conn.BeginTransaction();
        }

        /// <summary>初始化OracleCommand 对象</summary>
        /// <param name="conn">Connection 对象</param>
        /// <param name="cmdType">CommandType 值之一。</param>
        /// <param name="cmdText">已重写。 获取或设置要对数据源执行的 Transact-SQL 语句或存储过程。</param>
        /// <param name="commandParameters">参数</param>
        private static OracleCommand PrepareCommand(OracleConnection conn, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {

            //Open the connection if required
            if (conn.State != ConnectionState.Open)
                conn.Open();
            OracleCommand cmd = new OracleCommand(cmdText, conn);
            //Set up the command
            cmd.CommandType = cmdType;

            //Bind it to the transaction if it exists
            //if (trans != null)
            //    cmd.Transaction = trans;

            // Bind the parameters passed in
            if (commandParameters != null && commandParameters.Length > 0)
            {
                foreach (OracleParameter parm in commandParameters)
                    cmd.Parameters.Add(parm);
            }
            return cmd;
        }

        /// <summary>初始化SqlCommand 对象</summary>
        /// <param name="trans">Optional transaction object</param>
        /// <param name="cmdType">CommandType 值之一。</param>
        /// <param name="cmdText">已重写。 获取或设置要对数据源执行的 Transact-SQL 语句或存储过程。 </param>
        /// <param name="commandParameters">参数</param>
        private static OracleCommand PrepareCommand(OracleTransaction trans, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {
            if (trans == null)
                throw new ArgumentNullException("transaction");
            if (trans != null && trans.Connection == null)
                throw new ArgumentException("The transaction was rollbacked	or commited, please	provide	an open	transaction.", "transaction");

            //Open the connection if required
            if (trans.Connection.State != ConnectionState.Open)
                trans.Connection.Open();
            OracleCommand cmd = new OracleCommand(cmdText, trans.Connection);
            //Set up the command
            cmd.CommandType = cmdType;

            //Bind it to the transaction if it exists
            //if (trans != null)
            //    cmd.Transaction = trans;

            // Bind the parameters passed in
            if (commandParameters != null)
            {
                foreach (OracleParameter parm in commandParameters)
                    cmd.Parameters.Add(parm);
            }
            return cmd;
        }

        /// <summary>针对 Connection 执行 SQL 语句并返回受影响的行数。</summary>
        /// <param name="connString">数据库连接字符串</param>
        /// <param name="cmdType">指定如何解释命令字符串。</param>
        /// <param name="cmdText">要执行的Sql语句,或存储过程名称。</param>
        /// <param name="commandParameters">传入或传出的参数值</param>
        /// <returns>针对 Connection 执行 SQL 语句并返回受影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {
            // Create a new Oracle Connection
            OracleConnection connection = new OracleConnection(connectionString);
            return ExecuteNonQuery(connection, cmdType, cmdText, commandParameters);
        }


        /// <summary>返回数量</summary>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or PL/SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        public static Int32 ExecuteInt32(string connectionString, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {
            object obj = ExecuteScalar(connectionString, cmdType, cmdText, commandParameters);
            if (obj != null)
            {
                try
                {
                    return Convert.ToInt32(obj);
                }
                catch
                {
                    return 0;
                }
            }
            else return 0;
        }

        /// <summary>
        /// Execute an OracleCommand (that returns no resultset) against an existing database transaction 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders", new OracleParameter(":prodid", 24));
        /// </remarks>
        /// <param name="trans">事务对象</param>
        /// <param name="cmdType">指定如何解释命令字符串。</param>
        /// <param name="cmdText">要执行的Sql语句,或存储过程名称。</param>
        /// <param name="commandParameters">传入或传出的参数值</param>
        /// <returns>针对 Connection 执行 SQL 语句并返回受影响的行数</returns>
        public static int ExecuteNonQuery(OracleTransaction trans, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = PrepareCommand(trans, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>针对 Connection 执行 SQL 语句并返回受影响的行数</summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OracleParameter(":prodid", 24));
        /// </remarks>
        /// <param name="conn">数据库连接对象</param>
        /// <param name="commandType">指定如何解释命令字符串</param>
        /// <param name="commandText">要执行的Sql语句,或存储过程名称。</param>
        /// <param name="commandParameters">传入或传出的参数值</param>
        /// <returns>针对 Connection 执行 SQL 语句并返回受影响的行数</returns>
        public static int ExecuteNonQuery(OracleConnection connection, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {
            try
            {
                OracleCommand cmd = PrepareCommand(connection, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                cmd.Dispose();
                return val;
            }
            catch (Exception e)
            {
                Log(e.Message + "\n" + cmdText, commandParameters);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            return 0;
        }

        /// <summary>返回执行结果</summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType">指定如何解释命令字符串。</param>
        /// <param name="cmdText">Sql语句或存储过程名</param>
        /// <param name="commandParameters">传入的参数集</param>
        /// <returns>返回OracleDataReader</returns>
        public static OracleDataReader ExecuteReader(OracleConnection conn, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {
            //Create the command and connection
            //OracleConnection conn = new OracleConnection(connectionString);
            try
            {
                //Prepare the command to execute
                OracleCommand cmd = PrepareCommand(conn, cmdType, cmdText, commandParameters);

                //Execute the query, stating that the connection should close when the resulting datareader has been read
                OracleDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                //rdr[
                //cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception e)
            {
                conn.Close();
                Log(e.Message + "\n" + cmdText, commandParameters);
            }
            return null;
        }

        /// <summary>返回执行结果。</summary>
        /// <param name="connString">数据库连接字符串</param>
        /// <param name="cmdType">指定如何解释命令字符串。</param>
        /// <param name="cmdText">要执行的Sql语句,或存储过程名称。</param>
        /// <param name="commandParameters">传入或传出的参数值</param>
        /// <returns>放回执行结果集合</returns>
        public static DataSet ExecuteDataSet(string connectionString, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {
            // Create a new Sql command
            OracleConnection connection = new OracleConnection(connectionString);
            DataSet ds = new DataSet();
            try
            {
                OracleCommand cmd = PrepareCommand(connection, cmdType, cmdText, commandParameters);
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.Fill(ds);
                return ds;
            }
            catch (Exception e)
            {
                Log(e.Message + "\r\n" + cmdText, commandParameters);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            return null;
        }

        ///// <summary>返回执行结果。</summary>
        ///// <param name="connString">数据库连接字符串</param>
        ///// <param name="cmdType">指定如何解释命令字符串。</param>
        ///// <param name="cmdText">要执行的Sql语句,或存储过程名称。</param>
        ///// <param name="commandParameters">传入或传出的参数值</param>
        ///// <returns>放回执行结果集合</returns>
        //public static IList<T> ExecuteInfo<T>(string connectionString, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters) 
        //    where T : IReaderToInfo, new()
        //{
        //    IList<T> Ts = new List<T>();

        //    OracleConnection connection = new OracleConnection(connectionString);
        //    OracleDataReader reader = null;
            
        //    try
        //    {
        //        reader = OracleHelper.ExecuteReader(connection, cmdType, cmdText, commandParameters);
        //        if (reader != null)
        //        {
        //            while (reader.Read())
        //            {
        //                T t = new T();
        //                t.ReaderToInfo(reader);
        //                Ts.Add(t);
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader = null;
        //        }
        //        if (connection.State != ConnectionState.Closed)
        //        {
        //            connection.Close();
        //            connection = null;
        //        }
        //    }
        //    if (Ts == null || Ts.Count <= 0) return null;
        //    return Ts;
        //}

        /// <summary>分页返回执行结果</summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="pageIndex">页数</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="totalRecords">当此方法返回时，此参数包含在集合中返回的记录的总数。该参数未经初始化即被传递。</param>
        /// <param name="tblName">要显示的表或多个表的连接</param>
        /// <param name="fldName">要显示的字段列表</param>
        /// <param name="strCondition">查询条件,不包括WHERE</param>
        /// <param name="fldSort">排序字段列表或条件,不包括ORDER BY</param>
        /// <param name="commandParameters">传入的参数集</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(OracleConnection conn,
            int pageIndex, int pageSize, out int totalRecords, string tblName, string fldName, string strCondition, string fldSort,
            params OracleParameter[] commandParameters)
        {
            string select = "";
            int PageLowerBound = (pageIndex /*- 1*/) * pageSize;
            int PageUpperBound = PageLowerBound + pageSize;
            totalRecords = 0;

            string sTemp = "";
            if (!string.IsNullOrEmpty(strCondition))
            {
                sTemp += " WHERE " + strCondition;
            }
            string stotalRecords = "SELECT COUNT(*) FROM {0}{1}";
            object obj = ExecuteScalar(conn.ConnectionString, CommandType.Text, string.Format(stotalRecords, tblName, sTemp), commandParameters);
            if (obj != null && obj is decimal)
            {
                totalRecords = Convert.ToInt32(obj);
            }
            if (totalRecords < 1 || totalRecords < (PageLowerBound + 1))
            {
                return null;
            }
            if (!string.IsNullOrEmpty(fldSort))
            {
                sTemp += " ORDER BY " + fldSort;
            }

            if (pageIndex == 0)
                select = string.Format("SELECT * FROM (SELECT {0} FROM {1}{2}) WHERE ROWNUM<={3}", fldName, tblName, sTemp, PageUpperBound);
            else
                select = string.Format("SELECT * FROM (SELECT A.*,ROWNUM RNM FROM (SELECT {0} FROM {1}{2}) A WHERE ROWNUM <= {3}) WHERE RNM > {4}", fldName, tblName, sTemp, PageUpperBound, PageLowerBound);

            try
            {
                //Prepare the command to execute
                conn.Open();
                OracleCommand cmd = PrepareCommand(conn, CommandType.Text, select, commandParameters);

                //Execute the query, stating that the connection should close when the resulting datareader has been read
                OracleDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception e)
            {
                //If an error occurs close the connection as the reader will not be used and we expect it to close the connection
                conn.Close();
                Log(e.Message + "\n" + select, commandParameters);
            }
            return null;
        }

        /// <summary>
        /// Execute an OracleCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new OracleParameter(":prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or PL/SQL command</param>
        /// <param name="commandParameters">an array of OracleParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            return ExecuteScalar(connection, cmdType, cmdText, commandParameters);
        }

        /// <summary>
        /// Execute an OracleCommand that returns the first column of the first record against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(conn, CommandType.StoredProcedure, "PublishOrders", new OracleParameter(":prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or PL/SQL command</param>
        /// <param name="commandParameters">an array of OracleParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(OracleConnection connection, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
        {
            try
            {
                OracleCommand cmd = PrepareCommand(connection, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception e)
            {
                Log(e.Message + "\n" + cmdText, commandParameters);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            return null;
        }

        ///	<summary>
        ///	Execute	a OracleCommand (that returns a 1x1 resultset)	against	the	specified SqlTransaction
        ///	using the provided parameters.
        ///	</summary>
        ///	<param name="transaction">A	valid SqlTransaction</param>
        ///	<param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        ///	<param name="cmdText">The stored procedure name	or PL/SQL command</param>
        ///	<param name="commandParameters">An array of	OracleParamters used to execute the command</param>
        ///	<returns>An	object containing the value	in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string cmdText, params OracleParameter[] commandParameters)
        {
            try
            {
                if (transaction == null)
                    throw new ArgumentNullException("transaction");
                if (transaction != null && transaction.Connection == null)
                    throw new ArgumentException("The transaction was rollbacked	or commited, please	provide	an open	transaction.", "transaction");

                // Create a	command	and	prepare	it for execution
                OracleCommand cmd = PrepareCommand(transaction.Connection, commandType, cmdText, commandParameters);

                // Execute the command & return	the	results
                object retval = cmd.ExecuteScalar();

                // Detach the OracleParameters	from the command object, so	they can be	used again
                cmd.Parameters.Clear();
                return retval;
            }
            catch (Exception e)
            {
                Log(e.Message + "\n" + cmdText, commandParameters);
            }
            return null;
        }

        /// <summary>
        /// Add a set of parameters to the cached
        /// </summary>
        /// <param name="cacheKey">Key value to look up the parameters</param>
        /// <param name="commandParameters">Actual parameters to cached</param>
        public static void CacheParameters(string cacheKey, params OracleParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        /// <summary>
        /// Fetch parameters from the cache
        /// </summary>
        /// <param name="cacheKey">Key to look up the parameters</param>
        /// <returns></returns>
        public static  OracleParameter[] GetCachedParameters(string cacheKey)
        {
            OracleParameter[] cachedParms = ( OracleParameter[])parmCache[cacheKey];

            if (cachedParms == null)
                return null;

            // If the parameters are in the cache
            OracleParameter[] clonedParms = new OracleParameter[cachedParms.Length];

            // return a copy of the parameters
            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (OracleParameter)((ICloneable)cachedParms[i]).Clone();

            return clonedParms;
        }


        /// <summary>
        /// Converter to use boolean data type with Oracle
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns></returns>
        public static Int16 OraBit(Boolean? value)
        {
            if (value != null && (Boolean)value)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Converter to use boolean data type with Oracle
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns></returns>
        public static Boolean OraBool(Int16 value)
        {
            if (value == 1)
                return true;
            else
                return false;
        }

        /// <summary>获取查询对象</summary>
        /// <param name="queryField">要创建查询的QueryField对象</param>
        public static QueryInfo GetFormatQuery(QueryField queryField)
        {
            //为空返回
            if (queryField == null) return new QueryInfo();
            if (queryField.QueryFieldInfos.Count <= 0 && queryField.QueryFieldInfosOr.Count <= 0) return new QueryInfo();
            //Oracle参数集合
            List<OracleParameter> sp = new List<OracleParameter>();
            //查询SQL合集
            StringBuilder sb = new StringBuilder();
            //循环变量
            int i = 0, j = 0;//, k = 0;
            //连接方式 "=","LIKE","<","<=",">",">="
            String fs = "";
            //值
            String value = "";
            String tsql = "";
            string PN = "";//参数变量

            #region 获取 OR 集合
            foreach (QueryFieldInfo queryFieldInfo in queryField.QueryFieldInfosOr)
            {
                if (string.IsNullOrEmpty(queryFieldInfo.QueryString.Trim())) continue;
                try
                {
                    //设定默认连接方式
                    fs = "=";
                    //初始化
                    tsql = "";

                    //分割多匹配
                    String[] fgs = queryFieldInfo.QueryString.Split('|');

                    //多匹配循环计数清零；
                    j = 0;
                    //初始化
                    tsql = "";
                    foreach (String fg in fgs)
                    {
                        value = fg.Trim();
                        //为空退出本次循环
                        if (String.IsNullOrEmpty(value)) continue;
                        PN = ":p" + i + "_" + j;

                        //设定默认连接方式
                        fs = "=";

                        switch (queryFieldInfo.FieldType)
                        {
                            #region Guid,String
                            case CSharpType.Guid:
                            case CSharpType.String:
                                if (Regex.IsMatch(value, @"\*|\?"))
                                {
                                    fs = "LIKE";
                                    value = Regex.Replace(value, @"\*", "%");
                                    value = Regex.Replace(value, @"\?", "_");
                                }
                                if (Regex.IsMatch(value, @"^<>|^!="))
                                {
                                    if (fs == "LIKE") fs = "NOT LIKE";
                                    else fs = "<>";
                                    value = value.Substring(2);
                                }
                                //增加查询SQL合集
                                sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), value));
                                if (tsql.Length > 0) tsql += " OR ";
                                tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                break;
                            #endregion
                            #region Int16,Int32,Int64,Decimal,Double,Single,Byte
                            case CSharpType.Int16:
                            case CSharpType.Int32:
                            case CSharpType.Decimal:
                            case CSharpType.Double:
                            case CSharpType.Single:
                            case CSharpType.Byte:
                                if (Regex.IsMatch(value, @"^\d*[.]{0,1}\d*$"))
                                {
                                    //增加查询SQL合集
                                    try
                                    {
                                        object tv = GetValue(queryFieldInfo.FieldType, value);
                                        sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), tv));
                                        if (tsql.Length > 0) tsql += " OR ";
                                        tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                    }
                                    catch { }
                                }
                                else
                                {
                                    //开始字符有< <= <> != > >=
                                    if (Regex.IsMatch(value, @"^(<|<=|<>|!=|>|>=)\d*[.]{0,1}\d*$"))
                                    {
                                        if (Regex.IsMatch(value, @"^(<=|>=|<>|!=)"))
                                        {
                                            //开始字符有 <= <> !=  >=
                                            fs = value.Substring(0, 2);
                                            if (fs == "!=") fs = "<>";
                                            value = value.Substring(2);
                                        }
                                        else
                                        {
                                            //开始字符有< >
                                            fs = value.Substring(0, 1);
                                            value = value.Substring(1);
                                        }
                                        //增加查询SQL合集
                                        try
                                        {
                                            object tv = GetValue(queryFieldInfo.FieldType, value);
                                            sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), tv));
                                            if (tsql.Length > 0) tsql += " OR ";
                                            tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        if (Regex.IsMatch(value, @"^\d*[.]{0,1}\d*[~]{1}\d*[.]{0,1}\d*$"))
                                        {
                                            string[] temps = value.Split('~');
                                            if (temps.Length == 2 && !String.IsNullOrEmpty(temps[0].Trim()) && !String.IsNullOrEmpty(temps[1].Trim()))
                                            {
                                                try
                                                {
                                                    object tv1 = GetValue(queryFieldInfo.FieldType, temps[0]);
                                                    object tv2 = GetValue(queryFieldInfo.FieldType, temps[1]);
                                                    sp.Add(OracleHelper.NewOracleParameter(PN + "q", GetOracleDbType(queryFieldInfo.FieldType), tv1));
                                                    sp.Add(OracleHelper.NewOracleParameter(PN + "h", GetOracleDbType(queryFieldInfo.FieldType), tv2));
                                                    if (tsql.Length > 0) tsql += " OR ";
                                                    tsql += string.Format("({0} >= {1}q AND {0} <= {1}h)", queryFieldInfo.FieldName, PN);
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                                break;
                            #endregion
                            #region DateTime
                            case CSharpType.DateTime:
                                if (Regex.IsMatch(value, @"^(1|2)\d{3}-\d{1,2}-\d{1,2}[ ]{0,1}\d{0,2}[:]{0,1}\d{0,2}[:]{0,1}\d{0,2}$"))
                                {
                                    //存数字增加查询SQL合集
                                    try
                                    {
                                        DateTime dt = DateTime.Parse(value);
                                        sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), dt));
                                        if (tsql.Length > 0) tsql += " OR ";
                                        tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                    }
                                    catch { }
                                }
                                else
                                {
                                    if (Regex.IsMatch(value, @"^(<|<=|<>|!=|>|>=)(1|2)\d{3}-\d{1,2}-\d{1,2}[ ]{0,1}\d{0,2}[:]{0,1}\d{0,2}[:]{0,1}\d{0,2}$"))
                                    {
                                        if (Regex.IsMatch(value, @"^(<=|>=|<>|!=)"))
                                        {
                                            fs = value.Substring(0, 2);
                                            if (fs == "!=") fs = "<>";
                                            value = value.Substring(2);
                                        }
                                        else
                                        {
                                            fs = value.Substring(0, 1);
                                            value = value.Substring(1);
                                        }
                                        //增加查询SQL合集
                                        try
                                        {
                                            DateTime dt = DateTime.Parse(value);
                                            sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), dt));
                                            if (tsql.Length > 0) tsql += " OR ";
                                            tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        if (Regex.IsMatch(value, @"^(1|2)\d{3}-\d{1,2}-\d{1,2}[ ]{0,1}\d{0,2}[:]{0,1}\d{0,2}[:]{0,1}\d{0,2}[~]{1}(1|2)\d{3}-\d{1,2}-\d{1,2}[ ]{0,1}\d{0,2}[:]{0,1}\d{0,2}[:]{0,1}\d{0,2}$"))
                                        {
                                            string[] temps = value.Split('~');


                                            if (temps.Length == 2 && !String.IsNullOrEmpty(temps[0].Trim()) && !String.IsNullOrEmpty(temps[1].Trim()))
                                            {
                                                try
                                                {
                                                    DateTime dt01 = DateTime.Parse(temps[0]);
                                                    DateTime dt02 = DateTime.Parse(temps[1]);
                                                    sp.Add(OracleHelper.NewOracleParameter(PN + "q", GetOracleDbType(queryFieldInfo.FieldType), dt01));
                                                    sp.Add(OracleHelper.NewOracleParameter(PN + "h", GetOracleDbType(queryFieldInfo.FieldType), dt02));
                                                    if (tsql.Length > 0) tsql += " OR ";
                                                    tsql += string.Format("({0} >= {1}q AND {0} <= {1}h)", queryFieldInfo.FieldName, PN);
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                                break;
                            #endregion
                            #region  Boolean
                            case CSharpType.Boolean:
                                if (Regex.IsMatch(value, @"^<>|^!="))
                                {
                                    fs = "<>";
                                    value = value.Substring(2);
                                }
                                try
                                {
                                    bool bt = bool.Parse(value);
                                    sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), OracleHelper.OraBit(bt)));
                                    if (tsql.Length > 0) tsql += " OR ";
                                    tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                }
                                catch { }
                                break;
                            case CSharpType.Char:
                                if (Regex.IsMatch(value, @"^<>|^!="))
                                {
                                    fs = "<>";
                                    value = value.Substring(2);
                                }
                                if (value.Trim().Length == 1)
                                {
                                    sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), GetValue(queryFieldInfo.FieldType, value)));
                                    if (tsql.Length > 0) tsql += " OR ";
                                    tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                }
                                break;
                            #endregion
                        }

                        //多匹配循环计数加1；
                        j++;
                    }

                    //查询不为空添加到 查询SQL合集中
                    if (!String.IsNullOrEmpty(tsql))
                    {
                        if (sb.Length > 0) sb.Append(" OR ");
                        else sb.Append("(");
                        sb.AppendFormat("({0})", tsql);
                    }

                }
                catch { }
                //循环总计数加1；
                i++;
            }
            if (sb.Length > 0) sb.Append(")");
            #endregion

            #region 获取 AND 集合
            foreach (QueryFieldInfo queryFieldInfo in queryField.QueryFieldInfos)
            {
                if (string.IsNullOrEmpty(queryFieldInfo.QueryString.Trim())) continue;
                try
                {
                    //设定默认连接方式
                    fs = "=";
                    //初始化
                    tsql = "";

                    //分割多匹配
                    String[] fgs = queryFieldInfo.QueryString.Split('|');

                    //多匹配循环计数清零；
                    j = 0;
                    //初始化
                    tsql = "";
                    foreach (String fg in fgs)
                    {
                        value = fg.Trim();
                        //为空退出本次循环
                        if (String.IsNullOrEmpty(value)) continue;
                        PN = ":p" + i + "_" + j;

                        //设定默认连接方式
                        fs = "=";

                        switch (queryFieldInfo.FieldType)
                        {
                            #region Guid,String
                            case CSharpType.Guid:
                            case CSharpType.String:
                                if (Regex.IsMatch(value, @"\*|\?"))
                                {
                                    fs = "LIKE";
                                    value = Regex.Replace(value, @"\*", "%");
                                    value = Regex.Replace(value, @"\?", "_");
                                }
                                if (Regex.IsMatch(value, @"^<>|^!="))
                                {
                                    if (fs == "LIKE") fs = "NOT LIKE";
                                    else fs = "<>";
                                    value = value.Substring(2);
                                }
                                //增加查询SQL合集
                                sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), value));
                                if (tsql.Length > 0) tsql += " OR ";
                                tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                break;
                            #endregion
                            #region Int16,Int32,Int64,Decimal,Double,Single,Byte
                            case CSharpType.Int16:
                            case CSharpType.Int32:
                            case CSharpType.Decimal:
                            case CSharpType.Double:
                            case CSharpType.Single:
                            case CSharpType.Byte:
                                if (Regex.IsMatch(value, @"^\d*[.]{0,1}\d*$"))
                                {
                                    //增加查询SQL合集
                                    try
                                    {
                                        object tv = GetValue(queryFieldInfo.FieldType, value);
                                        sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), tv));
                                        if (tsql.Length > 0) tsql += " OR ";
                                        tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                    }
                                    catch { }
                                }
                                else
                                {
                                    //开始字符有< <= <> != > >=
                                    if (Regex.IsMatch(value, @"^(<|<=|<>|!=|>|>=)\d*[.]{0,1}\d*$"))
                                    {
                                        if (Regex.IsMatch(value, @"^(<=|>=|<>|!=)"))
                                        {
                                            //开始字符有 <= <> !=  >=
                                            fs = value.Substring(0, 2);
                                            if (fs == "!=") fs = "<>";
                                            value = value.Substring(2);
                                        }
                                        else
                                        {
                                            //开始字符有< >
                                            fs = value.Substring(0, 1);
                                            value = value.Substring(1);
                                        }
                                        //增加查询SQL合集
                                        try
                                        {
                                            object tv = GetValue(queryFieldInfo.FieldType, value);
                                            sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), tv));
                                            if (tsql.Length > 0) tsql += " OR ";
                                            tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        if (Regex.IsMatch(value, @"^\d*[.]{0,1}\d*[~]{1}\d*[.]{0,1}\d*$"))
                                        {
                                            string[] temps = value.Split('~');
                                            if (temps.Length == 2 && !String.IsNullOrEmpty(temps[0].Trim()) && !String.IsNullOrEmpty(temps[1].Trim()))
                                            {
                                                try
                                                {
                                                    object tv1 = GetValue(queryFieldInfo.FieldType, temps[0]);
                                                    object tv2 = GetValue(queryFieldInfo.FieldType, temps[1]);
                                                    sp.Add(OracleHelper.NewOracleParameter(PN + "q", GetOracleDbType(queryFieldInfo.FieldType), tv1));
                                                    sp.Add(OracleHelper.NewOracleParameter(PN + "h", GetOracleDbType(queryFieldInfo.FieldType), tv2));
                                                    if (tsql.Length > 0) tsql += " OR ";
                                                    tsql += string.Format("({0} >= {1}q AND {0} <= {1}h)", queryFieldInfo.FieldName, PN);
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                                break;
                            #endregion
                            #region DateTime
                            case CSharpType.DateTime:
                                if (Regex.IsMatch(value, @"^(1|2)\d{3}-\d{1,2}-\d{1,2}[ 0-9:]*$"))
                                {
                                    //存数字增加查询SQL合集
                                    try
                                    {
                                        DateTime dt = DateTime.Parse(value);
                                        sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), dt));
                                        if (tsql.Length > 0) tsql += " OR ";
                                        tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                    }
                                    catch { }
                                }
                                else
                                {
                                    if (Regex.IsMatch(value, @"^(<|<=|<>|!=|>|>=)(1|2)\d{3}-\d{1,2}-\d{1,2}[ 0-9:]*$"))
                                    {
                                        if (Regex.IsMatch(value, @"^(<=|>=|<>|!=)"))
                                        {
                                            fs = value.Substring(0, 2);
                                            if (fs == "!=") fs = "<>";
                                            value = value.Substring(2);
                                        }
                                        else
                                        {
                                            fs = value.Substring(0, 1);
                                            value = value.Substring(1);
                                        }
                                        //增加查询SQL合集
                                        try
                                        {
                                            DateTime dt = DateTime.Parse(value);
                                            sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), dt));
                                            if (tsql.Length > 0) tsql += " OR ";
                                            tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        if (Regex.IsMatch(value, @"^(1|2)\d{3}-\d{1,2}-\d{1,2}[ 0-9:]*[~]{1}(1|2)\d{3}-\d{1,2}-\d{1,2}[ 0-9:]*$"))
                                        {
                                            string[] temps = value.Split('~');


                                            if (temps.Length == 2 && !String.IsNullOrEmpty(temps[0].Trim()) && !String.IsNullOrEmpty(temps[1].Trim()))
                                            {
                                                try
                                                {
                                                    DateTime dt01 = DateTime.Parse(temps[0]);
                                                    DateTime dt02 = DateTime.Parse(temps[1]);
                                                    sp.Add(OracleHelper.NewOracleParameter(PN + "q", GetOracleDbType(queryFieldInfo.FieldType), dt01));
                                                    sp.Add(OracleHelper.NewOracleParameter(PN + "h", GetOracleDbType(queryFieldInfo.FieldType), dt02));
                                                    if (tsql.Length > 0) tsql += " OR ";
                                                    tsql += string.Format("({0} >= {1}q AND {0} <= {1}h)", queryFieldInfo.FieldName, PN);
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                                break;
                            #endregion
                            #region  Boolean
                            case CSharpType.Boolean:
                                if (Regex.IsMatch(value, @"^<>|^!="))
                                {
                                    fs = "<>";
                                    value = value.Substring(2);
                                }
                                try
                                {
                                    bool bt = bool.Parse(value);
                                    sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), OracleHelper.OraBit(bt)));
                                    if (tsql.Length > 0) tsql += " OR ";
                                    tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                }
                                catch { }
                                break;
                            case CSharpType.Char:
                                if (Regex.IsMatch(value, @"^<>|^!="))
                                {
                                    fs = "<>";
                                    value = value.Substring(2);
                                }
                                if (value.Trim().Length == 1)
                                {
                                    sp.Add(OracleHelper.NewOracleParameter(PN, GetOracleDbType(queryFieldInfo.FieldType), GetValue(queryFieldInfo.FieldType, value)));
                                    if (tsql.Length > 0) tsql += " OR ";
                                    tsql += string.Format("{0} {1} {2}", queryFieldInfo.FieldName, fs, PN);
                                }
                                break;
                            #endregion
                        }

                        //多匹配循环计数加1；
                        j++;
                    }

                    //查询不为空添加到 查询SQL合集中
                    if (!String.IsNullOrEmpty(tsql))
                    {
                        if (sb.Length > 0) sb.Append(" AND ");
                        sb.AppendFormat("({0})", tsql);
                    }

                }
                catch { }
                //循环总计数加1；
                i++;
            }
            #endregion

            return new QueryInfo(sb.ToString(), sp);
        }

        public static object GetValue(CSharpType _CSharpType, string value)
        {
            switch (_CSharpType)
            {
                case CSharpType.String: return value;
                case CSharpType.Int16: return Int16.Parse(value);
                case CSharpType.Int32: return Int32.Parse(value);
                case CSharpType.Boolean: return OracleHelper.OraBit(bool.Parse(value));
                case CSharpType.DateTime: return DateTime.Parse(value);
                case CSharpType.Decimal: return Decimal.Parse(value);
                case CSharpType.Guid: return value;
                case CSharpType.Byte: return Byte.Parse(value);
                case CSharpType.Char: return value[0];
                case CSharpType.Double: return Double.Parse(value);
                case CSharpType.Single: return Single.Parse(value);
                default: return value;
            }
        }

        public static OracleType GetOracleDbType(CSharpType _CSharpType)
        {
            switch (_CSharpType)
            {
                case CSharpType.String: return OracleType.VarChar;
                case CSharpType.Int16: return OracleType.Int16;
                case CSharpType.Int32: return OracleType.Int32;
                case CSharpType.Boolean: return OracleType.Char;
                case CSharpType.DateTime: return OracleType.DateTime;
                case CSharpType.Decimal: return OracleType.Number;
                case CSharpType.Guid: return OracleType.VarChar;
                case CSharpType.Byte: return OracleType.Byte;
                case CSharpType.Char: return OracleType.Char;
                case CSharpType.Chars: return OracleType.Char;
                case CSharpType.Double: return OracleType.Double;
                case CSharpType.Single: return OracleType.Float;
                default: return OracleType.VarChar;
            }
        }

        /// <summary>
        /// 更具DataReader获取属性值
        /// </summary>
        /// <param name="dr"></param>
        public static T ReaderToInfo<T>(IDataReader dr) where T : new()
        {
            T info = new T();
            Type t = info.GetType();
            PropertyInfo[] pi = t.GetProperties();
            Dictionary<string, PropertyInfo> pid = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo item in pi)
            {
                pid.Add(item.Name.ToLower(), item);
            }
            int s = dr.FieldCount;
            for (int i = 0; i < s; i++)
            {
                object dd=dr[i];
                dd = dr.GetName(i);
                dd = dr.GetString(i);
                if (!(dr[i] is DBNull) && pid.ContainsKey(dr.GetName(i).ToLower()))
                {
                    string stype = pid[dr.GetName(i).ToLower()].PropertyType.FullName;
                    switch (stype)
                    {
                        case "System.Int32":
                            pid[dr.GetName(i).ToLower()].SetValue(info, Convert.ToInt32(dr[i]), null);
                            break;
                        case "System.Int64":
                            pid[dr.GetName(i).ToLower()].SetValue(info, Convert.ToInt64(dr[i]), null);
                            break;
                        case "System.Decimal":
                            pid[dr.GetName(i).ToLower()].SetValue(info, Convert.ToDecimal(dr[i]), null);
                            break;
                        case "System.String":
                            pid[dr.GetName(i).ToLower()].SetValue(info, Convert.ToString(dr[i]), null);
                            break;
                        case "System.Char":
                            pid[dr.GetName(i).ToLower()].SetValue(info, Convert.ToString(dr[i])[0], null);
                            break;
                        case "System.Boolean":
                            pid[dr.GetName(i).ToLower()].SetValue(info, Convert.ToString(dr[i]) == "Y", null);
                            break;
                        case "System.DateTime":
                            try
                            {
                                DateTime dt = DateTime.Parse(dr[i].ToString());
                                pid[dr.GetName(i).ToLower()].SetValue(info, dt, null);
                            }
                            catch { }
                            break;
                        default:
                            pid[dr.GetName(i).ToLower()].SetValue(info, Convert.ToString(dr[i]), null);
                            break;
                    }
                }
            }
            return info;
        }
    }
}
