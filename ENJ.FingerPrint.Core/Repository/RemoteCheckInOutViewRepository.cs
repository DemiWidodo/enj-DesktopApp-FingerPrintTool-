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
        private SqlConnection localDBConn = new SqlConnection("Data Source=DEVELOPER-PC; Initial Catalog=att2000; User Id=sa; Password=P@ssw0rd;");
        OleDbConnection remoteMDBConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\115.85.80.83\\EntryPassDBOnline\\FPCENTRAL\\att2000.mdb;");
        OdbcConnection remoteDSN = new OdbcConnection("DSN=FPCENTRAl");

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
                remoteDSN.Open();
                remoteDSN.Close();
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
                    remoteCheckInOut.ServerIdentity = "M72";

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
