using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateCommodityDefaultingRuleCollection))]
	public class RateCommodityDefaultingRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<RateCommodityDefaultingRuleCollection>
	{
		public void TestUniqueConfigurations()
		{
			var rateCommodityDefaultingRuleCollection = new RateCommodityDefaultingRuleCollection();
			var rateCommmodityDefaultingRow1 = new RateCommodityDefaultingRule();
			var rateCommmodityDefaultingRow2 = new RateCommodityDefaultingRule();
			rateCommodityDefaultingRuleCollection.Add(rateCommmodityDefaultingRow1);
			rateCommodityDefaultingRuleCollection.Add(rateCommmodityDefaultingRow2);
			rateCommodityDefaultingRuleCollection.Add(rateCommmodityDefaultingRow1);
			rateCommmodityDefaultingRow1.RateCommodityCode = "GEN";
			rateCommmodityDefaultingRow2.RateCommodityCode = "GEN";
			rateCommmodityDefaultingRow1.TransportMode = Core.Constants.TransportModes.Sea;
			rateCommmodityDefaultingRow2.TransportMode = Core.Constants.TransportModes.Sea;

			AssertHasError("Duplicate Setting", rateCommmodityDefaultingRow1.RateCommodityCodeInfo, "Commodity default with identical values already exists");
			AssertHasError("Duplicate Setting", rateCommmodityDefaultingRow2.RateCommodityCodeInfo, "Commodity default with identical values already exists");

			rateCommmodityDefaultingRow1.TransportMode = Core.Constants.TransportModes.Air;

			AssertNoErrors("There are now no duplicates", rateCommmodityDefaultingRow1.RateCommodityCodeInfo);

			rateCommmodityDefaultingRow1.TransportMode = Core.Constants.TransportModes.Sea;
			rateCommmodityDefaultingRow1.RateCommodityCode = "GENL";

			AssertHasError("Duplicate Setting", rateCommmodityDefaultingRow1.RateCommodityCodeInfo, "Commodity default with identical values already exists");
			AssertHasError("Duplicate Setting", rateCommmodityDefaultingRow2.RateCommodityCodeInfo, "Commodity default with identical values already exists");

			rateCommmodityDefaultingRow1.TransportMode = Core.Constants.TransportModes.Air;

			AssertNoErrors("There are now no duplicates", rateCommmodityDefaultingRow1.RateCommodityCodeInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RateCommodityDefaultingRuleCollection GetCollectionToTest()
		{
			return new RateCommodityDefaultingRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RateCommodityDefaultingRule();
		}

		#endregion
	}
}

