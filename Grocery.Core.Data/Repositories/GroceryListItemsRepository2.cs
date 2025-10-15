using System;
using System.Collections.Generic;
using System.Linq;
using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        private readonly List<GroceryListItem> groceryListItems = new();

        public GroceryListItemsRepository()
        {
            CreateTable(@"
                CREATE TABLE IF NOT EXISTS GroceryListItems (
                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [NAME] NVARCHAR(80) UNIQUE NOT NULL DEFAULT '',
                    [GroceryListId] INTEGER NOT NULL,
                    [ProductId] INTEGER NOT NULL,
                    [Amount] INTEGER NOT NULL
                )");

            // Seed with unique NAMEs to satisfy UNIQUE constraint and avoid OR IGNORE skips.
            List<string> insertQueries =
            [
                @"INSERT OR IGNORE INTO GroceryListItems(NAME, GroceryListId, ProductId, Amount) VALUES('seed-1', 1, 1, 3)",
                @"INSERT OR IGNORE INTO GroceryListItems(NAME, GroceryListId, ProductId, Amount) VALUES('seed-2', 1, 2, 1)",
                @"INSERT OR IGNORE INTO GroceryListItems(NAME, GroceryListId, ProductId, Amount) VALUES('seed-3', 1, 3, 4)",
                @"INSERT OR IGNORE INTO GroceryListItems(NAME, GroceryListId, ProductId, Amount) VALUES('seed-4', 2, 1, 2)",
                @"INSERT OR IGNORE INTO GroceryListItems(NAME, GroceryListId, ProductId, Amount) VALUES('seed-5', 2, 2, 5)"
            ];
            InsertMultipleWithTransaction(insertQueries);

            GetAll();
        }

        public List<GroceryListItem> GetAll()
        {
            groceryListItems.Clear();
            const string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItems";
            OpenConnection();
            try
            {
                using (var command = new SqliteCommand(selectQuery, Connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int groceryListId = reader.GetInt32(1);
                        int productId = reader.GetInt32(2);
                        int amount = reader.GetInt32(3);
                        groceryListItems.Add(new GroceryListItem(id, groceryListId, productId, amount));
                    }
                }
            }
            finally
            {
                CloseConnection();
            }
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int id)
        {
            return groceryListItems.Where(g => g.GroceryListId == id).ToList();
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            // Provide a unique NAME to satisfy UNIQUE NOT NULL constraint.
            const string insertQuery =
                "INSERT INTO GroceryListItems(NAME, GroceryListId, ProductId, Amount) VALUES(@Name, @GroceryListId, @ProductId, @Amount) RETURNING Id;";
            OpenConnection();
            try
            {
                using (var command = new SqliteCommand(insertQuery, Connection))
                {
                    command.Parameters.AddWithValue("@Name", "gli-" + Guid.NewGuid().ToString("N"));
                    command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                    command.Parameters.AddWithValue("@ProductId", item.ProductId);
                    command.Parameters.AddWithValue("@Amount", item.Amount);

                    item.Id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            finally
            {
                CloseConnection();
            }
            return item;
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            const string deleteQuery = "DELETE FROM GroceryListItems WHERE Id = @Id;";
            OpenConnection();
            try
            {
                using (var command = new SqliteCommand(deleteQuery, Connection))
                {
                    command.Parameters.AddWithValue("@Id", item.Id);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection();
            }
            return item;
        }

        public GroceryListItem? Get(int id)
        {
            const string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItems WHERE Id = @Id;";
            GroceryListItem? gli = null;
            OpenConnection();
            try
            {
                using (var command = new SqliteCommand(selectQuery, Connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            gli = new GroceryListItem(
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                reader.GetInt32(2),
                                reader.GetInt32(3));
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }
            return gli;
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            int recordsAffected;
            const string updateQuery =
                "UPDATE GroceryListItems SET GroceryListId = @GroceryListId, ProductId = @ProductId, Amount = @Amount WHERE Id = @Id;";
            OpenConnection();
            try
            {
                using (var command = new SqliteCommand(updateQuery, Connection))
                {
                    command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                    command.Parameters.AddWithValue("@ProductId", item.ProductId);
                    command.Parameters.AddWithValue("@Amount", item.Amount);
                    command.Parameters.AddWithValue("@Id", item.Id);
                    recordsAffected = command.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection();
            }
            return recordsAffected > 0 ? item : null;
        }
    }
}
