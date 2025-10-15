// csharp
using Grocery.Core.Data.Helpers;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace Grocery.Core.Data
{
    public abstract class DatabaseConnection : IDisposable
    {
        protected SqliteConnection Connection { get; }
        private readonly string databaseName;

        public DatabaseConnection()
        {
            databaseName = ConnectionHelper.ConnectionStringValue("GroceryAppDb");
            var fileName = string.IsNullOrWhiteSpace(databaseName) ? "grocery.db" : Path.GetFileName(databaseName);

            // Create DB under Grocery.App output folder: <AppBase>\GroceryDb\grocery.db
            string appBase = AppContext.BaseDirectory;
            string dbDir = Path.Combine(appBase, "GroceryDb");
            Directory.CreateDirectory(dbDir);
            string dbPath = Path.Combine(dbDir, fileName);

            var csb = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };

            Connection = new SqliteConnection(csb.ConnectionString);

            OpenConnection();
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA busy_timeout=5000;";
                cmd.ExecuteNonQuery();
            }
            CloseConnection();
        }

        protected void OpenConnection()
        {
            if (Connection.State != System.Data.ConnectionState.Open) Connection.Open();
        }

        protected void CloseConnection()
        {
            if (Connection.State != System.Data.ConnectionState.Closed) Connection.Close();
        }

        public void CreateTable(string commandText)
        {
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }

        public void InsertMultipleWithTransaction(List<string> linesToInsert)
        {
            OpenConnection();
            var transaction = Connection.BeginTransaction();
            try
            {
                linesToInsert.ForEach(l => Connection.ExecuteNonQuery(l));
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                transaction.Rollback();
            }
            finally
            {
                transaction.Dispose();
            }
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}
