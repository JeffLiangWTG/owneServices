using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SimplifiedEntryStrategyTest : CargoReleaseMergeStrategyTest
	{
		public void TestMergeSELines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice1 = declaration.Invoices.AddNew();
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			invoice1.JZ_OA_SellerAddress = seller.MainAddress.PK;
			var line1 = invoice1.InvoiceLines.AddNew();
			line1.JI_Tariff = "6206900040";
			var soldToParty = Factory.NewWithValidTestData<OrgHeader>();
			line1.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			var line2 = invoice1.InvoiceLines.AddNew();
			line2.JI_Tariff = "6206900040";
			line2.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertEquals("Entry Lines", 1, seEntry.MergedLines.Count);

			line2.JI_OA_SoldToPartyAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Entry Lines", 2, seEntry.MergedLines.Count);

			var invoice2 = declaration.Invoices.AddNew();
			var line3 = invoice2.InvoiceLines.AddNew();
			line3.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;
			line3.JI_OA_Seller = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Two Invoices with different Sellers, lines cannot be merged to one line", 3, seEntry.MergedLines.Count);
		}

		public void TestMergeSELinesForSeller()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice1 = declaration.Invoices.AddNew();
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			invoice1.JZ_OA_SellerAddress = seller.MainAddress.PK;
			var line1 = invoice1.InvoiceLines.AddNew();
			line1.JI_Tariff = "6206900040";

			var line2 = invoice1.InvoiceLines.AddNew();
			line2.JI_Tariff = "6206900040";

			var line3 = invoice1.InvoiceLines.AddNew();
			line3.JI_Tariff = "6206900040";
			line3.JI_OA_Seller = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			AssertEquals("With different Sellers, lines cannot be merged to one line", 2, seEntry.MergedLines.Count);
			AssertEquals(line1.CusEntryLine, line2.CusEntryLine);
			AssertNotEquals(line1.CusEntryLine, line3.CusEntryLine);
		}

		public void TestMergeWhenMessageTypeChanged()
		{
			var declaration = GetMergedSEDeclaration();
			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entry);
			Factory.Save();

			declaration.RunPreSaveValidation();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoErrors("PreCondition", declaration);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Import entry is deleted while merging", true, entry.IsDeleted);

			AssertEquals("EXP entry should have been created", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("EXP entry should have been created", CusEntryHeaderMessageTypeList.Codes.Export, declaration.ActiveEntryHeaders[0].CH_MessageType);
		}

		public override void TestSetForeignKeyToEntrySummary()
		{
			var declaration = GetMergedSEDeclaration();
			var ensEntries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			var seEntries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.ACECargoRelease);

			AssertEquals(1, ensEntries.Length);
			AssertEquals(1, seEntries.Length);
			AssertEquals("CH_CH_PrimeEntry is set", ensEntries[0].PK, seEntries[0].CH_CH_PrimeEntry);
		}

		public override void TestEntryNumberIsReassignedToCRLWhenENSIsDisable()
		{
			var declaration = GetMergedSEDeclaration();
			var ensEntries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			var seEntries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.ACECargoRelease);
			AssertEquals(1, ensEntries.Length);
			AssertEquals(1, seEntries.Length);
			ensEntries[0].EntryNumber = "01212322";
			AssertEquals(ensEntries[0], seEntries[0].RelatedENSEntry);
			AssertEquals("01212322", seEntries[0].EntryNumber);

			declaration.US_EnableENS = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ensEntries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			seEntries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.ACECargoRelease);
			AssertEquals(0, ensEntries.Length);
			AssertEquals(1, seEntries.Length);
			AssertNull(seEntries[0].RelatedENSEntry);
			AssertEquals("01212322", seEntries[0].EntryNumber);
		}

		public void TestWhenEntryHasMessageShouldNotCreate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice1 = declaration.Invoices.AddNew();
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			invoice1.JZ_OA_SellerAddress = seller.MainAddress.PK;
			var line1 = invoice1.InvoiceLines.AddNew();
			line1.JI_Tariff = "6206900040";

			var soldToParty = Factory.NewWithValidTestData<OrgHeader>();
			line1.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var seEntry1 = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry1.Messages.AddNew(typeof(MQEDIMessage));

			seEntry1.AllEntryLines.DeleteAll();
			invoice1.InvoiceLines.RemoveAndDeleteAll();
			var line2 = invoice1.InvoiceLines.AddNew();
			invoice1.JZ_OA_SellerAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			line2.JI_OA_SoldToPartyAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			line2.JI_Tariff = "6206900040";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var seEntry2 = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertEquals("Entry Lines", seEntry1.PK, seEntry2.PK);
		}

		public void TestEntryLinesWithProductExclusionAndExclusionNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();

			var line1 = declaration.InvoiceLines.AddNew();
			line1.US_ExclusionNumber = "ALU123456";
			line1.US_ProductExclusion = "02";
			var line2 = declaration.InvoiceLines.AddNew();
			line2.US_ExclusionNumber = "ALU123456";
			line2.US_ProductExclusion = "02";

			var line3 = declaration.InvoiceLines.AddNew();
			line3.US_ExclusionNumber = "ALU123456";
			line3.US_ProductExclusion = "03";
			var line4 = declaration.InvoiceLines.AddNew();
			line4.US_ExclusionNumber = "ALU144446";
			line4.US_ProductExclusion = "02";
			var line5 = declaration.InvoiceLines.AddNew();
			line5.US_ExclusionNumber = "ALU144445";
			line5.US_ProductExclusion = "03";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("5 Lines should have merged into 4", 4, entry.MergedLines.Count);

			AssertEquals(line2.US_ExclusionNumber, line1.US_ExclusionNumber);
			AssertEquals(line2.US_ProductExclusion, line1.US_ProductExclusion);

			AssertEquals(line1.US_ExclusionNumber, line3.US_ExclusionNumber);
			AssertNotEquals(line1.US_ProductExclusion, line3.US_ProductExclusion);

			AssertNotEquals(line1.US_ExclusionNumber, line4.US_ExclusionNumber);
			AssertEquals(line1.US_ProductExclusion, line4.US_ProductExclusion);

			AssertNotEquals(line1.US_ExclusionNumber, line5.US_ExclusionNumber);
			AssertNotEquals(line1.US_ProductExclusion, line5.US_ProductExclusion);
		}

		public void TestCreateStandAloneCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			declaration.US_EnableCRL = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(seEntry, declaration.ActiveEntryHeaders.SimplifiedEntry);

			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(seEntry, declaration.ActiveEntryHeaders.SimplifiedEntry);
			AssertEquals(1, declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines.Count);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
		}

		public void TestLinesWithSupTariffDoNotGetMergedWithLinesWithoutSupTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_MergeBy = "TRF";
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8407.90.9060";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8407.90.9060";
			invoiceLine2.US_SupTariff = "9802.00.5030";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(3, declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines.Count);

			invoiceLine.US_SupTariff = "9802.00.5030";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines.Count);
		}

		public void TestMergeLinesWithAdditionalSupTariffs()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_MergeBy = "TRF";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "3920992000";
			invoiceLine1.US_SupTariff = "99038801";
			invoiceLine1.SupFormattedAdditionalTariff1 = "99038802";
			invoiceLine1.SupFormattedAdditionalTariff2 = "99038501";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3920992000";
			invoiceLine2.US_SupTariff = "99038801";
			invoiceLine2.SupFormattedAdditionalTariff1 = "99038802";
			invoiceLine2.SupFormattedAdditionalTariff2 = "99038502";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(8, declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines.Count);

			invoiceLine2.SupFormattedAdditionalTariff2 = "99038501";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(4, declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines.Count);
		}

		protected override void AssertCargoReleaseKeyForLine(Customs.Business.MergeKey key, JobComInvoiceLine invoiceLine)
		{
			base.AssertCargoReleaseKeyForLine(key, invoiceLine);
			Assert(key.Contains(invoiceLine.JI_OA_SoldToPartyAddress));
			Assert(key.Contains(invoiceLine.InvoiceHeader.JZ_OA_SellerAddress));
			Assert(key.Contains(invoiceLine.US_ZoneStatus));
			Assert(key.Contains(invoiceLine.US_PrivilegedStatusDate));
			Assert(key.Contains(invoiceLine.US_FTZCurrentTariff));
			Assert(key.Contains(invoiceLine.JI_OA_ShipToPartyAddress));
		}

		protected override ImportMessageStatusList.MessageType CargoReleaseEntryType => ImportMessageStatusList.MessageType.ACECargoRelease;

		protected override CargoReleaseMergeStrategy GetMergeStrategy(JobDeclaration declaration) => new SimplifiedEntryStrategy(declaration);

		JobDeclaration GetMergedSEDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}
	}
}
