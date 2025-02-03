using Microsoft.AspNetCore.Mvc;
using MiniValidation;
using receipt.processor;
using receipt.processor.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<ReceiptStorage>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapPost("/receipts/process", ([FromBody] ReceiptDto receipt, ReceiptStorage storage) =>
    {
        if (!MiniValidator.TryValidate(receipt, out _))
            return Results.BadRequest(new Error("The receipt is invalid."));

        var id = storage.ProcessReceipt(receipt.ToReceipt().CalculatePoints());
        return Results.Ok(new ProcessResult(id));
    })
    .Produces<PointsResult>()
    .Produces<Error>(400)
    .WithSummary("Submits a receipt for processing.")
    .WithDescription("Submits a receipt for processing.");

app.MapGet("/receipts/{id}/points", (string id, ReceiptStorage storage) =>
    {
        if (!Guid.TryParse(id, out var idGuid))
            return Results.NotFound(new Error("No receipt found for that ID."));

        var points = storage.GetReceiptPoints(idGuid);

        return points is null
            ? Results.NotFound(new Error("No receipt found for that ID."))
            : Results.Ok(new PointsResult(points.Value));
    })
    .Produces<PointsResult>()
    .Produces<Error>(404)
    .WithSummary("Returns the points awarded for the receipt.")
    .WithSummary("Returns the points awarded for the receipt.");

app.Run();

public partial class Program;