namespace Alpha;

public record Aggregate
{
    public Guid FooId { get; init; }

    public string FooName { get; init; } = null!;

    public Guid BarId { get; init; }

    public int BarCost { get; init; }
}