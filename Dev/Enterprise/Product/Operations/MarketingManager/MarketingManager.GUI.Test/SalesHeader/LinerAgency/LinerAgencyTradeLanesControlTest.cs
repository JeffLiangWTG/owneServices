using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(LinerAgencyTradeLanesControl))]
	class LinerAgencyTradeLanesControlTest : TradeLanesControlBaseTest
	{
		protected override TradeLanesControl GetNewControlForTest()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.LinerAgency);
			return new LinerAgencyTradeLanesControl(product);
		}
	}
}
