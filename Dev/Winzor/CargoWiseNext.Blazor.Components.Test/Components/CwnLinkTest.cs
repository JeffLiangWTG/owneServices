using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnLinkTest : BunitTestContext
{
	[Test]
	public void CwnLink_RenderTest()
	{
		var cut = RenderComponent<CwnLink>();

		cut.MarkupMatches(@"<span class=""cwn-link"" />");
	}

	[Test]
	public void CwnLink_ChildContentTest()
	{
		var cut = RenderComponent<CwnLink>(parameter => parameter.AddChildContent("My content"));

		var uut = cut.Find(".cwn-link");

		Assert.That(uut.TextContent, Is.EqualTo("My content"));
	}
}
