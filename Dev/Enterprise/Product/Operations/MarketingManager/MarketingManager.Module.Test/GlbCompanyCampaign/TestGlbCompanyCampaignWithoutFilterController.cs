using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(GlbCompanyCampaignWithoutFilterController))]
	public class TestGlbCompanyCampaignWithoutFilterController : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbCompanyCampaignWithoutFilter;
		}
	}
}
