using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class ExportCustomsManifestHeaderFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var header1 = Factory.New<ExportCustomsManifestHeader>();
			var line1 = header1.Lines.AddNew();
			line1.FillWithValidTestData();

			var header2 = Factory.New<ExportCustomsManifestHeader>();
			var line2 = header2.Lines.AddNew();
			line2.FillWithValidTestData();
			Factory.Save();

			TestFetchForView(header1, header2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			return new ExportCustomsManifestHeaderCollectionForTesting(factory);
		}
	}
}
