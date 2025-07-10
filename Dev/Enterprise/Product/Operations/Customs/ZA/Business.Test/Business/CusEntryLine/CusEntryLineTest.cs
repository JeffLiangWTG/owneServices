using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;
using ECB = Enterprise.Customs.Business;
using ECBT = Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	sealed class CusEntryLineTest : ECBT.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestIsDeclarationIntegrated()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var testDec = Factory.New<JobDeclaration>();
				var entryHeader = testDec.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				testDec.Invoices.AddNew();
				var invoiceLine = testDec.InvoiceLines.AddNew();
				invoiceLine.JI_CustomsQuantity = 123;
				invoiceLine.JI_CustomsUnitQty = "KG";
				invoiceLine.JI_CL = entryLine.PK;
				Assert(entryLine.IsDeclarationIntegrated);
				testDec.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				Assert(!entryLine.IsDeclarationIntegrated);
			}
		}

		public void TestNoNullExceptionOnMRN()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			AssertEquals(ZString.Empty, (entryLine as ILineLevelInformation).PreviousProcedureMRN);
		}

		[TestDate(2015, 6, 1)]
		public void TestRebateDescription_EffectiveDescription()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			// todo: if JE_MergeBy is disregarded in ZA, it should be removed from UI
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.TariffAndDescription;
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			var line2 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "Some description";
			line2.JI_Description = "Some description";
			DoMerge(declaration);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(1, entryHeader.MergedLines.Count);
			AssertEquals("Some description", entryHeader.MergedLines[0].EffectiveDescription);
			line2.JI_Description = "Some other description";
			DoMerge(declaration);
			entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(2, entryHeader.MergedLines.Count);
			AssertEquals("Some description", entryHeader.MergedLines[0].EffectiveDescription);
			AssertEquals("Some other description", entryHeader.MergedLines[1].EffectiveDescription);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304050", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1), description: "Some more description");
			Factory.Save();
			line1.JI_Tariff = "1020304050";
			DoMerge(declaration);
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("Some more description", entryLine.EffectiveDescription);
			line1.JI_Colour = "blue";
			DoMerge(declaration);
			AssertEquals("Some more description Colour; blue", entryLine.EffectiveDescription);
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeAmount = 1.6;
			fee.CF_ChargeType = "4P1";
			AssertEquals("Rebate Amount; 1.60", entryLine.RebateDescription);
		}

		public void TestMergeByTariff_MixedCaseDescription()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "Some description";
			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Description = "Some description";
			DoMerge(declaration);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(1, entryHeader.MergedLines.Count);
			AssertEquals("Some description", entryHeader.MergedLines[0].EffectiveDescription);
			line2.JI_Description = "SOME description";
			DoMerge(declaration);
			entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(1, entryHeader.MergedLines.Count);
			AssertEquals("Some description", entryHeader.MergedLines[0].EffectiveDescription);
			line2.JI_Description = "Some other description";
			DoMerge(declaration);
			entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(1, entryHeader.MergedLines.Count);
			AssertEquals("", entryHeader.MergedLines[0].EffectiveDescription);
		}

		[TestDate(2005, 6, 2)]
		public override void TestMoneyInLocalCurrency()
		{
			RefCurrency newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			ZDateTime from = new ZDateTime(2005, 6, 1);
			ZDateTime to = new ZDateTime(2005, 6, 5);
			newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);
			BaseJobDeclaration declaration = ImportJobDeclaration;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;
			BaseJobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line2.JI_LinePrice = 200.0m;
			BaseInvoiceLineCharge line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line1ONS.J7_Amount = 5.0m;
			BaseInvoiceLineCharge line1OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line1OFT.J7_Amount = 10.0m;
			BaseInvoiceLineCharge line2ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line2ONS.J7_Amount = 10.0m;
			BaseInvoiceLineCharge line2OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line2OFT.J7_Amount = 20.0m;
			DoMerge(declaration);
			ECB.CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(0m, entryLine.FOBInLocalCurrency.Amount);
			AssertEquals(0m, entryLine.CIFInLocalCurrency.Amount);
			AssertEquals(0m, entryLine.OverseasFreightInLocalCurrency.Amount);
			AssertEquals(0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
			AssertEquals(0m, entryLine.TAndIInLocalCurrency.Amount);
		}

		public void TestProcedureMeasure()
		{
			var universalTestHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			var rateType_ZA_REF = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "REF", "Refd");
			universalTestHelper.LoadOrCreateNewCusRateCode(Factory, "6P1", rateType_ZA_REF.PK);
			Factory.Save();
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(2075, 1, 1);
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "64", "00", "6", "", "EXP", "");
			var tariffType1P1 = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			var tariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "6###", startDate, endDate);
			universalTestHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "11", tariff);
			Factory.Save();
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var ceInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			ceInstruction.CEI_Style = "64";
			JobComInvoiceHeader invoice = dec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Procedure = "6400";
			line1.JI_Description = "abcde";
			line1.CusLineTariffDetails.RemoveAll();
			new LineMerger(dec).DoMerge();
			var entryLine = dec.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(ZString.Empty, entryLine.ProcedureMeasure);
			line1.CusLineTariffDetails.AddNew("6P1", "6###");
			AssertEquals("6###11", entryLine.ProcedureMeasure);
		}

		public void TestDescription()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			var testTariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var testTariff = testHelper.CreateTariff("ZA", testTariffType.PK, "123321", new ZDateTime(2016, 1, 1), new ZDateTime(2017, 1, 1), "123321DESC", 1, "VAT");
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "123321";
			line.JI_Description = "LINE";
			DoMerge(declaration);
			ICusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Description", "LINE", entryLine.Description);
		}

		public void TestImportDutyPaid()
		{
			JobDeclaration dec = GetDeclarationWithTwoInvoiceLinesAndOneEntryLine();
			AssertEquals(125.0m, dec.CustomsEntryHeaders[0].MergedLines[0].ImportDutyPaid);
		}

		public void TestDutySch1P2BPaid()
		{
			JobDeclaration dec = GetDeclarationWithTwoInvoiceLinesAndOneEntryLine();
			AssertEquals(45.0m, dec.CustomsEntryHeaders[0].MergedLines[0].ImportDutySch1P2BPaid);
		}

		public void TestImportVATPaid()
		{
			JobDeclaration dec = GetDeclarationWithTwoInvoiceLinesAndOneEntryLine();
			AssertEquals(45.0m, dec.CustomsEntryHeaders[0].MergedLines[0].ImportVATPaid);
		}

		public void TestImportCustomsValue()
		{
			JobDeclaration dec = GetDeclarationWithTwoInvoiceLinesAndOneEntryLine();
			AssertEquals(370.0m, dec.CustomsEntryHeaders[0].MergedLines[0].ImportCustomsValue);
		}

		public void TestAddQuantity()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = testDec.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 123;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CL = entryLine.PK;
			InvoiceLineQuantityCollection quantities = entryLine.CustomsQuantities;
			AssertEquals("Qty", invoiceLine.JI_CustomsQuantity, quantities[0].Quantity);
			AssertEquals("UQ", invoiceLine.JI_CustomsUnitQty, quantities[0].UnitOfQuantity);
		}

		public void TestHasChangesStillFalseAfterGettingAddInfo()
		{
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			//Invoice Line 1
			var line1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var addInfo1ForLine1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1ForLine1.CY_Code = "DDC";
			addInfo1ForLine1.CY_Data = "Certificate";
			entryLine.AdditionalInformationCodes.Add(addInfo1ForLine1);
			var addInfo2ForLine1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2ForLine1.CY_Code = "ATV";
			addInfo2ForLine1.CY_Data = "125";
			entryLine.AdditionalInformationCodes.Add(addInfo2ForLine1);
			var addInfo3ForLine1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo3ForLine1.CY_Code = "VPB";
			addInfo3ForLine1.CY_Data = "897";
			entryLine.AdditionalInformationCodes.Add(addInfo3ForLine1);
			var addInfo4ForLine1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo4ForLine1.CY_Code = "OLI";
			addInfo4ForLine1.CY_Data = "Levy Item";
			entryLine.AdditionalInformationCodes.Add(addInfo4ForLine1);
			line1.JI_CL = entryLine.PK;
			//Invoice Line 2
			var line2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;
			Factory.Save();
			var result = GetAddInfoString(entryLine.AdditionalInformationCodes);
			AssertEquals("AdditionalInformation with two merged lines", "DDC=Certificate^ATV=125^VPB=897^OLI=Levy Item", result.Trim(ECB.CodeInfoCollection.CodeInfoSeparator));
			AssertEquals("HasChanges", false, declaration.HasChanges);
		}

		public void TestCustomsDuty()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			DoMerge(declaration);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			entryLine.Fees.GetOrAddFeeByFeeType("1P1").CF_ChargeAmount = 200.0m;
			entryLine.Fees.GetOrAddFeeByFeeType("VAT").CF_ChargeAmount = 100.0m;
			entryLine.Fees.GetOrAddFeeByFeeType("12A").CF_ChargeAmount = 300.0m;
			AssertEquals(500.0m, entryLine.CustomsDuty);
			AssertEquals(0m, entryLine.VAT);
			entryLine.Fees.GetOrAddFeeByFeeType("12A").CF_ChargeAmount = 0m;
			line.JI_ZZF_NKTaxType = "VAT";
			AssertEquals(200.0m, entryLine.CustomsDuty);
			AssertEquals(100.0m, entryLine.VAT);
		}

		public void TestAdditionalInformation()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			Assert("No Additional Information is the list", entryLine.AdditionalInformationCodes.Count == 0);
		}

		public void TestJI_PreviousEntryLineNumber()
		{
			JobComInvoiceLine invoiceLine = GetMergedInvoiceLine();
			CusEntryLine entryLine = invoiceLine.CusEntryLine;
			AssertEquals(0, entryLine.PreviousEntryLineNumber);
			invoiceLine.JI_PreviousEntryLineNumber = 3;
			AssertEquals(3, entryLine.PreviousEntryLineNumber);
		}

		public void TestJI_CustomsQuantity()
		{
			JobComInvoiceLine invoiceLine = GetMergedInvoiceLine();
			CusEntryLine entryLine = invoiceLine.CusEntryLine;
			AssertEquals(0.01m, entryLine.CustomsQuantity);
			invoiceLine.JI_CustomsQuantity = 123m;
			AssertEquals(123m, entryLine.CustomsQuantity);
		}

		public void TestJI_CustomsUnitQty()
		{
			JobComInvoiceLine invoiceLine = GetMergedInvoiceLine();
			CusEntryLine entryLine = invoiceLine.CusEntryLine;
			invoiceLine.JI_CustomsUnitQty = "M3";
			AssertEquals("M3", entryLine.CustomsUnitQty);
			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("KG", entryLine.CustomsUnitQty);
		}

		public void TestPrimaryPreference()
		{
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure1.ZZ6_ProcedureCode = ProcedureCodes._10;
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure2.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure2.ZZ6_ProcedureCode = ProcedureCodes._41;
			var inst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst1.CEI_Style = ProcedureCodes._10;
			var inst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst2.CEI_Style = ProcedureCodes._41;
			var invoiceLine = GetMergedInvoiceLine();
			var entryLine = invoiceLine.CusEntryLine;
			AssertEquals(ZString.Empty, entryLine.PrimaryPreference);
			invoiceLine.JI_PrimaryPreference = "TA";
			AssertEquals("TA", entryLine.PrimaryPreference);
			invoiceLine.JI_CEI = inst1.PK;
			AssertEquals("TA", entryLine.PrimaryPreference);
			invoiceLine.JI_CEI = inst2.PK;
			AssertEquals("100", entryLine.PrimaryPreference);
		}

		public void TestJI_TakeUpInTradeStatistics()
		{
			JobComInvoiceLine invoiceLine = GetMergedInvoiceLine();
			CusEntryLine entryLine = invoiceLine.CusEntryLine;
			AssertEquals(ZBool.True, entryLine.JI_TakeUpInTradeStatistics);
			invoiceLine.JI_TakeUpInTradeStatistics = ZBool.False;
			AssertEquals(ZBool.False, entryLine.JI_TakeUpInTradeStatistics);
		}

		public void TestJI_Description()
		{
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			AssertEquals(ZString.Empty, entryLine.Description);
			line.JI_Description = "Desc";
			AssertEquals("Desc", entryLine.Description);
		}

		public void TestIsImport()
		{
			var entryLine = Factory.New<CusEntryLine>();
			AssertEquals(false, entryLine.IsImport);
			var entryHeader = Factory.New<CusEntryHeader>();
			entryLine.CL_CH = entryHeader.PK;
			entryHeader.CH_JE = declaration.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals(true, entryLine.IsImport);
		}

		public void TestRoundCustomsValue()
		{
			var header = declaration.Invoices.AddNew();
			header.JobComInvoiceLines.AddNew();
			header.JobComInvoiceLines.AddNew();
			DoMerge(declaration);
			var testEntryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			testEntryLine.CL_CustomsValue = 1.50m;
			testEntryLine.RoundCustomsValue();
			AssertEquals("Round 50c", 1m, testEntryLine.CL_CustomsValue);
			testEntryLine.CL_CustomsValue = 0m;
			testEntryLine.RoundCustomsValue();
			AssertEquals("Less than 1", 1m, testEntryLine.CL_CustomsValue);
			testEntryLine.CL_CustomsValue = 5.51m;
			testEntryLine.RoundCustomsValue();
			AssertEquals("Round 51c", 6m, testEntryLine.CL_CustomsValue);
		}

		public void TestRefreshAdditionalInformation()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry("VIN");
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "10";
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_VIN = "BA111GP";
			DoMerge(declaration);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "VIN", "BA111GP");
			line.JI_VIN = "BA222GP";
			DoMerge(declaration);
			AssertHasNoCustomsCodeWithValue(entryLine.AdditionalInformationCodes, "VIN", "BA111GP");
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "VIN", "BA222GP");
		}

		public void TestProcedureCategory()
		{
			var universalTestHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalTestHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "I", "75", "", "", "", "", "");
			universalTestHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "", "", "", "", "");
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._75;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			line.JI_Procedure = line.EntryInstruction.CEI_Style + ProcedureCodes._00;
			new LineMerger(declaration).DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Category I", UniversalReferenceConstants.ProcedureCategoryCodes._I, entryLine.ProcedureCategory);
			entryInstruction.CEI_Style = ProcedureCodes._40;
			new LineMerger(declaration).DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Category E", UniversalReferenceConstants.ProcedureCategoryCodes._E, entryLine.ProcedureCategory);
		}

		public void TestDutySch1P2B()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			DoMerge(declaration);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var fee = entryLine.Fees.GetOrAddFeeByFeeType("1P1");
			AssertEquals(0m, entryLine.DutySch1P2B);
			fee.CF_ChargeType = "12B";
			fee.CF_ChargeAmount = 200.0m;
			AssertEquals(200.0m, entryLine.DutySch1P2B);
		}

		public void TestProvisionalPaymentAmount_PPEOnHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			entryInstruction.CEI_ProvisionalPaymentAmount = 55m;
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = invHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.InvoiceLines.Add(invoiceLine);
			AssertEquals("PPE Not Removed", 55m, entryLine.ProvisionalPaymentAmount);
			AssertEquals(0m, entryLine.GetCalcFeeValues().ProvisionalPayments.Sum(x => x.Value));
			entryInstruction.CEI_ProvisionalPaymentType = ZString.Empty;
			entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
			var ppe = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", HeaderLevelProvisionalPayments.Codes.PPE, 33m, "1", "REF1");
			AssertEquals("PPE Removed & 'Case Closed' is not ticked", 0m, entryLine.ProvisionalPaymentAmount);
			AssertEquals(0m, entryLine.GetCalcFeeValues().ProvisionalPayments.Sum(x => x.Value));
			ppe.C9_RemAdvReceived = true;
			AssertEquals("PPE Removed & 'Case Closed' is ticked", 0m, entryLine.ProvisionalPaymentAmount);
			AssertEquals(0m, entryLine.GetCalcFeeValues().ProvisionalPayments.Sum(x => x.Value));
		}

		public void TestProvisionalPaymentsAndPenalties()
		{
			CombineAssertions("Export", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				var entryLine = entryHeader.MergedLines.AddNew();
				var pp1 = entryLine.ProvisionalPayments.AddNew("PEN", 0);
				var pp2 = entryLine.ProvisionalPayments.AddNew("PEN", 11.11);
				var pp3 = entryLine.ProvisionalPayments.AddNew("XXT", 22.11);
				var pp4 = entryLine.ProvisionalPayments.AddNew("", 33.11);
				var pp5 = entryLine.ProvisionalPayments.AddNew("PPA", 44.11);
				var pp6 = entryLine.ProvisionalPayments.AddNew("PPA", 55.11);
				var pp7 = entryLine.ProvisionalPayments.AddNew("FOR", 66.11);
				AssertEquals(0m, entryLine.ProvisionalPaymentAmount);
				AssertEquals(0m, entryLine.PenaltyAmount);
			});
			CombineAssertions("Import", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var entryLine = entryHeader.MergedLines.AddNew();
				var pp1 = entryLine.ProvisionalPayments.AddNew("PEN", 0);
				var pp2 = entryLine.ProvisionalPayments.AddNew("PEN", 11.11);
				var pp3 = entryLine.ProvisionalPayments.AddNew("XXT", 22.11);
				var pp4 = entryLine.ProvisionalPayments.AddNew("", 33.11);
				var pp5 = entryLine.ProvisionalPayments.AddNew("PPA", 44.11);
				var pp6 = entryLine.ProvisionalPayments.AddNew("PPA", 55.11);
				var pp7 = entryLine.ProvisionalPayments.AddNew("FOR", 66.11);
				AssertEquals(99.22m, entryLine.ProvisionalPaymentAmount);
				AssertEquals(77.22m, entryLine.PenaltyAmount);
			});
			CombineAssertions("Exbond", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				var entryLine = entryHeader.MergedLines.AddNew();
				var pp1 = entryLine.ProvisionalPayments.AddNew("PEN", 0);
				var pp2 = entryLine.ProvisionalPayments.AddNew("PEN", 11.11);
				var pp3 = entryLine.ProvisionalPayments.AddNew("XXT", 22.11);
				var pp4 = entryLine.ProvisionalPayments.AddNew("", 33.11);
				var pp5 = entryLine.ProvisionalPayments.AddNew("PPA", 44.11);
				var pp6 = entryLine.ProvisionalPayments.AddNew("PPA", 55.11);
				var pp7 = entryLine.ProvisionalPayments.AddNew("FOR", 66.11);
				AssertEquals(99.22m, entryLine.ProvisionalPaymentAmount);
				AssertEquals(77.22m, entryLine.PenaltyAmount);
			});
			CombineAssertions("Line 1 Include Instruction Amount", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
				entryInstruction.CEI_ProvisionalPaymentAmount = 55m;
				var invHeader = declaration.Invoices.AddNew();
				var invLine1 = invHeader.InvoiceLines.AddNew();
				invLine1.JI_CEI = entryInstruction.PK;
				var invLine2 = invHeader.InvoiceLines.AddNew();
				invLine2.JI_CEI = entryInstruction.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = entryHeader.MergedLines.AddNew();
				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				entryLine2.CL_LineNumber = 2;
				entryLine1.InvoiceLines.Add(invLine1);
				entryLine2.InvoiceLines.Add(invLine2);
				entryLine1.ProvisionalPayments.AddNew("PEN", 10m);
				entryLine1.ProvisionalPayments.AddNew("PPA", 20m);
				entryLine2.ProvisionalPayments.AddNew("PEN", 11m);
				entryLine2.ProvisionalPayments.AddNew("PPA", 21m);
				AssertEquals(75m, entryLine1.ProvisionalPaymentAmount);
				AssertEquals(10m, entryLine1.PenaltyAmount);
				AssertEquals(21m, entryLine2.ProvisionalPaymentAmount);
				AssertEquals(11m, entryLine2.PenaltyAmount);
			});
			CombineAssertions("Line 1 not even include Instruction Amount  for export job", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
				entryInstruction.CEI_ProvisionalPaymentAmount = 55m;
				var invHeader = declaration.Invoices.AddNew();
				var invLine1 = invHeader.InvoiceLines.AddNew();
				invLine1.JI_CEI = entryInstruction.PK;
				var invLine2 = invHeader.InvoiceLines.AddNew();
				invLine2.JI_CEI = entryInstruction.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = entryHeader.MergedLines.AddNew();
				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				entryLine2.CL_LineNumber = 2;
				entryLine1.InvoiceLines.Add(invLine1);
				entryLine2.InvoiceLines.Add(invLine2);
				entryLine1.ProvisionalPayments.AddNew("PEN", 10m);
				entryLine1.ProvisionalPayments.AddNew("PPA", 20m);
				entryLine2.ProvisionalPayments.AddNew("PEN", 11m);
				entryLine2.ProvisionalPayments.AddNew("PPA", 21m);
				AssertEquals(0m, entryLine1.ProvisionalPaymentAmount);
				AssertEquals(0m, entryLine1.PenaltyAmount);
				AssertEquals(0m, entryLine2.ProvisionalPaymentAmount);
				AssertEquals(0m, entryLine2.PenaltyAmount);
			});
			CombineAssertions("Not Include Case Closed Type PP", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
				entryInstruction.CEI_ProvisionalPaymentAmount = 55m;
				var invHeader = declaration.Invoices.AddNew();
				var invLine1 = invHeader.InvoiceLines.AddNew();
				invLine1.JI_CEI = entryInstruction.PK;
				var invLine2 = invHeader.InvoiceLines.AddNew();
				invLine2.JI_CEI = entryInstruction.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var pp1 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPE", 0, "1", "REF1", true);
				var pp2 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPA", 0, "1", "REF2", true);
				var pp3 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPC", 0, "2", "REF3", true);
				var pp4 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPA", 0, "2", "REF4", false);
				var pp5 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PEN", 0, "2", "REF5", true);
				var entryLine1 = entryHeader.MergedLines.AddNew();
				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				entryLine2.CL_LineNumber = 2;
				entryLine1.InvoiceLines.Add(invLine1);
				entryLine2.InvoiceLines.Add(invLine2);
				entryLine1.ProvisionalPayments.AddNew("PEN", 10m);
				entryLine1.ProvisionalPayments.AddNew("PPA", 20m);
				entryLine1.ProvisionalPayments.AddNew("PPC", 30m);
				entryLine2.ProvisionalPayments.AddNew("PEN", 11m);
				entryLine2.ProvisionalPayments.AddNew("PPA", 21m);
				entryLine2.ProvisionalPayments.AddNew("PPC", 31m);
				AssertEquals(30m, entryLine1.ProvisionalPaymentAmount);
				AssertEquals(10m, entryLine1.PenaltyAmount);
				AssertEquals(21m, entryLine2.ProvisionalPaymentAmount);
				AssertEquals(0m, entryLine2.PenaltyAmount);
			});
		}

		public void TestGetCalcFeeValues()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", "DTY");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "12B", "EX1");
			var testTariff1 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99991", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff1, "CU1", "KG");
			var testTariff2 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99992", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff2, "CU1", "LI");
			Factory.Save();
			var testOrgDeclaration = Factory.New<JobDeclaration>();
			testOrgDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testOrgDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrgInstruction = testOrgDeclaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			var testOrgInvHeader = testOrgDeclaration.Invoices.AddNew();
			var testOrgInvLine = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine.JI_CustomsQuantity = 150;
			testOrgInvLine.JI_CustomsUnitQty = "KG";
			testOrgInvLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgInvLine2 = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine2.JI_CustomsQuantity = 50;
			testOrgInvLine2.JI_CustomsUnitQty = "KG";
			testOrgInvLine2.JI_ZZF_NKTaxType = "VAT";
			var testOrgEntry = testOrgDeclaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			testOrgInvLine.JI_CL = testEntryLine.PK;
			testOrgInvLine2.JI_CL = testEntryLine.PK;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("1P1", 21);
			testEntryLine.Fees.AddOrUpdate("2P1", 20);
			testEntryLine.Fees.AddOrUpdate("12A", 13);
			testEntryLine.Fees.AddOrUpdate("12B", 22);
			testEntryLine.Fees.AddOrUpdate("VAT", 23);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);
			Factory.Save();
			var tester = testEntryLine.GetCalcFeeValues();
			CombineAssertions(() =>
			{
				AssertEquals(3, tester.CustomsDutiesExcluding12B.Count());
				AssertNotNull(tester.CustomsDutiesExcluding12B.Single(x => x.Code == "1P1" && x.Value == 21m));
				AssertEquals(54m, tester.CustomsDutyExcluding12B);
				AssertEquals(22m, tester.S1P2BDuty);
				AssertEquals(23m, tester.ValueAddedTax);
				AssertEquals(24m, tester.ProvisionalPayment);
				AssertEquals(1, tester.ProvisionalPayments.Count());
				AssertNotNull(tester.ProvisionalPayments.Single(x => x.Code == "PPA" && x.Value == 24m));
				AssertEquals(25m, tester.Penalty);
				AssertEquals(1, tester.Penalties.Count());
				AssertNotNull(tester.Penalties.Single(x => x.Code == "PEN" && x.Value == 25m));
				AssertEquals(41m, tester.CustomsDutiesSchedule1P1andSchedule2);
			});
		}

		public void TestGetOtherDA63Duties()
		{
			var testEntryLine = Factory.New<CusEntryLine>();
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("12A", 2);
			testEntryLine.Fees.AddOrUpdate("12A", 2);
			testEntryLine.Fees.AddOrUpdate("13A", 2);
			testEntryLine.Fees.AddOrUpdate("13B", 2);
			testEntryLine.Fees.AddOrUpdate("13C", 2);
			testEntryLine.Fees.AddOrUpdate("13D", 2);
			testEntryLine.Fees.AddOrUpdate("13E", 2);
			testEntryLine.Fees.AddOrUpdate("15A", 2);
			testEntryLine.Fees.AddOrUpdate("17A", 2);
			testEntryLine.Fees.AddOrUpdate("15B", 2);
			testEntryLine.Fees.AddOrUpdate("2P1", 2);
			testEntryLine.Fees.AddOrUpdate("2P2", 2);
			testEntryLine.Fees.AddOrUpdate("2P3", 2);
			testEntryLine.Fees.AddOrUpdate("VAT", 50m);
			testEntryLine.Fees.AddOrUpdate("VXX", 40m);
			testEntryLine.Fees.AddOrUpdate("XXX", 99.99m);
			var tester = testEntryLine.OtherDA63Duties;
			AssertEquals(24m, tester);
		}

		public void TestGSTVATAmount()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var fee = entryLine.Fees.AddOrUpdate("VAT", 50m);
			var fee2 = entryLine.Fees.AddOrUpdate("VXX", 40m);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals("BLT- No InvoiceLine", 0m, entryLine.GSTVATAmount);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals("ITF - No InvoiceLine", 50m, entryLine.GSTVATAmount);
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = "VXX";
			entryLine.InvoiceLines.Add(invoiceLine);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals("BLT- InvoiceLine - VXX", 40m, entryLine.GSTVATAmount);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals("ITF - InvoiceLine - VXX", 50m, entryLine.GSTVATAmount);
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals("BLT- InvoiceLine - VAT", 50m, entryLine.GSTVATAmount);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals("ITF - InvoiceLine - VAT", 50m, entryLine.GSTVATAmount);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			ICusCodeDataTypeSupporter supporter = entryLine;
			supporter.AssertType(null, CusCodeDataTypeList.Codes.VOCValueAfter);
			supporter.AssertType(null, CusCodeDataTypeList.Codes.VOCValueBefore);
			supporter.AssertType(typeof(ProvisionalPaymentAmountCodeData), CusCodeDataTypeList.Codes.PPAmount);
			supporter.AssertType(typeof(AdditionalInformation), CusCodeDataTypeList.Codes.AdditionalInformation);
			supporter.AssertType(null, CusCodeDataTypeList.Codes.VPBAmount);
			supporter.AssertType(null, CusCodeDataTypeList.Codes.CaseNumber);
			supporter.AssertType(null, "ZZ!");
			var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			additionalInfo.CY_Data = "DSD";
			var pp = entryLine.ProvisionalPayments.AddNew();
			pp.CY_Code = "PPA";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(additionalInfo.PK);
			AssertEquals(typeof(AdditionalInformation), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(pp.PK);
			AssertEquals(typeof(ProvisionalPaymentAmountCodeData), codeData.GetType());
		}

		public void TestMergedCustomsQuantityisGreaterOrEqualsThanzerodotzerooneWithSmallCustomsQuantity()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "NON";
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			testInst.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			var testInvoice = declaration.Invoices.AddNew();
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInst.PK;
			testInvoiceLine1.JI_TargetEntryLineNumber = 2;
			testInvoiceLine1.JI_CustomsQuantity = 0.005;
			var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine2.JI_CEI = testInst.PK;
			testInvoiceLine2.JI_TargetEntryLineNumber = 2;
			testInvoiceLine2.JI_CustomsQuantity = 0.001;
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			var line = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			AssertEquals(0.01m, line.CustomsQuantity);
		}

		public void TestMergedCustomsQuantityisGreaterOrEqualsThanzerodotzeroone()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "NON";
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			testInst.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			var testInvoice = declaration.Invoices.AddNew();
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInst.PK;
			testInvoiceLine1.JI_TargetEntryLineNumber = 2;
			testInvoiceLine1.JI_CustomsQuantity = 0.05;
			var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine2.JI_CEI = testInst.PK;
			testInvoiceLine2.JI_TargetEntryLineNumber = 2;
			testInvoiceLine2.JI_CustomsQuantity = 0.02;
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			var line = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			AssertEquals(0.05m, line.CustomsQuantity);
		}

		public void TestIsSpecifiedMotorVehicle()
		{
			var universalHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "13A,13B,13C,13D,4", "", "IMP");
			universalHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "", "", "", "IMP");
			var tariffType1P1 = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType4P1 = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P1");
			Factory.Save();
			var tariff = universalHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010102030", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			universalHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.SpecifiedMotorVehicle, "true", tariff);
			var tariff4P1 = universalHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "4100101010", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			universalHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.PRCC, "true", tariff4P1);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "NON";
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			testInst.CEI_DateForDuty = ZDate.Today;
			var testInvoice = declaration.Invoices.AddNew();
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInst.PK;
			testInvoiceLine1.JI_CustomsQuantity = 0.05;
			testInvoiceLine1.JI_Tariff = "1010102030";
			declaration.DoMerge();
			AssertEquals(true, declaration.ActiveEntryHeaders[0].AllEntryLines[0].IsSpecifiedMotorVehicle);
			testInvoiceLine1.JI_Tariff = "4100101010";
			AssertEquals(false, declaration.ActiveEntryHeaders[0].AllEntryLines[0].IsSpecifiedMotorVehicle);
		}

		public void TestConcurrencyExceptionHandlingForAddInfo()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			entryLine.CL_VPBAmount = 1m;
			Factory.RefreshEnabled = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var entryLineReloaded = newFactory.Load<CusEntryLine>(entryLine.PK);
			entryLineReloaded.CL_VPBAmount = 2m;
			newFactory.RefreshEnabled = false;
			newFactory.Save();

			entryLine.Delete();
			var ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => Factory.Save());
			ZExceptionReporting.HandleSaveException(ex);
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			new LineMerger((JobDeclaration)declaration).DoMerge();
		}

		protected override void SetUp()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		}

		JobDeclaration declaration;
		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().JI_CL = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew().PK;
			return dec.CustomsEntryHeaders[0].MergedLines[0];
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bO = base.GetNewBusinessObjectForDeleteTest(factory);
			ECBT.TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(bO.Factory);
			return bO;
		}
		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		JobDeclaration GetDeclarationWithTwoInvoiceLinesAndOneEntryLine()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().JI_CL = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew().PK;
			JobComInvoiceLine line = dec.InvoiceLines[0];
			line.JI_ImportDutyPaid = 100.0m;
			line.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 20.0m);
			line.JI_ImportVATPaid = 20.0m;
			line.JI_ImportCustomsValue = 250.0m;
			JobComInvoiceLine line1 = dec.Invoices[0].JobComInvoiceLines.AddNew();
			line1.JI_CL = dec.CustomsEntryHeaders[0].MergedLines[0].PK;
			line1.JI_ImportDutyPaid = 25.0m;
			line1.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 25.0m);
			line1.JI_ImportVATPaid = 25.0m;
			line1.JI_ImportCustomsValue = 120.0m;
			return dec;
		}

		void AssertHasNoCustomsCodeWithValue(AdditionalInformationCollection collection, ZString cusCodeToCheck, ZString value)
		{
			if (collection.Count > 0)
			{
				bool found = false;
				foreach (AdditionalInformation item in collection)
				{
					if (item.CY_Code == cusCodeToCheck && item.CY_Data == value)
					{
						found = true;
					}
				}

				if (!found)
				{
					Assert("Code With Value Not Found", true);
				}
			}
			else
			{
				Assert("Empty Collection", true);
			}
		}

		void AssertHasCustomsCode(AdditionalInformationCollection addInfoArr, string cusCode, string value)
		{
			bool cusCodeFound = false;
			foreach (AdditionalInformation addInfo in addInfoArr)
			{
				cusCodeFound = (addInfo.CY_Code.ToUpper() == cusCode.ToUpper());
				if (cusCodeFound)
				{
					AssertEquals(cusCode + "Value:", value, addInfo.CY_Data);
					return;
				}
			}

			Assert(cusCode + " does not Exists", cusCodeFound);
		}

		JobComInvoiceLine GetMergedInvoiceLine()
		{
			var header = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "8708.93.25 3";
			line.JI_CL = entryLine.PK;
			return line;
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";
			foreach (JobComInvoiceLine line in declaration.InvoiceLines)
			{
				line.JI_CEI = testInstruction.PK;
			}
		}

		ZString GetAddInfoString(AdditionalInformationCollection addInfoArray)
		{
			var result = ZString.Empty;
			foreach (AdditionalInformation currentInfo in addInfoArray)
			{
				result += currentInfo.CY_Code + "=" + currentInfo.CY_Data + ECB.CodeInfoCollection.CodeInfoSeparator;
			}

			return result;
		}
	}
}
