using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignContactTextRangeFilter))]
	public class CampaignContactTextRangeFilterTest : ModuleTextFilterTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CampaignContactTextRangeFilter("Test", DummyBizoSchema.Z0_Code);
		}
	}
}
