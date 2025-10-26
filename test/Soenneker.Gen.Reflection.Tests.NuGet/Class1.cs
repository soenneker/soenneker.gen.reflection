using Soenneker.Gen.Adapt.Tests.Types;

namespace Soenneker.Gen.Adapt.Tests.NuGet;

public class Class1
{
    public void Test()
    {
        var blah = new ComplexSourceDto().Adapt<ComplexDestDto>();
        
        // Force the generator to create the mapping between nested objects
        var nested = new NestedObjectDto().Adapt<NestedDestObjectDto>();
    }
}