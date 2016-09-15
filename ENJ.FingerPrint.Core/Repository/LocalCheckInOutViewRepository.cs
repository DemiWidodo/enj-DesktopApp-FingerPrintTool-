using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENJ.FingerPrint.Entity.ViewObject;

namespace ENJ.FingerPrint.Core.Repository
{
    public class LocalCheckInOutViewRepository : IDisposable
    {
        // Local Server Name : ENJ-FS1\SQLEXPRESS
        private SqlConnection dbConn = new SqlConnection("Data Source=ENJ-FS1\\SQLEXPRESS; Initial Catalog=att2000; User Id=gimsadmin; Password=EnjGA20120723;");

        public bool CheckLocalConnection()
        {
            bool checkConnection = false;

            try
            {
                dbConn.Open();
                dbConn.Close();
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

        public void Dispose()
        {

        }
    }
}
