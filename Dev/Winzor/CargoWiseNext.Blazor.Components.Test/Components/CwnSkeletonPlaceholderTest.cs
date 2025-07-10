using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnSkeletonPlaceholderTest : BunitTestContext
{
	[Test]
	public void CwnSkeletonPlaceholder_Renders()
	{
		// Act
		var cut = RenderComponent<CwnSkeletonPlaceholder>();

		// Assert
		cut.MarkupMatches(@"<div class=""cwn-skeleton-placeholder"" />");
	}

	[Test]
	public void CwnSkeletonPlaceholder_WhenWidthAndHeight()
	{
		// Act
		var cut = RenderComponent<CwnSkeletonPlaceholder>(parameters => parameters
			.Add(p => p.Width, "100px")
			.Add(p => p.Height, "50px"));

		// Assert
		Assert.That(cut.Find("div").GetAttribute("style"), Is.EqualTo("width:100px;height:50px;"));
	}

	[Test]
	public void CwnSkeletonPlaceholder_WhenBorderRadius()
	{
		// Act
		var cut = RenderComponent<CwnSkeletonPlaceholder>(parameters => parameters
			.Add(p => p.BorderRadius, "4px"));

		// Assert
		Assert.That(cut.Find("div").GetAttribute("style"), Is.EqualTo("border-radius:4px;"));
	}

	[Test]
	public void CwnSkeletonPlaceholder_WhenStyle()
	{
		// Act
		var cut = RenderComponent<CwnSkeletonPlaceholder>(parameters => parameters
			.Add(p => p.Style, "background-color: blue;"));

		// Assert
		Assert.That(cut.Find("div").GetAttribute("style"), Is.EqualTo("background-color: blue;"));
	}

	[Test]
	public void CwnSkeletonPlaceholder_WhenClass()
	{
		// Act
		var cut = RenderComponent<CwnSkeletonPlaceholder>(parameters => parameters
			.Add(p => p.Class, "cwn-custom-class"));

		// Assert
		var actual = cut.Find("div").GetAttribute("class");
		Assert.That(actual, Does.StartWith("cwn-skeleton-placeholder"));
		Assert.That(actual, Does.EndWith("cwn-custom-class"));
	}
}
