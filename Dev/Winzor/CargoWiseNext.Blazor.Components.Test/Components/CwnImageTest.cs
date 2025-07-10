using Bunit;
using static Bunit.ComponentParameterFactory;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnImageTest : BunitTestContext
{
	[Test]
	public void CwnImage_RenderTest()
	{
		var cut = RenderComponent<CwnImage>();
		cut.MarkupMatches("<img class=\"cwn-image cwn-image--fill cwn-image--center\">");
	}

	[Test]
	public void CwnImage_SrcTest()
	{
		var parameter = Parameter(nameof(CwnImage.Src), "https://example.com/image.jpg");
		var cut = RenderComponent<CwnImage>(parameter);
		Assert.That(cut.Instance.Src, Is.EqualTo("https://example.com/image.jpg"));
		cut.MarkupMatches("<img src=\"https://example.com/image.jpg\" class=\"cwn-image cwn-image--fill cwn-image--center\">");
	}

	[Test]
	public void CwnImage_AltTest()
	{
		var parameter = Parameter(nameof(CwnImage.Alt), "Image alt text");
		var cut = RenderComponent<CwnImage>(parameter);
		Assert.That(cut.Instance.Alt, Is.EqualTo("Image alt text"));
		cut.MarkupMatches("<img alt=\"Image alt text\" class=\"cwn-image cwn-image--fill cwn-image--center\">");
	}

	[Test]
	public void CwnImage_SizeTest()
	{
		var parameter = ComponentParameter.CreateParameter(nameof(CwnImage.Width), 200);
		var parameter2 = ComponentParameter.CreateParameter(nameof(CwnImage.Height), 100);
		var cut = RenderComponent<CwnImage>(parameter, parameter2);
		Assert.That(cut.Instance.Width, Is.EqualTo(200));
		Assert.That(cut.Instance.Height, Is.EqualTo(100));
		cut.MarkupMatches("<img class=\"cwn-image cwn-image--fill cwn-image--center\" width=\"200\" height=\"100\">");
	}

	[TestCase(ObjectFit.None, "cwn-image--none")]
	[TestCase(ObjectFit.Cover, "cwn-image--cover")]
	[TestCase(ObjectFit.Contain, "cwn-image--contain")]
	[TestCase(ObjectFit.Fill, "cwn-image--fill")]
	[TestCase(ObjectFit.ScaleDown, "cwn-image--scale-down")]
	public void CwnImage_ObjectFitTest(ObjectFit objectFit, string expected)
	{
		var parameter = Parameter(nameof(CwnImage.ObjectFit), objectFit);
		var cut = RenderComponent<CwnImage>(parameter);
		Assert.That(cut.Instance.ObjectFit, Is.EqualTo(objectFit));
		cut.MarkupMatches($"<img class=\"cwn-image {expected} cwn-image--center\">");
	}

	[TestCase(ObjectPosition.Center, "cwn-image--center")]
	[TestCase(ObjectPosition.Top, "cwn-image--top")]
	[TestCase(ObjectPosition.Bottom, "cwn-image--bottom")]
	[TestCase(ObjectPosition.Left, "cwn-image--left")]
	[TestCase(ObjectPosition.LeftTop, "cwn-image--left-top")]
	[TestCase(ObjectPosition.LeftBottom, "cwn-image--left-bottom")]
	[TestCase(ObjectPosition.Right, "cwn-image--right")]
	[TestCase(ObjectPosition.RightTop, "cwn-image--right-top")]
	[TestCase(ObjectPosition.RightBottom, "cwn-image--right-bottom")]
	public void CwnImage_ObjectPositionTest(ObjectPosition objectPosition, string expected)
	{
		var parameter = Parameter(nameof(CwnImage.ObjectPosition), objectPosition);
		var cut = RenderComponent<CwnImage>(parameter);
		Assert.That(cut.Instance.ObjectPosition, Is.EqualTo(objectPosition));
		cut.MarkupMatches($"<img class=\"cwn-image cwn-image--fill {expected}\">");
	}
}
