using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnCalloutTest : BunitTestContext
{
	[Test]
	public void CwnCallout_RenderTest()
	{
		var cut = RenderComponent<CwnCallout>();

		var callout = cut.Find(".cwn-callout");
		Assert.That(callout.GetAttribute("class"), Does.Contain("cwn-callout--info"));

		var icon = cut.Find(".cwn-icon");
		Assert.That(icon.GetAttribute("class"), Does.Contain("cwn-callout__icon--info"));
	}

	[Test]
	public void CwnCallout_ChildContentTest()
	{
		var cut = RenderComponent<CwnCallout>(parameter => parameter.AddChildContent("My content"));

		var uut = cut.Find(".cwn-callout__content");

		Assert.That(uut.TextContent, Is.EqualTo("My content"));
	}

	[Test]
	public void CwnCallout_ChildContent_WhenClassContentTest()
	{
		var cut = RenderComponent<CwnCallout>(parameter => parameter
			.AddChildContent("Any content")
			.Add(c => c.ClassContent, "custom-content-class"));

		var uut = cut.Find(".cwn-callout__content");

		Assert.That(uut.TextContent, Is.EqualTo("Any content"));
		Assert.That(uut.GetAttribute("class"), Does.Contain("custom-content-class"));
	}

	[TestCase(CalloutType.Info, "cwn-callout--info", "cwn-icon--status-info", "cwn-callout__icon--info")]
	[TestCase(CalloutType.Error, "cwn-callout--error", "cwn-icon--status-critical", "cwn-callout__icon--error")]
	[TestCase(CalloutType.Warning, "cwn-callout--warning", "cwn-icon--status-warning", "cwn-callout__icon--warning")]
	public void CwnCallout_TypeTest(CalloutType type, string expectedClass, string expectedIconClass, string expectedIconCalloutClass)
	{
		var cut = RenderComponent<CwnCallout>(parameter => parameter.Add(p => p.Type, type));

		var callout = cut.Find(".cwn-callout");
		Assert.That(callout.GetAttribute("class"), Does.Contain(expectedClass));

		var icon = cut.Find(".cwn-icon");
		Assert.That(icon.GetAttribute("class"), Does.Contain(expectedIconClass));
		Assert.That(icon.GetAttribute("class"), Does.Contain(expectedIconCalloutClass));
	}
}
