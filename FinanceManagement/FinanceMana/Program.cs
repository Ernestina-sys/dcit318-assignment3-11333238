using System;
using System.Collections.Generic;

// Core Models using Records
public record Transaction(
    int Id,
    DateTime Date,
    decimal Amount,
    string Category
);

// Payment Behavior Using Interfaces
public interface ITransactionProcessor
{
    void Process(Transaction transaction);
}

// Concrete Implementations of ITransactionProcessor
public class BankTransferProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"Bank Transfer Processing: ${transaction.Amount:F2} for {transaction.Category}");
        Console.WriteLine("Transaction processed through traditional banking network.");
    }
}

public class MobileMoneyProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"Mobile Money Processing: ${transaction.Amount:F2} for {transaction.Category}");
        Console.WriteLine("Transaction processed via mobile payment platform.");
    }
}

public class CryptoWalletProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"Crypto Wallet Processing: ${transaction.Amount:F2} for {transaction.Category}");
        Console.WriteLine("Transaction processed on blockchain network.");
    }
}

// Base Account Class
public class Account
{
    public string AccountNumber { get; }
    public decimal Balance { get; protected set; }

    public Account(string accountNumber, decimal initialBalance)
    {
        AccountNumber = accountNumber;
        Balance = initialBalance;
    }

    public virtual void ApplyTransaction(Transaction transaction)
    {
        Balance -= transaction.Amount;
        Console.WriteLine($"Transaction applied to Account {AccountNumber}. New balance: ${Balance:F2}");
    }
}

// Sealed Specialized Account
public sealed class SavingsAccount : Account
{
    public SavingsAccount(string accountNumber, decimal initialBalance)
        : base(accountNumber, initialBalance)
    {
    }

    public override void ApplyTransaction(Transaction transaction)
    {
        if (transaction.Amount > Balance)
        {
            Console.WriteLine("Insufficient funds");
            return;
        }

        Balance -= transaction.Amount;
        Console.WriteLine($"Savings Account Transaction Applied. Updated balance: ${Balance:F2}");
    }
}

// Main Finance Application
public class FinanceApp
{
    private List<Transaction> transactions = new List<Transaction>();

    public void Run()
    {
        // Instantiate a SavingsAccount
        var savingsAccount = new SavingsAccount("SAV-001", 1000m);
        Console.WriteLine($"Created Savings Account: {savingsAccount.AccountNumber} with initial balance: ${savingsAccount.Balance:F2}");
        Console.WriteLine();

        // Create three Transaction records
        var transaction1 = new Transaction(1, DateTime.Now, 150.75m, "Groceries");
        var transaction2 = new Transaction(2, DateTime.Now.AddHours(-2), 89.50m, "Utilities");
        var transaction3 = new Transaction(3, DateTime.Now.AddHours(-1), 45.25m, "Entertainment");

        // Create processors
        var mobileProcessor = new MobileMoneyProcessor();
        var bankProcessor = new BankTransferProcessor();
        var cryptoProcessor = new CryptoWalletProcessor();

        // Process and apply transactions
        Console.WriteLine("=== Transaction Processing ===");

        // Transaction 1 - Mobile Money
        Console.WriteLine($"\n--- Processing Transaction {transaction1.Id} ---");
        mobileProcessor.Process(transaction1);
        savingsAccount.ApplyTransaction(transaction1);
        transactions.Add(transaction1);

        // Transaction 2 - Bank Transfer
        Console.WriteLine($"\n--- Processing Transaction {transaction2.Id} ---");
        bankProcessor.Process(transaction2);
        savingsAccount.ApplyTransaction(transaction2);
        transactions.Add(transaction2);

        // Transaction 3 - Crypto Wallet
        Console.WriteLine($"\n--- Processing Transaction {transaction3.Id} ---");
        cryptoProcessor.Process(transaction3);
        savingsAccount.ApplyTransaction(transaction3);
        transactions.Add(transaction3);

        // Display final summary
        Console.WriteLine("\n=== Transaction Summary ===");
        Console.WriteLine($"Total Transactions Processed: {transactions.Count}");
        Console.WriteLine($"Final Account Balance: ${savingsAccount.Balance:F2}");

        Console.WriteLine("\nTransaction Details:");
        foreach (var txn in transactions)
        {
            Console.WriteLine($"- ID: {txn.Id}, Amount: ${txn.Amount:F2}, Category: {txn.Category}, Date: {txn.Date:yyyy-MM-dd HH:mm}");
        }
    }
}

// Program Entry Point
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Finance Management System ===");
        Console.WriteLine("Demonstrating Records, Interfaces, and Sealed Classes\n");

        var financeApp = new FinanceApp();
        financeApp.Run();

        Console.WriteLine("\n=== System Demo Complete ===");
        Console.ReadKey(); // Keep console open
    }
}