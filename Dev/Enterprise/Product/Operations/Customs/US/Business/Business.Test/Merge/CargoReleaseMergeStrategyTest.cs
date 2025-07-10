using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class CargoReleaseMergeStrategyTest : ImportEntryCreationStrategyTest
	{
		public virtual void TestSetForeignKeyToEntrySummary()
		{
			SetupDataForMerging();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader[] ensEntries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			CusEntryHeader[] cargoReleaseEntries = declaration.ActiveEntryHeaders.GetEntryWithType(CargoReleaseEntryType);

			AssertEquals(1, ensEntries.Length);
			AssertEquals(1, cargoReleaseEntries.Length);
			AssertEquals("CH_CH_PrimeEntry is set", ensEntries[0].PK, cargoReleaseEntries[0].CH_CH_PrimeEntry);
		}

		public virtual void TestEntryNumberIsReassignedToCRLWhenENSIsDisable()
		{
			SetupDataForMerging();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader[] ensEntries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			CusEntryHeader[] crlEntries = declaration.ActiveEntryHeaders.GetEntryWithType(CargoReleaseEntryType);
			AssertEquals(1, ensEntries.Length);
			AssertEquals(1, crlEntries.Length);
			ensEntries[0].EntryNumber = "01212322";
			AssertEquals(ensEntries[0], crlEntries[0].RelatedENSEntry);
			AssertEquals("01212322", crlEntries[0].EntryNumber);

			declaration.US_EnableENS = false;
			declaration.DoMerge();
			ensEntries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			crlEntries = declaration.ActiveEntryHeaders.GetEntryWithType(CargoReleaseEntryType);
			AssertEquals(0, ensEntries.Length);
			AssertEquals(1, crlEntries.Length);
			AssertNull(crlEntries[0].RelatedENSEntry);
			AssertEquals("01212322", crlEntries[0].EntryNumber);
		}

		public void TestJE_MergeByIsConsidered()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			declaration.Invoices.AddNew();

			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader crlEntry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertEquals(2, crlEntry.MergedLines.Count);
		}

		public void TestGetKeyForLine()
		{
			SetupDataForMerging();
			AssertCargoReleaseKeyForLine(GetMergeStrategy(declaration).GetKeyForLine(invoiceLine), invoiceLine);
		}

		protected virtual void AssertCargoReleaseKeyForLine(Customs.Business.MergeKey key, JobComInvoiceLine invoiceLine)
		{
			Assert(key.Contains(invoiceLine.JI_Tariff));
			Assert(key.Contains(invoiceLine.US_UC_NKCountryOfOrigin));
			Assert(key.Contains(invoiceLine.ManufacturerFallBackToSupplierNumber));
			Assert(key.Contains(invoiceLine.JI_OA_ConsigneeAddress));
		}

		protected abstract ImportMessageStatusList.MessageType CargoReleaseEntryType { get; }

		protected abstract CargoReleaseMergeStrategy GetMergeStrategy(JobDeclaration declaration);

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected DeclarationTestHelper helper;
		protected Bill bill;

		protected virtual void SetupDataForMerging()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = helper.Consignor.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_ITDate = new ZDateTime(2007, 5, 2, 2, 43, 23);

			bill = declaration.Bills.AddNew();
			bill.ITNumber = "IT123456";
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = helper.Consignee.PK;
			invoice.US_DateOfExportFromCountryOfOrigin = new ZDateTime(2007, 4, 10, 4, 43, 23);
			invoice.US_DateOfExport = new ZDateTime(2007, 4, 11, 3, 23, 43);
			invoice.US_UC_NKCountryOfExport = "NZ";
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1000100011";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			helper = new DeclarationTestHelper(Factory);
			helper.Consignor.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "69-9999999JC", Core.Constants.CountryCodes.UnitedStates);
		}
	}
}
