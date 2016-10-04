using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENJ.FingerPrint.Entity.ViewObject;

namespace ENJ.FingerPrint.Core.Repository
{
    public class LocalCheckInOutViewRepository : IDisposable
    {
        // Local Server Name : DEVELOPER-PC\
        private SqlConnection dbConn = new SqlConnection("Data Source=ENJ-FP4\\SQLEXPRESS; Initial Catalog=att2000; User Id=gimsadmin; Password=EnjGA20120723;");
        private OleDbConnection localMDBConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\FSDB\\att2000.mdb;");
        private OdbcConnection localDSN = new OdbcConnection("DSN=ATT2000");
        private string toLocalDSN = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\FSDB\\att2000.mdb;";
        private string toRemoteDSN = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\115.85.80.83\\EntryPassDBOnline\\FPCENTRAL\\att2000.mdb;";
        private int localCount = 0;
        private int localUserInfoCount = 0;
        private int localSqlUserInfo = 0;
        private int remoteCount = 0;
        private int remoteSqlUserInfo = 0;
        private int remoteUserInfoCount = 0;

        public bool CheckLocalConnection()
        {
            bool checkConnection = false;

            try
            {
                dbConn.Open();
                dbConn.Close();
                localMDBConn.Open();
                localMDBConn.Close();
                checkConnection = true;
            }
            catch (Exception)
            {
                checkConnection = false;
                throw;
            }

            return checkConnection;
        }

        public void UpdateLocalDataFingerPrint(LocalCheckInOutViewObject model )
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = " UPDATE [att2000].[dbo].[CHECKINOUT] SET Memoinfo = 'INJECT' WHERE USERID = '" + model.UserId + "' AND CHECKTIME = '" + model.CheckTime + "'";
            cmd.Connection = dbConn;

            dbConn.Open();
            cmd.ExecuteNonQuery();
            dbConn.Close();
        }

        public bool CompareUserInfoLocalToFPCENTRAL()
        {
            bool compare = false;
            OleDbCommand cmd;
            OleDbDataAdapter adapter;
            OleDbDataAdapter fpCentralAdapter;
            DataSet ds;
            DataSet dsFpCentral;

            try
            {
                using (OleDbConnection localCon = new OleDbConnection())
                {
                    localCon.ConnectionString = toLocalDSN;
                    localCon.Open();

                    int templateRemoteCount = 0;


                    //CHECKING COUNT MDB LOCAL SERVER MACHINE
                    adapter = new OleDbDataAdapter(" SELECT USERID ,Badgenumber ,Name ,DEFAULTDEPTID ,ATT ,INLATE ,OUTEARLY ,OVERTIME ,SEP " +
                        " , HOLIDAY, LUNCHDURATION, privilege, InheritDeptSch, InheritDeptSchClass, AutoSchPlan " +
                        " , MinAutoSchInterval, RegisterOT, InheritDeptRule, EMPRIVILEGE FROM USERINFO ", localCon);
                    ds = new DataSet();  //TEMPLATE -> table name in att2000.mdb
                    adapter.Fill(ds, "USERINFO");
                    int templateLocalCount = ds.Tables[0].Rows.Count;
                    localUserInfoCount = templateLocalCount;
                    //END CHECKING COUNT MDB LOCAL SERVER MACHINE


                    using (OleDbConnection remoteConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\115.85.80.83\\EntryPassDBOnline\\FPCENTRAL\\att2000.mdb;"))
                    {
                        //remoteConn.ConnectionString = toRemoteDSN;
                        remoteConn.Open();

                        //CHECKING COUNT MDB REMOTE SERVER MACHINE
                        fpCentralAdapter = new OleDbDataAdapter(" SELECT USERID ,Badgenumber ,Name ,DEFAULTDEPTID ,ATT ,INLATE ,OUTEARLY ,OVERTIME ,SEP " +
                        " , HOLIDAY, LUNCHDURATION, privilege, InheritDeptSch, InheritDeptSchClass, AutoSchPlan " +
                        " , MinAutoSchInterval, RegisterOT, InheritDeptRule, EMPRIVILEGE FROM USERINFO ", remoteConn);
                        dsFpCentral = new DataSet();  //TEMPLATE -> table name in att2000.mdb
                        fpCentralAdapter.Fill(dsFpCentral, "USERINFO");
                        templateRemoteCount = dsFpCentral.Tables[0].Rows.Count;
                        remoteUserInfoCount = templateRemoteCount;
                        //END CHECKING COUNT MDB REMOTE SERVER MACHINE                      
                    }


                    if (templateLocalCount > 0 && templateRemoteCount > 0)
                    {
                        //LOAD DATA TO MODEL//

                        List<LocalUserInfoTempViewObject> listLocalTemplateViewObjects = new List<LocalUserInfoTempViewObject>();
                        LocalUserInfoTempViewObject localTemplateViewObject = new LocalUserInfoTempViewObject();

                        for (int i = 0; i < templateLocalCount; i++)
                        {
                            localTemplateViewObject = new LocalUserInfoTempViewObject();
                            localTemplateViewObject.UserId = ds.Tables[0].Rows[i].ItemArray[0].ToString();
                            localTemplateViewObject.badgeNumber = ds.Tables[0].Rows[i].ItemArray[1].ToString();

                            listLocalTemplateViewObjects.Add(localTemplateViewObject);
                        }

                        List<RemoteUserInfoTempViewObject> listRemoteTemplateViewObjects =
                            new List<RemoteUserInfoTempViewObject>();
                        RemoteUserInfoTempViewObject remoteTemplateViewObject = new RemoteUserInfoTempViewObject();

                        for (int i = 0; i < templateRemoteCount; i++)
                        {
                            remoteTemplateViewObject = new RemoteUserInfoTempViewObject();
                            remoteTemplateViewObject.UserId = dsFpCentral.Tables[0].Rows[i].ItemArray[0].ToString();
                            remoteTemplateViewObject.badgeNumber = dsFpCentral.Tables[0].Rows[i].ItemArray[1].ToString();

                            listRemoteTemplateViewObjects.Add(remoteTemplateViewObject);
                        }

                        //END LOAD DATA TO MODEL//

                        List<NewUserInfoTempViewObject> listNewDataTemplateViewObjects = new List<NewUserInfoTempViewObject>();
                        NewUserInfoTempViewObject newDataTemplateViewObjects = new NewUserInfoTempViewObject();

                        foreach (var itemRemoteTemplate in listRemoteTemplateViewObjects.OrderBy(o => o.UserId))
                        {
                            var tempNewDataTemplate =
                                listLocalTemplateViewObjects.Where(w =>
                                        w.UserId == itemRemoteTemplate.UserId &&
                                        w.badgeNumber == itemRemoteTemplate.badgeNumber).ToList();
                            if (tempNewDataTemplate == null || tempNewDataTemplate.Count == 0)
                            {
                                newDataTemplateViewObjects = new NewUserInfoTempViewObject();
                                newDataTemplateViewObjects.UserId = itemRemoteTemplate.UserId;
                                newDataTemplateViewObjects.badgeNumber = itemRemoteTemplate.badgeNumber;

                                listNewDataTemplateViewObjects.Add(newDataTemplateViewObjects);
                            }

                        }

                        //INJECT NEW TEMPLATE DATA INTO LOCAL TEMPLATE DATA

                        if (listNewDataTemplateViewObjects.Count > 0 || listNewDataTemplateViewObjects != null)
                        {
                            try
                            {
                                bool injectDataMDB = InjectLocalUserInfoData(listNewDataTemplateViewObjects);

                                if (injectDataMDB)
                                {
                                    compare = true;
                                }

                            }
                            catch (Exception ex)
                            {
                                compare = false;
                            }
                        }

                        //END INJECT NEW TEMPLATE DATA INTO LOCAL TEMPLATE DATA
                    }
                    else if (templateLocalCount == 0 || templateRemoteCount == 0)
                    {
                        compare = false;
                    }
                }
            }
            catch (Exception ex)
            {
                compare = false;
            }

            return compare;
        }

        public bool CompareMDBLocalToFPCENTRAL()
        {
            bool compare = false;
            OleDbCommand cmd;
            OleDbDataAdapter adapter;
            OleDbDataAdapter fpCentralAdapter;
            DataSet ds;
            DataSet dsFpCentral;

            try
            {
                using (OleDbConnection localCon = new OleDbConnection())
                {
                    localCon.ConnectionString = toLocalDSN;
                    localCon.Open();

                    int templateRemoteCount = 0;


                    //CHECKING COUNT MDB LOCAL SERVER MACHINE
                    adapter = new OleDbDataAdapter("SELECT * FROM TEMPLATE", localCon);
                    ds = new DataSet();  //TEMPLATE -> table name in att2000.mdb
                    adapter.Fill(ds, "TEMPLATE");
                    int templateLocalCount = ds.Tables[0].Rows.Count;
                    localCount = templateLocalCount;
                    //END CHECKING COUNT MDB LOCAL SERVER MACHINE


                    using (OleDbConnection remoteConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\115.85.80.83\\EntryPassDBOnline\\FPCENTRAL\\att2000.mdb;"))
                    {
                        //remoteConn.ConnectionString = toRemoteDSN;
                        remoteConn.Open();

                        //CHECKING COUNT MDB REMOTE SERVER MACHINE
                        fpCentralAdapter = new OleDbDataAdapter("SELECT * FROM TEMPLATE", remoteConn);
                        dsFpCentral = new DataSet();  //TEMPLATE -> table name in att2000.mdb
                        fpCentralAdapter.Fill(dsFpCentral, "TEMPLATE");
                        templateRemoteCount = dsFpCentral.Tables[0].Rows.Count;
                        remoteCount = templateRemoteCount;
                        //END CHECKING COUNT MDB REMOTE SERVER MACHINE                      
                    }


                    if (templateLocalCount > 0 && templateRemoteCount > 0)
                    {
                        //LOAD DATA TO MODEL//

                        List<LocalTemplateViewObject> listLocalTemplateViewObjects = new List<LocalTemplateViewObject>();
                        LocalTemplateViewObject localTemplateViewObject = new LocalTemplateViewObject();

                        for (int i = 0; i < templateLocalCount; i++)
                        {
                            localTemplateViewObject = new LocalTemplateViewObject();
                            localTemplateViewObject.TemplateId = ds.Tables[0].Rows[i].ItemArray[0].ToString();
                            localTemplateViewObject.UserId = ds.Tables[0].Rows[i].ItemArray[1].ToString();
                            localTemplateViewObject.FingerId = ds.Tables[0].Rows[i].ItemArray[2].ToString();

                            listLocalTemplateViewObjects.Add(localTemplateViewObject);
                        }

                        List<RemoteTemplateViewObject> listRemoteTemplateViewObjects =
                            new List<RemoteTemplateViewObject>();
                        RemoteTemplateViewObject remoteTemplateViewObject = new RemoteTemplateViewObject();

                        for (int i = 0; i < templateRemoteCount; i++)
                        {
                            remoteTemplateViewObject = new RemoteTemplateViewObject();
                            remoteTemplateViewObject.TemplateId = dsFpCentral.Tables[0].Rows[i].ItemArray[0].ToString();
                            remoteTemplateViewObject.UserId = dsFpCentral.Tables[0].Rows[i].ItemArray[1].ToString();
                            remoteTemplateViewObject.FingerId = dsFpCentral.Tables[0].Rows[i].ItemArray[2].ToString();

                            listRemoteTemplateViewObjects.Add(remoteTemplateViewObject);
                        }

                        //END LOAD DATA TO MODEL//

                        List<NewDataTemplateViewObject> listNewDataTemplateViewObjects = new List<NewDataTemplateViewObject>();
                        NewDataTemplateViewObject newDataTemplateViewObjects = new NewDataTemplateViewObject();

                        foreach (var itemRemoteTemplate in listRemoteTemplateViewObjects.OrderBy(o => o.TemplateId))
                        {
                            var tempNewDataTemplate =
                                listLocalTemplateViewObjects.Where(w =>
                                        w.TemplateId == itemRemoteTemplate.TemplateId &&
                                        w.UserId == itemRemoteTemplate.UserId &&
                                        w.FingerId == itemRemoteTemplate.FingerId).ToList();
                            if (tempNewDataTemplate == null || tempNewDataTemplate.Count == 0)
                            {
                                newDataTemplateViewObjects = new NewDataTemplateViewObject();
                                newDataTemplateViewObjects.TemplateId = itemRemoteTemplate.TemplateId;
                                newDataTemplateViewObjects.UserId = itemRemoteTemplate.UserId;
                                newDataTemplateViewObjects.FingerId = itemRemoteTemplate.FingerId;

                                listNewDataTemplateViewObjects.Add(newDataTemplateViewObjects);
                            }

                        }

                        //INJECT NEW TEMPLATE DATA INTO LOCAL TEMPLATE DATA

                        if (listNewDataTemplateViewObjects.Count > 0 || listNewDataTemplateViewObjects != null)
                        {
                            try
                            {
                                bool injectDataMDB = InjectLocalTemplateData(listNewDataTemplateViewObjects);

                                if (injectDataMDB)
                                {
                                    compare = true;
                                }

                            }
                            catch (Exception ex)
                            {
                                compare = false;
                            }
                        }

                        //END INJECT NEW TEMPLATE DATA INTO LOCAL TEMPLATE DATA
                    }
                    else if (templateLocalCount == 0 || templateRemoteCount == 0)
                    {
                        compare = false;
                    }
                }
            }
            catch (Exception ex)
            {
                compare = false;
            }

            return compare;
        }

        private bool InjectLocalUserInfoData(List<NewUserInfoTempViewObject> model)
        {
            bool result = false;

            OleDbCommand cmd;
            OleDbDataAdapter adapter;
            OleDbDataAdapter fpCentralAdapter;
            DataSet ds;
            DataSet dsFpCentral;

            if (model.Count > 0 || model != null)
            {

                if (localUserInfoCount < remoteUserInfoCount) // INSERT NEW DATA INTO LOCAL MDB ATT2000 Database
                {

                    foreach (var itemModel in model.OrderBy(o => o.UserId))
                    {
                        using (OleDbConnection insLocalConn = new OleDbConnection())
                        {

                            OleDbConnection remoteConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\115.85.80.83\\EntryPassDBOnline\\FPCENTRAL\\att2000.mdb;");
                            //remoteConn.ConnectionString = toRemoteDSN;
                            remoteConn.Open();
                            fpCentralAdapter = new OleDbDataAdapter(" SELECT USERID ,Badgenumber ,Name ,DEFAULTDEPTID ,ATT ,INLATE ,OUTEARLY ,OVERTIME ,SEP " +
                                " , HOLIDAY, LUNCHDURATION, privilege, InheritDeptSch, InheritDeptSchClass, AutoSchPlan " +
                                " , MinAutoSchInterval, RegisterOT, InheritDeptRule, EMPRIVILEGE FROM USERINFO WHERE USERID = " + itemModel.UserId + " AND Badgenumber = '" + itemModel.badgeNumber + "'", remoteConn);
                            dsFpCentral = new DataSet();
                            fpCentralAdapter.Fill(dsFpCentral, "USERINFO");

                            int remoteCheck = dsFpCentral.Tables[0].Rows.Count;

                            if (remoteCheck > 0) // CHECKING DATA FOUND
                            {
                                try
                                {

                                    OleDbCommand cmdOleDb;
                                    OleDbCommand cmdAtt2000;
                                    OleDbDataAdapter da;
                                    DataTable dt = new DataTable();

                                    OleDbConnection conInsLocalConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\FSDB\\att2000.mdb;");

                                    conInsLocalConn.Open();
                                    cmdOleDb = new OleDbCommand("insert into USERINFO (USERID ,Badgenumber ,Name ,DEFAULTDEPTID ,ATT ,INLATE ,OUTEARLY ,OVERTIME ,SEP " +
                                       " , HOLIDAY, LUNCHDURATION, privilege, InheritDeptSch, InheritDeptSchClass, AutoSchPlan " +
                                       " , MinAutoSchInterval, RegisterOT, InheritDeptRule, EMPRIVILEGE)" +
                                       " values (@USERID ,@Badgenumber ,@Name ,@DEFAULTDEPTID ,@ATT ,@INLATE ,@OUTEARLY ,@OVERTIME ,@SEP " +
                                       " , @HOLIDAY, @LUNCHDURATION, @privilege, @InheritDeptSch, @InheritDeptSchClass, @AutoSchPlan " +
                                       " , @MinAutoSchInterval, @RegisterOT, @InheritDeptRule, @EMPRIVILEGE) ", conInsLocalConn);
                                    cmdOleDb.Parameters.AddWithValue("@USERID", dsFpCentral.Tables[0].Rows[0]["USERID"]);
                                    cmdOleDb.Parameters.AddWithValue("@Badgenumber", dsFpCentral.Tables[0].Rows[0]["Badgenumber"]);
                                    cmdOleDb.Parameters.AddWithValue("@Name", dsFpCentral.Tables[0].Rows[0]["Name"]);
                                    cmdOleDb.Parameters.AddWithValue("@DEFAULTDEPTID", dsFpCentral.Tables[0].Rows[0]["DEFAULTDEPTID"]);
                                    cmdOleDb.Parameters.AddWithValue("@ATT", dsFpCentral.Tables[0].Rows[0]["ATT"]);
                                    cmdOleDb.Parameters.AddWithValue("@INLATE", dsFpCentral.Tables[0].Rows[0]["INLATE"]);
                                    cmdOleDb.Parameters.AddWithValue("@OUTEARLY", dsFpCentral.Tables[0].Rows[0]["OUTEARLY"]);
                                    cmdOleDb.Parameters.AddWithValue("@OVERTIME", dsFpCentral.Tables[0].Rows[0]["OVERTIME"]);
                                    cmdOleDb.Parameters.AddWithValue("@SEP", dsFpCentral.Tables[0].Rows[0]["SEP"]);
                                    cmdOleDb.Parameters.AddWithValue("@HOLIDAY", dsFpCentral.Tables[0].Rows[0]["HOLIDAY"]);
                                    cmdOleDb.Parameters.AddWithValue("@LUNCHDURATION", dsFpCentral.Tables[0].Rows[0]["LUNCHDURATION"]);
                                    cmdOleDb.Parameters.AddWithValue("@privilege", dsFpCentral.Tables[0].Rows[0]["privilege"]);
                                    cmdOleDb.Parameters.AddWithValue("@InheritDeptSch", dsFpCentral.Tables[0].Rows[0]["InheritDeptSch"]);
                                    cmdOleDb.Parameters.AddWithValue("@InheritDeptSchClass", dsFpCentral.Tables[0].Rows[0]["InheritDeptSchClass"]);
                                    cmdOleDb.Parameters.AddWithValue("@AutoSchPlan", dsFpCentral.Tables[0].Rows[0]["AutoSchPlan"]);
                                    cmdOleDb.Parameters.AddWithValue("@MinAutoSchInterval", dsFpCentral.Tables[0].Rows[0]["MinAutoSchInterval"]);
                                    cmdOleDb.Parameters.AddWithValue("@RegisterOT", dsFpCentral.Tables[0].Rows[0]["RegisterOT"]);
                                    cmdOleDb.Parameters.AddWithValue("@InheritDeptRule", dsFpCentral.Tables[0].Rows[0]["InheritDeptRule"]);
                                    cmdOleDb.Parameters.AddWithValue("@EMPRIVILEGE", dsFpCentral.Tables[0].Rows[0]["EMPRIVILEGE"]);

                                    cmdOleDb.ExecuteNonQuery();
                                    conInsLocalConn.Close();

                                    result = true;
                                }
                                catch (Exception ex)
                                {
                                    result = false;
                                }
                            }
                        }

                    }

                }
            }

            return result;
        }

        private bool InjectLocalTemplateData(List<NewDataTemplateViewObject> model)
        {
            bool result = false;

            OleDbCommand cmd;
            OleDbDataAdapter adapter;
            OleDbDataAdapter fpCentralAdapter;
            DataSet ds;
            DataSet dsFpCentral;

            if (model.Count > 0 || model != null)
            {

                if (localCount < remoteCount) // INSERT NEW DATA INTO LOCAL MDB ATT2000 Database
                {

                    foreach (var itemModel in model.OrderBy(o => o.TemplateId))
                    {
                        using (OleDbConnection insLocalConn = new OleDbConnection())
                        {

                            OleDbConnection remoteConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\115.85.80.83\\EntryPassDBOnline\\FPCENTRAL\\att2000.mdb;");
                            //remoteConn.ConnectionString = toRemoteDSN;
                            remoteConn.Open();
                            fpCentralAdapter = new OleDbDataAdapter("SELECT TEMPLATEID, USERID, FINGERID, TEMPLATE, USETYPE, Flag, DivisionFP, TEMPLATE4 FROM TEMPLATE WHERE USERID = " + itemModel.UserId + " AND FINGERID = " + itemModel.FingerId, remoteConn);
                            dsFpCentral = new DataSet();
                            fpCentralAdapter.Fill(dsFpCentral, "TEMPLATE");

                            int remoteCheck = dsFpCentral.Tables[0].Rows.Count;

                            if (remoteCheck > 0) // CHECKING DATA FOUND
                            {
                                try
                                {

                                    OleDbCommand cmdOleDb;
                                    OleDbCommand cmdAtt2000;
                                    OleDbDataAdapter da;
                                    DataTable dt = new DataTable();

                                    OleDbConnection conInsLocalConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\FSDB\\att2000.mdb;");

                                    conInsLocalConn.Open();
                                    cmdOleDb = new OleDbCommand("insert into TEMPLATE (USERID, FINGERID, TEMPLATE, USETYPE, Flag, DivisionFP, TEMPLATE4)" +
                                                          " values (@USERID ,@FINGERID ,@TEMPLATE ,@USETYPE ,@Flag ,@DivisionFP ,@TEMPLATE4) ", conInsLocalConn);
                                    cmdOleDb.Parameters.AddWithValue("@USERID", dsFpCentral.Tables[0].Rows[0]["USERID"]);
                                    cmdOleDb.Parameters.AddWithValue("@FINGERID", dsFpCentral.Tables[0].Rows[0]["FINGERID"]);
                                    cmdOleDb.Parameters.AddWithValue("@TEMPLATE", dsFpCentral.Tables[0].Rows[0]["TEMPLATE"]);
                                    cmdOleDb.Parameters.AddWithValue("@USETYPE", dsFpCentral.Tables[0].Rows[0]["USETYPE"]);
                                    cmdOleDb.Parameters.AddWithValue("@Flag", dsFpCentral.Tables[0].Rows[0]["Flag"]);
                                    cmdOleDb.Parameters.AddWithValue("@DivisionFP", dsFpCentral.Tables[0].Rows[0]["DivisionFP"]);
                                    cmdOleDb.Parameters.AddWithValue("@TEMPLATE4", dsFpCentral.Tables[0].Rows[0]["TEMPLATE4"]);

                                    cmdOleDb.ExecuteNonQuery();
                                    conInsLocalConn.Close();

                                    result = true;
                                }
                                catch (Exception ex)
                                {
                                    result = false;
                                }
                            }
                        }

                    }

                } 
            }

            return result;

        }

        public bool InjectUserInfoToSQL()
        {
            bool result = false;

            try
            {
                OleDbCommand cmd;
                OleDbDataAdapter dataAdapter;
                SqlDataAdapter sqlUserInfo;
                DataSet ds;
                DataSet dsSqlUserInfo;

                List<LocalCheckInOutViewObject> listLocalCheckInOut = new List<LocalCheckInOutViewObject>();
                LocalCheckInOutViewObject localCheckInOut = new LocalCheckInOutViewObject();
                using (OleDbConnection localCon =
                    new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\FSDB\\att2000.mdb;"))
                {
                    localCon.Open();

                    //CHECKING COUNT MDB LOCAL SERVER MACHINE
                    dataAdapter =
                        new OleDbDataAdapter(
                            " SELECT USERID ,Badgenumber ,Name ,DEFAULTDEPTID ,ATT ,INLATE ,OUTEARLY ,OVERTIME ,SEP " +
                            " , HOLIDAY, LUNCHDURATION, privilege, InheritDeptSch, InheritDeptSchClass, AutoSchPlan " +
                            " , MinAutoSchInterval, RegisterOT, InheritDeptRule, EMPRIVILEGE FROM USERINFO", localCon);
                    ds = new DataSet(); //TEMPLATE -> table name in att2000.mdb
                    dataAdapter.Fill(ds, "USERINFO");
                    int templateLocalCount = ds.Tables[0].Rows.Count;
                    remoteSqlUserInfo = templateLocalCount;
                    //END CHECKING COUNT MDB LOCAL SERVER MACHINE

                    using (
                        SqlConnection localSqlConn =
                            new SqlConnection(
                                "Data Source=ENJ-FP4\\SQLEXPRESS; Initial Catalog=att2000; User Id=gimsadmin; Password=EnjGA20120723;"))
                    {
                        localSqlConn.Open();
                        sqlUserInfo = new SqlDataAdapter(" SELECT USERID ,Badgenumber FROM USERINFO ", localSqlConn);
                        dsSqlUserInfo = new DataSet();
                        sqlUserInfo.Fill(dsSqlUserInfo, "USERINFO");
                        int templatelocalSqlUserInfo = dsSqlUserInfo.Tables[0].Rows.Count;
                        localSqlUserInfo = templatelocalSqlUserInfo;
                    }

                    if (localSqlUserInfo < remoteSqlUserInfo)
                    {
                        if (remoteSqlUserInfo > 0 && localSqlUserInfo > 0)
                        {
                            //LOAD DATA TO MODEL//

                            List<RemoteUserInfoTempViewObject> listRemoteTemplateViewObjects =
                                new List<RemoteUserInfoTempViewObject>();
                            RemoteUserInfoTempViewObject remoteTemplateViewObject = new RemoteUserInfoTempViewObject();

                            for (int i = 0; i < remoteSqlUserInfo; i++)
                            {
                                remoteTemplateViewObject = new RemoteUserInfoTempViewObject();
                                remoteTemplateViewObject.UserId = ds.Tables[0].Rows[i].ItemArray[0].ToString();
                                remoteTemplateViewObject.badgeNumber = ds.Tables[0].Rows[i].ItemArray[1].ToString();

                                listRemoteTemplateViewObjects.Add(remoteTemplateViewObject);
                            }

                            List<LocalUserInfoTempViewObject> listLocalTemplateViewObjects =
                                new List<LocalUserInfoTempViewObject>();
                            LocalUserInfoTempViewObject localTemplateViewObject = new LocalUserInfoTempViewObject();

                            for (int i = 0; i < localSqlUserInfo; i++)
                            {
                                localTemplateViewObject = new LocalUserInfoTempViewObject();
                                localTemplateViewObject.UserId = dsSqlUserInfo.Tables[0].Rows[i].ItemArray[0].ToString();
                                localTemplateViewObject.badgeNumber =
                                    dsSqlUserInfo.Tables[0].Rows[i].ItemArray[1].ToString();

                                listLocalTemplateViewObjects.Add(localTemplateViewObject);
                            }

                            //END LOAD DATA TO MODEL//

                            List<NewUserInfoTempViewObject> listNewDataTemplateViewObjects =
                                new List<NewUserInfoTempViewObject>();
                            NewUserInfoTempViewObject newDataTemplateViewObjects = new NewUserInfoTempViewObject();

                            foreach (var itemRemoteTemplate in listRemoteTemplateViewObjects.OrderBy(o => o.UserId))
                            {
                                var tempNewDataTemplate =
                                    listLocalTemplateViewObjects.Where(w =>
                                        w.UserId == itemRemoteTemplate.UserId &&
                                        w.badgeNumber == itemRemoteTemplate.badgeNumber).ToList();
                                if (tempNewDataTemplate == null || tempNewDataTemplate.Count == 0)
                                {
                                    newDataTemplateViewObjects = new NewUserInfoTempViewObject();
                                    newDataTemplateViewObjects.UserId = itemRemoteTemplate.UserId;
                                    newDataTemplateViewObjects.badgeNumber = itemRemoteTemplate.badgeNumber;

                                    listNewDataTemplateViewObjects.Add(newDataTemplateViewObjects);
                                }

                            }

                            //INJECT NEW TEMPLATE DATA INTO LOCAL TEMPLATE DATA

                            if (listNewDataTemplateViewObjects.Count > 0 || listNewDataTemplateViewObjects != null)
                            {
                                LocalUserInfoTempViewObject localSQLInsert = new LocalUserInfoTempViewObject();

                                try
                                {
                                    foreach (var listItem in listNewDataTemplateViewObjects.OrderBy(o => o.UserId))
                                    {

                                        dataAdapter =
                                            new OleDbDataAdapter(
                                                " SELECT USERID ,Badgenumber ,Name ,DEFAULTDEPTID ,ATT ,INLATE ,OUTEARLY ,OVERTIME ,SEP " +
                                                " , HOLIDAY, LUNCHDURATION, privilege, InheritDeptSch, InheritDeptSchClass, AutoSchPlan " +
                                                " , MinAutoSchInterval, RegisterOT, InheritDeptRule, EMPRIVILEGE FROM USERINFO WHERE USERID = " + listItem.UserId + " AND Badgenumber = '" + listItem.badgeNumber + "'", localCon);
                                        ds = new DataSet(); //TEMPLATE -> table name in att2000.mdb
                                        dataAdapter.Fill(ds, "USERINFO");
                                        int checkUserInfoCount = ds.Tables[0].Rows.Count;

                                        if (checkUserInfoCount > 0)
                                        {
                                            try
                                            {
                                                SqlCommand injectUserInfoCmd = new SqlCommand();
                                                injectUserInfoCmd.CommandType = System.Data.CommandType.Text;
                                                injectUserInfoCmd.CommandText = "INSERT INTO USERINFO (USERID ,Badgenumber ,Name ,DEFAULTDEPTID ,ATT ,INLATE ,OUTEARLY ,OVERTIME ,SEP " +
                                               " , HOLIDAY, LUNCHDURATION, privilege, InheritDeptSch, InheritDeptSchClass, AutoSchPlan " +
                                               " , MinAutoSchInterval, RegisterOT, InheritDeptRule, EMPRIVILEGE)" +
                                               " VALUES (@USERID ,@Badgenumber ,@Name ,@DEFAULTDEPTID ,@ATT ,@INLATE ,@OUTEARLY ,@OVERTIME ,@SEP " +
                                               " , @HOLIDAY, @LUNCHDURATION, @privilege, @InheritDeptSch, @InheritDeptSchClass, @AutoSchPlan " +
                                               " , @MinAutoSchInterval, @RegisterOT, @InheritDeptRule, @EMPRIVILEGE) ";
                                                injectUserInfoCmd.Connection = dbConn;

                                                injectUserInfoCmd.Parameters.AddWithValue("@USERID", ds.Tables[0].Rows[0]["USERID"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@Badgenumber", ds.Tables[0].Rows[0]["Badgenumber"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@Name", ds.Tables[0].Rows[0]["Name"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@DEFAULTDEPTID", ds.Tables[0].Rows[0]["DEFAULTDEPTID"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@ATT", ds.Tables[0].Rows[0]["ATT"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@INLATE", ds.Tables[0].Rows[0]["INLATE"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@OUTEARLY", ds.Tables[0].Rows[0]["OUTEARLY"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@OVERTIME", ds.Tables[0].Rows[0]["OVERTIME"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@SEP", ds.Tables[0].Rows[0]["SEP"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@HOLIDAY", ds.Tables[0].Rows[0]["HOLIDAY"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@LUNCHDURATION", ds.Tables[0].Rows[0]["LUNCHDURATION"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@privilege", ds.Tables[0].Rows[0]["privilege"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@InheritDeptSch", ds.Tables[0].Rows[0]["InheritDeptSch"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@InheritDeptSchClass", ds.Tables[0].Rows[0]["InheritDeptSchClass"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@AutoSchPlan", ds.Tables[0].Rows[0]["AutoSchPlan"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@MinAutoSchInterval", ds.Tables[0].Rows[0]["MinAutoSchInterval"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@RegisterOT", ds.Tables[0].Rows[0]["RegisterOT"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@InheritDeptRule", ds.Tables[0].Rows[0]["InheritDeptRule"]);
                                                injectUserInfoCmd.Parameters.AddWithValue("@EMPRIVILEGE", ds.Tables[0].Rows[0]["EMPRIVILEGE"]);

                                                dbConn.Open();
                                                injectUserInfoCmd.ExecuteNonQuery();
                                                dbConn.Close();
                                                result = true;
                                            }
                                            catch (Exception ex)
                                            {
                                                result = false;
                                            }
                                        }                                        

                                        result = true;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    result = false;
                                }
                            }

                            //END INJECT NEW TEMPLATE DATA INTO LOCAL TEMPLATE DATA
                        }

                    }

                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool InjectCheckInOutToSQL()
        {
            bool result = false;
            DateTime dt = new DateTime();
            dt = DateTime.Now;

            string strCurrentDate = DateTime.Parse(dt.ToString()).ToString("MM/dd/yyyy");

            try
            {

                OleDbCommand cmd;
                OleDbDataAdapter dataAdapter;
                DataSet ds;

                List<LocalCheckInOutViewObject> listLocalCheckInOut = new List<LocalCheckInOutViewObject>();
                LocalCheckInOutViewObject localCheckInOut = new LocalCheckInOutViewObject();

                using (OleDbConnection localCon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\FSDB\\att2000.mdb;"))
                {                    
                    localCon.Open();

                    //CHECKING COUNT MDB LOCAL SERVER MACHINE
                    dataAdapter = new OleDbDataAdapter(
                        " SELECT T1.CHECKTIME ,T1.USERID" +
                        " ,T1.CHECKTYPE ,T1.VERIFYCODE ,T1.SENSORID " +
                        " ,T1.Memoinfo ,T1.WorkCode ,T1.sn ,T1.UserExtFmt" +
                        "  FROM CHECKINOUT T1 INNER JOIN USERINFO T2 ON T1.USERID = T2.USERID" +
                        " WHERE CHECKTIME BETWEEN #" + strCurrentDate + " 00:00:00# AND #" + strCurrentDate + " 23:59:00# " +
                        " AND  (Memoinfo IS NULL OR Memoinfo <> 'INJECTED')", localCon);
                    ds = new DataSet();  //TEMPLATE -> table name in att2000.mdb
                    dataAdapter.Fill(ds, "CHECKINOUT");
                    int templateLocalCount = ds.Tables[0].Rows.Count;
                    localCount = templateLocalCount;
                    //END CHECKING COUNT MDB LOCAL SERVER MACHINE

                    if (localCount > 0)
                    {
                        listLocalCheckInOut = new List<LocalCheckInOutViewObject>();

                        for (int i = 0; i < localCount; i++)
                        {
                            localCheckInOut = new LocalCheckInOutViewObject();
                            localCheckInOut.CheckTime = Convert.ToDateTime(ds.Tables[0].Rows[i]["CHECKTIME"].ToString());
                            localCheckInOut.UserId = Convert.ToInt32(ds.Tables[0].Rows[i]["USERID"]);
                            localCheckInOut.CheckType = ds.Tables[0].Rows[i]["CHECKTYPE"].ToString();
                            localCheckInOut.VerifyCode = Convert.ToInt32(ds.Tables[0].Rows[i]["VERIFYCODE"]);
                            localCheckInOut.SensorId = ds.Tables[0].Rows[i]["SENSORID"].ToString();
                            localCheckInOut.MemoInfo = ds.Tables[0].Rows[i]["Memoinfo"].ToString();
                            localCheckInOut.WorkCode = Convert.ToInt32(ds.Tables[0].Rows[0]["WorkCode"]);
                            localCheckInOut.Sn = ds.Tables[0].Rows[i]["sn"].ToString();
                            localCheckInOut.UserExtFmt = Convert.ToByte(ds.Tables[0].Rows[i]["UserExtFmt"]);

                            listLocalCheckInOut.Add(localCheckInOut);
                        }

                        if (listLocalCheckInOut.Count > 0 || listLocalCheckInOut != null)
                        {

                            LocalCheckInOutViewObject localSQLInsert = new LocalCheckInOutViewObject();

                            foreach (var listItem in listLocalCheckInOut.OrderBy(o => o.UserId))
                            {
                                localSQLInsert = new LocalCheckInOutViewObject();
                                localCheckInOut.CheckTime = listItem.CheckTime;
                                localCheckInOut.UserId = listItem.UserId;
                                localCheckInOut.CheckType = listItem.CheckType;
                                localCheckInOut.VerifyCode = listItem.VerifyCode;
                                localCheckInOut.SensorId = listItem.SensorId;
                                localCheckInOut.MemoInfo = listItem.MemoInfo;
                                localCheckInOut.WorkCode = listItem.WorkCode;
                                localCheckInOut.Sn = listItem.Sn;
                                localCheckInOut.UserExtFmt = listItem.UserExtFmt;

                                bool pullInToSQL = InsertTimeAttendanceToSQL(localCheckInOut);

                                if (pullInToSQL)
                                {
                                    OleDbCommand updateLocalMDB;
                                    using (
                                        OleDbConnection updateLocalMDBConn =
                                            new OleDbConnection(
                                                "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\FSDB\\att2000.mdb;"))
                                    {
                                        updateLocalMDBConn.Open();
                                        updateLocalMDB = new OleDbCommand(" UPDATE CHECKINOUT SET Memoinfo = 'INJECTED' " +
                                                                          " WHERE USERID = " + listItem.UserId + " AND CHECKTIME = #" + listItem.CheckTime + "# ", updateLocalMDBConn);

                                        updateLocalMDB.ExecuteNonQuery();
                                        updateLocalMDBConn.Close();
                                    }

                                }
                            }

                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        private bool InsertTimeAttendanceToSQL(LocalCheckInOutViewObject model)
        {
            bool result = false;

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = " INSERT INTO [att2000].[dbo].[CHECKINOUT](  " +
                                  " USERID,CHECKTIME,CHECKTYPE,VERIFYCODE,SENSORID,Memoinfo,WorkCode,sn,UserExtFmt) " +
                                  "  VALUES( " + model.UserId + " ,'" + model.CheckTime + "','" + model.CheckType
                                  + "', " + model.VerifyCode + " ,'" + model.SensorId + "',NULL,0,NULL,0 )";
                cmd.Connection = dbConn;

                dbConn.Open();
                cmd.ExecuteNonQuery();
                dbConn.Close();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }


        public void Dispose()
        {

        }
    }
}
