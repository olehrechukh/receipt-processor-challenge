using receipt.processor.Models;

namespace receipt.processor;

public record Receipt(
    string Retailer,
    DateOnly PurchaseDate,
    TimeOnly PurchaseTime,
    List<Item> Items,
    decimal Total)
{
    public long CalculatePoints()
    {
        var points = 0L;

        // One point for every alphanumeric character in the retailer name.
        points += Retailer.Count(char.IsLetterOrDigit);

        // 50 points if the total is a round dollar amount with no cents.
        if (Total == (int)Total)
            points += 50;

        // 25 points if the total is a multiple of 0.25.
        if (Total % 0.25m == 0)
            points += 25;

        // 5 points for every two items on the receipt.
        points += Items.Count / 2 * 5;

        // If the trimmed length of the item description is a multiple of 3,
        // multiply the price by 0.2 and round up to the nearest integer.
        foreach (var item in Items)
            if (item.ShortDescription.Trim().Length % 3 == 0)
                points += (int)Math.Ceiling(item.Price * 0.2m);

        // 6 points if the day in the purchase date is odd.
        if (PurchaseDate.Day % 2 != 0)
            points += 6;

        // 10 points if the time of purchase is between 2:00pm and 4:00pm.
        // I assume that does NOT include 14:00 and 16:00 (inclusive).
        if (PurchaseTime is { Hour: 14, Minute: > 0 } || PurchaseTime.Hour == 15)
            points += 10;

        return points;
    }
}