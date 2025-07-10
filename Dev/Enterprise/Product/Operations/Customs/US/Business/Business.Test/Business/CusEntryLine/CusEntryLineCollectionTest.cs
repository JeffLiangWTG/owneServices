using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusEntryLineCollection))]
	sealed class CusEntryLineCollectionTest : Customs.Business.Testing.CusEntryLineCollectionAbstractTest<CusEntryLine, CusEntryLineCollection>
	{
		public void TestFindNonSecondaryLineByLineNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.AddSecondaryInvoiceLine();
			invoiceLine3.AddSecondaryInvoiceLine();
			invoiceLine3.AddSecondaryInvoiceLine();
			invoiceLine3.AddSecondaryInvoiceLine();
			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(invoiceLine1.CusEntryLine, entry.MergedLines.FindNonSecondaryLineByLineNumber(1));
			AssertEquals(invoiceLine2.CusEntryLine, entry.MergedLines.FindNonSecondaryLineByLineNumber(2));
			AssertEquals(invoiceLine3.CusEntryLine, entry.MergedLines.FindNonSecondaryLineByLineNumber(3));
			AssertEquals(invoiceLine4.CusEntryLine, entry.MergedLines.FindNonSecondaryLineByLineNumber(4));
		}

		public void TestFindByFormattedLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 10360;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = (short)50;
			AssertEquals("FindByFormattedLineNumber", entryLine, entryHeader.MergedLines.FindByFormattedLineNumber("H80"));
			AssertEquals("FindByFormattedLineNumber", entryLine2, entryHeader.MergedLines.FindByFormattedLineNumber("050"));
			AssertEquals("FindByFormattedLineNumber", null, entryHeader.MergedLines.FindByFormattedLineNumber("017"));
		}

		public void TestCustomSort()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryline1 = entry.MergedLines.AddNew();
			entryline1.CL_LineNumber = 2;
			entryline1.CL_AdValoremTariff = "2000000000";
			CusEntryLine entryline2 = entry.MergedLines.AddNew();
			entryline2.CL_LineNumber = 3;
			entryline2.CL_AdValoremTariff = "3000000000";
			CusEntryLine entryline3 = entry.MergedLines.AddNew();
			entryline3.CL_LineNumber = 1;
			entryline3.CL_AdValoremTariff = "1000000000";
			CusEntryLine entryline1Child1 = entry.MergedLines.AddNew();
			entryline1Child1.CL_LineNumber = 2;
			entryline1Child1.US_CL_ParentLine = entryline1.PK;
			entryline1Child1.CL_AdValoremTariff = "2020000000";
			invoiceLine2.JI_CL = entryline1Child1.PK;
			CusEntryLine entryline1Child2 = entry.MergedLines.AddNew();
			entryline1Child2.CL_LineNumber = 2;
			entryline1Child2.US_CL_ParentLine = entryline1.PK;
			entryline1Child2.CL_AdValoremTariff = "2010000000";
			invoiceLine1.JI_CL = entryline1Child2.PK;
			entry.MergedLines.CustomSort();
			AssertEquals(entryline3, entry.MergedLines[0]);
			AssertEquals(entryline1, entry.MergedLines[1]);
			AssertEquals(entryline1Child2, entry.MergedLines[2]);
			AssertEquals(entryline1Child1, entry.MergedLines[3]);
			AssertEquals(entryline2, entry.MergedLines[4]);
		}

		public void TestHasSoftwoodLumberLines()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "44091020";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine1.RefreshInvoiceLines();
			entryLine2.RefreshInvoiceLines();
			AssertEquals(false, entryHeader.MergedLines.HasSoftwoodLumberLines);
			invoiceLine2.JI_Tariff = "44091020";
			AssertEquals(true, entryHeader.MergedLines.HasSoftwoodLumberLines);
		}

		public void TestHasPGALinesRequireThreeTimesCustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Tariff = "7209170030";
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Tariff = "8524100020";
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.JI_Tariff = "2922292700";
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_FDAIndicator = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_VNEInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_VNEInd = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_TSCAInd = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_PSTIndicator = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_ODSInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_ODSInd = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_ATFInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_ATFInd = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_CPSCInd = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_AMSInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_AMSInd = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_NOPInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_NOPInd = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_HFCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", true, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
			invoiceLine2.US_HFCInd = ZString.Empty;
			AssertEquals("HasPGALinesRequireThreeTimesCustomsValue", false, entryHeader.MergedLines.HasPGALinesRequireThreeTimesCustomsValue);
		}

		[TestDate(2009, 6, 1)]
		public void TestVisaOrQuotaLinesMerchandiseValue()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = CreateVisaDeclaration();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("VisaOrQuotaLinesMerchandiseValue", 3066m, entry.MergedLines.VisaOrQuotaLinesMerchandiseValue);
			AssertEquals("NonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees", 6910.60m, entry.MergedLines.NonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees);
			declaration.InvoiceLines[0].US_98GoodsValue = 3000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("VisaOrQuotaLinesMerchandiseValue", 4214m, entry.MergedLines.VisaOrQuotaLinesMerchandiseValue);
		}

		public void TestHasMultileManufacturerIDs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			manufacturer.FillWithValidTestData();
			manufacturer.OH_FullName = "Manufacturer";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
			OrgHeader manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.FillWithValidTestData();
			manufacturer2.OH_FullName = "Manufacturer2";
			manufacturer2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "123EREQU6LON");
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("HasMultipleManufacturerID", false, entryHeader.HasMultipleManufacturerIDs);
			invoiceLine2.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			Assert(declaration.MergeManager.RequiresMerge);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("HasMultipleManufacturerID", true, entryHeader.HasMultipleManufacturerIDs);
			invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			invoiceLine1.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			Assert(declaration.MergeManager.RequiresMerge);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("HasMultipleManufacturerID", true, entryHeader.HasMultipleManufacturerIDs);
		}

		public void TestHasMultileManufacturerIDsNotGivingFalsePositiveWithBlankMIDs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			manufacturer.FillWithValidTestData();
			manufacturer.OH_FullName = "Manufacturer1";
			OrgHeader manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.FillWithValidTestData();
			manufacturer2.OH_FullName = "Manufacturer2";
			AssertEquals("HasMultipleManufacturerID", false, entryHeader.HasMultipleManufacturerIDs);
		}

		public void TestHasMultipleECCN()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLineCollection collection = entryHeader.MergedLines;
			AssertEquals(false, collection.HasMultipleECCN);
			CusEntryLine entryLine1 = collection.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			CusEntryLine entryLine2 = collection.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine1.US_ECCN = "ADSDS";
			AssertEquals(false, collection.HasMultipleECCN);
			invoiceLine2.US_ECCN = "ASGGG";
			AssertEquals(true, collection.HasMultipleECCN);
		}

		public void TestHasMultipleLicenseDetails()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C44 });
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLineCollection collection = new CusEntryLineCollection(entryHeader, Factory);
			AssertEquals(false, collection.HasMultipleLicenseDetails);
			CusEntryLine entryLine1 = collection.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			CusEntryLine entryLine2 = collection.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertEquals(false, collection.HasMultipleLicenseDetails);
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C44;
			AssertEquals(true, collection.HasMultipleLicenseDetails);
		}

		public void TestHasLaceyActData()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLineCollection collection = entryHeader.MergedLines;
			AssertEquals(false, collection.HasLaceyActData);
			CusEntryLine entryLine1 = collection.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			CusEntryLine entryLine2 = collection.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			AssertEquals(false, collection.HasLaceyActData);
			invoiceLine2.LaceyActLines.AddNew();
			invoiceLine2.LaceyActLines.AddNew();
			AssertEquals(true, collection.HasLaceyActData);
		}

		public void TestHas98130075Articles()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98130075";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Has98130075Articles", true, entry.MergedLines.Has98130075Articles);
		}

		protected override CusEntryLineCollection GetCollectionToTest()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new CusEntryLineCollection(entryHeader, Factory);
		}

		JobDeclaration CreateVisaDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH";
			invoiceHeader.JZ_InvoiceAmount = 9426m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "FOB";
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802008068";
			invoiceLine1.JI_InvoiceQuantity = 0m;
			invoiceLine1.JI_CustomsQuantity = 0m;
			invoiceLine1.US_98GoodsValue = 1852m;
			invoiceLine1.JI_Tariff = "9102111010";
			invoiceLine1.JI_InvoiceQuantity = 1000m;
			invoiceLine1.JI_InvoiceUQ = "NO";
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsUnitQty = "NO";
			invoiceLine1.JI_LinePrice = 3406m;
			JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			childLine2.US_SupTariff = "9802008068";
			childLine2.JI_InvoiceQuantity = 0m;
			childLine2.JI_CustomsQuantity = 0m;
			childLine2.US_98GoodsValue = 1010m;
			childLine2.JI_Tariff = "9102111020";
			childLine2.JI_InvoiceQuantity = 1000m;
			childLine2.JI_InvoiceUQ = "NO";
			childLine2.JI_CustomsQuantity = 1000m;
			childLine2.JI_CustomsUnitQty = "NO";
			childLine2.JI_LinePrice = 1609m;
			JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
			childLine4.US_SupTariff = "9802008068";
			childLine4.JI_InvoiceQuantity = 0m;
			childLine4.JI_CustomsQuantity = 0m;
			childLine4.US_98GoodsValue = 0m;
			childLine4.JI_Tariff = "9102111030";
			childLine4.JI_InvoiceQuantity = 1000m;
			childLine4.JI_InvoiceUQ = "NO";
			childLine4.JI_CustomsQuantity = 1000m;
			childLine4.JI_CustomsUnitQty = "NO";
			childLine4.JI_LinePrice = 1345m;
			JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
			childLine6.US_SupTariff = "9802008068";
			childLine6.JI_InvoiceQuantity = 0m;
			childLine6.JI_CustomsQuantity = 0m;
			childLine6.US_98GoodsValue = 204m;
			childLine6.JI_Tariff = "9102111040";
			childLine6.JI_InvoiceQuantity = 1000m;
			childLine6.JI_InvoiceUQ = "NO";
			childLine6.JI_CustomsQuantity = 1000m;
			childLine6.JI_CustomsUnitQty = "NO";
			childLine6.JI_LinePrice = 0m;
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
