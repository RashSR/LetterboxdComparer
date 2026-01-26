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

        #region Delete
        public bool Delete<T>(List<T> entities) where T : BaseEntity
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Read
        public List<T>? Read<T>() where T : BaseEntity
        {
            if(typeof(T) == typeof(LetterboxdUser))
                return ReadUsers<T>();

            throw new NotImplementedException();
        }

        private List<T>? ReadUsers<T>() where T : BaseEntity
        {
            var users = new List<LetterboxdUser>();

            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, user_name, export_date
                FROM User;
            ";

            SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string userName = reader.GetString(1);
                DateTime exportDate = reader.IsDBNull(2) ? DateTime.MinValue : DateTime.Parse(reader.GetString(2), null, System.Globalization.DateTimeStyles.RoundtripKind);
                LetterboxdUser user = new LetterboxdUser(userName, exportDate);
                user.Id = reader.GetInt32(0);
                users.Add(user);
            }

            return users.Cast<T>().ToList();
        }

        #endregion

        #region Update
        public bool Update<T>(List<T> entities) where T : BaseEntity
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
