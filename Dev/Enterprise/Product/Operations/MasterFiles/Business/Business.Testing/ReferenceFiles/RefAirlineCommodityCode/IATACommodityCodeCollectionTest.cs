using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(IATACommodityCodeCollection))]
	class IATACommodityCodeCollectionTest : ActiveBusinessObjectCollectionTestCase<IATACommodityCodeCollection>
	{
		public void TestAdditionalFilter()
		{
			RefAirlineCommodityCode iataCommodityCode = Factory.New<RefAirlineCommodityCode>();
			iataCommodityCode.RAC_AirlineID = "";

			RefAirlineCommodityCode airlineSpecificCommodityCode = Factory.New<RefAirlineCommodityCode>();
			airlineSpecificCommodityCode.RAC_AirlineID = "123";

			IATACommodityCodeCollection collection = new IATACommodityCodeCollection(Factory);

			AssertEquals("Should contain IATA commodity codes", true, collection.Contains(iataCommodityCode));
			AssertEquals("Should not contain airline specific commodity code", false, collection.Contains(airlineSpecificCommodityCode));
		}
	}
}
