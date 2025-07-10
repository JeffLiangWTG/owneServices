using Bunit;
using static Bunit.ComponentParameterFactory;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnAvatarTest : BunitTestContext
{
	[Test]
	public void CwnAvatar_RenderTest()
	{
		var cut = RenderComponent<CwnAvatar>();
		cut.MarkupMatches("<div role=\"img\" class=\"cwn-avatar cwn-avatar--medium\">");
	}

	[TestCase(Size.Small, "cwn-avatar--small")]
	[TestCase(Size.Medium, "cwn-avatar--medium")]
	[TestCase(Size.Large, "cwn-avatar--large")]
	public void CwnAvatar_SizeTest(Size size, string expected)
	{
		var parameter = Parameter(nameof(CwnAvatar.Size), size);
		var cut = RenderComponent<CwnAvatar>(parameter);
		Assert.That(cut.Instance.Size, Is.EqualTo(size));
		cut.MarkupMatches($"<div role=\"img\" class=\"cwn-avatar {expected}\">");
	}

	[Test]
	public void CwnAvatar_ChildContentTest()
	{
		var cut = RenderComponent<CwnAvatar>(parameters => parameters.AddChildContent<CwnImage>());
		Assert.That(cut.Find(".cwn-image"), Is.Not.Null);
	}
}
