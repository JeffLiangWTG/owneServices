using Bunit;
using static Bunit.ComponentParameterFactory;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnDividerTest : BunitTestContext
{
	[TestCase(DividerType.FullWidth, "cwn-divider--full-width")]
	[TestCase(DividerType.Inset, "cwn-divider--inset")]
	[TestCase(DividerType.Middle, "cwn-divider--middle")]
	public void CwnDivider_HorizontalDividerTypeTest(DividerType dividerType, string expected)
	{
		var parameter = Parameter(nameof(CwnDivider.Vertical), false);
		var parameter2 = Parameter(nameof(CwnDivider.DividerType), dividerType);
		var cut = RenderComponent<CwnDivider>(parameter, parameter2);
		Assert.That(cut.Instance.Vertical, Is.False);
		Assert.That(cut.Instance.DividerType, Is.EqualTo(dividerType));
		cut.MarkupMatches($"<hr class=\"cwn-divider {expected}\" />");
	}

	[TestCase(DividerType.FullWidth, "cwn-divider--vertical")]
	[TestCase(DividerType.Inset, "cwn-divider--vertical cwn-divider--inset")]
	[TestCase(DividerType.Middle, "cwn-divider--vertical cwn-divider--middle")]
	public void CwnDivider_VerticalDividerTypeTest(DividerType dividerType, string expected)
	{
		var parameter = Parameter(nameof(CwnDivider.Vertical), true);
		var parameter2 = Parameter(nameof(CwnDivider.DividerType), dividerType);
		var cut = RenderComponent<CwnDivider>(parameter, parameter2);
		Assert.That(cut.Instance.Vertical, Is.True);
		Assert.That(cut.Instance.DividerType, Is.EqualTo(dividerType));
		cut.MarkupMatches($"<hr class=\"cwn-divider {expected}\" />");
	}
}
