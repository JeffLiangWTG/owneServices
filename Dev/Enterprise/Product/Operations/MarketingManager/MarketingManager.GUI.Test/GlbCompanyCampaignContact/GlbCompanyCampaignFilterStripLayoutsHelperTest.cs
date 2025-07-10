using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignFilterStripLayoutsHelper))]
	public class GlbCompanyCampaignFilterStripLayoutsHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCurrentUserTablePrefix()
		{
			AssertEquals(GlbCompanyCampaignSchema.Constants.Prefix, new GlbCompanyCampaignFilterStripLayoutsHelper().CurrentUserTablePrefix);
		}

		public void TestCurrentUserPk()
		{
			GlbCompanyCampaignFilterStripLayoutsHelper helper = new GlbCompanyCampaignFilterStripLayoutsHelper();
			ZGuid parsed;
			ZGuid.TryParse("1417C207-0B3C-49D7-B195-E8D83A8A8740", out parsed);
			helper.BizObjPK = parsed;
			AssertEquals(helper.BizObjPK, helper.CurrentUserPk);
		}

		public void TestAdditionalEntityID()
		{
			GlbCompanyCampaignFilterStripLayoutsHelper helper = new GlbCompanyCampaignFilterStripLayoutsHelper();
			ZGuid.TryParse("f5a87b6f-6572-4bd8-827e-59d1b5c2e6a3", out helper.AdditionalObjPK);
			AssertEquals(helper.AdditionalObjPK, helper.AdditionalEntityID);
		}
	}
}
