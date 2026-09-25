using System;
using AwesomeAssertions;

namespace Soenneker.Gen.Reflection.Tests;

public class FastAccessTests
{
    [Test]
    public void TypedAccessors_ReadAndWriteWithoutBoxing()
    {
        var model = new FastAccessModel();
        var metadata = model.GetTypeGen();
        var property = metadata.GetProperty(nameof(FastAccessModel.Value))!.Value;
        var getter = property.GetGetter<FastAccessModel, int>();
        var setter = property.GetSetter<FastAccessModel, int>();
        setter(model, 42);
        getter(model).Should().Be(42);
        property.GetValue(model).Should().Be(42);
        Action wrongType = () => property.GetGetter<FastAccessModel, string>();
        wrongType.Should().Throw<InvalidOperationException>();
        Action readOnly = () => metadata.GetProperty(nameof(FastAccessModel.ReadOnly))!.Value.GetSetter<FastAccessModel, int>();
        readOnly.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Lookup_PreservesNamesOverloadsAndMissingMembers()
    {
        var metadata = new FastAccessModel().GetTypeGen();
        foreach (var property in metadata.Properties)
            metadata.GetProperty(property.Name)!.Value.Name.Should().Be(property.Name);
        foreach (var field in metadata.Fields)
            metadata.GetField(field.Name)!.Value.Name.Should().Be(field.Name);
        foreach (var method in metadata.Methods)
            metadata.GetMethod(method.Name)!.Value.Name.Should().Be(method.Name);
        metadata.GetProperty("value").Should().BeNull();
        metadata.GetProperty("Missing").Should().BeNull();
        metadata.GetField("Missing").Should().BeNull();
        metadata.GetMethod("Missing").Should().BeNull();
        metadata.GetMethod("Overload")!.Value.ParameterTypes.Should().BeEmpty();
        metadata.GetProperty(null!).Should().BeNull();
    }

    [Test]
    public void ManualMetadata_UsesFallbackLookup()
    {
        var generated = new FastAccessModel().GetTypeGen();
        var manual = new TypeInfoGen("Manual", "Manual", "Manual", false, true, false, false,
            generated.Fields, generated.Properties, generated.Methods, null, null);
        manual.GetProperty("Value")!.Value.Name.Should().Be("Value");
        manual.GetProperty("Missing").Should().BeNull();
        manual.GetField("Missing").Should().BeNull();
        manual.GetMethod("Missing").Should().BeNull();
    }
}

public class FastAccessModel
{
    public int Value { get; set; }
    public int ReadOnly => Value;
    public int Field;
    public void Overload() { }
    public void Overload(int value) { }
}
