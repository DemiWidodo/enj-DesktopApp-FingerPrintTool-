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
    public class RemoteCheckInOutViewRepository : IDisposable
    {
        private SqlConnection dbConn = new SqlConnection("Data Source=115.85.80.83; Initial Catalog=att2000; User Id=gimsadmin; Password=EnjGA20120723;");
        private SqlConnection remoteDBConn = new SqlConnection("Data Source=115.85.80.83; Initial Catalog=att2000; User Id=gimsadmin; Password=EnjGA20120723;");
        private SqlConnection localDBConn = new SqlConnection("Data Source=ENJ-FP2\\SQLEXPRESS; Initial Catalog=att2000; User Id=gimsadmin; Password=EnjGA20120723;");
        OleDbConnection remoteMDBConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\115.85.80.83\\EntryPassDBOnline\\FPCENTRAL\\att2000.mdb;");
        OdbcConnection remoteDSN = new OdbcConnection("DSN=FPCENTRAl");

        private string toLocalDSN = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\FSDB\\att2000.mdb;";
        private string toRemoteDSN = "DSN=FPCENTRAL";
        private int localCount = 0;
        private int remoteCount = 0;

        private LocalCheckInOutViewRepository localCheckInOutViewRepository = new LocalCheckInOutViewRepository();

        public bool CheckRemoteConnection()
        {
            bool checkConnection = false;

            try
            {
                dbConn.Open();
                dbConn.Close();
                remoteMDBConn.Open();
                remoteMDBConn.Close();
                checkConnection = true;
            }
            catch (Exception)
            {
                checkConnection = false;
            }

            return checkConnection;
        }

        public bool ProceedInjectFingerPrintData()
        {
            bool result = false;            

            try
            {
                bool injectTimesheet = InjectDataTimesheetFP();
                if (injectTimesheet)
                {
                    result = true;
                } else if (!injectTimesheet)
                {
                    result = false;
                }
            }
            catch (Exception)
            {
                result = false;
            }


            return result;
        }


        private bool InjectDataTimesheetFP()
        {
            IFormatProvider dateCheckFormat = new DateCheckFormat();//format date from the checklog machine
            IFormatProvider timeCheckFormat = new TimeCheckFormat();

            RemoteCheckInOutViewObject remoteCheckInOut = new RemoteCheckInOutViewObject();
            LocalCheckInOutViewObject localCheckInOut = new LocalCheckInOutViewObject();

            bool result = false;
            DateTime dateCheckTime = new DateTime();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = " 	DECLARE  " +
                   "   @CheckCurrentYear integer, " +
                   "   @CheckCurrentMonth integer, " +
                   "   @CheckCurrentDay integer, " +
                   "   @CheckCurrentHour integer, " +
                   "   @CheckCurrentMinute integer, " +
                   "   @CheckCurrentSecond integer, " +
                   "   @STARTOFFICEHOUR DATETIME2, " +
                   "   @STARTCOMPAREDATE DATETIME2 , " +
                   "   @SYSCURRDATETIME DATETIME2 = GETDATE(); " +
                   "         SET @CheckCurrentYear = DATEPART(YEAR, GETDATE()); " +
                   "         SET @CheckCurrentMonth = DATEPART(MONTH, GETDATE()); " +
                   "         SET @CheckCurrentDay = DATEPART(DAY, GETDATE()); " +
                   "         SET @CheckCurrentHour = DATEPART(HOUR, GETDATE()); " +
                   "         SET @CheckCurrentMinute = DATEPART(MINUTE, GETDATE()); " +
                   "         SET @CheckCurrentSecond = DATEPART(SECOND, GETDATE()); " +
                   "         SET @STARTOFFICEHOUR = DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())); " +
                   "         SET @STARTCOMPAREDATE = DATEADD(MINUTE, -5, GETDATE()); " +
                   "         SELECT " +
                   "     T1.USERID ,T1.CHECKTIME ,T1.CHECKTYPE, " +
                   "     T1.VERIFYCODE ,T1.SENSORID ,T1.Memoinfo, " +
                   "     T1.WorkCode ,T1.sn ,T1.UserExtFmt ,T2.Badgenumber AS StaffNo , T2.Name " +
                   "     FROM CHECKINOUT T1 " +
                   "     INNER JOIN USERINFO T2 ON T1.USERID = T2.USERID " +
                   "     WHERE (T1.Memoinfo IS NULL OR T1.Memoinfo <> 'INJECT') AND T1.CHECKTIME BETWEEN @STARTCOMPAREDATE AND @SYSCURRDATETIME; ";
            cmd.Connection = localDBConn;

            localDBConn.Open();
            IDataReader rdr = cmd.ExecuteReader();

            if (rdr.IsClosed)
            {
                result = false;
            }
            else if (!rdr.IsClosed)
            {

                int index = 0;
                while (rdr.Read())
                {
                    var userId = rdr["USERID"];
                    var checkTime = rdr["CHECKTIME"];
                    var strStaffNo = rdr["StaffNo"];
                    dateCheckTime = Convert.ToDateTime(checkTime);

                    remoteCheckInOut = new RemoteCheckInOutViewObject();
                    localCheckInOut = new LocalCheckInOutViewObject();

                    remoteCheckInOut.CheckTime = dateCheckTime;
                    remoteCheckInOut.UserId = Convert.ToInt32(userId);
                    remoteCheckInOut.CheckType = "I";
                    remoteCheckInOut.SensorId = "1";
                    remoteCheckInOut.VerifyCode = 1;
                    remoteCheckInOut.MemoInfo = null;
                    remoteCheckInOut.WorkCode = 0;
                    remoteCheckInOut.UserExtFmt = 0;
                    remoteCheckInOut.Sn = null;
                    remoteCheckInOut.StaffNo = strStaffNo.ToString();
                    remoteCheckInOut.TrDate = DateTime.Parse(dateCheckTime.ToString()).ToString("yyyy-MM-dd");
                    remoteCheckInOut.TrTime = DateTime.Parse(dateCheckTime.ToString()).ToString("HH:mm:ss");
                    remoteCheckInOut.ServerIdentity = "M66";

                    InjectToRemoteTable(remoteCheckInOut);

                    localCheckInOut.CheckTime = dateCheckTime;
                    localCheckInOut.UserId = Convert.ToInt32(userId);
                    localCheckInOut.CheckType = "I";
                    localCheckInOut.SensorId = "1";
                    localCheckInOut.VerifyCode = 1;
                    localCheckInOut.MemoInfo = "INJECT";
                    localCheckInOut.WorkCode = 0;
                    localCheckInOut.UserExtFmt = 0;
                    localCheckInOut.Sn = null;
                    localCheckInOut.StaffNo = strStaffNo.ToString();
                    localCheckInOut.TrDate = DateTime.Parse(dateCheckTime.ToString()).ToString("yyyy-MM-dd");
                    localCheckInOut.TrTime = DateTime.Parse(dateCheckTime.ToString()).ToString("HH:mm:ss");

                    localCheckInOutViewRepository.UpdateLocalDataFingerPrint(localCheckInOut);

                    result = true;

                    index++;
                }

                localDBConn.Close();
            }

            return result;
        }

        private void InjectToRemoteTable(RemoteCheckInOutViewObject model)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = " INSERT INTO [att2000].[dbo].[CHECKINOUT](  " +
                              " USERID,CHECKTIME,CHECKTYPE,VERIFYCODE,SENSORID,Memoinfo,WorkCode,sn,UserExtFmt,STAFFNO,TRDATE,TRTIME,ServerIdentity) " +
                              "  VALUES( " + model.UserId + " ,'" + model.CheckTime + "','" + model.CheckType
                              + "', " + model.VerifyCode + " ,'" + model.SensorId + "',NULL,0,NULL,0,'" + model.StaffNo +
                              "','" + model.TrDate + "','" + model.TrTime + "','" + model.ServerIdentity +"')";
            cmd.Connection = remoteDBConn;

            remoteDBConn.Open();
            cmd.ExecuteNonQuery();
            remoteDBConn.Close();
        }

        public bool CompareFPCENTRALToMDLocal()
        {
            bool compare = false;
            OleDbCommand cmd;
            OleDbDataAdapter adapter;
            OleDbDataAdapter fpCentralAdapter;
            DataSet ds;
            DataSet dsFpCentral;

            try
            {
                using (OleDbConnection remoteCon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\115.85.80.83\\EntryPassDBOnline\\FPCENTRAL\\att2000.mdb;"))
                {
                    //remoteCon.ConnectionString = toRemoteDSN;
                    remoteCon.Open();

                    int templateLocalCount = 0;


                    //CHECKING COUNT MDB LOCAL SERVER MACHINE
                    adapter = new OleDbDataAdapter("SELECT * FROM TEMPLATE", remoteCon);
                    ds = new DataSet();  //TEMPLATE -> table name in att2000.mdb
                    adapter.Fill(ds, "TEMPLATE");
                    int templateRemoteCount = ds.Tables[0].Rows.Count;
                    remoteCount = templateRemoteCount;
                    //END CHECKING COUNT MDB LOCAL SERVER MACHINE


                    using (OleDbConnection localConn = new OleDbConnection())
                    {
                        localConn.ConnectionString = toLocalDSN;
                        localConn.Open();

                        //CHECKING COUNT MDB REMOTE SERVER MACHINE
                        fpCentralAdapter = new OleDbDataAdapter("SELECT * FROM TEMPLATE", localConn);
                        dsFpCentral = new DataSet();  //TEMPLATE -> table name in att2000.mdb
                        fpCentralAdapter.Fill(dsFpCentral, "TEMPLATE");
                        templateLocalCount = dsFpCentral.Tables[0].Rows.Count;
                        localCount = templateLocalCount;
                        //END CHECKING COUNT MDB REMOTE SERVER MACHINE                      
                    }


                    if (templateRemoteCount > 0 && templateLocalCount > 0)
                    {
                        //LOAD DATA TO MODEL//

                        List<RemoteTemplateViewObject> listRemoteTemplateViewObjects = new List<RemoteTemplateViewObject>();
                        RemoteTemplateViewObject remoteTemplateViewObject = new RemoteTemplateViewObject();

                        for (int i = 0; i < templateRemoteCount; i++)
                        {
                            remoteTemplateViewObject = new RemoteTemplateViewObject();
                            remoteTemplateViewObject.TemplateId = ds.Tables[0].Rows[i].ItemArray[0].ToString();
                            remoteTemplateViewObject.UserId = ds.Tables[0].Rows[i].ItemArray[1].ToString();
                            remoteTemplateViewObject.FingerId = ds.Tables[0].Rows[i].ItemArray[2].ToString();

                            listRemoteTemplateViewObjects.Add(remoteTemplateViewObject);
                        }

                        List<LocalTemplateViewObject> listLocalTemplateViewObjects =
                            new List<LocalTemplateViewObject>();
                        LocalTemplateViewObject localTemplateViewObject = new LocalTemplateViewObject();

                        for (int i = 0; i < templateLocalCount; i++)
                        {
                            localTemplateViewObject = new LocalTemplateViewObject();
                            localTemplateViewObject.TemplateId = dsFpCentral.Tables[0].Rows[i].ItemArray[0].ToString();
                            localTemplateViewObject.UserId = dsFpCentral.Tables[0].Rows[i].ItemArray[1].ToString();
                            localTemplateViewObject.FingerId = dsFpCentral.Tables[0].Rows[i].ItemArray[2].ToString();

                            listLocalTemplateViewObjects.Add(localTemplateViewObject);
                        }

                        //END LOAD DATA TO MODEL//

                        List<NewDataTemplateViewObject> listNewDataTemplateViewObjects = new List<NewDataTemplateViewObject>();
                        NewDataTemplateViewObject newDataTemplateViewObjects = new NewDataTemplateViewObject();

                        foreach (var itemLocalTemplate in listLocalTemplateViewObjects.OrderBy(o => o.TemplateId))
                        {
                            var tempNewDataTemplate =
                                listRemoteTemplateViewObjects.Where(w =>
                                        w.TemplateId == itemLocalTemplate.TemplateId &&
                                        w.UserId == itemLocalTemplate.UserId &&
                                        w.FingerId == itemLocalTemplate.FingerId).ToList();
                            if (tempNewDataTemplate == null || tempNewDataTemplate.Count == 0)
                            {
                                newDataTemplateViewObjects = new NewDataTemplateViewObject();
                                newDataTemplateViewObjects.TemplateId = itemLocalTemplate.TemplateId;
                                newDataTemplateViewObjects.UserId = itemLocalTemplate.UserId;
                                newDataTemplateViewObjects.FingerId = itemLocalTemplate.FingerId;

                                listNewDataTemplateViewObjects.Add(newDataTemplateViewObjects);
                            }

                        }

                        //INJECT NEW TEMPLATE DATA INTO LOCAL TEMPLATE DATA

                        if (listNewDataTemplateViewObjects.Count > 0 || listNewDataTemplateViewObjects != null)
                        {
                            try
                            {
                                bool injectDataMDB = InjectRemoteTemplateData(listNewDataTemplateViewObjects);

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

        private bool InjectRemoteTemplateData(List<NewDataTemplateViewObject> model)
        {
            bool result = false;

            OleDbCommand cmd;
            OleDbDataAdapter adapter;
            OleDbDataAdapter fpCentralAdapter;
            DataSet ds;
            DataSet dsFpCentral;

            if (model.Count > 0 || model != null)
            {

                if (localCount > remoteCount) // INSERT NEW DATA INTO LOCAL MDB ATT2000 Database
                {

                    foreach (var itemModel in model.OrderBy(o => o.TemplateId))
                    {
                        using (OleDbConnection insRemoteConn = new OleDbConnection())
                        {

                            OleDbConnection localConn = new OleDbConnection();
                            localConn.ConnectionString = toLocalDSN;
                            localConn.Open();
                            fpCentralAdapter = new OleDbDataAdapter("SELECT TEMPLATEID, USERID, FINGERID, TEMPLATE, USETYPE, Flag, DivisionFP, TEMPLATE4 FROM TEMPLATE WHERE USERID = " + itemModel.UserId + " AND FINGERID = " + itemModel.FingerId, localConn);
                            dsFpCentral = new DataSet();
                            fpCentralAdapter.Fill(dsFpCentral, "TEMPLATE");

                            int localCheck = dsFpCentral.Tables[0].Rows.Count;

                            if (localCheck > 0) // CHECKING DATA FOUND
                            {
                                try
                                {

                                    OleDbCommand cmdOleDb;
                                    OleDbCommand cmdAtt2000;
                                    OleDbDataAdapter da;
                                    DataTable dt = new DataTable();

                                    OleDbConnection conInsRemoteConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\115.85.80.83\\EntryPassDBOnline\\FPCENTRAL\\att2000.mdb;");

                                    conInsRemoteConn.Open();
                                    cmdOleDb = new OleDbCommand("insert into TEMPLATE (USERID, FINGERID, TEMPLATE, USETYPE, Flag, DivisionFP, TEMPLATE4)" +
                                                          " values (@USERID ,@FINGERID ,@TEMPLATE ,@USETYPE ,@Flag ,@DivisionFP ,@TEMPLATE4) ", conInsRemoteConn);
                                    cmdOleDb.Parameters.AddWithValue("@USERID", dsFpCentral.Tables[0].Rows[0]["USERID"]);
                                    cmdOleDb.Parameters.AddWithValue("@FINGERID", dsFpCentral.Tables[0].Rows[0]["FINGERID"]);
                                    cmdOleDb.Parameters.AddWithValue("@TEMPLATE", dsFpCentral.Tables[0].Rows[0]["TEMPLATE"]);
                                    cmdOleDb.Parameters.AddWithValue("@USETYPE", dsFpCentral.Tables[0].Rows[0]["USETYPE"]);
                                    cmdOleDb.Parameters.AddWithValue("@Flag", dsFpCentral.Tables[0].Rows[0]["Flag"]);
                                    cmdOleDb.Parameters.AddWithValue("@DivisionFP", dsFpCentral.Tables[0].Rows[0]["DivisionFP"]);
                                    cmdOleDb.Parameters.AddWithValue("@TEMPLATE4", dsFpCentral.Tables[0].Rows[0]["TEMPLATE4"]);

                                    cmdOleDb.ExecuteNonQuery();
                                    conInsRemoteConn.Close();

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

        #region Function Collection Library
        private string GetCurrentMonth(string strMonth)
        {
            string varMonth = string.Empty;
            switch (strMonth)
            {
                case "1":
                    varMonth = "01";
                    break;
                case "2":
                    varMonth = "02";
                    break;
                case "3":
                    varMonth = "03";
                    break;
                case "4":
                    varMonth = "04";
                    break;
                case "5":
                    varMonth = "05";
                    break;
                case "6":
                    varMonth = "06";
                    break;
                case "7":
                    varMonth = "07";
                    break;
                case "8":
                    varMonth = "08";
                    break;
                case "9":
                    varMonth = "09";
                    break;
                case "10":
                    varMonth = "10";
                    break;
                case "11":
                    varMonth = "11";
                    break;
                case "12":
                    varMonth = "12";
                    break;
            }
            return varMonth;
        }

        public class DateCheckFormat : IFormatProvider
        {
            public object GetFormat(Type formatType)
            {
                return "yyyy/MM/dd";
            }
        }
        public class TimeCheckFormat : IFormatProvider
        {
            public object GetFormat(Type formatType)
            {
                return "hh:mm:ss";
            }
        }

        #endregion

        public void Dispose()
        {

        }
    }
}
