using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnRegionFlagTest : BunitTestContext
{
	[TestCase("au", "cwn-region-flag--au")]
	[TestCase("cn", "cwn-region-flag--cn")]
	[TestCase("us", "cwn-region-flag--us")]
	public void CwnIcon_RenderWithRegionFlag(string regionCode, string expectedClass)
	{
		// Act
		var cut = RenderComponent<CwnRegionFlag>(
			parameters => parameters.Add(p => p.RegionCode, regionCode));

		// Assert
		cut.MarkupMatches(@$"
			<span
				class=""
					cwn-region-flag
					{expectedClass}
					cwn-region-flag--medium
				""
				aria-hidden=""true""
				role=""img""
			/>");
	}
}
