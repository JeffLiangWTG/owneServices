using Bunit;
using static Bunit.ComponentParameterFactory;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnTextTest : BunitTestContext
{
	[Test]
	public void CwnText_RenderTest()
	{
		// Act
		var cut = RenderComponent<CwnText>();

		// Assert
		cut.MarkupMatches("<p class=\"wtg-text wtg-text-body1\" />");
	}

	[Test]
	public void CwnText_RenderWithChieldContentTest()
	{
		// Act
		var cut = RenderComponent<CwnText>(parameters => parameters.AddChildContent("Text content"));

		// Assert
		cut.MarkupMatches("<p class=\"wtg-text wtg-text-body1\">Text content</p>");
	}

	[Test]
	public void CwnText_DefaultPropertyValuesTest()
	{
		// Act
		var cut = RenderComponent<CwnText>();

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(cut.Instance.Typo, Is.EqualTo(ElementType.body1));
			Assert.That(cut.Instance.Align, Is.EqualTo(Align.Inherit));
			Assert.That(cut.Instance.Color, Is.Null);
			Assert.That(cut.Instance.HtmlTag, Is.Null);
		});
	}

	[TestCase(ElementType.h1, "<h1 class:ignore />")]
	[TestCase(ElementType.h2, "<h2 class:ignore />")]
	[TestCase(ElementType.h3, "<h3 class:ignore />")]
	[TestCase(ElementType.h4, "<h4 class:ignore />")]
	[TestCase(ElementType.h5, "<h5 class:ignore />")]
	[TestCase(ElementType.h6, "<h6 class:ignore />")]
	[TestCase(ElementType.body1, "<p class:ignore />")]
	[TestCase(ElementType.body2, "<p class:ignore />")]
	[TestCase(ElementType.subtitle1, "<p class:ignore />")]
	[TestCase(ElementType.subtitle2, "<p class:ignore />")]
	[TestCase(ElementType.caption, "<span class:ignore />")]
	public void CwnText_TypoTest(ElementType input, string expected)
	{
		// Arange
		var typo = Parameter(nameof(CwnText.Typo), input);

		// Act
		var cut = RenderComponent<CwnText>(typo);

		// Assert
		cut.MarkupMatches(expected);
	}

	[TestCase(Align.Inherit, "")]
	[TestCase(Align.Left, "wtg-text-align-left")]
	[TestCase(Align.Right, "wtg-text-align-right")]
	[TestCase(Align.Center, "wtg-text-align-center")]
	[TestCase(Align.Justify, "wtg-text-align-justify")]
	public void CwnText_AlignTest(Align input, string expected)
	{
		// Arange
		var align = Parameter(nameof(CwnText.Align), input);

		// Act
		var cut = RenderComponent<CwnText>(align);

		// Assert
		if (string.IsNullOrEmpty(expected))
		{
			Assert.That(cut.Markup, Does.Not.Contain("wtg-text-align-"));
		}
		else
		{
			Assert.That(cut.Markup, Does.Contain(expected));
		}
	}

	[TestCase(Color.Primary, "wtg-text-primary")]
	[TestCase(Color.Brand, "wtg-text-brand")]
	[TestCase(Color.Warning, "wtg-text-warning")]
	[TestCase(Color.Info, "wtg-text-info")]
	[TestCase(Color.Error, "wtg-text-error")]
	public void CwnText_ColorTest(Color input, string expected)
	{
		// Arange
		var color = Parameter(nameof(CwnText.Color), input);

		// Act
		var cut = RenderComponent<CwnText>(color);

		// Assert
		Assert.That(cut.Markup, Does.Contain(expected));
	}
}
