using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		public void TestDoMergeForImx()
		{
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_PreviousMRN = "JHN201706275000575";
			var testInvoice1 = declaration.Invoices.AddNew();
			testInvoice1.JZ_InvoiceNumber = "INV3";
			testInvoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine11 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = testInst.PK;
			invoiceLine11.JI_Tariff = "1";
			invoiceLine11.JI_PreviousEntryLineNumber = 10;
			var invoiceLine12 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = testInst.PK;
			invoiceLine12.JI_Tariff = "1";
			invoiceLine12.JI_PreviousEntryLineNumber = 11;
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(2, declaration.ActiveEntryHeaders[0].MergedLines.Count);
			AssertEquals((ZShort)10, invoiceLine11.CusEntryLine.CL_LineNumber);
			AssertEquals((ZShort)11, invoiceLine12.CusEntryLine.CL_LineNumber);
			AssertEquals("JHN201706275000575", invoiceLine11.CusEntryLine.Header.EntryNumber);
			AssertEquals("JHN201706275000575", invoiceLine11.CusEntryLine.Header.CH_BGMReference);
			AssertEquals("JHN201706275000575", invoiceLine12.CusEntryLine.Header.EntryNumber);
			AssertEquals("JHN201706275000575", invoiceLine12.CusEntryLine.Header.CH_BGMReference);
		}

		[TestDate(2018, 05, 01, 01, 01, 01)]
		public void TestRateSelectionForValuationDateNotSet()
		{
			testHelper.CreateTariffWithTwoDates(ZDateTime.Today.AddHours(12), ZDateTime.Today.AddHours(12), ZDateTime.Today.AddYears(1));
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			var entryInstruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invoiceLine.JI_Tariff = testHelper.Tariff1P1.ZZ1_TariffCode;
			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.JI_CustomsUnitQty = "LI";
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			declaration.DoMerge();
			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("entry.MergedLines.Count", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];
			AssertEquals("Rates as per Now (morning): entryLine.CustomsDuty", 10m, entryLine.CustomsDuty);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddHours(7);
			declaration.DoMerge();
			AssertEquals("Rates as per morning rate: entryLine.CustomsDuty", 10m, entryLine.CustomsDuty);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddHours(14);
			declaration.DoMerge();
			AssertEquals("Rates are updated to the afternoon rate: entryLine.CustomsDuty", 20m, entryLine.CustomsDuty);
		}

		public void TestRateSelectionWithValuationDateSet_VOC()
		{
			testHelper.CreateTariffWithTwoDates(ZDateTime.Today.AddDays(-1), ZDateTime.Today, ZDateTime.Today.AddYears(1));
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			var entryInstruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invoiceLine.JI_Tariff = testHelper.Tariff1P1.ZZ1_TariffCode;
			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.JI_CustomsUnitQty = "LI";
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			declaration.DoMerge();
			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("entry.MergedLines.Count", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];
			AssertEquals("Rates as per now: entryLine.CustomsDuty", 20m, entryLine.CustomsDuty);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-4);
			declaration.DoMerge();
			AssertEquals("Rates are updated to the assessment date: entryLine.CustomsDuty", 10m, entryLine.CustomsDuty);
		}

		[TestDate(2016, 10, 07)]
		public void TestRateSelectionValuationDate()
		{
			testHelper.CreateTariffWithTwoDates(ZDateTime.Today.AddDays(-1), ZDateTime.Today, ZDateTime.Today);
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			var entryInstruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invoiceLine.JI_Tariff = testHelper.Tariff1P1.ZZ1_TariffCode;
			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.JI_CustomsUnitQty = "LI";
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			declaration.DoMerge();
			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("entry.MergedLines.Count", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];
			AssertEquals("entryLine.CustomsDuty", 20m, entryLine.CustomsDuty);
		}

		[TestDate(2016, 8, 4)]
		public void TestBNDCalculation()
		{
			testHelper.SetupTariffsForBNDTests();
			/*
				VDF = 1000;
				LI = 1000;
				1P1 = 1000LI * 0,00035 C/LI = 0,35 ZAR
				12A = 1000LI * 3,909 C/LI = 39,09 ZAR
				15A = 1000LI * 258 C/LI = 2550 ZAR
				15B = 1000LI * 154 C/LI = 1540 ZAR
				DTY = 1P1 + 12A + 15A + 15B + VAT = 4129.3 ZAR
				VAT = ((VDF * 1.1) + DTY) * 0.14 = 732.20 ZAR
				BND = DTY + VAT = 4861.50 = 4861 (with ZA Customs Rounding)
			*/
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			var entryInstruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._40);
			entryInstruction.CEI_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;
			var invoiceLine = testHelper.AddInvoiceAndInvoiceLine(declaration, entryInstruction, 1000m, 1000m, "LI");
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			declaration.DoMerge();
			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("entry.MergedLines.Count", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("entryLine.Fees.Count", 2, entryLine.Fees.Count);
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 0m, entryLine.CustomsDuty);
				AssertEquals("entryLine.BND", new ZDecimal(4861m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
			});
			testHelper.Tariff1P1Rate.ZZ2_RateFormula = "0.00036 * [LI]";
			declaration.DoMerge();
			entry = declaration.CustomsEntryHeaders[0];
			entryLine = entry.MergedLines[0];
			AssertEquals("entryLine.BND = DTY + VAT = 4861.51 = 4862 (with ZA Customs Rounding)", new ZDecimal(4862m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
			invoiceLine.JI_PrimaryPreference = PrimaryPreference.Standard;
			testHelper.Procedure2.ZZ6_CalculateDuty = true;
			testHelper.Tariff1P1Rate.ZZ2_RateFormula = "0.00050 * [LI]";
			declaration.DoMerge();
			entry = declaration.CustomsEntryHeaders[0];
			entryLine = entry.MergedLines[0];
			AssertEquals("entryLine.BND = 154.50 = 154 (with ZA Customs Rounding)", new ZDecimal(154m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
			testHelper.Tariff1P1Rate.ZZ2_RateFormula = "0.00051 * [LI]";
			declaration.DoMerge();
			entry = declaration.CustomsEntryHeaders[0];
			entryLine = entry.MergedLines[0];
			AssertEquals("entryLine.BND = 154.51 = 155 (with ZA Customs Rounding)", new ZDecimal(155m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
		}

		[TestDate(2016, 8, 4)]
		public void TestBNDCalculationIsNotRoundedDownToZero()
		{
			testHelper.SetupTariffsForBNDTests();
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			var entryInstruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._40);
			entryInstruction.CEI_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;
			var invoiceLine = testHelper.AddInvoiceAndInvoiceLine(declaration, entryInstruction, 3m, 3m, "LI");
			testHelper.Procedure2.ZZ6_CalculateDuty = true;
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();

			testHelper.Tariff1P1Rate.ZZ2_RateFormula = "0.00051 * [LI]";
			declaration.DoMerge();
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.MergedLines[0];
			AssertEquals("entryLine.BND = 0.42 = 1 (with ZA Customs Rounding)", new ZDecimal(1m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
		}

		[TestDate(2016, 8, 4)]
		public void TestBNDCalculationForExportsWithoutTaxField()
		{
			testHelper.SetupTariffsForBNDCalculationForExportsWithoutTaxField();
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			var entryInstruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._67);
			entryInstruction.CEI_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;
			var invoiceLine = testHelper.AddInvoiceAndInvoiceLine(declaration, entryInstruction, 10000m, 5000m, "LI");
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._40;
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			declaration.DoMerge();
			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("entry.MergedLines.Count", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];
			AssertEquals("BND With VAT", new ZDecimal(2225m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
			invoiceLine.JI_ZZF_NKTaxType = ZString.Empty;
			declaration.DoMerge();
			entry = declaration.CustomsEntryHeaders[0];
			entryLine = entry.MergedLines[0];
			AssertEquals("BND without VAT", new ZDecimal(2225m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
			invoiceLine.JI_ZZF_NKTaxType = TaxOrFeeTypeCode.VEX;
			declaration.DoMerge();
			entry = declaration.CustomsEntryHeaders[0];
			entryLine = entry.MergedLines[0];
			AssertEquals("BND with VEX", new ZDecimal(2225m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
			invoiceLine.JI_Tariff = testHelper.Tariff1P1_2.ZZ1_TariffCode;
			declaration.DoMerge();
			entry = declaration.CustomsEntryHeaders[0];
			entryLine = entry.MergedLines[0];
			AssertEquals("BND with VEX Tariff", new ZDecimal(500m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
			invoiceLine.JI_Tariff = testHelper.Tariff1P1.ZZ1_TariffCode;
			invoiceLine.JI_PrimaryPreference = "200";
			declaration.DoMerge();
			entry = declaration.CustomsEntryHeaders[0];
			entryLine = entry.MergedLines[0];
			AssertEquals("BND with other Preference", new ZDecimal(2225m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
		}

		[TestDate(2016, 8, 4)]
		public void TestCPC40Calculation()
		{
			testHelper.SetupTariffsForBNDTests();
			testHelper.Tariff1P1Rate.ZZ2_RateFormula = "0.091 * [LI]";
			testHelper.Tariff12ARate.ZZ2_RateFormula = "3.909 * [LI]";
			testHelper.Tariff15ARate.ZZ2_RateFormula = "0 * [LI]";
			testHelper.Tariff15BRate.ZZ2_RateFormula = "154 * [LI]";
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			var entryInstruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._40);
			entryInstruction.CEI_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;
			var invoiceLine = testHelper.AddInvoiceAndInvoiceLine(declaration, entryInstruction, 1000m, 1000m, "LI");
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(testHelper.Tariff12A.ZZ1_ZZI_TariffTypeCode, testHelper.Tariff12A.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(testHelper.Tariff15A.ZZ1_ZZI_TariffTypeCode, testHelper.Tariff15A.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(testHelper.Tariff15B.ZZ1_ZZI_TariffTypeCode, testHelper.Tariff15B.ZZ1_TariffCode);
			declaration.DoMerge();
			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("entry.MergedLines.Count", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("entryLine.Fees.Count", 5, entryLine.Fees.Count);
				AssertEquals("entryLine.Fees['1P1'].Amount", 91m, entryLine.Fees.GetAmountIncludingLCOnly("1P1"));
				AssertEquals("entryLine.Fees['12A'].Amount", 3909m, entryLine.Fees.GetAmountIncludingLCOnly("12A"));
				AssertEquals("entryLine.Fees['15A'].Amount", ZDecimal.Zero, entryLine.Fees.GetAmountIncludingLCOnly("15A"));
				AssertEquals("entryLine.Fees['15B'].Amount", 154000m, entryLine.Fees.GetAmountIncludingLCOnly("15B"));
				AssertEquals("entryLine.Fees['VAT'].Amount", 22274.00m, entryLine.Fees.GetAmountIncludingLCOnly("VAT"));
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 0m, entryLine.CustomsDuty);
				AssertEquals("entryLine.BND", new ZDecimal(180274m).ToString(), entryLine.AdditionalInformationCodes["BND"]?.CY_Data ?? ZString.Empty);
			});
		}

		public void TestLineNumbersDoNotGetReusedWhenLineIsDeletedAfterSendingMessageToCustoms()
		{
			helper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var instruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Tariff = "1.1.1";
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("Entry Lines count", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Line number of first line", (short)1, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "B123";
			entry.CH_EntryStatus = "1";
			var invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Tariff = "2.2.2";
			merger.DoMerge();
			invoiceLine1.Delete();
			merger.DoMerge();
			var invoiceLine3 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_Tariff = "3.3.3";
			merger.DoMerge();
			AssertEquals("Entry Lines count", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Line number of 'third' line (line 1 has been deleted)", (short)3, invoiceLine3.CusEntryLine.CL_LineNumber);
		}

		public void TestExistingLinesKeepNumbersAfterSubmissionToCustoms()
		{
			helper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var instruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Tariff = "1.1.1";
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("Entry Lines count", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Line number of first line", (short)1, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "B123";
			entry.CH_EntryStatus = "1";
			var invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Tariff = "2.2.2";
			var invoiceLine3 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_Tariff = "3.3.3";
			merger.DoMerge();
			AssertEquals("Entry Lines count", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			invoiceLine1.Delete();
			merger.DoMerge();
			AssertEquals("Entry Lines count", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Line number of Line2", (short)2, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			AssertEquals("Line number of Line3", (short)3, declaration.CustomsEntryHeaders[0].MergedLines[1].CL_LineNumber);
		}

		[TestDate(2016, 8, 4)]
		public void TestEntryLineDescriptionAfterMerge()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
			Factory.Save();
			var startDate = new ZDateTime(1900, 01, 01);
			var endDate = new ZDateTime(2050, 01, 01);
			var longTariffDesc = "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. " + "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. " + "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.";
			var entryLineDesc = "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. " + "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. " + "The quick brown fox jumps over the lazy dog. The quick brown fox  VIN; 011011011";
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "9988770623", startDate, endDate, description: longTariffDesc);
			Factory.Save();
			var declaration = testHelper.GetTestDeclaration();
			var instruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_VIN = "011011011";
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			AssertEquals(longTariffDesc, invoiceLine.JI_Description);
			AssertEquals(entryLineDesc, entryLine.CL_Description);
		}

		public void TestUpdateProvisionalPaymentsWhenMerge()
		{
			var declaration = testHelper.GetTestDeclaration();
			var instruction = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Tariff = "1.1.1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Tariff = "2.1.1";
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var payInfo1 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPE, 1m, "1", "REF", true);
			var payInfo2 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPT, 2m, "1", "REF", false);
			merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLines = entryHeader.AllEntryLines.Cast<CusEntryLine>().OrderBy(x => x.CL_LineNumber);
			var entryLine1 = entryLines.FirstOrDefault();
			AssertEquals(0, entryLine1.ProvisionalPayments.Count);
			payInfo1.C9_RemAdvReceived = false;
			merger.DoMerge();
			entryLines = entryHeader.AllEntryLines.Cast<CusEntryLine>().OrderBy(x => x.CL_LineNumber);
			entryLine1 = entryLines.FirstOrDefault();
			AssertEquals(1, entryLine1.ProvisionalPayments.Count);
			AssertEquals(LineLevelProvisionalPayments.Codes.PPE, entryLine1.ProvisionalPayments[0].CY_Code);
			AssertEquals(1m, entryLine1.ProvisionalPayments[0].CY_Value);
			Assert(entryLine1.ProvisionalPayments[0].ReadOnly);
			var entryLine2 = entryLines.LastOrDefault();
			AssertEquals(0, entryLine2.ProvisionalPayments.Count);
		}

		public void TestCH_EntriesAfterMerge()
		{
			var declaration = testHelper.GetTestDeclaration();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FCLTEST1";
			container1.CO_FCL_LCL_AIR = "FCL";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "FCLTEST2";
			container2.CO_FCL_LCL_AIR = "FCL";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "FCLTEST3";
			container3.CO_FCL_LCL_AIR = "FCL";
			var instruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var instruction2 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._12);
			var instruction3 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._13);
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "CIF";
			invHeader.JZ_InvoiceAmount = 200210.10m;
			invHeader.JZ_RX_NKInvoice_Currency = "USD";
			var invLine1 = testHelper.AddInvoiceLine(invHeader, instruction1, "84501230", 50000m);
			var invLine2 = testHelper.AddInvoiceLine(invHeader, instruction1, "84501230", 100000m);
			var invLine3 = testHelper.AddInvoiceLine(invHeader, instruction1, "84501230", 100000m);
			declaration.MarkApportionmentDirty();
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(1, declaration.ActiveEntryHeaders[0].CH_TotalEntries);
			AssertEquals(1, declaration.ActiveEntryHeaders[0].CH_EntryNumber);
			invLine2.JI_CEI = instruction2.PK;
			invLine3.JI_CEI = instruction3.PK;
			invLine2.JI_Procedure = invLine2.EntryInstruction.CEI_Style + "00";
			invLine3.JI_Procedure = invLine3.EntryInstruction.CEI_Style + "00";
			invLine2.JI_Tariff = "84501230";
			invLine3.JI_Tariff = "29039915";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(3, declaration.ActiveEntryHeaders.Count);
			AssertEquals(3, declaration.ActiveEntryHeaders[0].CH_TotalEntries);
			AssertEquals(3, declaration.ActiveEntryHeaders[1].CH_TotalEntries);
			AssertEquals(3, declaration.ActiveEntryHeaders[2].CH_TotalEntries);
			AssertEquals(1, declaration.ActiveEntryHeaders[0].CH_EntryNumber);
			AssertEquals(2, declaration.ActiveEntryHeaders[1].CH_EntryNumber);
			AssertEquals(3, declaration.ActiveEntryHeaders[2].CH_EntryNumber);
			declaration.ActiveEntryHeaders[0].CH_TotalEntries = 4;
			declaration.ActiveEntryHeaders[1].CH_TotalEntries = 4;
			declaration.ActiveEntryHeaders[2].CH_TotalEntries = 4;
			declaration.ActiveEntryHeaders[0].CH_EntryNumber = 2;
			declaration.ActiveEntryHeaders[1].CH_EntryNumber = 3;
			declaration.ActiveEntryHeaders[2].CH_EntryNumber = 4;
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(3, declaration.ActiveEntryHeaders[0].CH_TotalEntries);
			AssertEquals(3, declaration.ActiveEntryHeaders[1].CH_TotalEntries);
			AssertEquals(3, declaration.ActiveEntryHeaders[2].CH_TotalEntries);
			AssertEquals(1, declaration.ActiveEntryHeaders[0].CH_EntryNumber);
			AssertEquals(2, declaration.ActiveEntryHeaders[1].CH_EntryNumber);
			AssertEquals(3, declaration.ActiveEntryHeaders[2].CH_EntryNumber);
		}

		public void TestCH_Packages_SigleEntryAfterMerge()
		{
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_MergeBy = "NON";
			declaration.JE_TotalNoOfPacks = 11;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FCLTEST1";
			container1.CO_FCL_LCL_AIR = "FCL";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "FCLTEST2";
			container2.CO_FCL_LCL_AIR = "FCL";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "FCLTEST3";
			container3.CO_FCL_LCL_AIR = "FCL";
			var instruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var currency = Factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_ExRateType = "CUS";
			exRate.RE_StartDate = new ZDateTime(2016, 03, 30);
			exRate.RE_ExpiryDate = new ZDateTime(2016, 04, 30);
			exRate.RE_SellRate = 0.088888m;
			exRate.RE_RX_NKExCurrency = "USD";
			exRate.RE_GC = declaration.Company.PK;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "CIF";
			invHeader.JZ_InvoiceAmount = 200210.10m;
			invHeader.JZ_RX_NKInvoice_Currency = "USD";
			var freightcharge = invHeader.Charges.AddNew("OFT", 6123.55m, "USD");
			freightcharge.J7_FullOrPartialApportionment = "PAA";
			freightcharge.J7_ExchangeRate = 0.088888m;
			freightcharge.J7_IsIncludedInITOT = true;
			freightcharge.J7_IsDutiable = false;
			testHelper.AddInvoiceLine(invHeader, instruction1, "84501230", 50000m);
			testHelper.AddInvoiceLine(invHeader, instruction1, "84501230", 100000m);
			declaration.MarkApportionmentDirty();
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(declaration.ActiveEntryHeaders[0].CH_Packages, 3);
			var container4 = declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "FCLTEST4";
			container4.CO_FCL_LCL_AIR = "XXX";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(declaration.ActiveEntryHeaders[0].CH_Packages, 11);
		}

		public void TestCH_Packages_MultiEntriesAfterMerge()
		{
			var declaration = testHelper.GetTestDeclaration();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FCLTEST1";
			container1.CO_FCL_LCL_AIR = "FCL";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "FCLTEST2";
			container2.CO_FCL_LCL_AIR = "FCL";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "FCLTEST3";
			container3.CO_FCL_LCL_AIR = "FCL";
			var instruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var instruction2 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._12);
			var instruction3 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._13);
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "FOB";
			invHeader.JZ_RX_NKInvoice_Currency = "ZAR";
			testHelper.AddInvoiceLine(invHeader, instruction1, "84501230", 75m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction2, "84501230", 0m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction3, "29039915", 75m).JI_ZZF_NKTaxType = "VEX";
			Factory.Save();

			CheckMerge1(declaration);
			CheckMerge2(declaration);
			CheckMerge3(declaration);
			CheckMerge4(declaration);
			CheckMerge5(declaration, instruction1);
			CheckMerge6(declaration, instruction1);
		}

		void CheckMerge1(JobDeclaration declaration)
		{
			declaration.DoMerge();
			AssertEquals(3, declaration.ActiveEntryHeaders.Count);
			CombineAssertions("Calculation on First Merge", () =>
			{
				AssertEquals(declaration.ActiveEntryHeaders[0].CH_Packages, 0);
				AssertEquals(declaration.ActiveEntryHeaders[1].CH_Packages, 0);
				AssertEquals(declaration.ActiveEntryHeaders[2].CH_Packages, 0);
			});
		}

		void CheckMerge2(JobDeclaration declaration)
		{
			declaration.ActiveEntryHeaders[0].CH_Packages = 1;
			declaration.ActiveEntryHeaders[1].CH_Packages = 2;
			declaration.ActiveEntryHeaders[2].CH_Packages = 3;
			declaration.DoMerge();
			AssertEquals(3, declaration.ActiveEntryHeaders.Count);
			CombineAssertions("No Change, simply Merge Again", () =>
			{
				AssertEquals(declaration.ActiveEntryHeaders[0].CH_Packages, 1);
				AssertEquals(declaration.ActiveEntryHeaders[1].CH_Packages, 2);
				AssertEquals(declaration.ActiveEntryHeaders[2].CH_Packages, 3);
			});
		}

		void CheckMerge3(JobDeclaration declaration)
		{
			declaration.ActiveEntryHeaders[0].CH_Packages = 4;
			declaration.ActiveEntryHeaders[1].CH_Packages = 5;
			declaration.ActiveEntryHeaders[2].CH_Packages = 6;
			declaration.Invoices[0].JobComInvoiceLines[1].JI_LinePrice = 88m;
			declaration.DoMerge();
			AssertEquals(3, declaration.ActiveEntryHeaders.Count);
			CombineAssertions("Change on inrelevant fields won't reset the UZ_Packages", () =>
			{
				AssertEquals(declaration.ActiveEntryHeaders[0].CH_Packages, 4);
				AssertEquals(declaration.ActiveEntryHeaders[1].CH_Packages, 5);
				AssertEquals(declaration.ActiveEntryHeaders[2].CH_Packages, 6);
			});
		}

		void CheckMerge4(JobDeclaration declaration)
		{
			declaration.JE_TotalNoOfPacks = 11;
			CombineAssertions("Change on declaration total package reset the UZ_Packages even before Merge", () =>
			{
				AssertEquals(declaration.ActiveEntryHeaders[0].CH_Packages, 0);
				AssertEquals(declaration.ActiveEntryHeaders[1].CH_Packages, 0);
				AssertEquals(declaration.ActiveEntryHeaders[2].CH_Packages, 0);
			});
			declaration.DoMerge();
			AssertEquals(3, declaration.ActiveEntryHeaders.Count);
			CombineAssertions("Change on declaration total package UZ_Packages as Zero after merge", () =>
			{
				AssertEquals(declaration.ActiveEntryHeaders[0].CH_Packages, 0);
				AssertEquals(declaration.ActiveEntryHeaders[1].CH_Packages, 0);
				AssertEquals(declaration.ActiveEntryHeaders[2].CH_Packages, 0);
			});
		}

		void CheckMerge5(JobDeclaration declaration, CusEntryInstruction instruction)
		{
			declaration.ActiveEntryHeaders[0].CH_Packages = 7;
			declaration.ActiveEntryHeaders[1].CH_Packages = 8;
			declaration.ActiveEntryHeaders[2].CH_Packages = 9;
			declaration.Invoices[0].JobComInvoiceLines[1].JI_CEI = instruction.PK;
			declaration.Invoices[0].JobComInvoiceLines[1].JI_Procedure = declaration.Invoices[0].JobComInvoiceLines[1].EntryInstruction.CEI_Style + "00";
			declaration.DoMerge();
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			CombineAssertions("Merge situation changed reset the UZ_Packages", () =>
			{
				AssertEquals(declaration.ActiveEntryHeaders[0].CH_Packages, 7);
				AssertEquals(declaration.ActiveEntryHeaders[1].CH_Packages, 9);
			});
		}

		void CheckMerge6(JobDeclaration declaration, CusEntryInstruction instruction)
		{
			declaration.Invoices[0].JobComInvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.JI_CEI = instruction.PK);
			declaration.Invoices[0].JobComInvoiceLines[2].JI_Tariff = "84501230";
			new LineMerger(declaration).DoMerge();
			CombineAssertions("Merge situation changed to single Line, use the Single Line Defaulting Logic", () =>
			{
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				AssertEquals(declaration.ActiveEntryHeaders[0].CH_Packages, 3);
			});
		}

		[TestDate(2016, 04, 01)]
		public void TestPaymentMethodDefaulting_AutoDeferral()
		{
			helper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateBasicTariff();
			var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var procedure3 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "13", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var procedure4 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "14", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var startDate = ZDateTime.Today.AddYears(0);
			var endDate = ZDateTime.Today.AddYears(1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, testHelper.TariffType1P1.PK, "88888", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff2Rate = helper.CreateRate(tariff2, testHelper.RateCode_ZA_DTY_D.PK, startDate, endDate, "0", testHelper.Preference.PK);
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			var countryZA = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "ZA");
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.SetAgentCode(countryZA, "AGT");
			helper.CreateCusApplicability(tariff2Rate, testHelper.TradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();

			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping = LineMergerTestHelper.AddMapping(collection, testAgent.PK, "BBR", "3234002346", importerPays: true, 1);
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_CustomsOffice = "BBR";
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var instruction2 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._12);
			var instruction3 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._13);
			var instruction4 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._14);
			instruction4.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			instruction4.CEI_ProvisionalPaymentAmount = 15m;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "FOB";
			invHeader.JZ_InvoiceAmount = 1m;
			invHeader.JZ_RX_NKInvoice_Currency = "ZAR";
			testHelper.AddInvoiceLine(invHeader, instruction1, "99999", 75m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction2, "88888", 0m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction3, "88888", 75m).JI_ZZF_NKTaxType = "VEX";
			testHelper.AddInvoiceLine(invHeader, instruction4, "88888", 75m).JI_ZZF_NKTaxType = "VEX";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(4, declaration.ActiveEntryHeaders.Count);
			var header1 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "11") as CusEntryHeader;
			var header2 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "12") as CusEntryHeader;
			var header3 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "13") as CusEntryHeader;
			var header4 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "14") as CusEntryHeader;
			CombineAssertions(() =>
			{
				AssertEquals("header1.CustomsDuty", 22.5m, header1.CustomsDuty);
				AssertEquals("header1.ValueAddedTax", 14.7m, header1.ValueAddedTax);
				AssertEquals("header1.CH_PaymentMethod", "D", header1.CH_PaymentMethod);
				AssertEquals("header2.CustomsDuty", 0m, header2.CustomsDuty);
				AssertEquals("header2.ValueAddedTax", 0.14m, header2.ValueAddedTax);
				AssertEquals("header2.CH_PaymentMethod", "D", header2.CH_PaymentMethod);
				AssertEquals("header3.CustomsDuty", 0m, header3.CustomsDuty);
				AssertEquals("header3.ValueAddedTax", 0m, header3.ValueAddedTax);
				AssertEquals("header3.CH_PaymentMethod", "F", header3.CH_PaymentMethod);
				AssertEquals("header4.CustomsDuty", 0m, header4.CustomsDuty);
				AssertEquals("header4.ValueAddedTax", 0m, header4.ValueAddedTax);
				AssertEquals("header4.CH_PaymentMethod", "C", header4.CH_PaymentMethod);
			});
		}

		[TestDate(2016, 04, 01)]
		public void TestPaymentMethodDefaulting_WithoutFAN()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "6", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "13A,13B,13C,13D,4", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var procedure3 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "13", "00", "4", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var procedure4 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import, false, true);
			var startDate = ZDateTime.Today.AddYears(0);
			var endDate = ZDateTime.Today.AddYears(1);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99999", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.3 * VFD", preference.PK);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "88888", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff2Rate = helper.CreateRate(tariff2, rateCode_ZA_DTY_D.PK, startDate, endDate, "0", preference.PK);
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			helper.CreateTaxOrFee("VEX", 0, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(tariff1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var declaration = testHelper.GetTestDeclaration();
			var instruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var instruction2 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._12);
			var instruction3 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._13);
			var instruction4 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._40);
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "FOB";
			invHeader.JZ_InvoiceAmount = 1m;
			invHeader.JZ_RX_NKInvoice_Currency = "ZAR";
			testHelper.AddInvoiceLine(invHeader, instruction1, "99999", 75m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction2, "99999", 0m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction3, "88888", 75m).JI_ZZF_NKTaxType = "VEX";
			testHelper.AddInvoiceLine(invHeader, instruction4, "99999", 75m).JI_ZZF_NKTaxType = "VAT";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(4, declaration.ActiveEntryHeaders.Count);
			var header1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == "11");
			var header2 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == "12");
			var header3 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == "13");
			var header4 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == "40");
			CombineAssertions(() =>
			{
				AssertEquals("header1 CustomsDuty", 22.5m, header1.CustomsDuty);
				AssertEquals("header1 ValueAddedTax", 14.7m, header1.ValueAddedTax);
				AssertEquals("header1 CH_PaymentMethod", ZString.Empty, header1.CH_PaymentMethod);
				AssertEquals("header2 CustomsDuty", 0.0m, header2.CustomsDuty);
				AssertEquals("header2 ValueAddedTax", 0.14m, header2.ValueAddedTax);
				AssertEquals("header2 CH_PaymentMethod", ZString.Empty, header2.CH_PaymentMethod);
				AssertEquals("header3 CustomsDuty", 0m, header3.CustomsDuty);
				AssertEquals(" header3 ValueAddedTax", 0m, header3.ValueAddedTax);
				AssertEquals("header3 CH_PaymentMethod", "F", header3.CH_PaymentMethod);
				AssertEquals("header4 CustomsDuty", ZDecimal.Zero, header4.CustomsDuty);
				AssertEquals("header4 ValueAddedTax", ZDecimal.Zero, header4.ValueAddedTax);
				AssertEquals(22.5m, header4.MergedLines.Cast<CusEntryLine>().Sum(x => x.Fees.GetAmountIncludingLCOnly("1P1")));
				AssertEquals(14.7m, header4.MergedLines.Cast<CusEntryLine>().Sum(x => x.Fees.GetAmountIncludingLCOnly("VAT")));
				AssertEquals("header4 CH_PaymentMethod", "F", header4.CH_PaymentMethod);
			});
		}

		[TestDate(2016, 04, 01)]
		public void TestPaymentMethodDefaulting_WithFAN()
		{
			helper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateBasicTariff();
			var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var procedure3 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "13", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var procedure4 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "14", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var startDate = ZDateTime.Today.AddYears(0);
			var endDate = ZDateTime.Today.AddYears(1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, testHelper.TariffType1P1.PK, "88888", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff2Rate = helper.CreateRate(tariff2, testHelper.RateCode_ZA_DTY_D.PK, startDate, endDate, "0", testHelper.Preference.PK);
			var countryZA = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "ZA");
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.SetAgentCode(countryZA, "AGT");
			helper.CreateCusApplicability(tariff2Rate, testHelper.TradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping = LineMergerTestHelper.AddMapping(collection, testAgent.PK, "BBR", "3234002346", importerPays: true, 1);
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_CustomsOffice = "BBR";
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var instruction2 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._12);
			var instruction3 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._13);
			var instruction4 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._14);
			instruction4.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			instruction4.CEI_ProvisionalPaymentAmount = 15m;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "FOB";
			invHeader.JZ_InvoiceAmount = 1m;
			invHeader.JZ_RX_NKInvoice_Currency = "ZAR";
			testHelper.AddInvoiceLine(invHeader, instruction1, "99999", 75m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction2, "88888", 0m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction3, "88888", 75m).JI_ZZF_NKTaxType = "VEX";
			testHelper.AddInvoiceLine(invHeader, instruction4, "88888", 75m).JI_ZZF_NKTaxType = "VEX";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(4, declaration.ActiveEntryHeaders.Count);
			var header1 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "11") as CusEntryHeader;
			var header2 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "12") as CusEntryHeader;
			var header3 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "13") as CusEntryHeader;
			var header4 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "14") as CusEntryHeader;
			CombineAssertions(() =>
			{
				AssertEquals(22.5m, header1.CustomsDuty);
				AssertEquals(14.7m, header1.ValueAddedTax);
				AssertEquals("D", header1.CH_PaymentMethod);
				AssertEquals(0m, header2.CustomsDuty);
				AssertEquals(0.14m, header2.ValueAddedTax);
				AssertEquals("D", header2.CH_PaymentMethod);
				AssertEquals(0m, header3.CustomsDuty);
				AssertEquals(0m, header3.ValueAddedTax);
				AssertEquals("F", header3.CH_PaymentMethod);
				AssertEquals(0m, header4.CustomsDuty);
				AssertEquals(0m, header4.ValueAddedTax);
				AssertEquals("C", header4.CH_PaymentMethod);
			});
		}

		[TestDate(2016, 04, 01)]
		public void TestPaymentMethodDefaulting_WithCashFAN()
		{
			testHelper.CreateBasicTariff();
			var startDate = ZDateTime.Today.AddYears(0);
			var endDate = ZDateTime.Today.AddYears(1);
			var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var procedure3 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "13", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var procedure4 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "14", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, testHelper.TariffType1P1.PK, "88888", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff2Rate = helper.CreateRate(tariff2, testHelper.RateCode_ZA_DTY_D.PK, startDate, endDate, "0", testHelper.Preference.PK);
			var countryZA = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "ZA");
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.SetAgentCode(countryZA, "AGT");
			helper.CreateCusApplicability(tariff2Rate, testHelper.TradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping = LineMergerTestHelper.AddMapping(collection, testAgent.PK, ZString.Empty, "3234002346", importerPays: true, 1);
			mapping.Cash = true;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_CustomsOffice = "BBR";
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var instruction2 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._12);
			var instruction3 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._13);
			var instruction4 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._14);
			instruction4.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			instruction4.CEI_ProvisionalPaymentAmount = 15m;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "FOB";
			invHeader.JZ_InvoiceAmount = 1m;
			invHeader.JZ_RX_NKInvoice_Currency = "ZAR";
			testHelper.AddInvoiceLine(invHeader, instruction1, "99999", 75m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction2, "88888", 0m).JI_ZZF_NKTaxType = "VAT";
			testHelper.AddInvoiceLine(invHeader, instruction3, "88888", 75m).JI_ZZF_NKTaxType = "VEX";
			testHelper.AddInvoiceLine(invHeader, instruction4, "88888", 75m).JI_ZZF_NKTaxType = "VEX";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(4, declaration.ActiveEntryHeaders.Count);
			var header1 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "11") as CusEntryHeader;
			var header2 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "12") as CusEntryHeader;
			var header3 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "13") as CusEntryHeader;
			var header4 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "14") as CusEntryHeader;
			CombineAssertions(() =>
			{
				AssertEquals(22.5m, header1.CustomsDuty);
				AssertEquals(14.7m, header1.ValueAddedTax);
				AssertEquals(ZString.Empty, header1.CH_PaymentMethod);
				AssertEquals(0m, header2.CustomsDuty);
				AssertEquals(0.14m, header2.ValueAddedTax);
				AssertEquals(ZString.Empty, header2.CH_PaymentMethod);
				AssertEquals(0m, header3.CustomsDuty);
				AssertEquals(0m, header3.ValueAddedTax);
				AssertEquals("F", header3.CH_PaymentMethod);
				AssertEquals(0m, header4.CustomsDuty);
				AssertEquals(0m, header4.ValueAddedTax);
				AssertEquals("C", header4.CH_PaymentMethod);
			});
			header1.CH_PaymentMethod = "C";
			header2.CH_PaymentMethod = "C";
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals(22.5m, header1.CustomsDuty);
				AssertEquals(14.7m, header1.ValueAddedTax);
				AssertEquals("C", header1.CH_PaymentMethod);
				AssertEquals(0m, header2.CustomsDuty);
				AssertEquals(0.14m, header2.ValueAddedTax);
				AssertEquals("C", header2.CH_PaymentMethod);
				AssertEquals(0m, header3.CustomsDuty);
				AssertEquals(0m, header3.ValueAddedTax);
				AssertEquals("F", header3.CH_PaymentMethod);
				AssertEquals(0m, header4.CustomsDuty);
				AssertEquals(0m, header4.ValueAddedTax);
				AssertEquals("C", header4.CH_PaymentMethod);
			});
		}

		[TestDate(2016, 04, 01)]
		public void TestPaymentMethodDefaulting_AmountDueDifferenceZeroOrLess()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			helper.CreateCustomsOfficeCusCodeEntry("BBR");
			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
			var countryZA = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "ZA");
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.SetAgentCode(countryZA, "AGT");
			var startDate = ZDateTime.Today.AddYears(0);
			var endDate = ZDateTime.Today.AddYears(1);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99999", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.3 * VFD", preference.PK);
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var testApplicability1 = helper.CreateCusApplicability(tariff1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			LineMergerTestHelper.AddMapping(collection, testAgent.PK, "BBR", "3234002346", importerPays: true, 1);
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_CustomsOffice = "BBR";
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "FOB";
			invHeader.JZ_InvoiceAmount = 1m;
			invHeader.JZ_RX_NKInvoice_Currency = "ZAR";
			testHelper.AddInvoiceLine(invHeader, instruction1, tariff1.ZZ1_TariffCode, 75m).JI_ZZF_NKTaxType = "VAT";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			var header1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == "11");
			CombineAssertions(() =>
			{
				AssertEquals(22.5m, header1.CustomsDuty);
				AssertEquals(14.7m, header1.ValueAddedTax);
				AssertEquals("D", header1.CH_PaymentMethod);
			});
			header1.CustomsDutyExcluding12BBefore = 25m;
			header1.ValueAddedTaxBefore = 12.2m;
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals(-2.5m, header1.CustomsDutyExcluding12BDifference);
				AssertEquals(2.5m, header1.ValueAddedTaxDifference);
				AssertEquals(0m, header1.AmountDueDifference);
				AssertEquals("F", header1.CH_PaymentMethod);
			});
			header1.CustomsDutyExcluding12BBefore = 30m;
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals(-7.5m, header1.CustomsDutyExcluding12BDifference);
				AssertEquals(-5.0m, header1.AmountDueDifference);
				AssertEquals("F", header1.CH_PaymentMethod);
			});
		}

		[TestDate(2018, 04, 03)]
		public void TestPaymentMethodDefaulting_Deferred()
		{
			helper.CreateCustomsOfficeCusCodeEntry("DFM");
			Factory.Save();
			var selection = new AutomaticDeferredSelection()
			{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 2 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, selection))
			{
				var agent = Factory.NewWithValidTestData<OrgHeader>();
				agent.CompanyData.OB_IsCreditor = true;
				agent.OH_FullName = "AGENT01";
				agent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
				Factory.Save();
				var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				var mapping1 = LineMergerTestHelper.AddMapping(maps, agent.PK, "DFM", "1111111111", importerPays: false, 5);
				mapping1.DutyDefermentAmount = 1000;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				var declaration = testHelper.GetTestDeclarationWithInvoiceLine();
				declaration.JE_DateOfArrival = ZDateTime.Now.AddDays(5);
				var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
				var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty, "Duty");
				var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
				var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
				var query = new ZQuery(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
				query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, "STANDARD");
				var tradeGroup = Factory.LoadTop1<CusRefTradeGroupView>(query);
				var startDate = ZDateTime.Today.AddYears(0);
				var endDate = ZDateTime.Today.AddYears(1);
				var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "88888", startDate, endDate, taxOrFeeCode: "VAT");
				var tariffRate = helper.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.0 * VFD", preference.PK);
				var applicability = helper.CreateCusApplicability(tariffRate, tradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
				Factory.Save();
				declaration.DoMerge();
				Factory.Save();
				CombineAssertions("Duty+VAT", () =>
				{
					var header = declaration.ActiveEntryHeaders[0];
					AssertEquals("CustomsDuty", 22.5m, header.CustomsDuty);
					AssertEquals("ValueAddedTax", 14.7m, header.ValueAddedTax);
					AssertEquals("CH_PaymentMethod", PaymentMethodCodeList.Codes.Defer, header.CH_PaymentMethod);
				});
				var invLine1 = declaration.InvoiceLines[0];
				invLine1.JI_Tariff = "88888";
				declaration.DoMerge();
				Factory.Save();
				CombineAssertions("VAT Only", () =>
				{
					var header = declaration.ActiveEntryHeaders[0];
					AssertEquals("CustomsDuty", 0m, header.CustomsDuty);
					AssertEquals("ValueAddedTax", 11.48m, header.ValueAddedTax);
					AssertEquals("CH_PaymentMethod", PaymentMethodCodeList.Codes.Defer, header.CH_PaymentMethod);
				});
			}
		}

		[TestDate(2018, 04, 03)]
		public void TestPaymentMethodDefaulting_DeferredMultipleEntries()
		{
			helper.CreateCustomsOfficeCusCodeEntry("DFM");
			Factory.Save();
			var selection = new AutomaticDeferredSelection()
			{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 2 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, selection))
			{
				var agent = Factory.NewWithValidTestData<OrgHeader>();
				agent.CompanyData.OB_IsCreditor = true;
				agent.OH_FullName = "AGENT01";
				agent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
				Factory.Save();
				var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				var mapping1 = LineMergerTestHelper.AddMapping(maps, agent.PK, "DFM", "1111111111", importerPays: false, 5);
				mapping1.DutyDefermentAmount = 1000;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				var declaration = testHelper.GetTestDeclarationWithInvoiceLine();
				declaration.JE_DateOfArrival = ZDateTime.Now.AddDays(5);
				var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
				var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty, "Duty");
				var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
				var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
				var query = new ZQuery(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
				query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, "STANDARD");
				var tradeGroup = Factory.LoadTop1<CusRefTradeGroupView>(query);
				var startDate = ZDateTime.Today.AddYears(0);
				var endDate = ZDateTime.Today.AddYears(1);
				var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "88888", startDate, endDate, taxOrFeeCode: "VAT");
				var tariffRate = helper.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.0 * VFD", preference.PK);
				var applicability = helper.CreateCusApplicability(tariffRate, tradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
				var procedure12 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
				var procedure13 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "13", "00", "", "", Common.ZA.ZAJobMessageTypeList.Codes.Import);
				Factory.Save();
				var instruction12 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._12);
				var instruction13 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._13);
				var invHeader2 = declaration.Invoices.AddNew();
				invHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invHeader2.JZ_InvoiceAmount = 1m;
				invHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				invHeader2.JZ_InvoiceNumber = "INV2";
				var invLine2 = testHelper.AddInvoiceLine(invHeader2, instruction12, "99999", 45m);
				invLine2.JI_ZZF_NKTaxType = TaxOrFeeTypeCode.VAT;
				var invHeader3 = declaration.Invoices.AddNew();
				invHeader3.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invHeader3.JZ_InvoiceAmount = 1m;
				invHeader3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				invHeader3.JZ_InvoiceNumber = "INV2";
				var invLine3 = testHelper.AddInvoiceLine(invHeader3, instruction13, "88888", 45m);
				invLine3.JI_ZZF_NKTaxType = TaxOrFeeTypeCode.VEX;
				declaration.DoMerge();
				Factory.Save();
				CombineAssertions("Duty+VAT", () =>
				{
					var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
					var header1 = entryHeaders.FirstOrDefault(a => a.CustomsProcedureCode == "11");
					var header2 = entryHeaders.FirstOrDefault(a => a.CustomsProcedureCode == "12");
					var header3 = entryHeaders.FirstOrDefault(a => a.CustomsProcedureCode == "13");
					AssertEquals("header1 CustomsDuty", 22.5m, header1.CustomsDuty);
					AssertEquals("header1 ValueAddedTax", 14.7m, header1.ValueAddedTax);
					AssertEquals("header1 CH_PaymentMethod", PaymentMethodCodeList.Codes.Defer, header1.CH_PaymentMethod);
					AssertEquals("header2 CustomsDuty", 13.5m, header2.CustomsDuty);
					AssertEquals("header2 ValueAddedTax", 8.82m, header2.ValueAddedTax);
					AssertEquals("header2 CH_PaymentMethod", PaymentMethodCodeList.Codes.Defer, header2.CH_PaymentMethod);
					AssertEquals("header3 CustomsDuty", 0.0m, header3.CustomsDuty);
					AssertEquals("header3 ValueAddedTax", 0.0m, header3.ValueAddedTax);
					AssertEquals("header3 CH_PaymentMethod", PaymentMethodCodeList.Codes.Free, header3.CH_PaymentMethod);
				});
				var invLine1 = declaration.InvoiceLines[0];
				invLine1.JI_Tariff = "88888";
				declaration.DoMerge();
				Factory.Save();
				CombineAssertions("VAT Only", () =>
				{
					var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
					var header1 = entryHeaders.FirstOrDefault(a => a.CustomsProcedureCode == "11");
					var header2 = entryHeaders.FirstOrDefault(a => a.CustomsProcedureCode == "12");
					var header3 = entryHeaders.FirstOrDefault(a => a.CustomsProcedureCode == "13");
					AssertEquals("header1 CustomsDuty", 0m, header1.CustomsDuty);
					AssertEquals("header1 ValueAddedTax", 11.48m, header1.ValueAddedTax);
					AssertEquals("header1 CH_PaymentMethod", PaymentMethodCodeList.Codes.Defer, header1.CH_PaymentMethod);
					AssertEquals("header2 CustomsDuty", 13.5m, header2.CustomsDuty);
					AssertEquals("header2 ValueAddedTax", 8.82m, header2.ValueAddedTax);
					AssertEquals("header2 CH_PaymentMethod", PaymentMethodCodeList.Codes.Defer, header2.CH_PaymentMethod);
					AssertEquals("header3 CustomsDuty", 0.0m, header3.CustomsDuty);
					AssertEquals("header3 ValueAddedTax", 0.0m, header3.ValueAddedTax);
					AssertEquals("header3 CH_PaymentMethod", PaymentMethodCodeList.Codes.Free, header3.CH_PaymentMethod);
				});
			}
		}

		[TestDate(2018, 04, 03)]
		public void TestPaymentMethodDefaulting_DeferredWithLinkedAccount()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			helper.CreateCustomsOfficeCusCodeEntry("DFM");
			Factory.Save();
			var messageDeferralSettings = new AutomaticDeferredSelection()
			{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 5 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var orgheader1 = Factory.NewWithValidTestData<OrgHeader>();
				orgheader1.CompanyData.OB_IsCreditor = true;
				orgheader1.OH_FullName = "ORG1";
				orgheader1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
				var orgheader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgheader2.CompanyData.OB_IsCreditor = true;
				orgheader2.OH_FullName = "ORG2";
				orgheader2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "22334455", Core.Constants.CountryCodes.SouthAfrica);
				var orgheader3 = Factory.NewWithValidTestData<OrgHeader>();
				orgheader3.CompanyData.OB_IsCreditor = true;
				orgheader3.OH_FullName = "ORG3";
				orgheader3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "33445566", Core.Constants.CountryCodes.SouthAfrica);
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.OH_FullName = "IMP1";
				importer.CompanyData.OB_IsCreditor = true;
				importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "99999999", Core.Constants.CountryCodes.SouthAfrica);
				Factory.Save();
				var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				var mapping1 = LineMergerTestHelper.AddMapping(maps, importer.PK, "DFM", "1111111111", importerPays: true, 5);
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				var declaration = testHelper.GetTestDeclarationWithInvoiceLine();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(10);
				declaration.JE_OH_Importer = importer.PK;
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("Importers agent is selected", importer.PK, declaration.JE_OH_AgentOverride);
				AssertContains("CH_BGMReference", "99999999", declaration.ActiveEntryHeaders[0].CH_BGMReference);
				AssertEquals("Payment is Defer", PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				var mapping2 = LineMergerTestHelper.AddMapping(maps, orgheader2.PK, "DFM", "2222222222", importerPays: true, 5);
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				declaration.JE_OH_AgentOverride = orgheader1.PK;
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("When Agent has no 'imp' mapping linked to the Importer the agent is changed to one that does", importer.PK, declaration.JE_OH_AgentOverride);
				AssertContains("CH_BGMReference", "99999999", declaration.ActiveEntryHeaders[0].CH_BGMReference);
				AssertEquals("Payment is Defer", PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				var mapping3 = LineMergerTestHelper.AddMapping(maps, orgheader3.PK, "DFM", "3333333333", importerPays: false, 5);
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("Importers agent is selected even when deferrable accounts exist", importer.PK, declaration.JE_OH_AgentOverride);
				AssertContains("CH_BGMReference", "99999999", declaration.ActiveEntryHeaders[0].CH_BGMReference);
				AssertEquals("Payment is Defer", PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				mapping1.OrganizationPK = orgheader1.PK;
				maps.RemoveAndDelete(mapping3);
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("Agent remains as was set in previous merge or on dec", importer.PK, declaration.JE_OH_AgentOverride);
				AssertEquals("Payment Method is left empty when a suitable account cannot be found", ZString.Empty, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);

				LineMergerTestHelper.AddMapping(maps, orgheader3.PK, "DFM", "4444444444", importerPays: false, 5);
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("JE_MessageType is IMP, agent is set to not ImporterPays account when no matched Importer account exists", orgheader3.PK, declaration.JE_OH_AgentOverride);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_CustomsOffice = "DFM";
				declaration.JE_TransportMode = "";
				declaration.JE_OH_AgentOverride = ZGuid.Empty;
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("JE_MessageType is EXW, agent is set to not ImporterPays account when no matched Importer account exists", orgheader3.PK, declaration.JE_OH_AgentOverride);
			}
		}

		[TestDate(2018, 04, 12)]
		public void TestPaymentMethodDefaulting_DeferredWithoutLinkedAccount()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			helper.CreateCustomsOfficeCusCodeEntry("DFM");
			Factory.Save();
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			agent1.CompanyData.OB_IsCreditor = true;
			agent1.OH_FullName = "AGENT01";
			agent1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			agent2.CompanyData.OB_IsCreditor = true;
			agent2.OH_FullName = "AGENT02";
			agent2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "22334455", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var selection = new AutomaticDeferredSelection()
			{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 4 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, selection))
			{
				var declaration = testHelper.GetTestDeclarationWithInvoiceLine();
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_DateOfArrival = new ZDateTime(2018, 04, 18);
				Factory.Save();
				DeferredWithoutLinkedAccountMapsTest1(declaration, agent1, agent2);
				DeferredWithoutLinkedAccountMapsTest2(declaration, agent1, agent2);
				DeferredWithoutLinkedAccountMapsTest3(declaration, agent1, agent2);
				DeferredWithoutLinkedAccountTestWithSecondDeclaration(declaration, agent1, agent2);
			}
		}

		void DeferredWithoutLinkedAccountMapsTest1(JobDeclaration declaration, OrgHeader agent1, OrgHeader agent2)
		{
			var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			LineMergerTestHelper.AddMapping(maps, agent1.PK, "DFM", "1111111111", importerPays: false, 6);
			LineMergerTestHelper.AddMapping(maps, agent2.PK, "DFM", "2222222222", importerPays: false, 5);
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("No sufficient balance, choose first account", agent1.PK, declaration.JE_OH_AgentOverride);
			AssertEquals("Should select first agent", "11223344", declaration.AgentCode);
			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			AssertContains("CH_BGMReference", "11223344", cusEntryHeader.CH_BGMReference);
			AssertEquals("Payment method is VAT when no balance", PaymentMethodCodeList.Codes.VATOnly, cusEntryHeader.CH_PaymentMethod);
			AssertEquals("ValueAddedTax should not be 0", 22.5m, cusEntryHeader.CustomsDuty);
			AssertEquals("Duty should not be 0", 14.7m, cusEntryHeader.ValueAddedTax);
		}

		void DeferredWithoutLinkedAccountMapsTest2(JobDeclaration declaration, OrgHeader agent1, OrgHeader agent2)
		{
			var maps2 = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			LineMergerTestHelper.AddMapping(maps2, agent1.PK, "DFM", "1111111111", importerPays: false, 5);
			LineMergerTestHelper.AddMapping(maps2, agent2.PK, "DFM", "2222222222", importerPays: false, 5).DutyDefermentAmount = 100;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps2);
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Only one has sufficient balance, choose it", agent2.PK, declaration.JE_OH_AgentOverride);
			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			AssertContains("CH_BGMReference", "22334455", cusEntryHeader.CH_BGMReference);
			AssertEquals("Payment method is Defer when account has balance", PaymentMethodCodeList.Codes.Defer, cusEntryHeader.CH_PaymentMethod);
		}

		void DeferredWithoutLinkedAccountMapsTest3(JobDeclaration declaration, OrgHeader agent1, OrgHeader agent2)
		{
			var maps3 = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			LineMergerTestHelper.AddMapping(maps3, agent1.PK, "DFM", "1111111111", importerPays: false, 5).DutyDefermentAmount = 1000;
			LineMergerTestHelper.AddMapping(maps3, agent2.PK, "DFM", "2222222222", importerPays: false, 8).DutyDefermentAmount = 100;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps3);
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("More than one have sufficient balance, choose the latest one", agent2.PK, declaration.JE_OH_AgentOverride);
			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			AssertContains("CH_BGMReference", "22334455", cusEntryHeader.CH_BGMReference);
		}

		void DeferredWithoutLinkedAccountTestWithSecondDeclaration(JobDeclaration declaration, OrgHeader agent1, OrgHeader agent2)
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "22334455DFM20180412";
			var payInfo1 = entry1.EntryPayInfos.AddNew();
			payInfo1.C9_PaymentAmount = 200.0m;
			payInfo1.C9_PaymentDate = new ZDateTime(2018, 04, 10);
			payInfo1.C9_TransactionType = CusEntryPayTypes.Duty;
			payInfo1.C9_PaymentParty = "D";
			var payInfo2 = entry1.EntryPayInfos.AddNew();
			payInfo2.C9_PaymentAmount = 100.0m;
			payInfo2.C9_PaymentDate = new ZDateTime(2018, 04, 10);
			payInfo2.C9_TransactionType = CusEntryPayTypes.Pending;
			var payInfo3 = entry1.EntryPayInfos.AddNew();
			payInfo3.C9_PaymentAmount = 100.0m;
			payInfo3.C9_PaymentDate = new ZDateTime(2018, 04, 10);
			payInfo3.C9_TransactionType = CusEntryPayTypes.ValueAddedTax;
			Factory.Save();
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Have EntryPayInfos, choose the sufficient account", agent1.PK, declaration.JE_OH_AgentOverride);
			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			AssertContains("CH_BGMReference", "11223344", cusEntryHeader.CH_BGMReference);
			var maps4 = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var mapping7 = LineMergerTestHelper.AddMapping(maps4, agent1.PK, "DFM", "1111111111", importerPays: false, 5);
			mapping7.VatDefermentAmount = 0m;
			mapping7.DutyDefermentAmount = 1000;
			var mapping8 = LineMergerTestHelper.AddMapping(maps4, agent2.PK, "DFM", "2222222222", importerPays: false, 8);
			mapping8.VatDefermentAmount = 0m;
			mapping8.DutyDefermentAmount = 330;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps4);
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Only one has sufficient balance, choose it", agent1.PK, declaration.JE_OH_AgentOverride);
			AssertContains("CH_BGMReference", "11223344", cusEntryHeader.CH_BGMReference);
			declaration.JE_OH_AgentOverride = agent2.PK;
			Factory.Save();
			AssertContains("CH_BGMReference has been changed", "22334455", cusEntryHeader.CH_BGMReference);
			cusEntryHeader.MessageStatus = Common.ZA.ZAMessageStatusList.Codes.Acknowledged;
			declaration.JE_OH_AgentOverride = agent1.PK;
			Factory.Save();
			AssertContains("CH_BGMReference has not been changed", "22334455", cusEntryHeader.CH_BGMReference);
		}

		[TestDate(2018, 04, 12)]
		public void TestPaymentMethodDefaulting_DeferredWithoutLinkedAccount_MultipleEntries()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, value: true);
			helper.CreateCustomsOfficeCusCodeEntry("DFM");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			Factory.Save();
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			agent1.CompanyData.OB_IsCreditor = true;
			agent1.OH_FullName = "AGENT01";
			agent1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			agent2.CompanyData.OB_IsCreditor = true;
			agent2.OH_FullName = "AGENT02";
			agent2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "22334455", Core.Constants.CountryCodes.SouthAfrica);
			var agent3 = Factory.NewWithValidTestData<OrgHeader>();
			agent3.CompanyData.OB_IsCreditor = true;
			agent3.OH_FullName = "AGENT03";
			agent3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "33445566", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var selection = new AutomaticDeferredSelection()
			{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 4 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, selection))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var declaration = testHelper.GetTestDeclarationWithInvoiceLine();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_DateOfArrival = new ZDateTime(2018, 04, 18);
				declaration.JE_OH_AgentOverride = agent2.PK;
				declaration.DoMerge();
				Factory.Save();
				AssertPaymentMethodNoneAndAgentUnchanged(declaration, agent2);
				var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				var mapping1 = LineMergerTestHelper.AddMapping(maps, agent1.PK, "DFM", "1111111111", importerPays: false, 6);
				var mapping2 = LineMergerTestHelper.AddMapping(maps, agent2.PK, "DFM", "2222222222", importerPays: false, 5);
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				declaration.DoMerge();
				Factory.Save();
				AssertFirstAccountAndAgent(declaration, agent1);

				SetupAndAssertForMultipleEntries(declaration, agent1, agent2, maps, mapping1, mapping2);
				SetupAndAssertWithAdditionalDeclaration(declaration, agent1, agent2, maps, mapping1, mapping2);
			}
		}

		void AssertPaymentMethodNoneAndAgentUnchanged(JobDeclaration declaration, OrgHeader agent)
		{
			var entryHeader1 = declaration.ActiveEntryHeaders[0];
			AssertEquals("Payment method is none when has no deferrable mappings", ZString.Empty, entryHeader1.CH_PaymentMethod);
			AssertEquals("Not deferrable, agent is unchanged", agent.PK, declaration.JE_OH_AgentOverride);
			AssertContains("CH_BGMReference", "", entryHeader1.CH_BGMReference);
			AssertEquals("Duty should not be 0", 22.5m, entryHeader1.CustomsDuty);
			AssertEquals("ValueAddedTax should not be 0", 14.7m, entryHeader1.ValueAddedTax);
		}

		void AssertFirstAccountAndAgent(JobDeclaration declaration, OrgHeader agent)
		{
			var entryHeader1 = declaration.ActiveEntryHeaders[0];
			AssertEquals("No sufficient balance, choose first account", agent.PK, declaration.JE_OH_AgentOverride);
			AssertEquals("Should select first agent", "11223344", declaration.AgentCode);
			AssertContains("CH_BGMReference", "11223344", entryHeader1.CH_BGMReference);
			AssertEquals("Payment method is VAT when no balance", PaymentMethodCodeList.Codes.VATOnly, entryHeader1.CH_PaymentMethod);
			AssertEquals("Duty should not be 0", 22.5m, entryHeader1.CustomsDuty);
			AssertEquals("ValueAddedTax should not be 0", 14.7m, entryHeader1.ValueAddedTax);
		}

		void SetupAndAssertForMultipleEntries(JobDeclaration declaration, OrgHeader agent1, OrgHeader agent2, FinancialAccountNumberPortMapCollection maps, FinancialAccountNumberPortMap mapping1, FinancialAccountNumberPortMap mapping2)
		{
			var instruction2 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._12);
			var invHeader2 = declaration.Invoices.AddNew();
			invHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invHeader2.JZ_InvoiceAmount = 1m;
			invHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invHeader2.JZ_InvoiceNumber = "INV2";
			var invLine2 = testHelper.AddInvoiceLine(invHeader2, instruction2, "99999", 45m);
			invLine2.JI_ZZF_NKTaxType = TaxOrFeeTypeCode.VAT;
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Should have multiple entries", 2, declaration.ActiveEntryHeaders.Count);
			AssertEquals("No sufficient balance, choose first account", agent1.PK, declaration.JE_OH_AgentOverride);
			AssertEquals("Should select first agent", "11223344", declaration.AgentCode);
			var entryHeader1 = declaration.ActiveEntryHeaders[0];
			AssertContains("CH_BGMReference", "11223344", entryHeader1.CH_BGMReference);
			AssertEquals("Payment method is VAT when no balance", PaymentMethodCodeList.Codes.VATOnly, entryHeader1.CH_PaymentMethod);
			AssertEquals("Duty should not be 0", 22.5m, entryHeader1.CustomsDuty);
			AssertEquals("ValueAddedTax should not be 0", 14.7m, entryHeader1.ValueAddedTax);
			var entryHeader2 = declaration.ActiveEntryHeaders[1];
			AssertContains("CH_BGMReference", "11223344", entryHeader2.CH_BGMReference);
			AssertEquals("Payment method is VAT when no balance", PaymentMethodCodeList.Codes.VATOnly, entryHeader2.CH_PaymentMethod);
			AssertEquals("Duty should not be 0", 13.5m, entryHeader2.CustomsDuty);
			AssertEquals("ValueAddedTax should not be 0", 8.82m, entryHeader2.ValueAddedTax);
			mapping1.DutyDefermentAmount = 30;
			mapping2.DutyDefermentAmount = 40;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Total Duty", 36.0m, entryHeader1.CustomsDuty + entryHeader2.CustomsDuty);
			AssertEquals("Total VAT", 23.52m, entryHeader1.ValueAddedTax + entryHeader2.ValueAddedTax);
			AssertEquals("Only one has sufficient balance, choose it", agent2.PK, declaration.JE_OH_AgentOverride);
			AssertContains("CH_BGMReference", "22334455", entryHeader1.CH_BGMReference);
			AssertEquals("Payment method is Defer when account has balance", PaymentMethodCodeList.Codes.Defer, entryHeader1.CH_PaymentMethod);
			mapping1.DutyDefermentAmount = 140;
			mapping2.DutyDefermentAmount = 140;
			mapping2.AccountStartDay = 8;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Total Duty", 36.0m, entryHeader1.CustomsDuty + entryHeader2.CustomsDuty);
			AssertEquals("Total VAT", 23.52m, entryHeader1.ValueAddedTax + entryHeader2.ValueAddedTax);
			AssertEquals("More than one have sufficient balance, choose the latest one", agent2.PK, declaration.JE_OH_AgentOverride);
			AssertContains("CH_BGMReference", "22334455", entryHeader1.CH_BGMReference);
		}

		void SetupAndAssertWithAdditionalDeclaration(JobDeclaration declaration, OrgHeader agent1, OrgHeader agent2, FinancialAccountNumberPortMapCollection maps, FinancialAccountNumberPortMap mapping1, FinancialAccountNumberPortMap mapping2)
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "22334455DFM20180412";
			var payInfo1 = entry1.EntryPayInfos.AddNew();
			payInfo1.C9_PaymentAmount = 200.0m;
			payInfo1.C9_PaymentDate = new ZDateTime(2018, 04, 10);
			payInfo1.C9_TransactionType = CusEntryPayTypes.Duty;
			payInfo1.C9_PaymentParty = "D";
			var payInfo2 = entry1.EntryPayInfos.AddNew();
			payInfo2.C9_PaymentAmount = 100.0m;
			payInfo2.C9_PaymentDate = new ZDateTime(2018, 04, 10);
			payInfo2.C9_TransactionType = CusEntryPayTypes.Pending;
			var payInfo3 = entry1.EntryPayInfos.AddNew();
			payInfo3.C9_PaymentAmount = 100.0m;
			payInfo3.C9_PaymentDate = new ZDateTime(2018, 04, 10);
			payInfo3.C9_TransactionType = CusEntryPayTypes.ValueAddedTax;
			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			var entryHeader1 = declaration.ActiveEntryHeaders[0];
			var entryHeader2 = declaration.ActiveEntryHeaders[1];
			AssertEquals("Total Duty", 36.0m, entryHeader1.CustomsDuty + entryHeader2.CustomsDuty);
			AssertEquals("Total VAT", 23.52m, entryHeader1.ValueAddedTax + entryHeader2.ValueAddedTax);
			AssertEquals("Acct 2 has EntryPayInfos worth 200, chooses account1", agent1.PK, declaration.JE_OH_AgentOverride);
			AssertContains("CH_BGMReference", "11223344", entryHeader1.CH_BGMReference);
			mapping1.VatDefermentAmount = 0m;
			mapping1.DutyDefermentAmount = 260;
			mapping2.VatDefermentAmount = 0m;
			mapping2.DutyDefermentAmount = 250;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Total Duty", 36.0m, entryHeader1.CustomsDuty + entryHeader2.CustomsDuty);
			AssertEquals("Total VAT", 23.52m, entryHeader1.ValueAddedTax + entryHeader2.ValueAddedTax);
			AssertEquals("Only one has balance > 200 + duty + vat", agent1.PK, declaration.JE_OH_AgentOverride);
			AssertContains("CH_BGMReference", "11223344", entryHeader1.CH_BGMReference);
			mapping1.DutyDefermentAmount = 360;
			mapping2.DutyDefermentAmount = 350;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Total Duty", 36.0m, entryHeader1.CustomsDuty + entryHeader2.CustomsDuty);
			AssertEquals("Total VAT", 23.52m, entryHeader1.ValueAddedTax + entryHeader2.ValueAddedTax);
			AssertEquals("Only one has sufficient balance, choose > 300 + duty + vat", agent1.PK, declaration.JE_OH_AgentOverride);
			AssertContains("CH_BGMReference", "11223344", entryHeader1.CH_BGMReference);
			declaration.JE_OH_AgentOverride = agent2.PK;
			Factory.Save();
			AssertContains("CH_BGMReference has been changed", "22334455", entryHeader1.CH_BGMReference);
			entryHeader1.MessageStatus = Common.ZA.ZAMessageStatusList.Codes.Acknowledged;
			declaration.JE_OH_AgentOverride = agent1.PK;
			Factory.Save();
			AssertNotContains("CH_BGMReference has not been changed", "11223344", entryHeader1.CH_BGMReference);
		}

		public void TestGetLineNumberAssigner()
		{
			var declaration = testHelper.GetTestDeclaration();
			var testInst = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var testInst2 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var testInst3 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._11);
			testInst3.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "00";
			var invLin2 = invHeader.InvoiceLines.AddNew();
			invLin2.JI_CEI = testInst2.PK;
			invLin2.JI_Procedure = invLin2.EntryInstruction.CEI_Style + "00";
			invLin2.JI_TargetEntryLineNumber = 5;
			var invLin3 = invHeader.InvoiceLines.AddNew();
			invLin3.JI_CEI = testInst3.PK;
			invLin3.JI_Procedure = invLin2.EntryInstruction.CEI_Style + "00";
			invLin3.JI_TargetEntryLineNumber = 5;
			declaration.DoMerge();
			AssertEquals(3, declaration.ActiveEntryHeaders.Count);
			var lineMerger = new LineMergerForTest(declaration);
			AssertEquals(false, lineMerger.GetLineNumberAssigner_Exposed(invLine.CusEntryLine.Header) is LineNumberAssigner);
			AssertEquals(true, lineMerger.GetLineNumberAssigner_Exposed(invLine.CusEntryLine.Header) is Customs.Business.LineNumberAssigner);
			AssertEquals(false, lineMerger.GetLineNumberAssigner_Exposed(invLin2.CusEntryLine.Header) is LineNumberAssigner);
			AssertEquals(true, lineMerger.GetLineNumberAssigner_Exposed(invLin2.CusEntryLine.Header) is Customs.Business.LineNumberAssigner);
			AssertEquals(true, lineMerger.GetLineNumberAssigner_Exposed(invLin3.CusEntryLine.Header) is LineNumberAssigner);
			AssertEquals(true, lineMerger.GetLineNumberAssigner_Exposed(invLin3.CusEntryLine.Header) is Customs.Business.LineNumberAssigner);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			AssertEquals(true, lineMerger.GetLineNumberAssigner_Exposed(invLine.CusEntryLine.Header) is LineNumberAssignerForIMX);
		}

		public void TestJI_ActualPriceSetWhenMerging()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, "ZAR"));
			currency.ExchangeRates.DeleteAll();
			RefExchangeRate rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = new ZDateTime(2016, 12, 1);
			rate.RE_ExpiryDate = new ZDateTime(2016, 12, 1);
			rate.RE_SellRate = 0.70m;
			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_CustomsOffice = "JHB";
			declaration.AgentCode = "ASBSD";
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 12, 1);
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			invoiceHeader.JZ_InvoiceCurrExRate = 0.7m;
			Factory.Save();
			AssertEquals(0m, invoiceLine1.JI_ActualPrice);
			declaration.DoMerge();
			AssertEquals(1428.57m, invoiceLine1.JI_ActualPrice);
		}

		public void TestMergeSplitsEntriesByBondAmount()
		{
			var bondHolder1 = Factory.NewWithValidTestData<OrgHeader>();
			bondHolder1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "1000", Core.Constants.CountryCodes.SouthAfrica);
			bondHolder1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "100000", "ZA");

			var bondHolder2 = Factory.NewWithValidTestData<OrgHeader>();
			bondHolder2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "2000", Core.Constants.CountryCodes.SouthAfrica);
			bondHolder2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "50000", "ZA");

			var remover = Factory.NewWithValidTestData<OrgHeader>();
			remover.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "3000", Core.Constants.CountryCodes.SouthAfrica);

			var subContractor = Factory.NewWithValidTestData<OrgHeader>();
			subContractor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "4000", Core.Constants.CountryCodes.SouthAfrica);

			var declaration = testHelper.GetTestDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			var entryInstruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._40);
			entryInstruction1.CEI_Description = "CEI 40";
			entryInstruction1.CEI_OH_Carrier = remover.PK;
			entryInstruction1.OH_SubContractor = subContractor.PK;
			entryInstruction1.CEI_OH_BondHolder = bondHolder1.PK;
			entryInstruction1.BHValid = true;

			var entryInstruction2 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._42);
			entryInstruction2.CEI_Description = "CEI 42";
			entryInstruction2.CEI_OH_Carrier = bondHolder2.PK;
			entryInstruction2.BHValid = true;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice1.JZ_InvoiceAmount = 1000m;

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction1.PK;
			var invoiceLine3 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction1.PK;
			var invoiceLine4 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction1.PK;
			var invoiceLine5 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = entryInstruction1.PK;

			var invoiceLine6 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_CEI = entryInstruction2.PK;
			var invoiceLine7 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_CEI = entryInstruction2.PK;
			var invoiceLine8 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine8.JI_CEI = entryInstruction2.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction1.PK;
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;

			AddCusEntryLineWithMockedBNDCore(entryHeader, invoiceLine1, 20000m);
			AddCusEntryLineWithMockedBNDCore(entryHeader, invoiceLine2, 30000m);
			AddCusEntryLineWithMockedBNDCore(entryHeader, invoiceLine3, 60000m);
			invoiceLine4.JI_CL = invoiceLine3.JI_CL;
			AddCusEntryLineWithMockedBNDCore(entryHeader, invoiceLine5, 50000m);
			AddCusEntryLineWithMockedBNDCore(entryHeader2, invoiceLine6, 30000m);
			AddCusEntryLineWithMockedBNDCore(entryHeader2, invoiceLine7, 40000m);
			AddCusEntryLineWithMockedBNDCore(entryHeader2, invoiceLine8, 20000m);

			using (ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (ZACustomsRegistry.Instance.PercentageOfBondAmountUsedBeforeSplit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 90))
			{
				var lineMerger = new LineMergerForTest(declaration);
				lineMerger.OnMergedExposed();
			}

			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Reload(true);
			CombineAssertions(() =>
			{
				var entryInstructions = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Cast<CusEntryInstruction>();
				AssertEquals("Should now be 6 Entry Instructions", 6, entryInstructions.Count());
				AssertEquals("Should now be 6 Entry Headers", 6, declaration.ActiveEntryHeaders.Count);

				AssertEntryInstruction("CEI 40", entryInstructions.ElementAt(0), remover, subContractor, bondHolder1, isRemoverEDI: false);
				AssertEntryInstruction("Copy 1 of CEI 40", entryInstructions.ElementAt(2), remover, subContractor, bondHolder1, isRemoverEDI: false);
				AssertEntryInstruction("Copy 2 of CEI 40", entryInstructions.ElementAt(3), remover, subContractor, bondHolder1, isRemoverEDI: false);

				AssertEntryInstruction("CEI 42", entryInstructions.ElementAt(1), bondHolder2, null, null, isRemoverEDI: true);
				AssertEntryInstruction("Copy 1 of CEI 42", entryInstructions.ElementAt(4), bondHolder2, null, null, isRemoverEDI: true);
				AssertEntryInstruction("Copy 2 of CEI 42", entryInstructions.ElementAt(5), bondHolder2, null, null, isRemoverEDI: true);

				AssertEquals("CEI 40 -> InvoiceLine1", entryInstructions.ElementAt(0).PK, invoiceLine1.JI_CEI);
				AssertEquals("CEI 40 -> InvoiceLine2", entryInstructions.ElementAt(0).PK, invoiceLine2.JI_CEI);
				AssertEquals("Copy 1 of CEI 40 -> InvoiceLine3", entryInstructions.ElementAt(2).PK, invoiceLine3.JI_CEI);
				AssertEquals("Copy 1 of CEI 40 -> InvoiceLine4", entryInstructions.ElementAt(2).PK, invoiceLine4.JI_CEI);
				AssertEquals("Copy 2 of CEI 40 -> InvoiceLine5", entryInstructions.ElementAt(3).PK, invoiceLine5.JI_CEI);
				AssertEquals("CEI 42 -> InvoiceLine6", entryInstructions.ElementAt(1).PK, invoiceLine6.JI_CEI);
				AssertEquals("Copy 1 of CEI 42 -> InvoiceLine7", entryInstructions.ElementAt(4).PK, invoiceLine7.JI_CEI);
				AssertEquals("Copy 2 of CEI 42 -> InvoiceLine8", entryInstructions.ElementAt(5).PK, invoiceLine8.JI_CEI);
			});
		}

		void AssertEntryInstruction(ZString description, CusEntryInstruction entryInstruction, OrgHeader carrier, OrgHeader subContractor, OrgHeader bondHolder, bool isRemoverEDI)
		{
			AssertEquals(description, entryInstruction.CEI_Description);
			AssertEquals($"{description} - CEI_OH_Carrier", carrier?.PK ?? ZGuid.Empty, entryInstruction.CEI_OH_Carrier);
			AssertEquals($"{description} - OH_SubContractor", subContractor?.PK ?? ZGuid.Empty, entryInstruction.OH_SubContractor);
			AssertEquals($"{description} - CEI_OH_BondHolder", bondHolder?.PK ?? ZGuid.Empty, entryInstruction.CEI_OH_BondHolder);
			AssertEquals($"{description} - CEI_RemoverEDI", isRemoverEDI, entryInstruction.CEI_RemoverEDI);
			AssertEquals($"{description} - CEI_SubContractorEDI", !isRemoverEDI, entryInstruction.CEI_SubContractorEDI);
		}

		Moq.Mock<CusEntryLine> AddCusEntryLineWithMockedBNDCore(CusEntryHeader entryHeader, JobComInvoiceLine invoiceLine, decimal bndValue, JobComInvoiceLine additionalInvoiceLine = null)
		{
			var mockEntryLine = Factory.NewMoq<CusEntryLine>();
			mockEntryLine.Protected().Setup<ZDecimal>("BNDCore").Returns(bndValue);
			invoiceLine.JI_CL = mockEntryLine.Object.PK;
			if (additionalInvoiceLine != null)
			{
				additionalInvoiceLine.JI_CL = mockEntryLine.Object.PK;
			}
			entryHeader.MergedLines.Add(mockEntryLine.Object);
			return mockEntryLine;
		}

		public void TestMergeWithMaxEntryLinesForExBond()
		{
			var declaration = testHelper.GetTestDeclaration();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			var entryInstruction1 = testHelper.AddEntryInstruction(declaration, ProcedureCodes._40);
			entryInstruction1.CEI_Description = "CEI 40";

			var invoice1 = declaration.Invoices.FirstOrDefault();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice1.JZ_InvoiceAmount = 1000m;

			var invoiceLines = new JobComInvoiceLine[8];
			for (var i = 0; i < 8; ++i)
			{
				invoiceLines[i] = (JobComInvoiceLine)invoice1.JobComInvoiceLines.AddNew();
				invoiceLines[i].JI_CEI = entryInstruction1.PK;
			}

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction1.PK;

			invoice1.JobComInvoiceLines.Cast<JobComInvoiceLine>().ForEach(line =>
			{
				var mockEntryLine = Factory.NewMoq<CusEntryLine>();
				line.JI_CL = mockEntryLine.Object.PK;
				entryHeader.MergedLines.Add(mockEntryLine.Object);
			});

			using (ZACustomsRegistry.Instance.ExbondMaxNumberEntryLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 3))
			{
				var lineMerger = new LineMergerForTest(declaration);
				lineMerger.OnMergedExposed();
			}

			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Reload(true);
			CombineAssertions(() =>
			{
				var entryInstructions = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Cast<CusEntryInstruction>().ToArray();
				AssertEquals("Should now be 3 Entry Instructions", 3, entryInstructions.Length);
				AssertEquals("Should now be 3 Entry Headers", 3, declaration.ActiveEntryHeaders.Count);
				AssertEquals("CEI 40", 1, entryInstructions.Where(x => x.CEI_Description == "CEI 40").Count());
				AssertEquals("Copy 1 of CEI 40", 1, entryInstructions.Where(x => x.CEI_Description == "Copy 1 of CEI 40").Count());
				AssertEquals("Copy 2 of CEI 40", 1, entryInstructions.Where(x => x.CEI_Description == "Copy 2 of CEI 40").Count());

				AssertEquals("CEI 40 -> InvoiceLine1", entryInstructions[0].PK, invoiceLines[0].JI_CEI);
				AssertEquals("CEI 40 -> InvoiceLine2", entryInstructions[0].PK, invoiceLines[1].JI_CEI);
				AssertEquals("CEI 40 -> InvoiceLine3", entryInstructions[0].PK, invoiceLines[2].JI_CEI);
				AssertEquals("Copy 1 of CEI 40 -> InvoiceLine4", entryInstructions[1].PK, invoiceLines[3].JI_CEI);
				AssertEquals("Copy 1 of CEI 40 -> InvoiceLine5", entryInstructions[1].PK, invoiceLines[4].JI_CEI);
				AssertEquals("Copy 1 of CEI 40 -> InvoiceLine6", entryInstructions[1].PK, invoiceLines[5].JI_CEI);
				AssertEquals("Copy 2 of CEI 40 -> InvoiceLine7", entryInstructions[2].PK, invoiceLines[6].JI_CEI);
				AssertEquals("Copy 2 of CEI 40 -> InvoiceLine8", entryInstructions[2].PK, invoiceLines[7].JI_CEI);
			});
		}

		protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<BaseJobDeclaration>();

		protected override string GetClearStatus()
		{
			helper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			return "1";
		}

		protected override void SetUp()
		{
			helper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper = new LineMergerTestHelper(Factory, helper);
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			base.SetUp();
		}

		ZAUniversalReferenceTestDataHelper helper;
		LineMergerTestHelper testHelper;

		class LineMergerForTest : LineMerger
		{
			public LineMergerForTest(JobDeclaration dec) : base(dec)
			{
			}

			public ILineNumberAssigner GetLineNumberAssigner_Exposed(Customs.Business.CusEntryHeader entryHeader)
			{
				return base.GetLineNumberAssigner(entryHeader);
			}

			public void PerformCountrySpecificOperationAfterMergeAfterCalculateDutyExposed() => PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();

			public void OnMergedExposed() => OnMerged();
		}
	}
}
