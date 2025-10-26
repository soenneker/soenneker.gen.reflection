namespace Soenneker.Gen.Adapt.Tests.Types;

public class GenericDestDto<T>
{
    public T Value { get; set; }
    public string Id { get; set; }
    public List<T> Items { get; set; } = [];
}

