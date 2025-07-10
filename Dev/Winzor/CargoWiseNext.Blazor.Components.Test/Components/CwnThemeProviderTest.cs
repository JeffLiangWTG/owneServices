using Bunit;
using SysColor = System.Drawing.Color;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnThemeProviderTest : BunitTestContext
{
	[Test]
	public void WtgThemeProvider_RenderTest()
	{
		// Act
		var cut = RenderComponent<CwnThemeProvider>();

		// Assert
		cut.MarkupMatches(string.Empty);
	}

	[Test]
	public void WtgThemeProvider_CustomTheme_BackgroundTest()
	{
		// Arrange
		var theme = new Theme
		{
			Palette = new Palette
			{
				AppBarBackgroundColor = SysColor.Magenta,
			}
		};

		// Act
		var cut = RenderComponent<CwnThemeProvider>(
			parameters => parameters.Add(p => p.Theme, theme));

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(cut.Markup, Does.StartWith("<style>"));
			Assert.That(cut.Markup, Does.EndWith("</style>"));
			Assert.That(cut.Markup, Does.Contain(".cwn-theme--custom"));
			Assert.That(cut.Markup, Does.Contain("--s-brand-bg-default:rgba(255, 0, 255, 1);"));
			Assert.That(cut.Markup, Does.Contain("--s-brand-txt-inv-hover:rgba(255, 0, 255, 1);"));
			Assert.That(cut.Markup, Does.Contain("--s-brand-txt-inv-active:rgba(255, 0, 255, 1);"));
		});
	}

	[Test]
	public void WtgThemeProvider_CustomTheme_TextColorTest()
	{
		// Arrange
		var theme = new Theme
		{
			Palette = new Palette
			{
				AppBarTextColor = SysColor.Magenta,
			}
		};

		// Act
		var cut = RenderComponent<CwnThemeProvider>(
			parameters => parameters.Add(p => p.Theme, theme));

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(cut.Markup, Does.StartWith("<style>"));
			Assert.That(cut.Markup, Does.EndWith("</style>"));
			Assert.That(cut.Markup, Does.Contain(".cwn-theme--custom"));
			Assert.That(cut.Markup, Does.Contain("--s-brand-txt-inv-default:rgba(255, 0, 255, 1);"));
		});
	}
}
