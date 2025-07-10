using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry.Testing
{
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
	using Enterprise.Customs.NZ.Business.Declaration.Testing;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.MasterFiles.Business;

	/// <summary>
	/// This class cannot be inherited from BaseLineMergerTest as when the base gets run, typeof NZ base JobDeclaration is used
	/// while overriden method to return a right collection return typeof FormalEntry JobDeclaration.
	/// </summary>
	public class LineMergerTestForImport : TestCaseWithFactory
	{
		//Merging on these fields
		/// <summary>
		/// Concession Code
		/// Country Of Export
		/// Country Of Origin
		/// Supplier Customs Code
		/// Invoice Currency
		/// Qualifying/Non Qual for preferential Duty rates
		/// </summary>

		public void TestCustomsValueRoundBeforeCalculateDuty()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupInvoiceGroup(0m, "NZD", 0m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 1000.49m);
			decCreator.SetupImportInvoiceLine("2208.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 1000.49m);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DateOfArrival = new ZDateTime(2019, 11, 30);
			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(sender);

			var entryLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(1000m, entryLine1.CL_CustomsValue);

			declaration.Invoices[0].JZ_InvoiceAmount = 1000.5m;
			declaration.Invoices[0].InvoiceLines[0].JI_LinePrice = 1000.5m;
			declaration.DoMerge(sender);

			entryLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(1001m, entryLine1.CL_CustomsValue);
		}

		public void TestCreateOtherInfosIfNecessary()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("DEM", 400m, "NZ", new ZDateTime(2010, 1, 1), new ZDateTime(2019, 11, 30, 23, 59, 0), "Deminimus");
			helper.CreateTaxOrFee("DEM", 1000m, "NZ", new ZDateTime(2019, 12, 1), new ZDateTime(2079, 6, 6, 23, 59, 0), "Deminimus");
			helper.CreateTaxOrFee("GST", 0.1250m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2010, 9, 30), "Goods and Services Tax");
			helper.CreateTaxOrFee("GST", 0.1500m, "NZ", new ZDateTime(2010, 10, 1), new ZDateTime(2079, 6, 6), "Goods and Services Tax");
			Factory.Save();

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupInvoiceGroup(0m, "NZD", 0m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 200m);
			decCreator.SetupImportInvoiceLine("2208.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 200m);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DateOfArrival = new ZDateTime(2019, 11, 30);

			var invoiceLine = declaration.FilteredInvoiceLines[0];
			Assert(!invoiceLine.OtherInfos.Cast<LineOtherInfo>().Any(x => x.ZO_Code == "LVX"));

			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(sender);
			Assert(invoiceLine.OtherInfos.Cast<LineOtherInfo>().Any(x => x.ZO_Code == "LVX"));

			invoiceLine.OtherInfos.RemoveAndDeleteAll();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.DoMerge(sender);
			Assert(!invoiceLine.OtherInfos.Cast<LineOtherInfo>().Any(x => x.ZO_Code == "LVX"));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.DoMerge(sender);
			Assert(!invoiceLine.OtherInfos.Cast<LineOtherInfo>().Any(x => x.ZO_Code == "LVX"));

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var invoiceHeader = declaration.Invoices[0];
			invoiceHeader.JZ_InvoiceAmount = 401m;
			invoiceLine.JI_LinePrice = 401m;
			declaration.DoMerge(sender);
			Assert(!invoiceLine.OtherInfos.Cast<LineOtherInfo>().Any(x => x.ZO_Code == "LVX"));

			invoiceHeader.JZ_InvoiceAmount = 400m;
			invoiceLine.JI_LinePrice = 400m;
			invoiceLine.JI_Tariff = "0000.00.00.00A";
			declaration.DoMerge(sender);
			Assert(!invoiceLine.OtherInfos.Cast<LineOtherInfo>().Any(x => x.ZO_Code == "LVX"));

			invoiceLine.JI_Tariff = "2208.00.00.00A";
			declaration.DoMerge(sender);
			Assert(invoiceLine.OtherInfos.Cast<LineOtherInfo>().Any(x => x.ZO_Code == "LVX"));
			var entryLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(60m, entryLine1.TotalDutyLevyGST);

			invoiceLine.OtherInfos.RemoveAndDeleteAll();
			declaration.JE_DateOfArrival = new ZDateTime(2019, 12, 1);
			invoiceHeader.JZ_InvoiceAmount = 401m;
			invoiceLine.JI_LinePrice = 401m;
			declaration.DoMerge(sender);
			Assert(invoiceLine.OtherInfos.Cast<LineOtherInfo>().Any(x => x.ZO_Code == "LVX"));
			AssertEquals(60.15m, entryLine1.TotalDutyLevyGST);
		}

		public void TestCallApportion()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			AssertEquals(2, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			invoiceHeader.JZ_InvoiceNumber = "1234";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_InvoiceAmount = 400m;

			JobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1];
			invoiceHeader2.JZ_InvoiceNumber = "1234";
			invoiceHeader2.JZ_OH_Supplier = supplier.PK;
			invoiceHeader2.JZ_InvoiceAmount = 700m;

			CusEntryHeader originalEntryHeader = (CusEntryHeader)declaration.CusEntryHeader;
			CusEntryLine entryLine = originalEntryHeader.MergedLines.AddNew();

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 300m;
			invoiceLine.JI_CustomsQuantity = 15m;
			invoiceLine.JI_CustomsUnitQty = "NMB";
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			invoiceLine.JI_QualifiesForPreferentialDuty = "N";
			entryLine.InvoiceLines.Add(invoiceLine);

			JobComInvoiceLine invoiceLine1 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 700m;
			invoiceLine1.JI_CustomsQuantity = 15m;
			invoiceLine1.JI_CustomsUnitQty = "NMB";
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CountryOfOrigin = "CN";
			invoiceLine1.JI_RN_NKCountryOfExport = "AU";
			invoiceLine1.JI_QualifiesForPreferentialDuty = "N";
			entryLine.InvoiceLines.Add(invoiceLine1);

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CustomsQuantity = 25m;
			invoiceLine2.JI_CustomsUnitQty = "NMB";
			invoiceLine2.JI_CountryOfOrigin = "CN";
			invoiceLine2.JI_RN_NKCountryOfExport = "AU";
			invoiceLine2.JI_QualifiesForPreferentialDuty = "N";
			entryLine.InvoiceLines.Add(invoiceLine2);

			CusEntryHeaderCharge charge1 = declaration.CusEntryHeader.Charges.AddNew();
			charge1.C1_ChargeType = "ENF";
			AssertEquals(invoiceHeader.JobComInvoiceLines[0].JI_EntryFeeAmount, ZDecimal.Zero);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, invoiceHeader.JobComInvoiceLines.Count);
			AssertNotEquals(invoiceHeader.JobComInvoiceLines[0].JI_EntryFeeAmount, ZDecimal.Zero);
			AssertEquals(true, invoiceHeader.JobComInvoiceLines[0].JI_IsApportioned);
			AssertEquals(true, invoiceHeader.JobComInvoiceLines[1].JI_IsApportioned);
			AssertEquals(true, invoiceHeader2.JobComInvoiceLines[0].JI_IsApportioned);
			AssertEquals(declaration.CusEntryHeader.Charges["ENF"].C1_ChargeAmount, invoiceHeader.JobComInvoiceLines[0].JI_EntryFeeAmount + invoiceHeader.JobComInvoiceLines[1].JI_EntryFeeAmount + invoiceHeader2.JobComInvoiceLines[0].JI_EntryFeeAmount);
		}

		public void TestDoNotCalculateEntryFeeIfEntryChargeIsWaived()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);

			declaration.CusEntryHeader.CH_EntryChargeWaived = true;
			merger.DoMerge();
			AssertEquals(0, declaration.CusEntryHeader.Charges.Count);

			declaration.CusEntryHeader.CH_EntryChargeWaived = false;
			merger.DoMerge();
			AssertEquals(2, declaration.CusEntryHeader.Charges.Count);
		}

		public void TestCalculateEntryFee_ConsolidatedDeclaration()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_MessageSubType = declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			var merger1 = new LineMerger(declaration1);
			var merger2 = new LineMerger(declaration2);
			merger1.DoMerge();
			merger2.DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("Can calculate entry fee before applied to consolidated declaration", 2, declaration1.CusEntryHeader.Charges.Count);
				AssertEquals("Can calculate entry fee before applied to consolidated declaration", 2, declaration2.CusEntryHeader.Charges.Count);
			});

			declaration1.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			declaration2.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			var consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
			consolidatedDeclaration.JobDeclarations.Add(declaration1);
			consolidatedDeclaration.JobDeclarations.Add(declaration2);
			declaration1.CusEntryHeader.Charges.RemoveAll();
			declaration2.CusEntryHeader.Charges.RemoveAll();
			merger1.DoMerge();
			merger2.DoMerge();

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition", consolidatedDeclaration.LeadDeclaration.PK, declaration1.PK);
				AssertEquals("Can calculate entry fee after applied to consolidated declaration for lead declaration", 2, declaration1.CusEntryHeader.Charges.Count);
				AssertEquals("Can't calculate entry fee after applied to consolidated declaration for non-lead declaration", 0, declaration2.CusEntryHeader.Charges.Count);
			});
		}

		#region TestMergeCalculatesGSTOnAntiDumpingDuty
		public void TestMergeCalculatesGSTOnAntiDumpingDuty()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			JobDeclaration declaration = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(0m, "NZD", 0m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines[0];
			invoiceLine.JI_AntiDumpingDutyAmount = 1000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Entry Lines", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryLine entryLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0];

			AssertEquals("EntryLine1.VFDWholeNZD", 10000m, entryLine1.VFDWholeNZD);
			AssertEquals("EntryLine1.AntiDumpingDutyAmount", 1000m, entryLine1.AntiDumpingDutyAmount);
			AssertEquals("EntryLine1.DutyAmount", 0m, entryLine1.DutyAmount);
			AssertEquals("EntryLine1.GSTAmount", 1375m, entryLine1.GSTAmount);
		}
		#endregion

		#region TestMergeNeverLosesEntryHeaderWithEDITransmitDate
		public void TestMergeNeverLosesEntryHeaderWithEDITransmitDate()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_DateOfArrival = new ZDateTime(2005, 1, 1);
			declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1234";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_InvoiceAmount = 272.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_IncoTerm = "FOB";

			CusClassification lookupCode = Factory.New<CusClassification>();
			lookupCode.CC_ClassificationType = CusClassification.ClassificationType.Both;
			lookupCode.CC_LookupCode = "WOMENSJACKETS";
			lookupCode.CC_Description = "WOMEN COTTON JACKETS";
			lookupCode.CC_TariffNum = "6202.92.01.00A";

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CC = lookupCode.PK;
			invoiceLine.JI_LinePrice = 272.00m;
			invoiceLine.JI_CustomsQuantity = 34m;
			invoiceLine.JI_CustomsUnitQty = "NMB";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			invoiceLine.JI_QualifiesForPreferentialDuty = "N";

			CusEntryHeader originalEntryHeader = (CusEntryHeader)declaration.CusEntryHeader;

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Active Entry Header should not change during merge", originalEntryHeader, declaration.CusEntryHeader);
		}
		#endregion

		#region TestMergeDoesntWriteClassificationBackToInvoiceLine
		public void TestMergeDoesntWriteClassificationBackToInvoiceLine()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_DateOfArrival = new ZDateTime(2005, 1, 1);

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1234";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_InvoiceAmount = 272.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_IncoTerm = "FOB";

			CusClassification lookupCode = Factory.New<CusClassification>();
			lookupCode.CC_ClassificationType = CusClassification.ClassificationType.Both;
			lookupCode.CC_LookupCode = "WOMENSJACKETS";
			lookupCode.CC_Description = "WOMEN COTTON JACKETS";
			lookupCode.CC_TariffNum = "6202.92.01.00A";

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CC = lookupCode.PK;
			invoiceLine.JI_LinePrice = 272.00m;
			invoiceLine.JI_CustomsQuantity = 34m;
			invoiceLine.JI_CustomsUnitQty = "NMB";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			invoiceLine.JI_QualifiesForPreferentialDuty = "N";

			AssertEquals("InvoiceLine.JI_Tariff Before Merge", "6202.92.01.00A", invoiceLine.JI_Tariff);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("InvoiceLine.JI_Tariff After Merge", "6202.92.01.00A", invoiceLine.JI_Tariff);
			AssertEquals("Declaration.MergedLines.Count After Merge", 1, declaration.CusEntryHeader.MergedLines.Count);
			AssertEquals("6202.92.09.00B", declaration.CusEntryHeader.MergedLines[0].CL_AdValoremTariff);
		}
		#endregion

		#region TestReMergeReAssignsLineNumbers
		public void TestReMergeReAssignsLineNumbers()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader header1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			SetUpDefaultValuesToInvoice(header1);
			JobComInvoiceLine line1 = header1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = header1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line3 = header1.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(header1);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryHeader entryHeader = (CusEntryHeader)declaration.CustomsEntryHeaders[0];
			AssertEquals("EntryHeader.MergedLines.Count", 3, entryHeader.MergedLines.Count);
			AssertEquals("EntryHeader.MergedLines[0].CL_LineNumber", new ZInt(1), entryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("EntryHeader.MergedLines[1].CL_LineNumber", new ZInt(2), entryHeader.MergedLines[1].CL_LineNumber);
			AssertEquals("EntryHeader.MergedLines[2].CL_LineNumber", new ZInt(3), entryHeader.MergedLines[2].CL_LineNumber);

			line2.Delete();
			merger.DoMerge();

			entryHeader = (CusEntryHeader)declaration.CustomsEntryHeaders[0];
			AssertEquals("EntryHeader.MergedLines.Count", 2, entryHeader.MergedLines.Count);
			AssertEquals("EntryHeader.MergedLines[0].CL_LineNumber", new ZInt(1), entryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("EntryHeader.MergedLines[1].CL_LineNumber", new ZInt(2), entryHeader.MergedLines[1].CL_LineNumber);
		}
		#endregion

		#region TestMergeOnPartsOfClassification
		public void TestMergeOnPartsOfClassification()
		{
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);
			line1.JI_PartsOfClassification = "1111";
			line2.JI_PartsOfClassification = "2222";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Customs Entry Line", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestTwoLinesWithSameDetails
		public void TestTwoLinesWithSameDetails()
		{
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Customs Entry Line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestTwoLinesWithDifferentConcessionCode
		public void TestTwoLinesWithDifferentConcessionCode()
		{
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);
			line1.JI_ConcessionCode = "12345";
			line2.JI_ConcessionCode = "54321";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Customs Entry Line", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestTwoLinesWithDifferentCountryExport
		public void TestTwoLinesWithDifferentCountryExport()
		{
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);
			line1.JI_RN_NKCountryOfExport = "US";
			line2.JI_RN_NKCountryOfExport = "AU";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Customs Entry Line", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestTwoLinesWithDifferentCountryOrigin
		public void TestTwoLinesWithDifferentCountryOrigin()
		{
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);
			line1.JI_CountryOfOrigin = "US";
			line2.JI_CountryOfOrigin = "AU";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Customs Entry Line", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestTwoLinesWithDifferentPreferenceDutyRate
		public void TestTwoLinesWithDifferentPreferenceDutyRate()
		{
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			line1.JI_QualifiesForPreferentialDuty = "Q";
			line2.JI_QualifiesForPreferentialDuty = "N";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Customs Entry Line", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region Test24DollarMerge
		public void Test24DollarMerge()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line3 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line4 = invoice2.JobComInvoiceLines.AddNew();

			line1.JI_LinePrice = 10000m;
			line1.JI_Tariff = "8801.10.00.00H";
			line1.JI_ConcessionCode = "987761E";

			line2.JI_LinePrice = 24;
			line2.JI_Tariff = "8803.10.00.00D";
			line2.JI_ConcessionCode = "996131D";
			line2.JI_JI_ParentLine = line1.PK;

			line3.JI_LinePrice = 25;
			line3.JI_Tariff = "8803.10.00.00D";
			line3.JI_ConcessionCode = "996131D";
			line3.JI_JI_ParentLine = line1.PK;

			line4.JI_LinePrice = 10;
			line4.JI_Tariff = "8803.10.00.00D";
			line4.JI_ConcessionCode = "996131D";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("1 Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("2 Entry Lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryLine entryLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0];
			CusEntryLine entryLine2 = declaration.CustomsEntryHeaders[0].MergedLines[1];

			AssertEquals("Line 1 & Line2", line1.JI_CL, line2.JI_CL);
			AssertEquals("Line 3 & Line4", line3.JI_CL, line4.JI_CL);
		}
		#endregion

		#region TestMergeLinesWithDifferentPermitInfo
		public void TestMergeLinesWithDifferentPermitInfo()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			line1.PermitCodes.AddNew("AF1", "222");
			line1.PermitCodes.AddNew("CUD", "3434");
			line2.PermitCodes.AddNew("AF1", "222");
			line2.PermitCodes.AddNew("CUD", "1111");

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Entry Header is created", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two Entry lines are created", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeLinesWithSamePermitInfo
		public void TestMergeLinesWithSamePermitInfo()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			line1.PermitCodes.AddNew("CUD", "3434");
			line1.PermitCodes.AddNew("AF1", "222");

			line2.PermitCodes.AddNew("AF1", "222");
			line2.PermitCodes.AddNew("CUD", "3434");

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Entry Header is created", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("One Entry lines are created", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeLinesWithDifferentProhibitedCodeInfo
		public void TestMergeLinesWithDifferentProhibitedCodeInfo()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			line1.ProhibitedCodes.AddNew("ANT", "");
			line2.ProhibitedCodes.AddNew("ANT", "");
			line2.ProhibitedCodes.AddNew("APC", "");

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Entry Header is created", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two Entry lines are created", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeLinesWithDifferentOtherInfos
		public void TestMergeLinesWithDifferentOtherInfos()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			line1.OtherInfos.AddNew("AWC", "3333");
			line1.OtherInfos.AddNew("BDP", "888");

			line2.OtherInfos.AddNew("AWC", "3333");

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Entry Header is created", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two Entry lines are created", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeOnDifferentSuppliers
		public void TestMergeOnDifferentSuppliers()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);

			invoice1.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			invoice2.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, invoice1.JZ_OH_Supplier)).PK;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("One entry header created", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines are created", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeOnDifferentCurrencies
		public void TestMergeOnDifferentCurrencies()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);

			invoice1.JZ_RX_NKInvoice_Currency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			invoice2.JZ_RX_NKInvoice_Currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, invoice1.JZ_RX_NKInvoice_Currency)).RX_Code;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("One entry header created", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines are created", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestDoMergeDoesntThrowAwayCusEntryHeader
		public void TestDoMergeDoesntThrowAwayCusEntryHeader()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One Entry header is created", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = (CusEntryHeader)declaration.CustomsEntryHeaders[0];
			merger.DoMerge();
			AssertEquals("Same entry header is reused", entryHeader, declaration.CustomsEntryHeaders[0]);
		}
		#endregion

		#region TestDoMergeRecyclesCusEntryLinesAndClearInvoiceLineReferenceToEntryLineWhenKeyChanges
		public void TestDoMergeRecyclesCusEntryLinesAndClearInvoiceLineReferenceToEntryLineWhenKeyChanges()
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryLine firstMergeEntryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Invoice lines reference to CusEntryLine", firstMergeEntryLine.PK, line1.JI_CL);
			AssertEquals("Invoice lines reference to CusEntryLine", firstMergeEntryLine.PK, line2.JI_CL);

			line1.JI_Tariff = "3919.10.09.51A";
			line2.JI_Tariff = "3919.10.09.51A";

			merger.DoMerge();
			Assert("Entry line created and referenced", !line1.JI_CL.IsEmpty);
			Assert("Entry line created and referenced", !line2.JI_CL.IsEmpty);
			Assert("Invoice Lines reference same entry line", line1.JI_CL == firstMergeEntryLine.PK);
			Assert("Invoice Lines reference same entry line", line2.JI_CL == firstMergeEntryLine.PK);
		}
		#endregion

		#region TestMergeDoesntBarfWhenEmptyCusEntryHeaderIsSavedInTheDBWithNoMergeLines
		public void TestMergeDoesntBarfWhenEmptyCusEntryHeaderIsSavedInTheDBWithNoMergeLines()
		{
			CusEntryHeader entryHeader = (CusEntryHeader)declaration.CusEntryHeader;
			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);

			declaration.SaveHandlingSaveExceptions();

			AssertEquals("Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Customs Entry Line", 0, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Customs Entry Line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region Implementation

		JobDeclaration declaration;
		JobComInvoiceHeader invoice1;
		JobComInvoiceHeader invoice2;
		CusClassification lookupCode1;
		CusClassification lookupCode2;
		OrgHeader supplier;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			invoice1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			lookupCode1 = Factory.New<CusClassification>();
			lookupCode1.CC_ClassificationType = CusClassification.ClassificationType.Both;
			lookupCode1.CC_LookupCode = "Clothes";
			lookupCode1.CC_TariffNum = "6103.41.00.11K";

			lookupCode2 = Factory.New<CusClassification>();
			lookupCode2.CC_ClassificationType = CusClassification.ClassificationType.Both;
			lookupCode2.CC_LookupCode = "Plate";
			lookupCode2.CC_TariffNum = "3919.10.09.51A";

			supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		protected void SetUpDefaultValuesToInvoice(JobComInvoiceHeader invoice)
		{
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.JZ_OH_Supplier = supplier.PK;
		}

		protected void SetUpDefaultValuesToInvoiceLines(JobComInvoiceHeader invoice)
		{
			foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
			{
				invoiceLine.JI_CC = lookupCode1.PK;
				invoiceLine.JI_ConcessionCode = "984732E";
				invoiceLine.JI_CountryOfOrigin = "AU";
				invoiceLine.JI_RN_NKCountryOfExport = "AU";
				invoiceLine.JI_QualifiesForPreferentialDuty = "Q";
			}
		}

		#endregion
	}

	public class LineMergerForExportTest : TestCaseWithFactory
	{
		#region TestMergeOnSameDetails
		public void TestMergeOnSameDetails()
		{
			PermitCode code1 = line1.PermitCodes.AddNew();
			code1.ZO_Code = "CUD";
			code1.ZO_Data = "111";
			PermitCode code2 = line1.PermitCodes.AddNew();
			code2.ZO_Code = "DOC";
			code2.ZO_Data = "111";

			PermitCode code3 = line2.PermitCodes.AddNew();
			code3.ZO_Code = "DOC";
			code3.ZO_Data = "111";
			PermitCode code4 = line2.PermitCodes.AddNew();
			code4.ZO_Code = "CUD";
			code4.ZO_Data = "111";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("One line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeWithDifferentTariffs
		public void TestMergeWithDifferentTariffs()
		{
			line1.JI_Tariff = "8301.10.00.00F";
			line2.JI_Tariff = "8301.10.00.00A";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeWithDifferentCountryOfOrigin
		public void TestMergeWithDifferentCountryOfOrigin()
		{
			line1.JI_CountryOfOrigin = "AU";
			line2.JI_CountryOfOrigin = "NZ";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeWithDifferentExchangeRateIndicator
		public void TestMergeWithDifferentExchangeRateIndicator()
		{
			invoice1.JZ_ExchangeRateIndicator = "NZD";
			invoice2.JZ_ExchangeRateIndicator = "FLO";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeWithDifferentCurrency
		public void TestMergeWithDifferentCurrency()
		{
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice2.JZ_RX_NKInvoice_Currency = "NZD";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region TestMergeWithOtherInfoCode
		public void TestMergeWithOtherInfoCode()
		{
			line1.PermitCodes.AddNew("ZZZ", "");
			line1.PermitCodes.AddNew("YYY", "");
			line2.PermitCodes.AddNew("XXX", "");
			line2.PermitCodes.AddNew("ZZZ", "");

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}
		#endregion

		#region Implementation

		JobDeclaration declaration;
		JobComInvoiceHeader invoice1;
		JobComInvoiceHeader invoice2;
		JobComInvoiceLine line1;
		JobComInvoiceLine line2;
		CusClassification lookupCode1;
		CusClassification lookupCode2;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			invoice1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			line1 = invoice1.JobComInvoiceLines.AddNew();
			line2 = invoice2.JobComInvoiceLines.AddNew();

			lookupCode1 = Factory.New<CusClassification>();
			lookupCode1.CC_ClassificationType = CusClassification.ClassificationType.Both;
			lookupCode1.CC_LookupCode = "Clothes";
			lookupCode1.CC_TariffNum = "6103.41.00.11K";

			lookupCode2 = Factory.New<CusClassification>();
			lookupCode2.CC_ClassificationType = CusClassification.ClassificationType.Both;
			lookupCode2.CC_LookupCode = "Plate";
			lookupCode2.CC_TariffNum = "3919.10.09.51A";

			SetUpDefaultValuesToInvoice(invoice1);
			SetUpDefaultValuesToInvoice(invoice2);
			SetUpDefaultValuesToInvoiceLines(invoice1);
			SetUpDefaultValuesToInvoiceLines(invoice2);
		}

		protected void SetUpDefaultValuesToInvoice(JobComInvoiceHeader invoice)
		{
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.JZ_ExchangeRateIndicator = "NZD";
		}

		protected void SetUpDefaultValuesToInvoiceLines(JobComInvoiceHeader invoice)
		{
			foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
			{
				invoiceLine.JI_CC = lookupCode1.PK;
				invoiceLine.JI_CountryOfOrigin = "AU";
			}
		}

		#endregion
	}

	public class LineMergerForCompletionEntryTest : TestCaseWithFactory
	{
		#region TestCompletionFromSightEntry
		public void TestCompletionFromSightEntry()
		{
			DecCreator.SetupTestConsignmentDetails();
			DecCreator.SetupTestForAir();
			DecCreator.SetupTestForImportFromAU();
			DecCreator.SetupInvoiceGroup(111m, "NZD", 0m, "NZD");
			DecCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 1000m, "AU", "AU", "Q");
			DecCreator.SetupImportInvoiceLine("6505900023J", "PLASTIC BITS", "", "", "", 1000m);
			DecCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			DecCreator.MergeDeclaration();
			DecCreator.MergeLineSetDutyAndTax(0, 0.00m, 139.37m);
			Declaration.DeclarationNumber = "01020304";
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			DecCreator.MergeDeclaration();
			CusEntryHeader completionCusEntryHeader = (CusEntryHeader)Declaration.CusEntryHeader;
			AssertEquals("CompletionCusEntryHeader", typeof(CompletionCusEntryHeader), completionCusEntryHeader.GetType());
			AssertEquals("Merged Line Count on CompletionCusEntryHeader", 1, completionCusEntryHeader.MergedLines.Count);
		}
		#endregion

		#region TestCompletionFromTemporaryImport
		public void TestCompletionFromTemporaryImport()
		{
			DecCreator.SetupTestConsignmentDetails();
			DecCreator.SetupTestForAir();
			DecCreator.SetupTestForExportToAU();
			DecCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			DecCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			DecCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			DecCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Declaration.JE_OriginalEntryNumber = "01020304";
			DecCreator.MergeDeclaration();
			CusEntryHeader completionCusEntryHeader = (CusEntryHeader)Declaration.CusEntryHeader;
			AssertEquals("CompletionCusEntryHeader", typeof(CompletionCusEntryHeader), completionCusEntryHeader.GetType());
			AssertEquals("Merged Line Count on CompletionCusEntryHeader", 1, completionCusEntryHeader.MergedLines.Count);
		}
		#endregion

		#region DecCreator
		TestFormalEntryCreator DecCreator
		{
			get
			{
				if (fDecCreator == null)
				{
					fDecCreator = new TestFormalEntryCreator(Declaration);
				}
				return fDecCreator;
			}
		}
		TestFormalEntryCreator fDecCreator;
		#endregion

		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion
	}
}
