using LetterboxdComparer.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LetterboxdComparer.Data
{
    internal class SQLiteHandler : ICRUDHandler
    {
        #region Constructor
        private readonly SqliteConnection _connection;

        public SQLiteHandler(string connectionString)
        {
            _connection = new(connectionString);
            _connection.Open();
            CreateTablesIfNotExist();
        }

        private void CreateTablesIfNotExist()
        {
            SqliteCommand createTableCmd = _connection.CreateCommand();
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

        #region Create
        public List<T>? Create<T>(List<T> entities) where T : BaseEntity
        {
            //Only entities without duplicate entries in DB are here
            if(entities is List<LetterboxdUser>)
            {
                List<T>? insertedUsers = BulkInsertUsers(entities);
                return insertedUsers;
            }

            throw new NotImplementedException();
        }

        private List<T>? BulkInsertUsers<T>(List<T> entities) where T : BaseEntity
        {
            if(entities is not List<LetterboxdUser> usersToCreate)
                return null;

            SqliteTransaction transaction = _connection.BeginTransaction();
            SqliteCommand insertCmd = _connection.CreateCommand();
            insertCmd.Transaction = transaction;
            insertCmd.CommandText = @"
            INSERT INTO User (user_name, export_date)
            VALUES ($username, $exportDate);
            ";

            foreach(LetterboxdUser user in usersToCreate)
            {
                insertCmd.Parameters.Clear();
                insertCmd.Parameters.AddWithValue("$username", user.UserName);
                insertCmd.Parameters.AddWithValue("$exportDate", user.ExportDate.ToString("O"));
                insertCmd.ExecuteNonQuery();

                SqliteCommand idCmd = _connection.CreateCommand();
                idCmd.CommandText = "SELECT last_insert_rowid();";
                user.Id = (int)(long)idCmd.ExecuteScalar()!;
            }

            transaction.Commit();
            List<T> results = [.. usersToCreate.Cast<T>()];
            return results;
        }
        #endregion



        public bool Delete<T>(List<T> entities) where T : BaseEntity
        {
            throw new NotImplementedException();
        }

        public List<T> Read<T>() where T : BaseEntity
        {
            throw new NotImplementedException();
        }

        public bool Update<T>(List<T> entities) where T : BaseEntity
        {
            throw new NotImplementedException();
        }
    }
}
