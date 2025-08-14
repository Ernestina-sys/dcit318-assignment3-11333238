using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace InventoryManagementSystem
{
    // Marker interface for logging
    public interface IInventoryEntity
    {
        int Id { get; }
    }

    // Immutable Inventory Record using positional syntax
    public record InventoryItem(int Id, string Name, int Quantity, DateTime DateAdded) : IInventoryEntity;

    // Generic Inventory Logger
    public class InventoryLogger<T> where T : IInventoryEntity
    {
        private List<T> _log;
        private readonly string _filePath;

        public InventoryLogger(string filePath)
        {
            _log = new List<T>();
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        // Add item to log
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _log.Add(item);
            Console.WriteLine($"Added item with ID {item.Id} to inventory log.");
        }

        // Return all items in the log
        public List<T> GetAll()
        {
            return new List<T>(_log); // Return a copy to maintain immutability
        }

        // Serialize all items to a file using JSON
        public void SaveToFile()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string jsonString = JsonSerializer.Serialize(_log, options);

                using (var writer = new StreamWriter(_filePath))
                {
                    writer.Write(jsonString);
                }

                Console.WriteLine($"Successfully saved {_log.Count} items to {_filePath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied when saving to file: {ex.Message}");
                throw;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Directory not found: {ex.Message}");
                throw;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO error occurred while saving: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON serialization error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while saving: {ex.Message}");
                throw;
            }
        }

        // Deserialize items from file into the log collection
        public void LoadFromFile()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Console.WriteLine($"File {_filePath} does not exist. Starting with empty inventory.");
                    _log = new List<T>();
                    return;
                }

                using (var reader = new StreamReader(_filePath))
                {
                    string jsonString = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        Console.WriteLine("File is empty. Starting with empty inventory.");
                        _log = new List<T>();
                        return;
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    _log = JsonSerializer.Deserialize<List<T>>(jsonString, options) ?? new List<T>();
                    Console.WriteLine($"Successfully loaded {_log.Count} items from {_filePath}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied when reading file: {ex.Message}");
                throw;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {ex.Message}");
                _log = new List<T>();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO error occurred while loading: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON deserialization error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while loading: {ex.Message}");
                throw;
            }
        }

        // Clear the current log (for simulation purposes)
        public void ClearLog()
        {
            _log.Clear();
            Console.WriteLine("Memory log cleared.");
        }
    }

    // Integration Layer - InventoryApp
    public class InventoryApp
    {
        private readonly InventoryLogger<InventoryItem> _logger;

        public InventoryApp(string filePath = "inventory.json")
        {
            _logger = new InventoryLogger<InventoryItem>(filePath);
        }

        // Add sample data to the inventory
        public void SeedSampleData()
        {
            Console.WriteLine("\n=== Seeding Sample Data ===");

            var sampleItems = new List<InventoryItem>
            {
                new InventoryItem(1, "Wireless Mouse", 25, DateTime.Now.AddDays(-10)),
                new InventoryItem(2, "Mechanical Keyboard", 15, DateTime.Now.AddDays(-8)),
                new InventoryItem(3, "USB-C Cable", 50, DateTime.Now.AddDays(-5)),
                new InventoryItem(4, "Monitor Stand", 8, DateTime.Now.AddDays(-3)),
                new InventoryItem(5, "Webcam HD", 12, DateTime.Now.AddDays(-1))
            };

            foreach (var item in sampleItems)
            {
                _logger.Add(item);
            }

            Console.WriteLine($"Seeded {sampleItems.Count} sample items into inventory.");
        }

        // Save data to file
        public void SaveData()
        {
            Console.WriteLine("\n=== Saving Data to File ===");
            _logger.SaveToFile();
        }

        // Load data from file
        public void LoadData()
        {
            Console.WriteLine("\n=== Loading Data from File ===");
            _logger.LoadFromFile();
        }

        // Print all items from the loaded data
        public void PrintAllItems()
        {
            Console.WriteLine("\n=== Current Inventory Items ===");
            var items = _logger.GetAll();

            if (!items.Any())
            {
                Console.WriteLine("No items found in inventory.");
                return;
            }

            Console.WriteLine($"{"ID",-5} {"Name",-20} {"Quantity",-10} {"Date Added",-12}");
            Console.WriteLine(new string('-', 55));

            foreach (var item in items.OrderBy(x => x.Id))
            {
                Console.WriteLine($"{item.Id,-5} {item.Name,-20} {item.Quantity,-10} {item.DateAdded:yyyy-MM-dd}");
            }

            Console.WriteLine($"\nTotal items: {items.Count}");
            Console.WriteLine($"Total quantity: {items.Sum(x => x.Quantity)}");
        }

        // Clear memory log for simulation
        public void ClearMemory()
        {
            Console.WriteLine("\n=== Simulating New Session - Clearing Memory ===");
            _logger.ClearLog();
        }
    }

    // Main Application
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== Inventory Management System ===");
                Console.WriteLine("Demonstrating C# Records, Generics, and File Operations\n");

                // Create an instance of InventoryApp
                var inventoryApp = new InventoryApp("inventory_data.json");

                // Step 1: Seed sample data
                inventoryApp.SeedSampleData();

                // Step 2: Save data to persist to disk
                inventoryApp.SaveData();

                // Step 3: Clear memory and simulate a new session
                inventoryApp.ClearMemory();

                // Step 4: Load data from file
                inventoryApp.LoadData();

                // Step 5: Print all items to confirm data was recovered
                inventoryApp.PrintAllItems();

                Console.WriteLine("\n=== Demonstrating Immutable Record Properties ===");
                var originalItem = new InventoryItem(100, "Test Item", 5, DateTime.Now);
                Console.WriteLine($"Original: {originalItem}");

                // Demonstrating record with expression for creating modified copies
                var updatedItem = originalItem with { Quantity = 10, Name = "Updated Test Item" };
                Console.WriteLine($"Updated:  {updatedItem}");
                Console.WriteLine($"Original unchanged: {originalItem}");

                Console.WriteLine("\n=== Application completed successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nApplication error: {ex.Message}");
                Console.WriteLine("Please check the error details above and try again.");

                // In a real application, you might want to log this to a file
                Console.WriteLine($"\nFull exception details:\n{ex}");
            }
            finally
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
