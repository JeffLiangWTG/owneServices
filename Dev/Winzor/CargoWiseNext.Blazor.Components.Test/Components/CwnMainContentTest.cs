using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnMainContentTest : BunitTestContext
{
	[Test]
	public void CwnMainContent_RenderTest()
	{
		// Act
		var cut = RenderComponent<CwnMainContent>();

		// Assert
		cut.MarkupMatches("<div class=\"cwn-layout__main-content\"></div>");
	}

	[Test]
	public void CwnMainContent_StyleTest()
	{
		// Arrange
		var style = "background-color: blue; height: 20px;";

		// Act
		var cut = RenderComponent<CwnMainContent>(parameters => parameters.Add(p => p.Style, style));

		// Assert
		cut.MarkupMatches($"<div class=\"cwn-layout__main-content\" style=\"{style}\"></div>");
	}

	[Test]
	public void CwnMainContent_ClassTest()
	{
		// Arrange
		var @class = "custom-css-class";

		// Act
		var cut = RenderComponent<CwnMainContent>(parameters => parameters.Add(p => p.Class, @class));

		// Assert
		cut.MarkupMatches($"<div class=\"cwn-layout__main-content {@class}\"></div>");
	}

	[Test]
	public void CwnMainContent_ChildContentTest()
	{
		// Arrange
		var content = "<p>Content</p>";

		// Act
		var cut = RenderComponent<CwnMainContent>(parameters => parameters.AddChildContent(content));

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(cut.Markup, Does.StartWith("<div class=\"cwn-layout__main-content\">"));
			Assert.That(cut.Markup, Does.Contain(content));
			Assert.That(cut.Markup, Does.EndWith("</div>"));
		});
	}
}
