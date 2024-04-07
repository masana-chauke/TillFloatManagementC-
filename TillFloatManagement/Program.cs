using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

interface Item
{
    string Description { get; set; }
    int Amount { get; set; }
}

class ConcreteItem : Item
{
    public string Description { get; set; } = "";
    public int Amount { get; set; }
}

interface Transaction
{
    List<Item> Items { get; set; }
    List<int> Paid { get; set; }
}

class ConcreteTransaction : Transaction
{
    public List<Item> Items { get; set; } = new List<Item>();
    public List<int> Paid { get; set; } = new List<int>();
}

class TillState
{
    public Dictionary<int, int> Till { get; set; } = new Dictionary<int, int>();
    public int TillStart { get; set; }
}

class Program
{
    static TillState InitializeTill()
    {
        var till = new Dictionary<int, int>();
        int tillStart = 0;

        var denominations = new Dictionary<string, int>()
        {
            {"5 x R50", 50},
            {"5 x R20", 20},
            {"6 x R10", 10},
            {"12 x R5", 5},
            {"10 x R2", 2},
            {"10 x R1", 1}
        };

        foreach (var kvp in denominations)
        {
            var noteParts = kvp.Key.Split(" x ");
            var num = int.Parse(noteParts[0]);
            var currency = kvp.Value;
            till[currency] = num;
            tillStart += num * currency;
        }

        return new TillState { Till = till, TillStart = tillStart };
    }

    static string CalculateChange(int change, Dictionary<int, int> till)
    {
        if (change == 0) return "No Change";

        var coins = till.Keys.Where(coin => coin <= change).OrderByDescending(coin => coin).ToList();
        var usedCoins = new List<int>();
        var remainingChange = change;

        foreach (var coin in coins)
        {
            while (remainingChange >= coin && (till.TryGetValue(coin, out int count) && count > 0))
            {
                usedCoins.Add(coin);
                remainingChange -= coin;
                till[coin]--;
            }
        }

        if (remainingChange != 0)
        {
            foreach (var coin in coins)
            {
                if (usedCoins.Count(c => c == coin) > 0)
                {
                    till[coin] += usedCoins.Count(c => c == coin);
                }
            }
            return "No Change";
        }

        return string.Join("-", usedCoins.Select(coin => $"R{coin}"));
    }

    static TillState ProcessTransaction(ConcreteTransaction transaction, TillState tillState)
    {
        var till = new Dictionary<int, int>(tillState.Till);
        var tillStart = tillState.TillStart;
        var transactionTotal = transaction.Items.Sum(item => item.Amount);
        var totalPaid = transaction.Paid.Sum();
        var changeTotal = totalPaid - transactionTotal;
        var changeBreakdown = CalculateChange(changeTotal, till);

        Console.WriteLine($"R{tillStart}, R{transactionTotal}, R{totalPaid}, R{changeTotal}, {changeBreakdown}");

        var updatedTillStart = tillStart + transactionTotal;
        transaction.Items.ForEach(item =>
        {
            if (till.ContainsKey(item.Amount))
            {
                till[item.Amount]--;
            }
        });

        return new TillState { Till = till, TillStart = updatedTillStart };
    }

    static List<ConcreteTransaction> ParseInput(string input)
    {
        var transactions = new List<ConcreteTransaction>();

        var lines = input.Trim().Split('\n');
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            var items = parts[0].Split(';').Select(item =>
            {
                var itemParts = item.Trim().Split(" R");
                return new ConcreteItem { Description = itemParts[0].Trim(), Amount = int.Parse(itemParts[1].Trim()) };
            }).Cast<Item>().ToList(); 

            var paid = parts[1].Split('-').Select(amount => int.Parse(amount.Substring(1).Trim())).ToList();

            transactions.Add(new ConcreteTransaction { Items = items, Paid = paid });
        }

        return transactions;
    }

    static void Main(string[] args)
    {
        var tillState = InitializeTill();
        var inputFile = File.ReadAllText("input.txt");
        var transactions = ParseInput(inputFile);

        Console.WriteLine("Transaction Summary:");
        Console.WriteLine("Till Start, Transaction Total, Paid, Change Total, Change Breakdown");

        foreach (var transaction in transactions)
        {
            tillState = ProcessTransaction(transaction, tillState);
        }

        Console.WriteLine($"Remaining Till Balance: R{tillState.TillStart}");
    }
}