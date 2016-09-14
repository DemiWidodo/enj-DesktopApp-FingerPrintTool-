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
        private SqlConnection dbConn = new SqlConnection("Data Source=LOCALHOST; Initial Catalog=att2000; User Id=gimsadmin; Password=EnjGA20120723;");

        public void Dispose()
        {

        }
    }
}
