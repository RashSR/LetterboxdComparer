using LetterboxdComparer.Entities;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace LetterboxdComparer.Data
{
    internal class SQLiteHandler : ICRUDHandler
    {
        #region Constructor
        public SQLiteHandler(string connectionString)
        {
            SqliteConnection connection = new(connectionString);
            connection.Open();
            CreateTablesIfNotExist(connection);
        }

        private static void CreateTablesIfNotExist(SqliteConnection connection)
        {
            SqliteCommand createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS User (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_name TEXT NOT NULL,
                export_date TEXT
            );
            ";
            createTableCmd.ExecuteNonQuery();
        }

        #endregion

        public bool Create<T>(List<T> entities) where T : BaseEntity
        {
            throw new System.NotImplementedException();
        }

        public bool Delete<T>(List<T> entities)
        {
            throw new System.NotImplementedException();
        }

        public List<T> Read<T>() where T : BaseEntity
        {
            throw new System.NotImplementedException();
        }

        public bool Update<T>(List<T> entities)
        {
            throw new System.NotImplementedException();
        }
    }
}
