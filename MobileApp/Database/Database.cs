using System.IO;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace MobileApp 
{
    /// <summary>
    /// Handles database related operations
    /// </summary>
    public static class Database
    {
        public static SqliteConnection connection = new SqliteConnection();
        /// <summary>
        /// Connects to the sqlite3 database
        /// </summary>
        /// <param name="connectionString">path to the sql file</param>
        /// <returns>SqliteConnection</returns>
        async public static Task<SqliteConnection> Connect(string connectionString)
        {
            try
            {
                FileStream stream = File.Open(connectionString, FileMode.Open, FileAccess.Read);
                connection.ConnectionString = $"Data Source={connectionString}";
                connection.Open();
                return connection;

            } catch (FileNotFoundException)
            {
                connection.ConnectionString = $"Data Source={connectionString}";
                connection.Open(); // Db file gets created here

                try
                {
                    string liteQuery = @"CREATE TABLE [Employee] (
                                      [fdebtorcode] VARCHAR(15),
                                      [limit] INTEGER
                                      )";

                    var command = new SqliteCommand(liteQuery, connection);
                    await command.ExecuteNonQueryAsync();

                    return connection;
                } catch (SqliteException err)
                {
                    string message = err.Message;
                    connection.Close();
                    Debug.WriteLine("Error in the sql query.");
                    Debug.WriteLine($"Error message:\n{message}");
                }

                // Microsoft.Data.Sqlite.SqliteException
                return null;

            }

        }

    }
}
