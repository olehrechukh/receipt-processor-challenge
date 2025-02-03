using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using receipt.processor.Models;
using Shouldly;
using Xunit;

namespace receipt.processor.tests;

public class ReceiptProcessorTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient = factory.CreateDefaultClient();
    private const string ReceiptErrorMessage = "The receipt is invalid.";

    [Fact]
    public async Task ProcessReceipt_ValidReceipt_ReturnsId()
    {
        var receipt = GetValidReceipt();

        var response = await _httpClient.PostAsJsonAsync("/receipts/process", receipt);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var processResult = await response.Content.ReadFromJsonAsync<ProcessResult>();
        processResult.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("M#M Corner Market")]
    public async Task ProcessReceipt_InvalidReceipt_ReturnsBadRequest(string retailer)
    {
        var invalidReceipt = GetValidReceipt() with { Retailer = retailer };

        var response = await _httpClient.PostAsJsonAsync("/receipts/process", invalidReceipt);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<Error>();
        error.Description.ShouldBe(ReceiptErrorMessage);
    }

    [Theory]
    [InlineData("2022-13-01")]
    [InlineData("2022-02-30")]
    [InlineData("2022-00-01")]
    [InlineData("2022-12-32")]
    [InlineData("2022-02-29")]
    [InlineData("abcd-ef-gh")]
    public async Task ProcessReceipt_InvalidDateFormat_ReturnsBadRequest(string purchaseDate)
    {
        var invalidReceipt = GetValidReceipt() with { PurchaseDate = purchaseDate };

        var response = await _httpClient.PostAsJsonAsync("/receipts/process", invalidReceipt);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<Error>();
        error.Description.ShouldBe(ReceiptErrorMessage);
    }


    [Theory]
    [InlineData("24:00")]
    [InlineData("24:01")]
    [InlineData("-01:00")]
    [InlineData("00:60")]
    [InlineData("00:61")]
    public async Task ProcessReceipt_InvalidTimeFormat_ReturnsBadRequest(string purchaseTime)
    {
        var invalidReceipt = GetValidReceipt() with { PurchaseTime = purchaseTime };

        var response = await _httpClient.PostAsJsonAsync("/receipts/process", invalidReceipt);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<Error>();
        error.Description.ShouldBe(ReceiptErrorMessage);
    }

    [Fact]
    public async Task ProcessReceipt_InvalidTotalFormat_ReturnsBadRequest()
    {
        var invalidReceipt = GetValidReceipt() with { Total = 6.495m };

        var response = await _httpClient.PostAsJsonAsync("/receipts/process", invalidReceipt);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<Error>();
        error.Description.ShouldBe(ReceiptErrorMessage);
    }

    [Fact]
    public async Task ProcessReceipt_EmptyItemsArray_ReturnsBadRequest()
    {
        var invalidReceipt = GetValidReceipt() with { Items = [] };

        var response = await _httpClient.PostAsJsonAsync("/receipts/process", invalidReceipt);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<Error>();
        error.Description.ShouldBe(ReceiptErrorMessage);
    }

    [Theory]
    [InlineData(1234)]
    [InlineData(12.345)]
    [InlineData(12.3)]
    [InlineData(12.000)]
    [InlineData(0)]
    public async Task ProcessReceipt_InvalidTotalPrice_ReturnsBadRequest(decimal totalPrice)
    {
        var invalidReceipt = GetValidReceipt() with { Total = totalPrice };

        var response = await _httpClient.PostAsJsonAsync("/receipts/process", invalidReceipt);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<Error>();
        error.Description.ShouldBe(ReceiptErrorMessage);
    }

    [Theory]
    [InlineData(1234)]
    [InlineData(12.345)]
    [InlineData(12.3)]
    [InlineData(12.000)]
    [InlineData(0)]
    public async Task ProcessReceipt_InvalidItemPrice_ReturnsBadRequest(decimal price)
    {
        var invalidReceipt = GetValidReceipt() with { Items = [new Item("Mountain Dew 12PK", price)] };

        var response = await _httpClient.PostAsJsonAsync("/receipts/process", invalidReceipt);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<Error>();
        error.Description.ShouldBe(ReceiptErrorMessage);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("E08493CD-A73A-4C08-A0F0-F2045F4DFD55")]
    public async Task GetPoints_InvalidId_ReturnsNotFound(string receiptId)
    {
        var response = await _httpClient.GetAsync($"/receipts/{receiptId}/points");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var error = await response.Content.ReadFromJsonAsync<Error>();
        error.Description.ShouldBe("No receipt found for that ID.");
    }

    [Fact]
    public async Task GetPoints_ValidId_ReturnsPoints()
    {
        ReceiptDto receipt = new(
            Retailer: "Target",
            PurchaseDate: "2022-01-01",
            PurchaseTime: "13:01",
            Items:
            [
                new Item("Mountain Dew 12PK", 6.49m),
                new Item("Emils Cheese Pizza", 12.25m),
                new Item("Knorr Creamy Chicken", 1.26m),
                new Item("Doritos Nacho Cheese", 3.35m),
                new Item("   Klarbrunn 12-PK 12 FL OZ  ", 12.00m)
            ],
            Total: 35.35m
        );

        var processResponse = await _httpClient.PostAsJsonAsync("/receipts/process", receipt);
        var processResult = await processResponse.Content.ReadFromJsonAsync<ProcessResult>();

        var pointsResponse = await _httpClient.GetAsync($"/receipts/{processResult.Id}/points");

        pointsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pointsResponseBody = await pointsResponse.Content.ReadFromJsonAsync<PointsResult>();
        pointsResponseBody.Points.ShouldBeGreaterThanOrEqualTo(28);
    }

    [Fact]
    public async Task GetPoints_ValidId_ReturnsPointsLessThanIntMaxValue()
    {
        ReceiptDto receipt = new(
            Retailer: "Target",
            PurchaseDate: "2022-01-01",
            PurchaseTime: "13:01",
            Items: Enumerable.Range(0, 6) // 1.2 * int.MaxValue
                .Select(_ => new Item("Emils Cheese Pizza", int.MaxValue + 0.00m))
                .ToList(),
            Total: 35.35m
        );

        var processResponse = await _httpClient.PostAsJsonAsync("/receipts/process", receipt);
        var processResult = await processResponse.Content.ReadFromJsonAsync<ProcessResult>();

        var pointsResponse = await _httpClient.GetAsync($"/receipts/{processResult.Id}/points");

        pointsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pointsResponseBody = await pointsResponse.Content.ReadFromJsonAsync<PointsResult>();
        pointsResponseBody.Points.ShouldBeGreaterThanOrEqualTo(int.MaxValue);
    }

    private static ReceiptDto GetValidReceipt() => new(
        Retailer: "M&M Corner Market",
        PurchaseDate: "2025-01-01",
        PurchaseTime: "13:01",
        Items: [new Item("Mountain Dew 12PK", 6.49m)],
        Total: 6.49m
    );
}