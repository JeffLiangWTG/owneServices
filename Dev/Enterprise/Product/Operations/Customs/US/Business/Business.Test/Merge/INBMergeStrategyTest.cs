using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class INBMergeStrategyTest : Customs.Business.Testing.EntryCreationStrategyTest
	{
		public void TestMergeWhenMessageTypeChangesFromIMPToEXP()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_InbondType = EntryTypeList.Codes.ImmediateTransportation;
			declaration.US_EnableINB = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader impEntry = declaration.ActiveEntryHeaders[0];
			AssertEquals("PreCondition", CusEntryHeaderMessageTypeList.Codes.InBond, impEntry.CH_MessageType);

			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices[0].RunPreSaveValidation();
			AssertNoErrors("PreCondition", declaration);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Import entry is deleted while merging", true, impEntry.IsDeleted);

			AssertEquals("EXP entry should have been created", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("EXP entry should have been created", CusEntryHeaderMessageTypeList.Codes.Export, declaration.ActiveEntryHeaders[0].CH_MessageType);
		}

		public override void TestGetKeyForHeader()
		{
			Bill bill = declaration.Bills.AddNew();
			bill.ITNumber = "123456";

			Customs.Business.MergeKey expectedKey = new Customs.Business.MergeKey();
			expectedKey.Add((ZString)CusEntryHeaderMessageTypeList.Codes.InBond);
			Assert(strategy.GetKeyForHeader(invoiceLine) == expectedKey);
		}

		public void TestGetKeyForLine()
		{
			invoiceLine.JI_Tariff = "1234567890";
			Customs.Business.MergeKey key = strategy.GetKeyForLine(invoiceLine);
			Assert(key.Contains(invoiceLine.JI_Tariff));
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		INBMergeStrategy strategy;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			strategy = new INBMergeStrategy(declaration);
			declaration.Bills.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
	}
}
