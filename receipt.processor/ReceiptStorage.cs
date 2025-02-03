using System.Collections.Concurrent;
using receipt.processor.Models;

namespace receipt.processor;

public class ReceiptStorage
{
    private readonly ConcurrentDictionary<Guid, long> _receiptPoints = new();

    public Guid ProcessReceipt(long points)
    {
        var id = Guid.NewGuid();
        _receiptPoints[id] = points;

        return id;
    }

    public long? GetReceiptPoints(Guid id) => _receiptPoints.TryGetValue(id, out var points) ? points : null;
}