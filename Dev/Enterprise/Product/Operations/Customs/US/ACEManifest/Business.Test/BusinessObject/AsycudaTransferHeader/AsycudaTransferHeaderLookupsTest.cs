using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AsycudaTransferHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrival = header.ArrivalHeaders.AddNew();
			var transferHeader = arrival.TransferHeaders.AddNew();
			AssertType<AsycudaTransferHeaderLookups>(transferHeader.Lookups);
		}

		public void TestBondedWarehouseCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var transferHeader = header.ArrivalHeaders.AddNew().TransferHeaders.AddNew();
			AssertEquals(typeof(BondedWarehouseCollection), transferHeader.Lookups.BondedWarehouseCollection.GetType());
		}

		public void TestCarrierCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var transferHeader = header.ArrivalHeaders.AddNew().TransferHeaders.AddNew();
			AssertEquals(typeof(USCarrierCombinedCollection), transferHeader.Lookups.CarrierCollection.GetType());
		}

		public void TestShippingProviders()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var transferHeader = header.ArrivalHeaders.AddNew().TransferHeaders.AddNew();
			AssertEquals(typeof(ShippingProviderCollection), transferHeader.Lookups.ShippingProviders.GetType());
		}
	}
}
