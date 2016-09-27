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
        // Local Server Name : ENJ-FP2\\SQLEXPRESS
        private SqlConnection dbConn = new SqlConnection("Data Source=ENJ-FP2\\SQLEXPRESS; Initial Catalog=att2000; User Id=gimsadmin; Password=EnjGA20120723;");
        private OleDbConnection localMDBConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\FSDB\\att2000.mdb;");
        private OdbcConnection localDSN = new OdbcConnection("DSN=ATT2000");
        private string toLocalDSN = "DSN=ATT2000";
        private string toRemoteDSN = "DSN=FPCENTRAL";
        private int localCount = 0;
        private int remoteCount = 0;

        public bool CheckLocalConnection()
        {
            bool checkConnection = false;

            try
            {
                dbConn.Open();
                dbConn.Close();
                localMDBConn.Open();
                localMDBConn.Close();
                localDSN.Open();
                localDSN.Close();
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

        public bool CompareMDBLocalToFPCENTRAL()
        {
            bool compare = false;
            OdbcCommand cmd;
            OdbcDataAdapter adapter;
            OdbcDataAdapter fpCentralAdapter;
            DataSet ds;
            DataSet dsFpCentral;

            try
            {
                using (OdbcConnection localCon = new OdbcConnection())
                {
                    localCon.ConnectionString = toLocalDSN;
                    localCon.Open();

                    int templateRemoteCount = 0;


                    //CHECKING COUNT MDB LOCAL SERVER MACHINE
                    adapter = new OdbcDataAdapter("SELECT * FROM TEMPLATE", localCon);
                    ds = new DataSet();  //TEMPLATE -> table name in att2000.mdb
                    adapter.Fill(ds, "TEMPLATE");
                    int templateLocalCount = ds.Tables[0].Rows.Count;
                    localCount = templateLocalCount;
                    //END CHECKING COUNT MDB LOCAL SERVER MACHINE


                    using (OdbcConnection remoteConn = new OdbcConnection())
                    {
                        remoteConn.ConnectionString = toRemoteDSN;
                        remoteConn.Open();

                        //CHECKING COUNT MDB REMOTE SERVER MACHINE
                        fpCentralAdapter = new OdbcDataAdapter("SELECT * FROM TEMPLATE", remoteConn);
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

        private bool InjectLocalTemplateData(List<NewDataTemplateViewObject> model)
        {
            bool result = false;

            OdbcCommand cmd;
            OdbcDataAdapter adapter;
            OdbcDataAdapter fpCentralAdapter;
            DataSet ds;
            DataSet dsFpCentral;

            if (model.Count > 0 || model != null)
            {

                if (localCount < remoteCount) // INSERT NEW DATA INTO LOCAL MDB ATT2000 Database
                {

                    foreach (var itemModel in model.OrderBy(o => o.TemplateId))
                    {
                        using (OdbcConnection insLocalConn = new OdbcConnection())
                        {

                            OdbcConnection remoteConn = new OdbcConnection();
                            remoteConn.ConnectionString = toRemoteDSN;
                            remoteConn.Open();
                            fpCentralAdapter = new OdbcDataAdapter("SELECT TEMPLATEID, USERID, FINGERID, TEMPLATE, USETYPE, Flag, DivisionFP, TEMPLATE4 FROM TEMPLATE WHERE USERID = " + itemModel.UserId + " AND FINGERID = " + itemModel.FingerId, remoteConn);
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


        public void Dispose()
        {

        }
    }
}
