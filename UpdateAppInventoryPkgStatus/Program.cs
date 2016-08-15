using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateAppInventoryPkgStatus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                SqlConnection dbCon = new SqlConnection(Properties.Settings.Default.Connection);

                try
                {
                    dbCon.Open();

                    SqlTransaction dbTran = dbCon.BeginTransaction();
                    try
                    {

                        string cmdText = string.Format("UPDATE AppCI SET PkgStatusID = (SELECT AppPkgStatusID FROM AppPkgStatus WHERE Status = '{0}') WHERE AppCIID = {1}"
                            , options.Status, options.AppCIID);

                        IDbCommand cmd = dbCon.CreateCommand();
                        cmd.CommandText = cmdText;
                        cmd.Transaction = dbTran;

                        cmd.ExecuteNonQuery();

                        cmd = dbCon.CreateCommand();

                        if (options.Status == "Production")
                            cmd.CommandText = string.Format("UPDATE AppCI SET StatusID = (SELECT AppStatusID FROM AppStatus WHERE Status = 'Complete') WHERE AppCIID = {0}"
                               , options.AppCIID);
                        else
                            cmd.CommandText = string.Format("UPDATE AppCI SET StatusID = (SELECT AppStatusID FROM AppStatus WHERE Status = 'In Progress') WHERE AppCIID = {0}"
                               , options.AppCIID);

                        cmd.Transaction = dbTran;
                        cmd.ExecuteNonQuery();

                        dbTran.Commit();

                        Console.WriteLine("Data updated sucessfully");
                    }
                    catch (Exception)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally{
                    if (dbCon.State == ConnectionState.Open)
                        dbCon.Close();
                }

            }
        }
    }
}
