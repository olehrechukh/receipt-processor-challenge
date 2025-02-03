using receipt.processor.Models;
using Shouldly;
using Xunit;

namespace receipt.processor.tests;

public class CalculatePointsTests
{
    [Theory]
    [InlineData("M&M Corner Market", 14)]
    [InlineData("Mountain Dew 12PK", 15)]
    [InlineData("Retailer", 8)]
    public void RetailerNamePoints_BasedOnRetailer(string retailer, int result)
    {
        var receipt = GetZeroPointsReceipt() with { Retailer = retailer };

        // Skip '&' and spaces
        receipt.CalculatePoints().ShouldBe(result);
    }

    [Theory]
    [InlineData(1.1, 0)]
    [InlineData(1, 75)] // total is a multiple of 0.25, so + 25
    public void TotalPoints_BasedTotalCents(decimal total, int result)
    {
        var receipt = GetZeroPointsReceipt() with { Total = total };

        receipt.CalculatePoints().ShouldBe(result);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 5)]
    [InlineData(3, 5)]
    [InlineData(4, 10)]
    public void RoundDollarAmountMultipleOf025_BasedOnItemsCount(int count, int result)
    {
        var receipt = GetZeroPointsReceipt() with
        {
            Items = Enumerable.Range(0, count).Select(_ => new Item("Gatorade", 2.25m)).ToList()
        };

        receipt.CalculatePoints().ShouldBe(result);
    }

    [Fact]
    public void ItemDescriptionMultipleOfThree_BasedOnItemsCountPrice()
    {
        var receipt = GetZeroPointsReceipt() with
        {
            Items =
            [
                new Item("123", 1), // +1
                new Item("123", 4.99m), // +1
                new Item("123", 5.01m), // +2
                new Item("1234", 5), // 0, trim value length = 4
                new Item("12345 ", 5) // 0, trim value length = 5
            ]
        };

        receipt.CalculatePoints().ShouldBe(14); // +10 for length, 5 points for every two items on the receipt.
    }


    [Theory]
    [InlineData(1, 6)]
    [InlineData(2, 0)]
    public void PurchaseDate_BasedOnOddDay(int day, int result)
    {
        var receipt = GetZeroPointsReceipt() with { PurchaseDate = new DateOnly(2025, 02, day) };

        receipt.CalculatePoints().ShouldBe(result);
    }

    [Theory]
    [InlineData(14, 00, 0)]
    [InlineData(14, 01, 10)]
    [InlineData(15, 59, 10)]
    [InlineData(16, 00, 0)]
    public void PurchaseTime_BasedOn2pmAnd4pmTime(int hour, int minutes, int result)
    {
        var receipt = GetZeroPointsReceipt() with { PurchaseTime = new TimeOnly(hour, minutes) };

        receipt.CalculatePoints().ShouldBe(result);
    }

    [Fact]
    public void ReadmeExample1()
    {
        Receipt receipt = new(
            "Target",
            new DateOnly(2022, 01, 01),
            new TimeOnly(13, 01),
            [
                new Item("Mountain Dew 12PK", 6.49m),
                new Item("Emils Cheese Pizza", 12.25m),
                new Item("Knorr Creamy Chicken", 1.26m),
                new Item("Doritos Nacho Cheese", 3.35m),
                new Item("   Klarbrunn 12-PK 12 FL OZ  ", 12.00m)
            ],
            35.35m
        );

        receipt.CalculatePoints().ShouldBe(28);
    }

    [Fact]
    public void ReadmeExample2()
    {
        Receipt receipt = new(
            "M&M Corner Market",
            new DateOnly(2022, 03, 20),
            new TimeOnly(14, 33),
            [
                new Item("Gatorade", 2.25m),
                new Item("Gatorade", 2.25m),
                new Item("Gatorade", 2.25m),
                new Item("Gatorade", 2.25m)
            ],
            9.00m
        );

        receipt.CalculatePoints().ShouldBe(109);
    }

    private static Receipt GetZeroPointsReceipt()
    {
        return new Receipt(
            "&&&",
            new DateOnly(2025, 02, 02),
            new TimeOnly(12, 30),
            [new Item("Item1", 1.50m)],
            1.51m
        );
    }
}