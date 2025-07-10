using Bunit;
using SysColor = System.Drawing.Color;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnLayoutTest : BunitTestContext
{
	[Test]
	public void CwnLayout_RenderTest()
	{
		// Act
		var cut = RenderComponent<CwnLayout>();

		// Assert
		cut.MarkupMatches(" <div class=\"cwn-layout\"></div>");
	}

	[Test]
	public void CwnLayout_StyleTest()
	{
		// Arrange
		var style = "background-color: blue; height: 20px;";

		// Act
		var cut = RenderComponent<CwnLayout>(parameters => parameters.Add(p => p.Style, style));

		// Assert
		cut.MarkupMatches($"<div class=\"cwn-layout\" style=\"{style}\"></div>");
	}

	[Test]
	public void CwnLayout_ClassTest()
	{
		// Arrange
		var @class = "custom-css-class";

		// Act
		var cut = RenderComponent<CwnLayout>(parameters => parameters.Add(p => p.Class, @class));

		// Assert
		cut.MarkupMatches($"<div class=\"cwn-layout {@class}\"></div>");
	}

	[Test]
	public void CwnLayout_ChildContentTest()
	{
		// Arrange
		var content = "<p>Content</p>";

		// Act
		var cut = RenderComponent<CwnLayout>(parameters => parameters.AddChildContent(content));
		var markup = cut.Markup.Trim();

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(markup, Does.StartWith("<div class=\"cwn-layout\">"));
			Assert.That(markup, Does.Contain(content));
			Assert.That(markup, Does.EndWith("</div>"));
		});
	}

	[Test]
	public void CwnLayout_ThemeTest()
	{
		// Arrange
		var theme = new Theme
		{
			Palette = new Palette
			{
				AppBarBackgroundColor = SysColor.Magenta,
				AppBarTextColor = SysColor.Orange,
				BackgroundColor = SysColor.Yellow,
				RecentFavBackgroundColor = SysColor.Blue,
			}
		};

		// Act
		var cut = RenderComponent<CwnLayout>(
			parameters => parameters.Add(p => p.Theme, theme));

		var style = cut.Find("style");
		var div = cut.Find("div");

		// Assert
		Assert.That(cut.Markup, Does.StartWith("<style>"));

		style.MarkupMatches(@"
		<style>.cwn-theme--custom {
			--s-brand-bg-default: rgba(255, 0, 255, 1);
			--s-brand-txt-inv-hover: rgba(255, 0, 255, 1);
			--s-brand-txt-inv-active: rgba(255, 0, 255, 1);
			--s-brand-txt-inv-default: rgba(255, 165, 0, 1);
			--s-theme-bg-default:rgba(255, 255, 0, 1);
			--s-theme-recent-fav-bg-default:rgba(0, 0, 255, 1);
		}
		</style>");

		div.MarkupMatches("<div class=\"cwn-layout cwn-theme--custom\" />");
	}
}
