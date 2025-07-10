using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNatures()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			var list = header.Lookups.Natures.GetAllCodes();

			AssertEquals(1, list.Length);
			AssertContainsExactElementsInAnyOrder(new[] { ShipmentTypeList.Codes.Export22 }, list);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			list = header.Lookups.Natures.GetAllCodes();

			AssertEquals(2, list.Length);
			AssertContainsExactElementsInAnyOrder(new[] { ShipmentTypeList.Codes.Export22, ShipmentTypeList.Codes.Import23 }, list);
		}
	}
}
