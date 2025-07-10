using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ENSMergeStrategyTest : EntryCreationStrategyTest
	{
		public void TestAddKeyWithEmptyInvoiceLine()
		{
			var line = Factory.New<JobComInvoiceLine>();
			AssertNull(line.Declaration);

			var strategy = new ENSMergeStrategy(declaration);
			var generator = new ENSLineKeyGenerator();
			AssertNoExceptionThrown(() => generator.AddKey(new MergeKey(), line));
		}

		public void TestMergeWithCottonFeeExempt()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.US_CottonFeeExempt = YesNoDefaultList.Codes.No;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);
		}

		public void TestMergeLinesForSeller()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
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

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("With different Sellers, lines cannot be merged to one line", 2, declaration.ActiveEntryHeaders.EntrySummaryEntry.MergedLines.Count);
			AssertEquals(line1.CusEntryLine, line2.CusEntryLine);
			AssertNotEquals(line1.CusEntryLine, line3.CusEntryLine);
		}

		public void TestMergeWithDifferentCBMAData()
		{
			var manufacturer1 = Factory.New<OrgHeader>();
			manufacturer1.OH_FullName = "THE MANUFACTURER";

			var manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.OH_FullName = "THE MANUFACTURER 2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.US_TaxApply = TaxApplyList.Codes.Override;
			line1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			line1.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			line1.US_ControlledGroupName = "CGN23";
			line1.US_FlavorContentCreditInd = ZBool.True;
			line1.US_TTBRateDesignationCode = "T101";
			line1.US_CBMADefaultTaxRate = 10m;

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.US_TaxApply = TaxApplyList.Codes.Override;
			line2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			line2.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			line2.US_ControlledGroupName = "CGN23";
			line2.US_FlavorContentCreditInd = ZBool.True;
			line2.US_TTBRateDesignationCode = "T101";
			line2.US_CBMADefaultTaxRate = 10m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(line1.CusEntryLine, line2.CusEntryLine);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			line2.US_TaxApply = TaxApplyList.Codes.No;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.US_TaxApply = TaxApplyList.Codes.Override;
			line2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.S;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			line2.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			line2.US_ControlledGroupName = "CGN24";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.US_ControlledGroupName = "CGN23";
			line2.US_FlavorContentCreditInd = ZBool.False;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.US_FlavorContentCreditInd = ZBool.True;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.US_TTBRateDesignationCode = "ABC";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.US_CBMADefaultTaxRate = 9m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(line1.CusEntryLine, line2.CusEntryLine);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			line2.US_TaxApply = TaxApplyList.Codes.Override;
			line2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			line2.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			line2.US_ControlledGroupName = "CGN23";
			line2.US_FlavorContentCreditInd = ZBool.True;
			line2.US_TTBRateDesignationCode = "T101";
			line2.US_CBMADefaultTaxRate = 10m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.US_ControlledGroupName = "CGN24";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.US_ControlledGroupName = "CGN23";
			line2.US_FlavorContentCreditInd = ZBool.False;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);

			line2.US_FlavorContentCreditInd = ZBool.True;
			line2.US_TTBRateDesignationCode = "ABC";
			line2.US_CBMADefaultTaxRate = 10m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);
		}

		public void TestMergeWithDifferentTaxUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.US_TaxApply = TaxApplyList.Codes.Override;
			line1.US_TaxCode = "022";
			line1.US_TaxRateS = "$3.89/KG";

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.US_TaxApply = TaxApplyList.Codes.Override;
			line2.US_TaxCode = "022";
			line2.US_TaxRateS = "$3.89/K";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);
		}

		public void TestMergeWhenMessageTypeChangesFromIMPToEXP()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader impEntry = declaration.ActiveEntryHeaders[0];
			AssertEquals("PreCondition", CusEntryHeaderMessageTypeList.Codes.EntrySummary, impEntry.CH_MessageType);

			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices[0].RunPreSaveValidation();
			AssertNoErrors("PreCondition", declaration);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Import entry is deleted while merging", true, impEntry.IsDeleted);

			AssertEquals("EXP entry should have been created", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("EXP entry should have been created", CusEntryHeaderMessageTypeList.Codes.Export, declaration.ActiveEntryHeaders[0].CH_MessageType);
		}

		public void TestMergeLinesWithDifferentOverriddenTaxRates()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine1.US_TaxRate = 0.50m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine2.US_TaxRate = 0.50m;

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine3.US_TaxRate = 0.75m;

			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.US_TaxApply = ZString.Empty;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine4.CusEntryLine);
			AssertNotEquals(invoiceLine3.CusEntryLine, invoiceLine4.CusEntryLine);
		}

		public void TestMergeLinesWithDifferentCensusWarningOverrides()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.CensusWarningOverrides.AddNew("1", "A");

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.CensusWarningOverrides.AddNew("1", "B");

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.CensusWarningOverrides.AddNew("1", "B");

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestLineWithOverridenDutyDontGetMergedWithLinesWithoutOverridenDuty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_OverrideDuty = true;

			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("InvoiceLine1 and InvoiceLine3 get merged together", invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertNotEquals("InvoiceLine2 not merged with other entry line", invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine2.US_OverrideDuty = false;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("InvoiceLine1 and InvoiceLine3 get merged together", invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals("InvoiceLine2 and InvoiceLine3 get merged together", invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestInvoiceLineWithOverridenFeeDoesNotGetMergedWithOtherInvoiceLinesWithoutThem()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			FeeCusCodeData feeData1 = invoiceLine1.FeeCusCodes.AddNew();
			feeData1.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			feeData1.CY_FeeAmount = 10m;
			feeData1.CY_IsOverridden = false;

			feeData1 = invoiceLine1.FeeCusCodes.AddNew();
			feeData1.CY_Code = Core.Constants.USCustoms.FeeCodes.Mushroom;
			feeData1.CY_FeeAmount = 10m;
			feeData1.CY_IsOverridden = false;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			FeeCusCodeData feeData2 = invoiceLine2.FeeCusCodes.AddNew();
			feeData2.CY_Code = Core.Constants.USCustoms.FeeCodes.Mushroom;
			feeData2.CY_FeeAmount = 10m;
			feeData2.CY_IsOverridden = true;

			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			FeeCusCodeData feeData3 = invoiceLine3.FeeCusCodes.AddNew();
			feeData3.CY_Code = Core.Constants.USCustoms.FeeCodes.Mushroom;
			feeData3.CY_FeeAmount = 10m;
			feeData3.CY_IsOverridden = false;

			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			FeeCusCodeData feeData4 = invoiceLine4.FeeCusCodes.AddNew();
			feeData4.CY_Code = Core.Constants.USCustoms.FeeCodes.Mushroom;
			feeData4.CY_FeeAmount = 20m;
			feeData4.CY_IsOverridden = false;

			feeData4 = invoiceLine4.FeeCusCodes.AddNew();
			feeData4.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			feeData4.CY_FeeAmount = 30m;
			feeData4.CY_IsOverridden = false;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine4.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestSelectedRateTypeForFeeIsTakenIntoAccount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			FeeCusCodeData feeData1 = invoiceLine1.FeeCusCodes.AddNew();
			feeData1.CY_Code = Core.Constants.USCustoms.FeeCodes.Mushroom;
			feeData1.CY_FeeAmount = 10m;
			feeData1.CY_SelectedRateType = "";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			FeeCusCodeData feeData2 = invoiceLine2.FeeCusCodes.AddNew();
			feeData2.CY_Code = Core.Constants.USCustoms.FeeCodes.Mushroom;
			feeData2.CY_FeeAmount = 10m;
			feeData2.CY_SelectedRateType = "P";

			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			FeeCusCodeData feeData3 = invoiceLine3.FeeCusCodes.AddNew();
			feeData3.CY_Code = Core.Constants.USCustoms.FeeCodes.Mushroom;
			feeData3.CY_FeeAmount = 10m;
			feeData3.CY_SelectedRateType = "S";

			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			FeeCusCodeData feeData4 = invoiceLine4.FeeCusCodes.AddNew();
			feeData4.CY_Code = Core.Constants.USCustoms.FeeCodes.Mushroom;
			feeData4.CY_FeeAmount = 20m;
			feeData4.CY_SelectedRateType = "S";

			feeData4 = invoiceLine4.FeeCusCodes.AddNew();
			feeData4.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			feeData4.CY_FeeAmount = 30m;
			feeData4.CY_SelectedRateType = "";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine3.CusEntryLine, invoiceLine4.CusEntryLine);
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
		}

		public void TestValuationDateDifferenceDoesNotMakeSeparateEntries()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.US_DateOfExport = new ZDateTime(2008, 1, 1);
			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.US_DateOfExport = new ZDateTime(2008, 1, 2);
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("There should be only one entry. ExportDate is reported at the line level", 1, declaration.ActiveEntryHeaders.Count);

			//it could be cloned from other countries' invoice etc
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2008, 1, 1);
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2008, 1, 2);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("There should be only one entry. ValuationDate(ExportDate) is reported at the line level", 1, declaration.ActiveEntryHeaders.Count);
		}

		public void TestManageUS_CL_ParentLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertNotNull("JI_ParentID is set", invoiceLine2.ParentTariffLine);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("one entry header", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryLine entryLine1 = invoiceLine1.CusEntryLine;
			CusEntryLine entryLine2 = invoiceLine2.CusEntryLine;
			AssertEquals("entryLine2's US_CL_ParentLine is set", entryLine1, entryLine2.ParentLine);

			invoiceLine2.JI_ParentID = ZGuid.Empty;
			invoiceLine1.Delete();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("US_CL_ParentLine is removed", ZGuid.Empty, invoiceLine2.CusEntryLine.US_CL_ParentLine);
		}

		public void TestDoNotMergeThisLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();

			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.US_SecondarySPI = "";
			invoiceLine2.US_SecondarySPI = "";
			invoiceLine2.JI_ParentID = ZGuid.Empty;
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine2.JI_ParentID = ZGuid.Empty;
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.JI_Tariff = ZString.Empty;
			invoiceLine1.US_ADDDepositValue = 10m;
			invoiceLine2.US_CVDDepositValue = 10m;
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.US_ADDDepositValue = ZDecimal.Zero;
			invoiceLine2.US_CVDDepositValue = ZDecimal.Zero;
			invoiceLine1.PSTLines.AddNew();
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.PSTLines.RemoveAndDeleteAll();
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.ACE_FDALines.AddNew();
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.ACE_FDALines.RemoveAndDeleteAll();
			invoiceLine1.NHTSALines.AddNew();
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.ATFLines.AddNew();
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.ATFLines.RemoveAndDeleteAll();
			invoiceLine1.FWSHeaders.AddNew();
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.FWSHeaders.RemoveAndDeleteAll();
			invoiceLine1.USHFCHeaders.AddNew();
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.USHFCHeaders.RemoveAndDeleteAll();
			invoiceLine1.FishingInformations.AddNew();
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));

			invoiceLine1.FishingInformations.RemoveAndDeleteAll();
			invoiceLine1.MiningInformations.AddNew();
			AssertEquals("DoNotMergeThisLine", true, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine1));
			AssertEquals("DoNotMergeThisLine", false, ENSMergeStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine2));
		}

		public void TestGenerateKeyForACELinesSetIndicator()
		{
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_SetInd, "X", "V", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_SetInd, "X", "X", false);// Two X's not merged together
		}

		public void TestGenerateKeyForACELinesADDDetails()
		{
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_ADCVDStat, "Y", "N", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_ADCVDStat, "Y", "Y", true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_ADDDecID, "1", "2", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_ADDDecID, "1", "1", true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_ADDDepositRateIndicator, "A", "S", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_ADDDepositRateIndicator, "A", "A", true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_ADDQty, 1m, 2m, true);
		}

		public void TestGenerateKeyForACELinesCVDDetails()
		{
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_CVDDepositRateIndicator, "A", "S", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_CVDDepositRateIndicator, "A", "A", true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_CVDQty, 1m, 2m, true);
		}

		public void TestGenerateKeyForAluminumSmeltAndCastCountryDetails()
		{
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_Prim_NA, true, false, false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_Prim_NA, true, true, true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKPrimCtry, "RU", "CN", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKPrimCtry, "SG", "SG", true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_Sec_NA, true, false, false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_Sec_NA, true, true, true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKSecCtry, "RU", "CN", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKSecCtry, "SG", "SG", true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKCastCtry, "RU", "CN", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKCastCtry, "SG", "SG", true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKCertOrigin, "RU", "CN", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKCertOrigin, "SG", "SG", true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKMeltCtry, "RU", "CN", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_RN_NKMeltCtry, "SG", "SG", true);
		}

		public void TestGenerateKeyForADCVDCert()
		{
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_ADD_Cert, true, false, false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_ADD_Cert, true, true, true);
		}

		public void TestGenerateKeyForACELinesPartyDetails()
		{
			OrgHeader party1 = Factory.New<OrgHeader>();
			OrgHeader party2 = Factory.New<OrgHeader>();

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.JI_OA_SoldToPartyAddress, party1.MainAddress.PK, party2.MainAddress.PK, false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.JI_OA_SoldToPartyAddress, party1.MainAddress.PK, party1.MainAddress.PK, true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.JI_OA_ExporterAddress, party1.MainAddress.PK, party2.MainAddress.PK, false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.JI_OA_ExporterAddress, party1.MainAddress.PK, party1.MainAddress.PK, true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress, party1.MainAddress.PK, party2.MainAddress.PK, false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress, party1.MainAddress.PK, party1.MainAddress.PK, true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress, party1.MainAddress.PK, party2.MainAddress.PK, false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress, party1.MainAddress.PK, party1.MainAddress.PK, true);
		}

		public void TestGenerateKeyForACELinesHFC()
		{
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_HFCInd, "D", "C", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_HFCInd, "D", "D", true);

			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_HFCDisclaimReason, "A", "B", false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_HFCDisclaimReason, "A", "A", true);
		}

		public void TestGenerateKeyForForACE_US_SchDLoading()
		{
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_SchDLoading, "3201", "3201", true);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_SchDLoading, "3201", "3901", false);
		}

		public void TestGenerateKeyForACEPGAs_PST()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_PSTIndicator = OGAIndicatorList.Codes.Declared;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_PSTIndicator = OGAIndicatorList.Codes.Declared;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_PSTIndicator = ZString.Empty;
			invoiceLine2.US_PSTIndicator = ZString.Empty;
			invoiceLine3.US_PSTIndicator = ZString.Empty;

			invoiceLine1.US_PSTDisclaimReason = "A";
			invoiceLine2.US_PSTDisclaimReason = "B";
			invoiceLine3.US_PSTDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_CPSC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_CPSCDisclaimReason = "A";
			invoiceLine2.US_CPSCDisclaimReason = "B";
			invoiceLine3.US_CPSCDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_AMS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_AMSDisclaimProgram = "A";
			invoiceLine2.US_AMSDisclaimProgram = "B";
			invoiceLine3.US_AMSDisclaimProgram = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_APHIS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_APHISInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_APHISInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_APHISInd = ZString.Empty;
			invoiceLine2.US_APHISInd = ZString.Empty;
			invoiceLine3.US_APHISInd = ZString.Empty;

			var aphisHeader = invoiceLine1.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			aphisHeader = invoiceLine2.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			aphisHeader = invoiceLine2.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_FDA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_FDADisclaimReason = "A";
			invoiceLine2.US_FDADisclaimReason = "B";
			invoiceLine3.US_FDADisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_FSIS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_FSISDisclaimReason = "A";
			invoiceLine2.US_FSISDisclaimReason = "B";
			invoiceLine3.US_FSISDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_FSISDisclaimReason = ZString.Empty;
			invoiceLine2.US_FSISDisclaimReason = ZString.Empty;
			invoiceLine3.US_FSISDisclaimReason = ZString.Empty;

			invoiceLine1.US_FSISInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_FSISInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_Lacey()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_LaceyDisclaimReason = "A";
			invoiceLine2.US_LaceyDisclaimReason = "B";
			invoiceLine3.US_LaceyDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_LaceyDisclaimReason = ZString.Empty;
			invoiceLine2.US_LaceyDisclaimReason = ZString.Empty;
			invoiceLine3.US_LaceyDisclaimReason = ZString.Empty;

			invoiceLine1.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_NHTSA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_NHTDisclaimReason = "A";
			invoiceLine2.US_NHTDisclaimReason = "B";
			invoiceLine3.US_NHTDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_NHTDisclaimReason = ZString.Empty;
			invoiceLine2.US_NHTDisclaimReason = ZString.Empty;
			invoiceLine3.US_NHTDisclaimReason = ZString.Empty;

			invoiceLine1.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_NMFS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_NMFS370Ind = ZString.Empty;
			invoiceLine2.US_NMFS370Ind = ZString.Empty;
			invoiceLine3.US_NMFS370Ind = ZString.Empty;

			invoiceLine1.US_NMFS370DisclaimReason = "A";
			invoiceLine2.US_NMFS370DisclaimReason = "B";
			invoiceLine3.US_NMFS370DisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_NMFS370DisclaimReason = ZString.Empty;
			invoiceLine2.US_NMFS370DisclaimReason = ZString.Empty;
			invoiceLine3.US_NMFS370DisclaimReason = ZString.Empty;

			invoiceLine1.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_NMFSAMRInd = ZString.Empty;
			invoiceLine2.US_NMFSAMRInd = ZString.Empty;
			invoiceLine3.US_NMFSAMRInd = ZString.Empty;

			invoiceLine1.US_NMFSAMRDisclaimReason = "A";
			invoiceLine2.US_NMFSAMRDisclaimReason = "B";
			invoiceLine3.US_NMFSAMRDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_NMFSAMRDisclaimReason = ZString.Empty;
			invoiceLine2.US_NMFSAMRDisclaimReason = ZString.Empty;
			invoiceLine3.US_NMFSAMRDisclaimReason = ZString.Empty;

			invoiceLine1.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_NMFSHMSInd = ZString.Empty;
			invoiceLine2.US_NMFSHMSInd = ZString.Empty;
			invoiceLine3.US_NMFSHMSInd = ZString.Empty;

			invoiceLine1.US_NMFSHMSDisclaimReason = "A";
			invoiceLine2.US_NMFSHMSDisclaimReason = "B";
			invoiceLine3.US_NMFSHMSDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_NMFSHMSDisclaimReason = ZString.Empty;
			invoiceLine2.US_NMFSHMSDisclaimReason = ZString.Empty;
			invoiceLine3.US_NMFSHMSDisclaimReason = ZString.Empty;

			invoiceLine1.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			invoiceLine3.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_ODS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_ODSDisclaimReason = "A";
			invoiceLine2.US_ODSDisclaimReason = "B";
			invoiceLine3.US_ODSDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_ODSDisclaimReason = ZString.Empty;
			invoiceLine2.US_ODSDisclaimReason = ZString.Empty;
			invoiceLine3.US_ODSDisclaimReason = ZString.Empty;

			invoiceLine1.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_ODSInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertNotEquals(invoiceLine1.CusEntryLine.PK, invoiceLine3.CusEntryLine.PK);
		}

		public void TestGenerateKeyForACEPGAs_DDTC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_DDTCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_DDTCInd = ZString.Empty;
			invoiceLine2.US_DDTCInd = ZString.Empty;
			invoiceLine3.US_DDTCInd = ZString.Empty;

			invoiceLine1.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_ATFInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_ATFInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_TSCA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_TSCACertification = "A";
			invoiceLine2.US_TSCACertification = "B";
			invoiceLine3.US_TSCACertification = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_TSCACertification = ZString.Empty;
			invoiceLine2.US_TSCACertification = ZString.Empty;
			invoiceLine3.US_TSCACertification = ZString.Empty;

			invoiceLine1.US_TSCADisclaimReason = "A";
			invoiceLine2.US_TSCADisclaimReason = "B";
			invoiceLine3.US_TSCADisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_TSCADisclaimReason = ZString.Empty;
			invoiceLine2.US_TSCADisclaimReason = ZString.Empty;
			invoiceLine3.US_TSCADisclaimReason = ZString.Empty;

			invoiceLine1.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertNotEquals(invoiceLine1.CusEntryLine.PK, invoiceLine3.CusEntryLine.PK);
		}

		public void TestGenerateKeyForACEPGAs_TTB()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_TTBInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_TTBInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_VNE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_TTBDisclaimReason = "A";
			invoiceLine2.US_TTBDisclaimReason = "B";
			invoiceLine3.US_TTBDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_TTBDisclaimReason = ZString.Empty;
			invoiceLine2.US_TTBDisclaimReason = ZString.Empty;
			invoiceLine3.US_TTBDisclaimReason = ZString.Empty;

			invoiceLine1.US_VNEDisclaimReason = "A";
			invoiceLine2.US_VNEDisclaimReason = "B";
			invoiceLine3.US_VNEDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_VNEDisclaimReason = ZString.Empty;
			invoiceLine2.US_VNEDisclaimReason = ZString.Empty;
			invoiceLine3.US_VNEDisclaimReason = ZString.Empty;

			invoiceLine1.US_VNEInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_VNEInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACEPGAs_FWS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_FWSDisclaimReason = "A";
			invoiceLine2.US_FWSDisclaimReason = "B";
			invoiceLine3.US_FWSDisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.US_FWSDisclaimReason = ZString.Empty;
			invoiceLine2.US_FWSDisclaimReason = ZString.Empty;
			invoiceLine3.US_FWSDisclaimReason = ZString.Empty;

			invoiceLine1.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_FWSInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGenerateKeyForACELinesLicenseAndPermits()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.LicenceAndPermits.AddNew("A", "1");
			invoiceLine1.LicenceAndPermits.AddNew("B", "1");

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.LicenceAndPermits.AddNew("A", "1");

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.LicenceAndPermits.AddNew("B", "1");
			invoiceLine3.LicenceAndPermits.AddNew("A", "1");

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
		}

		public override void TestGetKeyForHeader()
		{
			MergeKey key = strategy.GetKeyForHeader(invoiceLine);
			AssertEquals("Should not have FIRMS code", false, key.Contains(new ZString("Z210")));
		}

		public void TestCountervailingValueOverride()
		{
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			MergeKey key1 = strategy.GetKeyForLine(invoiceLine1);
			MergeKey key2 = strategy.GetKeyForLine(invoiceLine2);
			Assert("Precondition", key1 == key2);
			invoiceLine1.US_CVDDepositValue = 100m;
			invoiceLine2.US_CVDDepositValue = 100m;
			key1 = strategy.GetKeyForLine(invoiceLine1);
			key2 = strategy.GetKeyForLine(invoiceLine2);
			Assert("Keys are same", key1 != key2);
		}

		public void TestAntidumpingValueOverride()
		{
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			MergeKey key1 = strategy.GetKeyForLine(invoiceLine1);
			MergeKey key2 = strategy.GetKeyForLine(invoiceLine2);
			Assert("Precondition", key1 == key2);
			invoiceLine1.US_ADDDepositValue = 100m;
			invoiceLine2.US_ADDDepositValue = 100m;
			key1 = strategy.GetKeyForLine(invoiceLine1);
			key2 = strategy.GetKeyForLine(invoiceLine2);
			Assert("Keys are same", key1 != key2);
		}

		public void TestMergeLeavesXVEntriesInCorrectOrder()
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceLine x11 = invoice.JobComInvoiceLines.AddNew();
			x11.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine v11 = invoice.JobComInvoiceLines.AddNew();
			v11.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			JobComInvoiceLine v12 = invoice.JobComInvoiceLines.AddNew();
			v12.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			JobComInvoiceLine x21 = invoice.JobComInvoiceLines.AddNew();
			x21.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine v21 = invoice.JobComInvoiceLines.AddNew();
			v21.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			JobComInvoiceLine v22 = invoice.JobComInvoiceLines.AddNew();
			v22.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(7, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals(invoiceLine, declaration.CustomsEntryHeaders[0].MergedLines[0].RandomLine);
			AssertEquals(x11, declaration.CustomsEntryHeaders[0].MergedLines[1].RandomLine);
			AssertEquals(v11, declaration.CustomsEntryHeaders[0].MergedLines[2].RandomLine);
			AssertEquals(v12, declaration.CustomsEntryHeaders[0].MergedLines[3].RandomLine);

			AssertEquals(x21, declaration.CustomsEntryHeaders[0].MergedLines[4].RandomLine);
			AssertEquals(v21, declaration.CustomsEntryHeaders[0].MergedLines[5].RandomLine);
			AssertEquals(v22, declaration.CustomsEntryHeaders[0].MergedLines[6].RandomLine);
		}

		public void TestMergeWithLineTransactionsRelated()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.US_TransactionsRelated = YesNoDefaultList.Codes.No;

			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			line3.US_TransactionsRelated = YesNoDefaultList.Codes.No;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("3 Lines should have merged into 2", 2, entry.MergedLines.Count);
			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);
			AssertNotEquals(line1.CusEntryLine, line3.CusEntryLine);
			AssertEquals(line2.CusEntryLine, line3.CusEntryLine);
		}

		public void TestMergeWithLineUS_SchDLoading()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();

			var line1 = declaration.InvoiceLines.AddNew();
			line1.US_SchDLoading = "3902";

			var line2 = declaration.InvoiceLines.AddNew();
			line2.US_SchDLoading = "3902";

			var line3 = declaration.InvoiceLines.AddNew();
			line3.US_SchDLoading = "3901";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("3 Lines should have merged into 2", 2, entry.MergedLines.Count);
			AssertNotEquals(line1.CusEntryLine, line3.CusEntryLine);
			AssertEquals(line2.CusEntryLine, line1.CusEntryLine);
		}

		public void TestEntryLinesWithProductExclusionAndExclusionNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
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

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
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

		public void TestMergeLinesWithSupAdditionalTariffs()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

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
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(8, declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines.Count);

			invoiceLine2.SupFormattedAdditionalTariff2 = "99038501";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(4, declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines.Count);
		}

		public void TestGenerateKeyForACELinesDisclaimSanctions()
		{
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_DisclaimSanctions, true, false, false);
			AssertGenerateKeyForACELines(JobComInvoiceLine.Schema.US_DisclaimSanctions, true, true, true);
		}

		public void TestGenerateKeyForACEPGAs_DEA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_DEADisclaimReason = "A";
			invoiceLine2.US_DEADisclaimReason = "B";
			invoiceLine3.US_DEADisclaimReason = "A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestGetKeyForLine()
		{
			var key = strategy.GetKeyForLine(invoiceLine);
			Assert(key.Contains(invoice.PK));
			Assert(key.Contains(new ZString("")));

			Assert(key.Contains(invoiceLine.JI_Tariff));
			Assert(key.Contains(invoiceLine.US_UC_NKCountryOfOrigin));
			Assert(key.Contains(invoiceLine.US_ZoneStatus));
			Assert(key.Contains(invoiceLine.US_PrivilegedStatusDate));
			Assert(key.Contains(invoiceLine.US_IsNAFTANet));
			Assert(key.Contains(invoiceLine.US_PIRPRulingNo));
			Assert(key.Contains(invoiceLine.US_SPI));
			Assert(key.Contains(invoiceLine.US_UC_NKCountryOfExport));
			Assert(key.Contains(invoiceLine.US_DateOfExport));
			Assert(key.Contains(invoiceLine.US_SecondarySPI));
			Assert(key.Contains(invoiceLine.US_DateOfExportFromCountryOfOrigin));
			Assert(key.Contains(invoiceLine.US_VisaNo));
			Assert(key.Contains(invoiceLine.US_TextileCategoryNo));
			Assert(key.Contains(invoiceLine.US_VisaUQ));
			Assert(key.Contains(invoiceLine.US_AgricultureLicNo));
			Assert(key.Contains(invoiceLine.US_CottonCertificateNo));
			Assert(key.Contains(invoiceLine.US_SWPMIndicator));
			Assert(key.Contains(invoiceLine.US_CAExportCertificate));
			Assert(key.Contains(invoiceLine.US_WoolLicenceNo));
			Assert(key.Contains(invoiceLine.US_CBTPACertificateNo));
			Assert(key.Contains(invoiceLine.US_MiscPermitNo));
			Assert(key.Contains(invoiceLine.US_CVDCaseNo));
			Assert(key.Contains(invoiceLine.US_ADDCaseNo));
			Assert(key.Contains(invoiceLine.ManufacturerFallBackToSupplierNumber));
			Assert(key.Contains(invoiceLine.US_IsBondedCVD));
			Assert(key.Contains(invoiceLine.US_IsBondedADD));
			Assert(key.Contains(invoiceLine.US_FDAIndicator));
			Assert(key.Contains(invoiceLine.US_FCCIndicator));
			Assert(key.Contains(invoiceLine.US_DOTIndicator));
			Assert(key.Contains(invoiceLine.US_SelectedRateType));
			Assert(key.Contains(invoiceLine.US_DestinationState));
			Assert(key.Contains(invoiceLine.US_TransactionsRelated));
			Assert(key.Contains(invoice.US_TransactionsRelated));
			Assert(key.Contains(invoiceLine.US_SchDLoading));

			var keyWithoutAdditionalTariff = strategy.GetKeyForLine(invoiceLine);
			Assert(!keyWithoutAdditionalTariff.Contains(invoiceLine.PK));
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		ENSMergeStrategy strategy;
		DeclarationTestHelper helper;
		Bill bill;

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			helper = new DeclarationTestHelper(Factory);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_US_NKLocationOfGoods = "Z210";
			declaration.US_ITDate = new ZDateTime(2007, 5, 2, 2, 43, 23);

			strategy = new ENSMergeStrategy(declaration);
			bill = declaration.Bills.AddNew();
			bill.ITNumber = "IT123456";
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = helper.Consignee.PK;
			invoice.US_DateOfExportFromCountryOfOrigin = new ZDateTime(2007, 4, 10, 4, 43, 23);
			invoice.US_DateOfExport = new ZDateTime(2007, 4, 11, 3, 23, 43);
			invoice.US_UC_NKCountryOfExport = "NZ";
			invoice.US_DestinationState = "IL";
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1000100011";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = new ZDateTime(2007, 4, 12, 2, 32, 12);
			invoiceLine.US_IsNAFTANet = true;
			invoiceLine.US_PIRPRulingNo = "2342";
			invoiceLine.US_SPI = SpecialProgramList.Codes.BSharp;
			invoiceLine.US_SecondarySPI = PrimarySpecProgramIndicatorList.Codes.E;
			invoiceLine.US_VisaNo = "6334";
			invoiceLine.US_TextileCategoryNo = "325";
			invoiceLine.US_VisaUQ = "DF";
			invoiceLine.US_AgricultureLicNo = "345";
			invoiceLine.US_CottonCertificateNo = "564";
			invoiceLine.US_SWPMIndicator = SWPMList.Codes._1;
			invoiceLine.US_CAExportCertificate = "497";
			invoiceLine.US_WoolLicenceNo = "834";
			invoiceLine.US_CBTPACertificateNo = "01872";
			invoiceLine.US_MiscPermitNo = "723";
			invoiceLine.US_CVDCaseNo = "823";
			invoiceLine.US_ADDCaseNo = "9382";
			helper.Consignor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "69-9999999JC");
			invoiceLine.US_IsBondedCVD = ZBool.True;
			invoiceLine.US_IsBondedADD = ZBool.False;
			invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
		}

		void AssertGenerateKeyForACELines(string fieldNameInInvoiceLine, object value1, object value2, bool shouldBeMergedTogether)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1[fieldNameInInvoiceLine] = value1;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2[fieldNameInInvoiceLine] = value2;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3[fieldNameInInvoiceLine] = value1;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			if (shouldBeMergedTogether)
			{
				AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			}
			else
			{
				AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			}
		}
	}
}
