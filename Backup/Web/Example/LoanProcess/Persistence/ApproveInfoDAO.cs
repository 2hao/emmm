using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OracleClient;
using System.Configuration;
using FireWorkflow.Net.Persistence.OracleDAL;

namespace WebDemo.Example.LoanProcess.Persistence
{
    public class ApproveInfoDAO
    {
        string connectionString = "User Id=ISS;Password=webiss;Data Source=ism";

        public ApproveInfoDAO()
        {
            connectionString = ConfigurationManager.ConnectionStrings["OracleServer"].ConnectionString;
        }

        public bool attachDirty(ApproveInfo instance)
        {
            if (string.IsNullOrEmpty(instance.Id))
            {
                instance.Id = Guid.NewGuid().ToString().Replace("-", "");
                string insert = "INSERT INTO T_BIZ_LOAN_APPROVEINFO (" +
                    "ID, SN, APPROVER, DECISION, DETAIL )VALUES(:1, :2, :3, :4, :5)";
                OracleParameter[] insertParms = { 
				    OracleHelper.NewOracleParameter(":1", OracleType.VarChar, 50, instance.Id), 
				    OracleHelper.NewOracleParameter(":2", OracleType.VarChar, 50, instance.Sn), 
				    OracleHelper.NewOracleParameter(":3", OracleType.VarChar, 50, instance.Approver), 
				    OracleHelper.NewOracleParameter(":4", OracleType.Int16, OracleHelper.OraBit(instance.Decision)), 
				    OracleHelper.NewOracleParameter(":5", OracleType.VarChar, 50, instance.Detail)
			    };
                if (OracleHelper.ExecuteNonQuery(connectionString, CommandType.Text, insert, insertParms) != 1)
                    return false;
                else return true;
            }
            else
            {
                string update = "UPDATE T_BIZ_LOAN_APPROVEINFO SET " +
                "SN=:2, APPROVER=:3, DECISION=:4, DETAIL=:5" +
                " WHERE ID=:1";
                OracleParameter[] updateParms = { 
				    OracleHelper.NewOracleParameter(":2", OracleType.VarChar, 50, instance.Sn), 
				    OracleHelper.NewOracleParameter(":3", OracleType.VarChar, 50, instance.Approver), 
				    OracleHelper.NewOracleParameter(":4", OracleType.Int16, OracleHelper.OraBit(instance.Decision)), 
				    OracleHelper.NewOracleParameter(":5", OracleType.VarChar, 50, instance.Detail),
				    OracleHelper.NewOracleParameter(":1", OracleType.VarChar, 50, instance.Id)
			    };
                if (OracleHelper.ExecuteNonQuery(connectionString, CommandType.Text, update, updateParms) != 1)
                    return false;
                else return true;
            }
        }
        public ApproveInfo findBySnAndUserId(String sn, String userId)
        {

            string select = "SELECT * FROM T_BIZ_LOAN_APPROVEINFO WHERE SN=:1 and APPROVER=:2";
            OracleConnection conn = new OracleConnection(connectionString);
            OracleDataReader reader = null;
            try
            {
                OracleParameter[] selectParms = { 
				    OracleHelper.NewOracleParameter(":1", OracleType.VarChar, 50, sn),
				    OracleHelper.NewOracleParameter(":2", OracleType.VarChar, 50, userId)
				};
                reader = OracleHelper.ExecuteReader(conn, CommandType.Text, select, selectParms);
                if (reader.Read())
                {
                    ApproveInfo _ApproveInfo = new ApproveInfo()
                    {
                        Id = Convert.ToString(reader["id"]),
                        Sn = Convert.ToString(reader["sn"]),
                        Approver = Convert.ToString(reader["approver"]),
                        Decision = Convert.ToInt32(reader["decision"]) == 1 ? true : false,//Convert.ToString(reader["decision"]),
                        Detail = Convert.ToString(reader["detail"])
                    };
                    return _ApproveInfo;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (reader != null) reader.Close();
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return null;
        }
	
    }
}
