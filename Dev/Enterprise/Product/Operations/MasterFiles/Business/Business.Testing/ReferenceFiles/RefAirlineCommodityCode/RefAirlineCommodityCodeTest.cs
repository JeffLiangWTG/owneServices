using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefAirlineCommodityCode))]
	public class RefAirlineCommodityCodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableNameCore()
		{
			var airlineCommodityCode = Factory.NewWithValidTestData<RefAirlineCommodityCode>();
			airlineCommodityCode.RAC_Code = "1234";
			airlineCommodityCode.RAC_Description = "Testing";
			airlineCommodityCode.RAC_AirlineID = "123";

			AssertEquals("Airline Commodity Code - 1234 - Testing", airlineCommodityCode.HumanReadableName);
		}
	}
}
