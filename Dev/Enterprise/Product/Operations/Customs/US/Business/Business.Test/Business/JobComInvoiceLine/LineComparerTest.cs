using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class LineComparerTest : TestCaseWithFactory
	{
		public void TestLineComparerChecksInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice3 = declaration.Invoices.AddNew();

			invoice1.JobComInvoiceLines.AddNew();
			invoice1.JobComInvoiceLines.AddNew();
			invoice1.JobComInvoiceLines.AddNew();

			invoice2.JobComInvoiceLines.AddNew();
			invoice2.JobComInvoiceLines.AddNew();
			invoice2.JobComInvoiceLines.AddNew();
			invoice2.JobComInvoiceLines.AddNew();

			invoice3.JobComInvoiceLines.AddNew();
			invoice3.JobComInvoiceLines.AddNew();

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("lines merging for US should include being based on Invoices", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}
	}
}
