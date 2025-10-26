namespace Soenneker.Gen.Adapt.Tests.Types;

public class ComplexDestDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public NestedDestObjectDto NestedObject { get; set; }
    public List<string> Tags { get; set; } = [];
}

public class NestedDestObjectDto
{
    public string NestedId { get; set; }
    public string NestedName { get; set; }

}

public static class Test
{


}