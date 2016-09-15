using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENJ.FingerPrint.Core.Repository
{
    public class LocalCheckInOutViewRepository : IDisposable
    {
        // Local Server Name : ENJ-FS1\SQLEXPRESS
        private SqlConnection dbConn = new SqlConnection("Data Source=LOCALHOST; Initial Catalog=att2000; User Id=sa; Password=P@ssw0rd;");

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

        public void Dispose()
        {

        }
    }
}
