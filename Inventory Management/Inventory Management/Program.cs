using System;
using System.Collections.Generic;
using System.Linq;

// Marker Interface for Inventory Items
public interface IInventoryItem
{
    int Id { get; }
    string Name { get; }
    int Quantity { get; set; }
}

// Custom Exceptions
public class DuplicateItemException : Exception
{
    public DuplicateItemException(string message) : base(message) { }
}

public class ItemNotFoundException : Exception
{
    public ItemNotFoundException(string message) : base(message) { }
}

public class InvalidQuantityException : Exception
{
    public InvalidQuantityException(string message) : base(message) { }
}

// Electronic Item Class
public class ElectronicItem : IInventoryItem
{
    public int Id { get; }
    public string Name { get; }
    public int Quantity { get; set; }
    public string Brand { get; }
    public int WarrantyMonths { get; }

    public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
    {
        Id = id;
        Name = name;
        Quantity = quantity;
        Brand = brand;
        WarrantyMonths = warrantyMonths;
    }

    public override string ToString()
    {
        return $"Electronic [ID: {Id}, Name: {Name}, Qty: {Quantity}, Brand: {Brand}, Warranty: {WarrantyMonths} months]";
    }
}

// Grocery Item Class
public class GroceryItem : IInventoryItem
{
    public int Id { get; }
    public string Name { get; }
    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; }

    public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
    {
        Id = id;
        Name = name;
        Quantity = quantity;
        ExpiryDate = expiryDate;
    }

    public override string ToString()
    {
        return $"Grocery [ID: {Id}, Name: {Name}, Qty: {Quantity}, Expires: {ExpiryDate:yyyy-MM-dd}]";
    }
}

// Generic Inventory Repository
public class InventoryRepository<T> where T : IInventoryItem
{
    private Dictionary<int, T> items = new Dictionary<int, T>();

    public void AddItem(T item)
    {
        if (items.ContainsKey(item.Id))
        {
            throw new DuplicateItemException($"Item with ID {item.Id} already exists in the inventory.");
        }
        items[item.Id] = item;
    }

    public T GetItemById(int id)
    {
        if (!items.ContainsKey(id))
        {
            throw new ItemNotFoundException($"Item with ID {id} was not found in the inventory.");
        }
        return items[id];
    }

    public void RemoveItem(int id)
    {
        if (!items.ContainsKey(id))
        {
            throw new ItemNotFoundException($"Cannot remove item with ID {id} - item not found in inventory.");
        }
        items.Remove(id);
    }

    public List<T> GetAllItems()
    {
        return items.Values.ToList();
    }

    public void UpdateQuantity(int id, int newQuantity)
    {
        if (newQuantity < 0)
        {
            throw new InvalidQuantityException($"Quantity cannot be negative. Provided quantity: {newQuantity}");
        }

        if (!items.ContainsKey(id))
        {
            throw new ItemNotFoundException($"Cannot update quantity for item with ID {id} - item not found.");
        }

        items[id].Quantity = newQuantity;
    }

    public int Count => items.Count;
}

// Warehouse Manager Class
public class WareHouseManager
{
    private InventoryRepository<ElectronicItem> electronics = new InventoryRepository<ElectronicItem>();
    private InventoryRepository<GroceryItem> groceries = new InventoryRepository<GroceryItem>();

    public void SeedData()
    {
        Console.WriteLine("=== Seeding Inventory Data ===");

        try
        {
            // Add Electronic Items
            electronics.AddItem(new ElectronicItem(1001, "iPhone 15", 25, "Apple", 12));
            electronics.AddItem(new ElectronicItem(1002, "Samsung Galaxy S24", 30, "Samsung", 24));
            electronics.AddItem(new ElectronicItem(1003, "MacBook Pro", 15, "Apple", 12));

            // Add Grocery Items
            groceries.AddItem(new GroceryItem(2001, "Organic Milk", 50, DateTime.Now.AddDays(7)));
            groceries.AddItem(new GroceryItem(2002, "Whole Wheat Bread", 75, DateTime.Now.AddDays(5)));
            groceries.AddItem(new GroceryItem(2003, "Fresh Apples", 100, DateTime.Now.AddDays(14)));

            Console.WriteLine($"Successfully added {electronics.Count} electronic items and {groceries.Count} grocery items.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during data seeding: {ex.Message}");
        }

        Console.WriteLine();
    }

    public void PrintAllItems<T>(InventoryRepository<T> repo, string categoryName) where T : IInventoryItem
    {
        Console.WriteLine($"=== All {categoryName} Items ===");

        try
        {
            var items = repo.GetAllItems();

            if (items.Count == 0)
            {
                Console.WriteLine($"No {categoryName.ToLower()} items found in inventory.");
                return;
            }

            foreach (var item in items.OrderBy(i => i.Id))
            {
                Console.WriteLine($"  {item}");
            }

            Console.WriteLine($"Total {categoryName} Items: {items.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving {categoryName.ToLower()} items: {ex.Message}");
        }

        Console.WriteLine();
    }

    public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
    {
        try
        {
            var item = repo.GetItemById(id);
            int newQuantity = item.Quantity + quantity;
            repo.UpdateQuantity(id, newQuantity);
            Console.WriteLine($"✓ Successfully increased stock for '{item.Name}' by {quantity}. New quantity: {newQuantity}");
        }
        catch (ItemNotFoundException ex)
        {
            Console.WriteLine($"✗ Stock increase failed: {ex.Message}");
        }
        catch (InvalidQuantityException ex)
        {
            Console.WriteLine($"✗ Stock increase failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Unexpected error during stock increase: {ex.Message}");
        }
    }

    public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
    {
        try
        {
            var item = repo.GetItemById(id); // Get item details before removal
            string itemName = item.Name;
            repo.RemoveItem(id);
            Console.WriteLine($"✓ Successfully removed '{itemName}' (ID: {id}) from inventory.");
        }
        catch (ItemNotFoundException ex)
        {
            Console.WriteLine($"✗ Item removal failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Unexpected error during item removal: {ex.Message}");
        }
    }

    // Public properties to access repositories for testing
    public InventoryRepository<ElectronicItem> Electronics => electronics;
    public InventoryRepository<GroceryItem> Groceries => groceries;

    // Additional helper methods for comprehensive testing
    public void DemonstrateExceptionHandling()
    {
        Console.WriteLine("=== Exception Handling Demonstration ===");

        // Test 1: Try to add duplicate item
        Console.WriteLine("\n1. Testing Duplicate Item Exception:");
        try
        {
            electronics.AddItem(new ElectronicItem(1001, "Duplicate iPhone", 10, "Apple", 12));
        }
        catch (DuplicateItemException ex)
        {
            Console.WriteLine($"✗ Expected error caught: {ex.Message}");
        }

        // Test 2: Try to remove non-existent item
        Console.WriteLine("\n2. Testing Item Not Found Exception (Remove):");
        RemoveItemById(electronics, 9999);

        // Test 3: Try to update with invalid quantity
        Console.WriteLine("\n3. Testing Invalid Quantity Exception:");
        try
        {
            electronics.UpdateQuantity(1001, -5);
        }
        catch (InvalidQuantityException ex)
        {
            Console.WriteLine($"✗ Expected error caught: {ex.Message}");
        }

        // Test 4: Try to get non-existent item
        Console.WriteLine("\n4. Testing Item Not Found Exception (Get):");
        try
        {
            groceries.GetItemById(9999);
        }
        catch (ItemNotFoundException ex)
        {
            Console.WriteLine($"✗ Expected error caught: {ex.Message}");
        }

        Console.WriteLine();
    }

    public void DemonstrateSuccessfulOperations()
    {
        Console.WriteLine("=== Successful Operations Demonstration ===");

        // Successful stock increase
        Console.WriteLine("\n1. Increasing stock for existing item:");
        IncreaseStock(electronics, 1001, 10);

        // Successful item removal
        Console.WriteLine("\n2. Removing existing item:");
        RemoveItemById(groceries, 2003);

        // Successful quantity update
        Console.WriteLine("\n3. Updating quantity for existing item:");
        try
        {
            electronics.UpdateQuantity(1002, 50);
            var updatedItem = electronics.GetItemById(1002);
            Console.WriteLine($"✓ Successfully updated quantity for '{updatedItem.Name}' to {updatedItem.Quantity}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Quantity update failed: {ex.Message}");
        }

        Console.WriteLine();
    }
}

// Program Entry Point
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Warehouse Inventory Management System ===");
        Console.WriteLine("Demonstrating Collections, Generics, and Exception Handling\n");

        // Instantiate Warehouse Manager
        var warehouse = new WareHouseManager();

        // Execute main workflow
        warehouse.SeedData();

        // Print all items
        warehouse.PrintAllItems(warehouse.Groceries, "Grocery");
        warehouse.PrintAllItems(warehouse.Electronics, "Electronic");

        // Demonstrate exception handling scenarios
        warehouse.DemonstrateExceptionHandling();

        // Demonstrate successful operations
        warehouse.DemonstrateSuccessfulOperations();

        // Final inventory status
        Console.WriteLine("=== Final Inventory Status ===");
        warehouse.PrintAllItems(warehouse.Groceries, "Grocery");
        warehouse.PrintAllItems(warehouse.Electronics, "Electronic");

        Console.WriteLine("=== Warehouse Management System Demo Complete ===");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}