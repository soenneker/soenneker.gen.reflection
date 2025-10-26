namespace Soenneker.Gen.Reflection.Tests.Types;

public class ComplexSourceDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public NestedObjectDto NestedObject { get; set; }
    public List<string> Tags { get; set; } = [];
}

public class NestedObjectDto
{
    public string NestedId { get; set; }
    public string NestedName { get; set; }
}

