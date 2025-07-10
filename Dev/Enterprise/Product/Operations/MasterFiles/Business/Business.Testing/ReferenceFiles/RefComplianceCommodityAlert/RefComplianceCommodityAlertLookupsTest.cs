using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefComplianceCommodityAlertLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTradeDirections()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			AssertExpectResult(new[] { ("IMP", "Import"), ("EXP", "Export") }, alert.Lookups.TradeDirections);
		}

		public void TestAlertTypes()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			AssertExpectResult(new[] { ("NOM", "Nomenclature Alert"), ("COM", "Commodity Alert"), ("LOC", "Location Alert") }, alert.Lookups.AlertTypes);
		}

		public void TestCommodityRiskStatus()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			AssertExpectResult(new[] { ("HSK", "High Risk"), ("PRS", "Possible Risk") }, alert.Lookups.CommodityRiskStatus);
		}

		static void AssertExpectResult((string Code, string Desc)[] expectedCodes, CodeDescriptionPairList pairs)
		{
			var results =
				pairs
				.ToList<CodeDescriptionPair>().Select(x => (x.Code, x.Description));
			AssertContainsExactElementsInAnyOrder(expectedCodes, results);
		}
	}
}
