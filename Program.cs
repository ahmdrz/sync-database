using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using Newtonsoft.Json; // Install-Package Newtonsoft.Json

// Namespace... 
namespace SyncDatabases
{
    class Program
    {
        public struct Query
        {
            public string column;
            public string query;
        }
        
        public struct Table
        {
            public string name;
            public string column;
        }

        public struct Config
        {
            public string server;
            public string local;
            public bool servertolocal;
            public Table[] tables;
        }

        private static string serverConnectionString;
        private static string localConnectionString;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Starting...");
            Console.WriteLine("Reading files...");
            // This try-catch statement will read configuration file and will call Sync method.
            Config config = new Config();
            try
            {
                string result = File.ReadAllText("config.json");
                config = JsonConvert.DeserializeObject<Config>(result);
                serverConnectionString = config.server;
                localConnectionString = config.local;
                // For each tables, According to column , Program will execute the query...
                for (int i = 0; i < config.tables.Length; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Syncing " + config.tables[i].name + " ...");
                    Sync(config.tables[i].name, config.tables[i].column, config.servertolocal);
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occured [" + e.Message + "]");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Finished. Press any key to exit");
            Console.Read();
        }

        // Honestly this is the main function.
        // This function will Sync two databases , Actually two tables if the column value isn't exists.
        private static void Sync(string table, string column, bool ServerToLocal)
        {
            List<Query> list = new List<Query>();
            string server = ServerToLocal ? serverConnectionString : localConnectionString;
            string local = ServerToLocal ? localConnectionString : serverConnectionString;
            Console.ForegroundColor = ConsoleColor.Yellow;
            // Let's get all of records from the table which we need to receive datas from.
            Console.WriteLine("Getting " + table + " information...");
            SqlConnection con = new SqlConnection();
            try
            {
                con = new SqlConnection(server);
                SqlCommand cmd = new SqlCommand("select * from " + table + ";");
                cmd.CommandType = CommandType.Text;
                cmd.Connection = con;
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Query query = new Query();
                    string temp = "insert into " + table + " values(";
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object value = reader.GetValue(i);
                        if (value.GetType() == typeof(bool))
                            value = value.Equals(true) ? 1 : 0;
                        else if (value.GetType() == typeof(string))
                            value = "'" + value + "'";
                        else if (value.GetType() == typeof(DateTime))
                            value = "CAST('" + DateTime.Parse(value.ToString()).ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) + "' AS DATETIME)";
                        temp += value + (i < reader.FieldCount - 1 ? "," : "");
                    }
                    temp += ");";
                    query.query = temp;
                    query.column = reader[column].ToString();
                    list.Add(query);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occured [" + e.Message + "]");
                return;
            }
            finally
            {
                con.Close();
            }

            // And then check the records for conflicts...
            Console.WriteLine("Checking " + table + " information...");
            try
            {
                con = new SqlConnection(local);
                SqlCommand cmd = new SqlCommand("select * from " + table + ";");
                cmd.CommandType = CommandType.Text;
                cmd.Connection = con;
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<int> shouldRemoved = new List<int>();
                while (reader.Read())
                {
                    for (int i = 0; i < list.Count; i++)
                        if (reader[column].ToString() == list[i].column)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Conflict at " + i + " where " + column + " = " + list[i].column);
                            shouldRemoved.Add(i);
                        }
                }
                reader.Close();

                // Logic Error !!!
                if (shouldRemoved.Count >= list.Count)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Too many conflicts");
                    return;
                }
                // Removing conflicts from list.
                for (int i = 0; i < shouldRemoved.Count; i++)
                    list.RemoveAt(shouldRemoved[i]);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occured [" + e.Message + "]");
                return;
            }
            finally
            {
                con.Close();
            }
            
            // And then let's execute queries.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Uploading " + table + " information...");
            for (int i = 0; i < list.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                int current = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, current);
                Console.Write("\r" + "[" + (i + 1) + "/" + list.Count + "] : " + list[i].query);
                try
                {
                    con = new SqlConnection(local);
                    SqlCommand cmd = new SqlCommand(list[i].query);
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteScalar();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error occured [" + e.Message + "]");
                    break;
                }
                finally
                {
                    con.Close();
                }
            }
            Console.WriteLine();
        }
    }
}
