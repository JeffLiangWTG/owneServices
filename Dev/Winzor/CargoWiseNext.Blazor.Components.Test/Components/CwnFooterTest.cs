using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnFooterTest : BunitTestContext
{
	[Test]
	public void CwnFooter_RenderTest()
	{
		var cut = RenderComponent<CwnFooter>();
		cut.MarkupMatches("<footer class=\"cwn-layout__footer\"></footer>");
	}

	[Test]
	public void CwnFooter_ChildContentTest()
	{
		var cut = RenderComponent<CwnFooter>(parameters => parameters.AddChildContent("<h1>Hello!</h1>"));
		Assert.That(cut.Instance.ChildContent, Is.Not.Null);
		cut.MarkupMatches("<footer class=\"cwn-layout__footer\"><h1>Hello!</h1></div>");
	}
}
