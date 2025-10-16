﻿using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        private readonly List<Product> _products = [];
        
        public ProductRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS Product (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [Name] NVARCHAR(80) UNIQUE NOT NULL,
                            [Stock] INTEGER NOT NULL,
                            [Shelflife] DATE NOT NULL,
                            [Price] DECIMAL(10,2) NOT NULL);");
            List<string> insertQueries = [
                @"INSERT OR IGNORE INTO Product(Name, Stock, Shelflife, Price) VALUES('Melk', 300, '2025-09-25', 0.95)",
                @"INSERT OR IGNORE INTO Product(Name, Stock, Shelflife, Price) VALUES('Kaas', 100, '2025-09-30', 7.98)",
                @"INSERT OR IGNORE INTO Product(Name, Stock, Shelflife, Price) VALUES('Brood', 400, '2025-09-12', 2.19)",
                @"INSERT OR IGNORE INTO Product(Name, Stock, Shelflife, Price) VALUES('Cornflakes', 0, '2025-12-31', 1.48)"
            ];
            InsertMultipleWithTransaction(insertQueries);
        }
        public List<Product> GetAll()
        {
            _products.Clear();
            string selectQuery = "SELECT Id, Name, Stock, date(Shelflife), Price FROM Product";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = DateOnly.FromDateTime(reader.GetDateTime(3));
                    decimal price = reader.GetDecimal(4);
                    _products.Add(new(id, name, stock, shelfLife, price));
                }
            }
            CloseConnection();
            return _products;
        }

        public Product? Get(int id)
        {
            // get product from database
            string selectQuery = "SELECT Id, Name, Stock, date(Shelflife), Price FROM Product WHERE Id = @Id";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                command.Parameters.AddWithValue("Id", id);
                SqliteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int productId = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = DateOnly.FromDateTime(reader.GetDateTime(3));
                    decimal price = reader.GetDecimal(4);
                    CloseConnection();
                    return new(productId, name, stock, shelfLife, price);
                }
            }
            CloseConnection();
            // Er is iets mis gegaan, dus een return null
            return null;
        }

        public Product Add(Product item)
        {
            // add product to database
            string insertQuery = $"INSERT INTO Product(Name, Stock, Shelflife, Price) VALUES(@Name, @Stock, @Shelflife, @Price) Returning RowId;";
            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("Name", item.Name);
                command.Parameters.AddWithValue("Stock", item.Stock);
                command.Parameters.AddWithValue("Shelflife", item.ShelfLife.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("Price", item.Price);
                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            CloseConnection();
            return item;
        }

        public Product? Delete(Product item)
        {
            // delete product from database
            string deleteQuery = $"DELETE FROM Product WHERE Id = @Id;";
            OpenConnection();
            using (SqliteCommand command = new(deleteQuery, Connection))
            {
                command.Parameters.AddWithValue("Id", item.Id);
                int rowsAffected = command.ExecuteNonQuery();
                CloseConnection();
                if (rowsAffected == 0)
                {
                    return null; // Item niet gevonden, dus return null
                }
                return item; // Return the deleted item
            }
        }

        public Product? Update(Product item)
        {
            // update product in database
            string updateQuery = $"UPDATE Product SET Name = @Name, Stock = @Stock. Shelflife = @Shelflife, Price = @Price WHERE Id = @Id;";
            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("Name", item.Name);
                command.Parameters.AddWithValue("Stock", item.Stock);
                command.Parameters.AddWithValue("Shelflife", item.ShelfLife.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("Price", item.Price);
                command.Parameters.AddWithValue("Id", item.Id);
                int rowsAffected = command.ExecuteNonQuery();
                CloseConnection();
                if (rowsAffected == 0)
                {
                    return null; // Item niet gevonden, dus return null
                }
                return item; // Return the updated item
            }
            // Product? product = _products.FirstOrDefault(p => p.Id == item.Id);
            // if (product == null) return null;
            // product.Id = item.Id;
            // return product;
        }
        
        public bool ProductExists(string name)
        {
            // Als er een product is met dezelfde naam, dan return true
            string selectQuery = $"SELECT COUNT(1) FROM Product WHERE Name = {name}";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                // command.Parameters.AddWithValue("Name", name);
                int count = Convert.ToInt32(command.ExecuteScalar());
                CloseConnection();
                return count > 0;
            }
        }
    }
}