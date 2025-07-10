using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class VINInfoTypeProviderTest : TestCaseWithFactory
	{
		public void TestVehicleIdentificationNumber()
		{
			pack.APA_VINNumber = "123";
			provider = new VINInfoTypeProvider(pack.APA_VINNumber);

			AssertEquals("123", provider.VehicleIdentificationNumber.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();

			var bill = manifestHeader.Bills.AddNew();
			pack = bill.Packs.AddNew();
		}
		IVINInfoType provider;
		USExportAsycudaPack pack;
	}
}
