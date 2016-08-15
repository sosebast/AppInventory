using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.DirectoryServices;
using System.Security.Principal;
using System.Data.Entity;
using System.Threading;
using System.Collections.Concurrent;
using EntityFramework.Extensions;

namespace PrimaryUserMachineSync
{
    public class SyncUser
    {

        AppInventoryEntities db = new AppInventoryEntities();

        public void UpdatePrimaryHost()
        {
            LogHelper.SetNewLogPath(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "SyncUserMachines");
            LogHelper.SetNewErrorLogPath(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "SyncUserMachinesErrors");

            string currentUser = Environment.UserName;

            db.UserMachines.Update(a => new UserMachine() { IsPrimaryMachine = false, ModUser = currentUser, ModDt = DateTime.Now });

            var logins = db.UserInfoes.Where(a => (a.Dept != null || a.UserOwnerships1.Count != 0) && a.UserMachines.Count() > 0).OrderBy(a => a.UserName);

            IDbConnection dbConn = this.db.Database.Connection;

            if (dbConn.State == ConnectionState.Closed)
                dbConn.Open();

            var partitoner = Partitioner.Create(logins, EnumerablePartitionerOptions.NoBuffering);
            Parallel.ForEach(partitoner, (user) =>
            {
                try
                {
                    string machineName = this.GetUserPrimaryMachine(user.UserName);


                    if (!string.IsNullOrEmpty(machineName) && machineName != "NA")
                    {

                        string cmdText = string.Format(@"UPDATE [UserMachine] SET [IsPrimaryMachine] = 1, [ModUser] = '{0}', [ModDt] =  GETDATE() 
FROM [dbo].[UserMachine] AS j0 INNER JOIN [dbo].[Machine] AS j1 ON j1.[MachineID] = j0.[MachineID]
WHERE j0.[UserID] = '{1}'  AND j1.[Name] = '{2}'", currentUser, user.UserID, machineName);


                        IDbCommand cmd = dbConn.CreateCommand();
                        cmd.CommandText = cmdText;

                        cmd.ExecuteNonQuery();
                        Console.WriteLine(string.Format("{0},{1}", user.UserName, machineName));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Failed for {0}{2}{1}", user.UserName, ex.ToString(), Environment.NewLine));
                }
            });

            if (dbConn.State == ConnectionState.Open)
                dbConn.Close();
        }



        public string GetUserPrimaryMachine(string loginID)
        {
            string hostName = "NA";
            DirectoryEntry searchRoot = new DirectoryEntry("LDAP://" + Environment.UserDomainName);

            SearchResult searchResult = new DirectorySearcher(searchRoot) { Filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", loginID) }.FindOne();

            if (searchResult != null)
            {
                DirectoryEntry userDirEntry = searchResult.GetDirectoryEntry();

                if (userDirEntry.Properties.Contains("extensionAttribute4") && userDirEntry.Properties.Contains("distinguishedName"))
                {
                    string computerOwned = userDirEntry.Properties["extensionAttribute4"].Value.ToString();
                    string distinguishedName = userDirEntry.Properties["distinguishedName"].Value.ToString();

                    string[] computers = computerOwned.Split(new char[] { ',' });

                    int lastLogOn = -1;
                    string tempHostName = "NA";

                    foreach (string computer in computers)
                    {
                        if (computer.Substring(3, 2) == "LT") continue;

                        bool primaryFound = false;
                        try
                        {
                            SearchResult compSearchResult = new DirectorySearcher(searchRoot) { Filter = string.Format("(&(objectClass=computer)(sAMAccountName={0}$))", computer) }.FindOne();
                            if (searchResult != null)
                            {
                                DirectoryEntry compDirEntry = compSearchResult.GetDirectoryEntry();
                                if (compDirEntry != null && compDirEntry.Properties.Contains("managedBy") && compDirEntry.Properties["managedBy"].Value != null)
                                {
                                    tempHostName = computer;
                                    string compOwner = compDirEntry.Properties["managedBy"].Value.ToString();
                                    if (compOwner.ToUpper() == distinguishedName.ToUpper())
                                    {
                                        string userAttribute = string.Empty;
                                        string[] array = "extensionAttribute4,extensionAttribute5,extensionAttribute6".Split(new char[] { ',' });
                                        for (int i = 0; i < array.Length; i++)
                                        {
                                            string sName = array[i];
                                            if (compDirEntry.Properties.Contains(sName) && compDirEntry.Properties[sName].Value != null)
                                                userAttribute += compDirEntry.Properties[sName].Value.ToString();
                                        }

                                        XmlDocument users = new XmlDocument();
                                        XmlNode userNodes = null;
                                        try
                                        {
                                            users.LoadXml(userAttribute);
                                            userNodes = users.SelectSingleNode("f");
                                        }
                                        catch (Exception)
                                        {
                                            userNodes = users.CreateNode(XmlNodeType.Element, "f", null);
                                            users.AppendChild(userNodes);
                                        }

                                        foreach (XmlNode userNode in userNodes.SelectNodes("m"))
                                        {
                                            string name = Convert.ToString(userNode.Attributes["usr"].Value);
                                            if (name.ToUpper() == loginID.ToUpper())
                                            {
                                                if (Convert.ToInt32(userNode.Attributes["on"].Value) > lastLogOn)
                                                    hostName = computer;

                                                if (Convert.ToInt32(userNode.Attributes["rdp"].Value) == 1)
                                                {
                                                    hostName = computer;
                                                    primaryFound = true;
                                                    break;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                                compDirEntry.Dispose();
                                if (primaryFound) break;
                            }
                        }
                        catch (Exception)
                        {
                            //Unable to check if primary.
                        }
                    }

                    //if (hostName == "NA" && tempHostName != "NA")
                    //    hostName = tempHostName;
                }
                userDirEntry.Dispose();
            }
            searchRoot.Dispose();
            return hostName;
        }


    }

}
