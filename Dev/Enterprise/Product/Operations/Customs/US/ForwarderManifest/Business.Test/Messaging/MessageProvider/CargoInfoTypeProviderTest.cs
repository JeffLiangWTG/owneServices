using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class CargoInfoTypeProviderTest : TestCaseWithFactory
	{
		public void TestQuantity()
		{
			pack.APA_PackQty = 12;

			AssertEquals("12", provider.Quantity.Value);
		}

		public void TestQuantityUnitOfMeasure()
		{
			pack.APA_PackUQ = "KG";

			AssertEquals("KG", provider.QuantityUnitOfMeasure.Value);
		}

		public void TestCargoDescription()
		{
			pack.APA_GoodsDescription = "stuff";

			AssertEquals("stuff", provider.CargoDescription.Value);
		}

		public void TestMarksAndNumbers()
		{
			pack.APA_MarksAndNumbers = "stuff";

			AssertEquals("stuff", provider.MarksAndNumbers.Value);
		}

		public void TestCountryOfOrigin()
		{
			AssertEquals("US", provider.CountryOfOrigin.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();

			var bill = manifestHeader.Bills.AddNew();
			pack = bill.Packs.AddNew();

			provider = new CargoInfoTypeProvider(pack);
		}
		ICargoInfoType provider;
		USExportAsycudaPack pack;
	}
}
