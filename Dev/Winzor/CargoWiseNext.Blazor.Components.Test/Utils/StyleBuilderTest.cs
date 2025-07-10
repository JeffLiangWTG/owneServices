
namespace CargoWiseNext.Blazor.Components.Test;

public class StyleBuilderTest
{
	[Test]
	public void StyleBuilder_BuildTest()
	{
		var actual = new StyleBuilder().Build();

		Assert.That(actual, Is.Null);
	}

	[Test]
	public void StyleBuilder_AddStringStyle_OnlyOneTest()
	{
		var actual = new StyleBuilder()
			.AddStyle("width:10px;")
			.Build();

		Assert.That(actual, Is.EqualTo("width:10px;"));
	}

	[Test]
	public void StyleBuilder_AddStringStyle_ManyAtOnceTest()
	{
		var actual = new StyleBuilder()
			.AddStyle("width:10px;height:20px;margin: 0 0 0 10px;")
			.Build();

		Assert.That(actual, Is.EqualTo("width:10px;height:20px;margin: 0 0 0 10px;"));
	}

	[Test]
	public void StyleBuilder_AddStringStyle_ManyAtOnce_WithWhiteSpacesTest()
	{
		var actual = new StyleBuilder()
			.AddStyle("   width:10px; height:20px; margin: 0 0 0 10px;   ")
			.Build();

		Assert.That(actual, Is.EqualTo("width:10px; height:20px; margin: 0 0 0 10px;"));
	}

	[Test]
	public void StyleBuilder_AddStyle_OnlyOneTest()
	{
		var actual = new StyleBuilder()
			.AddStyle("width", "10px")
			.Build();

		Assert.That(actual, Is.EqualTo("width:10px;"));
	}

	[Test]
	public void StyleBuilder_AddStyle_ManyAtOnceTest()
	{
		var actual = new StyleBuilder()
			.AddStyle("width", "10px")
			.AddStyle("height", "20px")
			.AddStyle("margin", "0 0 10px 0")
			.Build();

		Assert.That(actual, Is.EqualTo("width:10px;height:20px;margin:0 0 10px 0;"));
	}

	[Test]
	public void StyleBuilder_AddStyle_ManyAtOnce_DifferentWaysTest()
	{
		var actual = new StyleBuilder()
			.AddStyle("width", "10px")
			.AddStyle("height", "20px")
			.AddStyle("margin", "0 0 10px 0")
			.AddStyle("background-color: blue; font-size:16px;")
			.Build();

		Assert.That(actual, Is.EqualTo("width:10px;height:20px;margin:0 0 10px 0;background-color: blue; font-size:16px;"));
	}
}
