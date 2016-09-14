using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENJ.FingerPrint.Core.Repository
{
    public class RemoteCheckInOutViewRepository : IDisposable
    {
        private SqlConnection dbConn = new SqlConnection("Data Source=115.85.80.83; Initial Catalog=ERDMERP; User Id=gimsadmin; Password=EnjGA20120723;");

        public void Dispose()
        {

        }
    }
}
