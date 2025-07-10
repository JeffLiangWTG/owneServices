using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GatewayServiceLevelCollection))]
	sealed class GatewayServiceLevelCollectionTest : ActiveBusinessObjectCollectionTestCase<GatewayServiceLevelCollection>
	{
		public void TestOnlyGatewayServiceLevelsAllowed()
		{
			var sl1 = Factory.NewWithValidTestData<RefServiceLevel>();
			sl1.RS_Code = "GW1";
			sl1.RS_IsGateway = true;

			var sl2 = Factory.NewWithValidTestData<RefServiceLevel>();
			sl2.RS_Code = "GW2";
			sl2.RS_IsGateway = false;

			var collection = GetCollectionToTest();

			AssertEquals(1, collection.Count);
			AssertEquals("GW1", collection[0].RS_Code);
		}
	}
}
