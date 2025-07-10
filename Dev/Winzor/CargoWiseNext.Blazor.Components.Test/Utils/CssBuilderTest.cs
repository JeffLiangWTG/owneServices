
namespace CargoWiseNext.Blazor.Components.Test;

public class CssBuilderTest
{
	[Test]
	public void CssBuilder_BuildTest()
	{
		var actual = new CssBuilder().Build();

		Assert.That(actual, Is.Empty);
	}

	[Test]
	public void CssBuilder_AddClass_OnlyOneTest()
	{
		var actual = new CssBuilder()
			.AddClass("my-class")
			.Build();

		Assert.That(actual, Is.EqualTo("my-class"));
	}

	[Test]
	public void CssBuilder_AddClass_MultipleTest()
	{
		var actual = new CssBuilder()
			.AddClass("my-class-1")
			.AddClass("my-class-2")
			.AddClass("my-class-3")
			.Build();

		Assert.That(actual, Is.EqualTo("my-class-1 my-class-2 my-class-3"));
	}

	[Test]
	public void CssBuilder_AddClass_MultipleInlineTest()
	{
		var actual = new CssBuilder()
			.AddClass("my-class-1 my-class-2    my-class-3  ")
			.Build();

		Assert.That(actual, Is.EqualTo("my-class-1 my-class-2 my-class-3"));
	}
}
