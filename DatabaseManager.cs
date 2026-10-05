using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    internal class DatabaseManager
    {
        //static string connectionString = "Server=localhost;Port=3306;Database=memorija;User=root;Password=;";
        static string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                                        ?? "Server=localhost;Database=memorija;User=root;Password=;";

        /*static string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                                        ?? "Server=gateway01.eu-central-1.prod.aws.tidbcloud.com;Port=4000;Database=memorija;User=3iiK1tqhw22KdGr.root;Password=WRserT71CkD6rflb;SslMode=Required;";*/
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

    }
}
