using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignContactNumberFilter))]
	public class CampaignContactNumberFilterTest : ModuleTextFilterTest
	{
		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CampaignContactNumberFilter("Test", DummyBizoSchema.Z0_Decimal);
		}
	}
}
