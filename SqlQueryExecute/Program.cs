using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlQueryExecute
{
    class Program
    {
        private static Regex databaseRegex = new Regex(@"USE \[([^\]]+)\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        static void Main(string[] args)
        {
            string sqlFilePath = args[0];
            string sqlServerName = args[1];
            string sqlUserName = args[2];
            string sqlPassword = args[3];

            if (!File.Exists(sqlFilePath))
                throw new Exception(string.Format("Sql file not exist {0}", sqlFilePath));

            using (StreamReader sr = new StreamReader(sqlFilePath))
            {
                string[] sqlQueries = sr.ReadToEnd().Split(new string[] { "GO\r\n" }, StringSplitOptions.None);
                SqlConnection connection = null;

                foreach (string queryText in sqlQueries)
                {
                    if (string.IsNullOrEmpty(queryText))
                        continue;

                    Match m = databaseRegex.Match(queryText);
                    if (m.Success)
                    {
                        if (connection != null)
                            connection.Close();

                        connection = new SqlConnection(GetConnectionString(sqlServerName, sqlUserName, sqlPassword, m.Groups[1].Value));
                        connection.Open();
                        continue;
                    }

                    SqlCommand command = new SqlCommand(queryText, connection);
                    command.ExecuteNonQuery();

                    Console.WriteLine(queryText);
                    Console.WriteLine("Done...");
                }

                if(connection != null)
                    connection.Close();
            }
        }

        static string GetConnectionString(string sqlServerName, string sqlUserName, string sqlPassword, string dataBaseName)
        {
            return string.Format("user id={0};password={1};Data Source={2};Database={3}", sqlUserName, sqlPassword,
                sqlServerName, dataBaseName);
        }
    }
}
