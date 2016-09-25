using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace SyncIntHome
{
    class Program
    {
        public struct Table 
        {
            public string name;
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
            Config config = new Config();
            try
            {
                string result = File.ReadAllText("config.json");
                config = JsonConvert.DeserializeObject<Config>(result);
                serverConnectionString = config.server;
                localConnectionString = config.local;
                for (int i = 0; i < config.tables.Length; i++)
                    Sync(config.tables[i].name, config.servertolocal);
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

        private static void Sync(string table,bool ServerToLocal)
        {
            List<string> list = new List<string>();
            string server = ServerToLocal ? serverConnectionString : localConnectionString;
            string local = ServerToLocal ? localConnectionString : serverConnectionString;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Getting " + table + " information...");
            try
            {
                SqlConnection con = new SqlConnection(server);
                SqlCommand cmd = new SqlCommand("select * from " + table + ";");
                cmd.CommandType = CommandType.Text;
                cmd.Connection = con;
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string temp = "insert into " + table + " values(";
                    for (int i = 0; i < reader.FieldCount; i++)
                        temp += reader.GetValue(i) + (i < reader.FieldCount - 1 ? "," : "");
                    temp += ");";
                    list.Add(temp);
                }
                con.Close();
                reader.Close();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occured [" + e.Message + "]");
                return;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Uploading " + table + " information...");
            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    SqlConnection con = new SqlConnection(local);
                    SqlCommand cmd = new SqlCommand(list[i]);
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteScalar();
                    con.Close();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error occured [" + e.Message + "]");
                    break;
                }
            }
        }
    }
}
