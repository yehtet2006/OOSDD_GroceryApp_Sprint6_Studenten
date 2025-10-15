// using Grocery.Core.Data.Helpers;
// using Grocery.Core.Interfaces.Repositories;
// using Grocery.Core.Models;
// using Microsoft.Data.Sqlite;
//
//
// namespace Grocery.Core.Data.Repositories
// {
//     public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
//     {
//         private readonly List<GroceryListItem> groceryListItems = new();
//
//         public GroceryListItemsRepository()
//         {
//             CreateTable(@"CREATE TABLE IF NOT EXISTS GroceryListItems (
//                             [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
//                             [NAME] NVARCHAR(80) UNIQUE NULL, 
//                             [GroceryListId] INTEGER NOT NULL,
//                             [ProductId] INTEGER NOT NULL,
//                             [Amount] INTEGER NOT NULL)");
//             List<string> insertQueries =
//             [
//                 @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(1, 1, 3)",
//                 @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(1, 2, 1)",
//                 @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(1, 3, 4)",
//                 @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(2, 1, 2)",
//                 @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(2, 2, 5)"
//             ];
//             InsertMultipleWithTransaction(insertQueries);
//             GetAll();
//         }
//
//         public List<GroceryListItem> GetAll()
//         {
//             groceryListItems.Clear(); // Clear the list before populating it again
//             string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItems"; // Adjusted to match the table schema
//             OpenConnection();
//             using (SqliteCommand command = new(selectQuery, Connection))
//             {
//                 SqliteDataReader reader = command.ExecuteReader(); // Execute the command and get a reader
//
//                 while (reader.Read())
//                 {
//                     int id = reader.GetInt32(0); // Read the Id from the first column
//                     int groceryListId = reader.GetInt32(1); // Read the GroceryListId from the second column
//                     int productId = reader.GetInt32(2); // Read the ProductId from the third column
//                     int amount = reader.GetInt32(3);
//                     groceryListItems.Add(new(id, groceryListId, productId, amount));
//                 }
//             }
//             CloseConnection();
//             return groceryListItems; 
//         }
//
//         public List<GroceryListItem> GetAllOnGroceryListId(int id)
//         {
//             return groceryListItems.Where(g => g.GroceryListId == id).ToList();
//         }
//
//         public GroceryListItem Add(GroceryListItem item)
//         {
//             int recordsAffected;
//             string insertQuery = $"INSERT INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(@GroceryListId, @ProductId, @Amount) Returning RowId;";
//             OpenConnection();
//             using (SqliteCommand command = new(insertQuery, Connection))
//             {
//                 command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
//                 command.Parameters.AddWithValue("@ProductId", item.ProductId);
//                 command.Parameters.AddWithValue("@Amount", item.Amount);
//                 
//                 //recordsAffected = command.ExecuteNonQuery();
//                 item.Id = Convert.ToInt32(command.ExecuteScalar());
//             }
//             CloseConnection();
//             return item;
//         }
//
//         public GroceryListItem? Delete(GroceryListItem item)
//         {
//             string deleteQuery = $"DELETE FROM GroceryListItems WHERE Id = {item.Id};";
//             OpenConnection();
//             Connection.ExecuteNonQuery(deleteQuery);
//             CloseConnection();
//             return item;
//         }
//
//         public GroceryListItem? Get(int id)
//         {
//             string selectQuery = $"Select Id, GroceryListId, ProductId, Amount FROM GroceryListItems WHERE Id = {id}";
//             GroceryListItem? gli = null;
//             OpenConnection();
//             using (SqliteCommand command = new(selectQuery, Connection))
//             {
//                 SqliteDataReader reader =  command.ExecuteReader();
//
//                 if (reader.Read())
//                 {
//                     int Id = reader.GetInt32(0);
//                     int GroceryListId = reader.GetInt32(1);
//                     int ProductId = reader.GetInt32(2);
//                     int Amount = reader.GetInt32(3);
//                     gli = new(Id, GroceryListId, ProductId, Amount);
//                 }
//             }
//             CloseConnection();
//             return gli;
//         }
//
//         public GroceryListItem? Update(GroceryListItem item)
//         {
//             int recordsAffected;
//             string updateQuery = $"UPDATE GroceryListItems SET GroceryListId = @GroceryList, ProductId = @ProductId, Amount = @Amount  WHERE Id = @Id;";
//             OpenConnection();
//             using (SqliteCommand command = new(updateQuery, Connection))
//             {
//                 command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
//                 command.Parameters.AddWithValue("@ProductId", item.ProductId);
//                 command.Parameters.AddWithValue("@Amount", item.Amount);
//                 command.Parameters.AddWithValue("@Id", item.Id);
//                 recordsAffected = command.ExecuteNonQuery();
//                 
//             }
//             CloseConnection();
//             return recordsAffected > 0 ? item : null;
//         }
//     }
// }
