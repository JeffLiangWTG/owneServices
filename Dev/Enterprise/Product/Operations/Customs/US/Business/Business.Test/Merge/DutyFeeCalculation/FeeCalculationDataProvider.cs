using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FeeCalculationDataProviderTest : TestCaseWithFactory
	{
		public void TestCustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.FormalEntry;
			var dataProvider = (IFeeCalculationDataProvider)entry.EntryLines.FirstOrDefault();
			AssertEquals(1000m, dataProvider.CustomsValue);

			dataProvider = new FeeCalculationDataProvider(dataProvider, 2000m);
			AssertEquals(2000m, dataProvider.CustomsValue);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}
	}
}
