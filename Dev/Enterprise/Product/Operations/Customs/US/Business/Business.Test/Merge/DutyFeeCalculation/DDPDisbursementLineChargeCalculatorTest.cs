using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DDPDisbursementChargeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2019, 07, 12)]
		public void TestCalculateExemptMPFInCombinedLines()
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsn = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, hsn.PK, "99038801", startDate, endDate);
			var ruleNmae = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "RULE", "RULE", Core.Constants.CountryCodes.UnitedStates, hsn.ZZI_TariffType);
			var a99 = helper.CreateNewOrGetExistingTariffAttribute(ruleNmae.ZY6_Name, TariffRuleList.Codes.AdditionalTariffs, tariff);
			Factory.Save();

			var chapter98HelperTest = new Chapter98HelperTest();

			invoice.JZ_InvoiceAmount = 300m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = chapter98HelperTest.Test99038801Tariff.UE_Tariff;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.US_SupTariff = chapter98HelperTest.Test9817002000Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = "8507500000";
			invoiceLine2.JI_LinePrice = 500000.00m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var dddCharges = invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge);
			AssertEquals("There is no 'DDD' Charge, because the duty and MPF are exempt.", 0, dddCharges.Length);
		}

		[TestDate(2019, 07, 12)]
		public void TestCalculateHMFInCombinedLines()
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsn = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, hsn.PK, "99038801", startDate, endDate);
			var ruleNmae = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "RULE", "RULE", Core.Constants.CountryCodes.UnitedStates, hsn.ZZI_TariffType);
			var a99 = helper.CreateNewOrGetExistingTariffAttribute(ruleNmae.ZY6_Name, TariffRuleList.Codes.AdditionalTariffs, tariff);
			Factory.Save();

			var chapter98HelperTest = new Chapter98HelperTest();
			declaration.US_IsHMFApplicable = "Y";

			invoice.JZ_InvoiceAmount = 300m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = chapter98HelperTest.Test99038801Tariff.UE_Tariff;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.US_SupTariff = chapter98HelperTest.Test9817002000Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = "8507500000";
			invoiceLine2.JI_LinePrice = 500000.00m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var hmfFee = invoiceLine2.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertNotNull("This invoice line should have '501' fee", hmfFee);

			var dddCharges = invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge);
			Assert("This invoice should have 'DDD' charge", dddCharges.Length == 1);

			AssertEquals(hmfFee.CY_FeeAmount, dddCharges[0].J7_Amount);
		}

		[TestDate(2019, 07, 12)]
		public void TestCalculateDDDChargeInCombinedLines()
		{
			var chapter98HelperTest = new Chapter98HelperTest();
			chapter98HelperTest.Test99038801Tariff.UE_Column1RateAdValorem = 0.25;
			chapter98HelperTest.Test99038802Tariff.UE_Column1RateAdValorem = 0.25;
			Factory.Save();

			declaration.US_IsHMFApplicable = "Y";

			invoice.JZ_InvoiceAmount = 300m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = chapter98HelperTest.Test99038801Tariff.UE_Tariff;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.US_SupTariff = chapter98HelperTest.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = "3701200060";
			invoiceLine2.JI_LinePrice = 10000.00m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var provDutyOnLine1 = invoiceLine.US_SupDuty;
			AssertEquals(1394.50m, provDutyOnLine1);

			var hmfFee = invoiceLine2.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertNotNull("This invoice line should have '501' fee", hmfFee);
			var hmfFeeAmount = hmfFee.CY_FeeAmount;
			AssertEquals(6.97m, hmfFeeAmount);

			var mpfFee = invoiceLine2.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertNotNull("This invoice line should have '499' fee", mpfFee);
			var mpfFeeAmount = mpfFee.CY_FeeAmount;
			AssertEquals(19.32m, mpfFeeAmount);

			var provDutyOnLine2 = invoiceLine2.US_SupDuty;
			AssertEquals(2789m, provDutyOnLine2);
			var dutyOnLine2 = invoiceLine2.US_Duty;
			AssertEquals(206.39m, dutyOnLine2);

			var dddCharges = invoiceLine2.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge);
			Assert("This invoice should have 'DDD' charge", dddCharges.Length == 1);

			AssertEquals("4418.56", CalculateMPFDifference(declaration), dddCharges[0].J7_Amount - (provDutyOnLine1 + hmfFeeAmount + mpfFeeAmount + provDutyOnLine2 + dutyOnLine2));
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateForInformalEntry()
		{
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			invoice.JZ_InvoiceAmount = 300m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.US_UC_NKCountryOfOrigin = "DE";

			var oft = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 10m, "USD");
			oft.J7_IsIncludedInITOT = true;

			var lch = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.LandingCharges, 10m, "USD");
			lch.J7_IsIncludedInITOT = true;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "3920995000";
			invoiceLine.JI_CustomsQuantity = 100m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200m;
			invoiceLine2.JI_Tariff = "3920995000";
			invoiceLine2.JI_CustomsQuantity = 100m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var dddAmount = invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency());

			AssertEquals(dddAmount, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateAndAggregateRoundedAmounts()
		{
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoice.JZ_InvoiceAmount = 91212.46m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.US_UC_NKCountryOfOrigin = "DE";

			InvoiceCharge oft = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 5220.00m, "USD");
			oft.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 86090.00m;
			invoiceLine.JI_Tariff = "4803.00.4000";
			invoiceLine.JI_CustomsQuantity = 27335.00000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 780.00m;
			invoiceLine2.JI_Tariff = "4808.90.4000";
			invoiceLine2.JI_CustomsQuantity = 95.00000m;

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 4342.46m;
			invoiceLine3.JI_Tariff = "4823.90.8600";
			invoiceLine3.JI_CustomsQuantity = 862.00000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Invoice DDP aggregated", 287.12m, invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency()));

			AssertEquals(287.12m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		public void TestCalculateForFTZ()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.FTZEntry;

			AssertEquals("Invoice DDP aggregated", 12.50m, invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency()));
		}

		[TestDate(2009, 6, 1)]
		public void TestDDDDoesNotMatchActualDisbursementAmount()
		{
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;

			invoice.JZ_InvoiceAmount = 27078.40m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			declaration.TopGroupInvoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 157.50m, "USD");

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 27078.40m;
			invoiceLine.JI_Tariff = "0101.10.0010";
			invoiceLine.JI_CustomsQuantity = 1m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("DDD amount", 56.41m, invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency()));
			AssertEquals(56.41m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		public void TestCalculateWithOverriddenFees()
		{
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoice.JZ_InvoiceAmount = 100000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.US_UC_NKCountryOfOrigin = "DE";

			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 700.88m, "USD");
			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.LandingCharges, 125.00m, "USD");

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 16248.60m;
			invoiceLine.JI_Tariff = "4203.30.0000";
			invoiceLine.JI_CustomsQuantity = 27335.00000m;
			invoiceLine.US_SPI = "AU";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			foreach (FeeCusCodeData feeData in invoiceLine.FeeCusCodes)
			{
				feeData.CY_IsOverridden = true;
			}

			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_Duty = 100m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var amount = invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency());
			AssertEquals("Total payble", 120.29m, amount);
			AssertEquals("Invoice DDP aggregated", declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_TotalPaid, amount);
		}

		[TestDate(2009, 6, 1)]
		public void TestCalculateWithOverriddenSupDuty()
		{
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000.00m;
			invoiceLine.US_SupTariff = "9904.02.37";//8.8% 
			invoiceLine.JI_Tariff = "0201.30.80 10";//26.4% 
			invoiceLine.JI_CustomsQuantity = 1500m;

			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_Duty = 100m;

			invoiceLine.US_OverrideSupDuty = true;
			invoiceLine.US_SupDuty = 200m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(325m, invoice.GroupCharges[0].J7_Amount);
		}

		public void TestCalculateForDDPWithOverriddenADD_CVD()
		{
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.US_UC_NKCountryOfOrigin = "IT";

			InvoiceCharge lch = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.LandingCharges, 702.29m, "USD");
			lch.J7_IsIncludedInITOT = true;

			InvoiceCharge oft = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 352.50m, "USD");
			oft.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000.00m;

			invoiceLine.US_ADDCaseNo = "A475828000";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDuty = 200m;
			Assert(invoiceLine.IsADDManual);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var dddCharge = invoiceLine.ApportionedCharges.GetCharge("DDD")[0];
			AssertEquals("Should not include ADD/CVD overridden", entry.MPFAmountForEntry + entry.TotalAntidumpingDuty, dddCharge.J7_Amount);
		}

		public void TestChargeBalance()
		{
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoice.JZ_InvoiceAmount = 16248.60m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.US_UC_NKCountryOfOrigin = "DE";

			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 700.88m, "USD");
			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.LandingCharges, 125.00m, "USD");

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 16248.60m;
			invoiceLine.JI_Tariff = "4203.30.0000";
			invoiceLine.JI_CustomsQuantity = 27335.00000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Invoice DDP aggregated", declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_TotalPaid, invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency()));

			string message;
			Assert("balanced", declaration.Invoices.AreChargesBalancedForInvoices(out message));
		}

		[TestDate(2022, 12, 06)]
		public void TestCalculateWithRateGreaterOne()
		{
			var chapter98HelperTest = new Chapter98HelperTest();
			chapter98HelperTest.Test99038801Tariff.UE_Column1RateAdValorem = 0.25;

			var tariff7307923010 = CreateNewTariffForTest("7317006560", "7", 0m);

			var adCaseNumber = Factory.New<USCACCase>();
			adCaseNumber.U5_CaseNumber = "A570055000";
			adCaseNumber.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			adCaseNumber.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			adCaseNumber.U5_ISOCountryCode = "SI";
			adCaseNumber.CaseTariffs.AddNew().U9_TariffNumber = "7317006560";
			var caseRate = adCaseNumber.CaseRates.AddNew();
			caseRate.U6_AddedDate = new ZDateTime(2022, 11, 01);
			caseRate.U6_EffectiveDate = new ZDateTime(2022, 11, 30);
			caseRate.U6_AdValoremRate = 2.6340m;
			Factory.Save();

			invoice.JZ_InvoiceAmount = 203120.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.US_UC_NKCountryOfOrigin = "IT";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000.00m;
			invoiceLine.JI_Tariff = "7317.00.6560";
			invoiceLine.US_SupTariff = chapter98HelperTest.Test99038801Tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 11600m;

			invoiceLine.US_ADDCaseNo = "A570055000";
			invoiceLine.US_ADDDepositRateIndicator = "A"; //263.40%

			invoice.US_DeductADDCVDDuty = YesNoDefaultList.Codes.Yes;
			AssertNoExceptionThrown(() => declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
			AssertEquals("Disbursement Cost", 759.90m, invoiceLine.ApportionedCharges.GetCharge("DDD", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Customs Value", 240.10m, invoiceLine.JI_CustomsValue);
			AssertEquals("Prov/Prog. Duty", 96.80m, invoiceLine.US_SupDuty);
		}

		[TestDate(2025, 04, 22)]
		public void TestCalculateDutyForCombinedLinesWithRateGreaterOne()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "6505002590", "7", 0.075m, "DOZ");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038803", "7", 0.25m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030163", "7", 1.25m, ZString.Empty);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2025, 03, 04), new ZDateTime(2079, 06, 06), "ALL IMPORTS OF ARTICLES THAT ARE PRODUCTS OF CHINA AND HONG KONG", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);
			var tariffView99038803 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038803", new ZDateTime(2022, 10, 14), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038803);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);
			var tariffView99030163 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030163", new ZDateTime(2025, 04, 09), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030163);
			helper.CreateTaxOrFee(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 0.3464m, Core.Constants.CountryCodes.UnitedStates, 32.71m, 634.62m, "AVL", new ZDateTime(2024, 10, 01), new ZDateTime(2079, 01, 01), "Merchandise Processing Fee");

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 75m;
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoice.JZ_InvoiceCurrExRate = 1.2923m;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.SupTariffFormatted = "9903.01.63";
			parentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.UnitedKingdom;
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			var childLine = invoice.JobComInvoiceLines.AddNew();
			childLine.JI_ParentID = parentLine.PK;
			childLine.JI_FormattedTariff = "6505.00.2590";
			childLine.SupTariffFormatted = "9903.88.03";
			childLine.SupFormattedAdditionalTariff1 = "9903.01.24";
			childLine.JI_LinePrice = 75m;
			AssertNoExceptionThrown(() => declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));

			AssertEquals("US_CustomsValue for parent line should be zero", 0m, parentLine.US_CustomsValue);
			AssertEquals("US_CustomsValue for child line should be 23 = (75*1.2923-32.71)/(1+0.075+0.25+1.25+0.2)", 23m, childLine.US_CustomsValue);
		}

		[TestDate(2017, 12, 1)]
		public void TestCalculateForDDPWithADD_CVD()
		{
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoice.JZ_InvoiceAmount = 203120.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.US_UC_NKCountryOfOrigin = "IT";

			InvoiceCharge lch = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.LandingCharges, 702.29m, "USD");
			lch.J7_IsIncludedInITOT = true;

			InvoiceCharge oft = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 352.50m, "USD");
			oft.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 203120.00m;
			invoiceLine.JI_Tariff = "7307.23.0000";
			invoiceLine.JI_CustomsQuantity = 11600m;

			invoiceLine.US_ADDCaseNo = "A475828000";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var dddCharge = invoiceLine.ApportionedCharges.GetCharge("DDD")[0];
			AssertEquals("Should not include ADD/CVD", entry.MPFAmountForEntry + entry.HMFAmountForEntry + entry.TotalDutyAmount, dddCharge.J7_Amount);

			invoice.US_DeductADDCVDDuty = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var dddChargeDeductADDCVDDuty = invoiceLine.ApportionedCharges.GetCharge("DDD")[0];
			AssertEquals("Should include ADD/CVD", entry.MPFAmountForEntry + entry.HMFAmountForEntry + entry.TotalDutyAmount + entry.TotalAntidumpingDuty, dddCharge.J7_Amount);
		}

		[TestDate(2009, 6, 1)]
		public void TestCalculateForTwoInvoicesWhenOneOfInvoicesIsDDP()
		{
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoice.JZ_InvoiceAmount = 2673.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			InvoiceCharge oft = invoice.Charges.AddNew("OFT", 369.35m, "USD");
			oft.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2673.00m;
			invoiceLine.JI_Tariff = "4803.00.4000";
			invoiceLine.JI_CustomsQuantity = 812.00000m;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "EXW";
			invoice2.JZ_InvoiceAmount = 203249.03m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice2.Charges.AddNew("OFT", 5000m, "USD");

			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 203249.03m;
			invoiceLine2.JI_Tariff = "8439.91.9000";

			AssertNoExceptionThrown(() => declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));

			AssertEquals("Disbursement Cost", 7.69m, invoiceLine.ApportionedCharges.GetCharge("DDD", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Disbursement Cost", 7.69m, invoice.GroupCharges.GetCharge("DDD", JobDeclaration.GetLocalCurrency()));
		}

		[TestDate(2009, 12, 12)]
		public void TestAdditionalDutyTariffCalculation()
		{
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000.00m;
			invoiceLine.US_SupTariff = "9904.02.37";//8.8% 
			invoiceLine.JI_Tariff = "0201.30.80 10";//26.4% 
			invoiceLine.JI_CustomsQuantity = 1500m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(invoice.GroupCharges[0].J7_Amount, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		[TestDate(2009, 6, 1)]
		public void TestCalculateForNormalCase()
		{
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			InvoiceCharge oft = invoice.Charges.AddNew("OFT", 500m, "USD");
			oft.J7_IsIncludedInITOT = true;

			InvoiceCharge ons = invoice.Charges.AddNew("ONS", 10m, "USD");
			ons.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "6206.90.0040";//0.06700000
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_CustomsSecondQuantity = 560m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("DDD", invoiceLine.ApportionedCharges[2].J7_ChargeType);
			AssertEquals(619.36m, invoiceLine.ApportionedCharges[2].J7_Amount);
			AssertEquals("IsIncludedInLines", true, invoiceLine.ApportionedCharges[2].J7_IsIncludedInITOT);

			AssertEquals("DDD", invoice.GroupCharges[0].J7_ChargeType);
			AssertEquals(619.36m, invoice.GroupCharges[0].J7_Amount);
			AssertEquals("IsIncludedInLines", true, invoice.GroupCharges[0].J7_IsIncludedInITOT);

			AssertEquals("JI_CustomsValue", 8870.64m, invoiceLine.JI_CustomsValue);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Still one charge", 1, invoice.GroupCharges.Count);
			AssertEquals(619.36m, invoice.GroupCharges[0].J7_Amount);

			AssertEquals(619.36m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		[TestDate(2009, 6, 1)]
		public void TestCalculateForAbnormalCase()
		{
			invoice.JZ_InvoiceAmount = 7923.20m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2300.80m;
			invoiceLine.JI_Tariff = "1806207300";
			AssertNotNull(invoiceLine.ImportTariff);

			invoiceLine.JI_CustomsQuantity = 960m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(473.18m, invoice.GroupCharges[0].J7_Amount);

			invoiceLine.US_SupTariff = "9904.17.22";
			AssertNotNull(invoiceLine.ImportSupTariff);
			Assert(invoiceLine.ImportSupTariff.Applies(TariffRuleList.Codes.AdditionalTariffs, ZDateTime.Today));

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(473.18m, invoice.GroupCharges[0].J7_Amount);
		}

		public void TestClearAndEmptyExistingSystemPopulatedDDDCharges()
		{
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_CustomsSecondQuantity = 560m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("DDD", invoiceLine.ApportionedCharges[0].J7_ChargeType);
			AssertEquals(true, invoiceLine.ApportionedCharges[0].J7_IsSystem);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0, invoiceLine.ApportionedCharges.Count);
		}

		[TestDate(2009, 6, 1)]
		public void TestDoNotCalculateIfThereIsNonDutiableUserEnteredDDD()
		{
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			InvoiceCharge oft = invoice.Charges.AddNew("OFT", 500m, "USD");
			oft.J7_IsIncludedInITOT = true;

			InvoiceCharge ons = invoice.Charges.AddNew("ONS", 10m, "USD");
			ons.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_CustomsSecondQuantity = 560m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("DDD", invoiceLine.ApportionedCharges[2].J7_ChargeType);
			AssertEquals(619.36m, invoiceLine.ApportionedCharges[2].J7_Amount);
			AssertEquals("IsIncludedInLines", true, invoiceLine.ApportionedCharges[2].J7_IsIncludedInITOT);

			AssertEquals("DDD", invoice.GroupCharges[0].J7_ChargeType);
			AssertEquals(619.36m, invoice.GroupCharges[0].J7_Amount);
			AssertEquals("IsIncludedInLines", true, invoice.GroupCharges[0].J7_IsIncludedInITOT);

			AssertEquals("JI_CustomsValue", 8870.64m, invoiceLine.JI_CustomsValue);

			InvoiceLineCharge dddUserEntered = invoiceLine.Charges.AddNew();
			dddUserEntered.J7_ChargeType = USCustomsChargeTypeList.Codes.DisbursementCharge;
			dddUserEntered.J7_Amount = 610m;
			dddUserEntered.J7_IsIncludedInITOT = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("only two charges are apportioned:OFT, ONS", 2, invoiceLine.ApportionedCharges.Count);
			AssertEquals("JI_CustomsValue", 8880m, invoiceLine.JI_CustomsValue);
		}

		[TestDate(2009, 6, 1)]
		public void TestCalculateForNormalCaseWhenApproximateDDDExists()
		{
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			InvoiceCharge oft = invoice.Charges.AddNew("OFT", 500m, "USD");
			oft.J7_IsIncludedInITOT = true;

			InvoiceCharge ons = invoice.Charges.AddNew("ONS", 10m, "USD");
			ons.J7_IsIncludedInITOT = true;

			InvoiceCharge approximateDDD = invoice.Charges.AddNew("DDD", 700m, "USD");
			approximateDDD.J7_IsIncludedInITOT = false;
			approximateDDD.J7_IsDutiable = true;//because it is an approximate

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 9300.00m;
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_CustomsSecondQuantity = 560m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			InvoiceLineApportionCharge apportionedApproximateDDD = null, calculatedDDD = null;

			foreach (InvoiceLineApportionCharge charge in invoiceLine.ApportionedCharges)
			{
				if (charge.J7_ChargeType == "DDD")
				{
					if (charge.J7_Amount == 700m)
					{
						apportionedApproximateDDD = charge;
					}
					else
					{
						calculatedDDD = charge;
					}
				}
			}

			AssertEquals(619.36m, calculatedDDD.J7_Amount);
			AssertEquals("This is an overriding charge", true, calculatedDDD.J7_AdjustedCharge);
			AssertEquals("IsIncludedInLines should be copied from an approximate charge", false, calculatedDDD.J7_IsIncludedInITOT);

			AssertEquals(700.00m, apportionedApproximateDDD.J7_Amount);
			AssertEquals("should be on for adjusted charge", false, apportionedApproximateDDD.J7_AdjustedCharge);
			AssertEquals("IsIncludedInLines should be copied from an approximate charge", false, apportionedApproximateDDD.J7_IsIncludedInITOT);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Still one charge", 1, invoice.GroupCharges.Count);
			AssertEquals(619.36m, invoice.GroupCharges[0].J7_Amount);

			AssertEquals(619.36m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		[TestDate(2009, 6, 1)]
		public void TestCalculateForDerivedDutyCalculation()
		{
			invoice.JZ_InvoiceAmount = 24000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			InvoiceCharge oft = invoice.Charges.AddNew("OFT", 990m, "USD");
			oft.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8211.10.0000";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";

			JobComInvoiceLine secondary1 = invoiceLine.AddSecondaryInvoiceLine();
			secondary1.JI_Tariff = "8211.92.9045";
			secondary1.JI_LinePrice = 16000m;
			secondary1.JI_CustomsQuantity = 16000m;

			JobComInvoiceLine secondary2 = invoiceLine.AddSecondaryInvoiceLine();
			secondary2.JI_Tariff = "8211.93.0030";
			secondary2.JI_LinePrice = 8000m;
			secondary2.JI_CustomsQuantity = 4000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("DDD", secondary1.ApportionedCharges[1].J7_ChargeType);
			AssertEquals(1193.63m, secondary1.ApportionedCharges[1].J7_Amount);
			AssertEquals("IsIncludedInLines", true, secondary1.ApportionedCharges[1].J7_IsIncludedInITOT);
			AssertEquals(14146.37m, secondary1.JI_CustomsValue);

			AssertEquals("DDD", secondary2.ApportionedCharges[1].J7_ChargeType);
			AssertEquals(596.81m, secondary2.ApportionedCharges[1].J7_Amount);
			AssertEquals("IsIncludedInLines", true, secondary2.ApportionedCharges[1].J7_IsIncludedInITOT);
			AssertEquals(7073.19m, secondary2.JI_CustomsValue);

			AssertEquals("DDD", invoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("invoice has the sum of DDD", 1790.44m, invoice.GroupCharges[0].J7_Amount);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Still one charge", 1, invoice.GroupCharges.Count);
			AssertEquals(1790.44m, invoice.GroupCharges[0].J7_Amount);

			AssertEquals(1790.44m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		[TestDate(2009, 6, 1)]
		public void TestCalculateForDerivedDutyCalculationWhenInfiniteLoopTakesPlace()
		{
			invoice.JZ_InvoiceAmount = 24000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			InvoiceCharge oft = invoice.Charges.AddNew("OFT", 990m, "USD");
			oft.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8211.10.0000";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";

			JobComInvoiceLine secondary1 = invoiceLine.AddSecondaryInvoiceLine();
			secondary1.JI_Tariff = "8211.92.9045";
			secondary1.JI_LinePrice = 16000m;
			//no quantity is set intentionally
			JobComInvoiceLine secondary2 = invoiceLine.AddSecondaryInvoiceLine();
			secondary2.JI_Tariff = "8211.93.0030";
			secondary2.JI_LinePrice = 8000m;
			//no quantity is set intentionally
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("DDD", secondary1.ApportionedCharges[1].J7_ChargeType);
			AssertEquals("IsIncludedInLines", true, secondary1.ApportionedCharges[1].J7_IsIncludedInITOT);
			AssertEquals(14429.51m, secondary1.JI_CustomsValue);

			AssertEquals("DDD", secondary2.ApportionedCharges[1].J7_ChargeType);
			AssertEquals("IsIncludedInLines", true, secondary2.ApportionedCharges[1].J7_IsIncludedInITOT);
			AssertEquals(7214.76m, secondary2.JI_CustomsValue);

			AssertEquals("DDD", invoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("invoice has the sum of DDD", 1365.73m, invoice.GroupCharges[0].J7_Amount);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Still one charge", 1, invoice.GroupCharges.Count);
			AssertEquals(1365.73m, invoice.GroupCharges[0].J7_Amount);
		}

		public void TestCalculateForRepairDutyCalculation()
		{
			invoice.JZ_InvoiceAmount = 500.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.US_SupTariff = "9802.00.4040";
				invoiceLine.JI_Tariff = "9102.11.1010";
				invoiceLine.JI_LinePrice = 3406.00m;
				invoiceLine.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine3.US_SupTariff = "9802.00.4040";
				invoiceLine3.US_98GoodsValue = 500m;
				invoiceLine3.JI_Tariff = "9102.11.1020";
				invoiceLine3.JI_LinePrice = 1609.00m;
				invoiceLine3.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine invoiceLine5 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine5.US_SupTariff = "9802.00.4040";
				invoiceLine5.JI_Tariff = "9102.11.1030";
				invoiceLine5.JI_CustomsQuantity = 1000m;
				invoiceLine5.JI_LinePrice = 1345.00m;

				JobComInvoiceLine invoiceLine7 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine7.US_SupTariff = "9802.00.4040";
				invoiceLine7.JI_Tariff = "9102.11.1040";
				invoiceLine7.JI_LinePrice = 204m;
				invoiceLine7.JI_CustomsQuantity = 1000m;

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				AssertEquals(351.55m, invoiceLine.ApportionedCharges[0].J7_Amount);
				AssertEquals(166.10m, invoiceLine3.ApportionedCharges[0].J7_Amount);
				AssertEquals(138.82m, invoiceLine5.ApportionedCharges[0].J7_Amount);
				AssertEquals(21.07m, invoiceLine7.ApportionedCharges[0].J7_Amount);
			}

			AssertEquals(677.54m, invoice.GroupCharges[0].J7_Amount);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Still one charge", 1, invoice.GroupCharges.Count);
			AssertEquals(677.54m, invoice.GroupCharges[0].J7_Amount);

			AssertEquals(677.54m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		[TestDate(2017, 12, 1)]
		public void TestCalculateForAssemblyDutyCalculation()
		{
			invoice.JZ_InvoiceAmount = 500.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.US_SupTariff = "9802.00.8068";
				invoiceLine.JI_Tariff = "9102.11.1010";
				invoiceLine.JI_LinePrice = 3406.00m;
				invoiceLine.JI_CustomsQuantity = 1000m;
				invoiceLine.US_98GoodsValue = 1852.00m;

				JobComInvoiceLine invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine3.US_SupTariff = "9802.00.8068";
				invoiceLine3.JI_Tariff = "9102.11.1020";
				invoiceLine3.JI_LinePrice = 1609.00m;
				invoiceLine3.JI_CustomsQuantity = 1000m;
				invoiceLine3.US_98GoodsValue = 1010.00m;

				JobComInvoiceLine invoiceLine5 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine5.US_SupTariff = "9802.00.8068";
				invoiceLine5.JI_Tariff = "9102.11.1030";
				invoiceLine5.JI_CustomsQuantity = 1000m;
				invoiceLine5.JI_LinePrice = 1345.00m;

				JobComInvoiceLine invoiceLine7 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine7.US_SupTariff = "9802.00.8068";
				invoiceLine7.JI_Tariff = "9102.11.1040";
				invoiceLine7.JI_CustomsQuantity = 1000m;
				invoiceLine7.US_98GoodsValue = 204m;

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(0m, invoiceLine7.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency()));

				// if it could not be the same, deduction should be less than the real disbursement amount
				AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable, invoice.GroupCharges[0].J7_Amount);

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(0m, invoiceLine7.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency()));
				AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable, invoice.GroupCharges[0].J7_Amount);
			}
		}

		[TestDate(2009, 12, 12)]
		public void TestCalculateForInLieuTariffCalculation()
		{
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9902.01.21";//0.06
			invoiceLine.JI_Tariff = "2933.19.2300";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("DDD", invoiceLine.ApportionedCharges[0].J7_ChargeType);
			AssertEquals(589.60m, invoiceLine.ApportionedCharges[0].J7_Amount);
			AssertEquals("IsIncludedInLines", true, invoiceLine.ApportionedCharges[0].J7_IsIncludedInITOT);
			AssertEquals(9410.40m, invoiceLine.JI_CustomsValue);

			AssertEquals("DDD", invoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("invoice has the sum of DDD", 589.60m, invoice.GroupCharges[0].J7_Amount);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Still one charge", 1, invoice.GroupCharges.Count);
			AssertEquals(589.60m, invoice.GroupCharges[0].J7_Amount);

			AssertEquals(589.60m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		public void TestDDPCalculationShouldNotHappenWhenOverridingChargeExists()
		{
			GroupInvoiceCharge charge = declaration.TopGroupInvoice.Charges.AddNew();
			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.DisbursementCharge;
			charge.J7_AdjustedCharge = true;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9902.01.21";//0.06
			invoiceLine.JI_Tariff = "2933.19.2300";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("DDD", invoiceLine.ApportionedCharges[0].J7_ChargeType);
			AssertEquals(100m, invoiceLine.ApportionedCharges[0].J7_Amount);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("DDD", invoiceLine.ApportionedCharges[0].J7_ChargeType);
			AssertEquals(100m, invoiceLine.ApportionedCharges[0].J7_Amount);
		}

		public void TestDutyDeductionIncludeADD_CVD()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570868000";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;

			var rate = addCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 0.23m;
			rate.U6_EffectiveDate = ZDateTime.Today;

			invoice.JZ_InvoiceAmount = 56898m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.Charges.AddNew("OFT", 2285m, "USD");

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9403.20.0011";//duty free
			AssertNotNull("PreCondition", invoiceLine.ImportTariff);
			invoiceLine.JI_LinePrice = 56898m;
			invoiceLine.JI_CustomsQuantity = 1100m;
			invoiceLine.US_ADDCaseNo = "A570868000";
			AssertNotEquals("PreCondition", 0m, invoiceLine.US_ADDDepositRate);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			AssertNotEquals("Precondition:ADD payable", 0m, invoiceLine.CusEntryLine.AntidumpingDuty);
			AssertEquals("Precondition:Duty free", 0m, invoiceLine.CusEntryLine.DutyAmount);

			var dddCharge = invoiceLine.ApportionedCharges.GetCharge("DDD")[0];
			AssertEquals(entry.MPFAmountForEntry + entry.HMFAmountForEntry, dddCharge.J7_Amount);

			invoice.US_DeductADDCVDDuty = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals("Precondition:ADD payable", 0m, invoiceLine.CusEntryLine.AntidumpingDuty);
			AssertEquals("Precondition:Duty free", 0m, invoiceLine.CusEntryLine.DutyAmount);

			var dddChargeDeductADDCVDDuty = invoiceLine.ApportionedCharges.GetCharge("DDD")[0];
			AssertEquals(entry.MPFAmountForEntry + entry.HMFAmountForEntry + entry.TotalAntidumpingDuty, dddCharge.J7_Amount);
		}

		[TestDate(2019, 09, 15)]
		public void TestTotalAmountOfDutyFeesTaxes()
		{
			var tariff99038803 = CreateNewTariffForTest("99038803", "7", 0.25m);
			var tariff7307995045 = CreateNewTariffForTest("7307995045", "7", 0.043m);
			var tariff7307929000 = CreateNewTariffForTest("7307929000", "7", 0.062m);
			var tariff7307923010 = CreateNewTariffForTest("7307923010", "7", 0m);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			invoice.JZ_InvoiceAmount = 23797.00m;
			invoice.JZ_Weight = 4000.000m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.US_DeductADDCVDDuty = YesNoDefaultList.Codes.Yes;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var invoiceCharge01 = invoice.Charges.AddNew();
			invoiceCharge01.J7_ChargeType = USCustomsChargeTypeList.Codes.LandingCharges;
			invoiceCharge01.J7_Amount = 1303.45m;
			invoiceCharge01.J7_IsIncludedInITOT = true;
			invoiceCharge01.J7_Calc_IsIncludedInInvoiceAmount = true;

			var invoiceCharge02 = invoice.Charges.AddNew();
			invoiceCharge02.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			invoiceCharge02.J7_Amount = 550.00m;
			invoiceCharge02.J7_IsIncludedInITOT = true;
			invoiceCharge02.J7_IsGSTApplicable = true;
			invoiceCharge02.J7_Calc_IsIncludedInInvoiceAmount = true;

			var invoiceCharge03 = invoice.Charges.AddNew();
			invoiceCharge03.J7_ChargeType = USCustomsChargeTypeList.Codes.DeductionCharge;
			invoiceCharge03.J7_Amount = 150.00m;
			invoiceCharge03.J7_IsIncludedInITOT = true;
			invoiceCharge03.J7_Calc_IsIncludedInInvoiceAmount = true;

			var invoiceLine01 = invoice.InvoiceLines.AddNew();
			invoiceLine01.US_SupTariff = "9903.88.03";
			invoiceLine01.JI_Tariff = "7307.99.5045";
			invoiceLine01.JI_LinePrice = 22785.80m;
			invoiceLine01.JI_CustomsQuantity = 3800m;
			invoiceLine01.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine01.US_ADDCaseNo = "A570067001";
			invoiceLine01.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine01.US_CVDCaseNo = "C570068001";
			invoiceLine01.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine01.JI_Description = "Invoice Line 01";

			var invoiceLine02 = invoice.InvoiceLines.AddNew();
			invoiceLine02.US_SupTariff = "9903.88.03";
			invoiceLine02.JI_Tariff = "7307.92.9000";
			invoiceLine02.JI_LinePrice = 613.20m;
			invoiceLine02.JI_CustomsQuantity = 80m;
			invoiceLine02.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine02.US_ADDCaseNo = "A570067001";
			invoiceLine02.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine02.US_CVDCaseNo = "C570068001";
			invoiceLine02.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine02.JI_Description = "Invoice Line 02";

			var invoiceLine03 = invoice.InvoiceLines.AddNew();
			invoiceLine03.US_SupTariff = "9903.88.03";
			invoiceLine03.JI_Tariff = "7307.92.3010";
			invoiceLine03.JI_LinePrice = 398.00m;
			invoiceLine03.JI_CustomsQuantity = 65m;
			invoiceLine03.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine03.US_ADDCaseNo = "A570067001";
			invoiceLine03.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine03.US_CVDCaseNo = "C570068001";
			invoiceLine03.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine03.JI_Description = "Invoice Line 03";
			Factory.Save();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CombineAssertions(() =>
			{
				AssertEquals("Invoice Header FOB Amount.", 16797.27m, invoice.JZ_Calc_FOBAmount);
				AssertEquals("Invoice Header CIF Amount.", 17347.27m, invoice.JZ_Calc_CIFAmount);
				AssertEquals("Invoice Header DDD Amount.", 4996.28m, invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0].J7_Amount);

				AssertEquals("Invoice Line 01 FOB Amount.", 16080.25m, invoiceLine01.FOBValueInLocalCurrency);
				AssertEquals("Invoice Line 01 DDD Amount.", 4787.24m, invoiceLine01.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0].J7_Amount);
				AssertEquals("Invoice Line 01 DED Amount.", 143.62m, invoiceLine01.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DeductionCharge)[0].J7_Amount);
				AssertEquals("Invoice Line 01 LCH Amount.", 1248.06m, invoiceLine01.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.LandingCharges)[0].J7_Amount);
				AssertEquals("Invoice Line 01 OFT Amount.", 526.63m, invoiceLine01.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.OverseasFreight)[0].J7_Amount);
				AssertEquals("Invoice Line 01 Duty.", 691.44m, invoiceLine01.US_Duty);
				AssertEquals("Invoice Line 01 Supper Duty.", 4020.00m, invoiceLine01.US_SupDuty);

				AssertEquals("Invoice Line 02 FOB Amount.", 426.65m, invoiceLine02.FOBValueInLocalCurrency);
				AssertEquals("Invoice Line 02 DDD Amount.", 134.92m, invoiceLine02.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0].J7_Amount);
				AssertEquals("Invoice Line 02 DED Amount.", 3.87m, invoiceLine02.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DeductionCharge)[0].J7_Amount);
				AssertEquals("Invoice Line 02 LCH Amount.", 33.59m, invoiceLine02.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.LandingCharges)[0].J7_Amount);
				AssertEquals("Invoice Line 02 OFT Amount.", 14.17m, invoiceLine02.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.OverseasFreight)[0].J7_Amount);
				AssertEquals("Invoice Line 02 Duty.", 26.41m, invoiceLine02.US_Duty);
				AssertEquals("Invoice Line 02 Supper Duty.", 106.50m, invoiceLine02.US_SupDuty);

				AssertEquals("Invoice Line 03 FOB Amount.", 290.37m, invoiceLine03.FOBValueInLocalCurrency);
				AssertEquals("Invoice Line 03 DDD Amount.", 74.12m, invoiceLine03.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0].J7_Amount);
				AssertEquals("Invoice Line 03 DED Amount.", 2.51m, invoiceLine03.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DeductionCharge)[0].J7_Amount);
				AssertEquals("Invoice Line 03 LCH Amount.", 21.80m, invoiceLine03.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.LandingCharges)[0].J7_Amount);
				AssertEquals("Invoice Line 03 OFT Amount.", 9.20m, invoiceLine03.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.OverseasFreight)[0].J7_Amount);
				AssertEquals("Invoice Line 03 Duty.", 0.00m, invoiceLine03.US_Duty);
				AssertEquals("Invoice Line 03 Supper Duty.", 72.75m, invoiceLine03.US_SupDuty);

				var entryHeader = declaration.ActiveEntryHeaders[0];
				AssertEquals("Entry Header DDD Amount.", 4996.28m, entryHeader.CH_TotalPaid);

				var entryLine01 = entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(invoiceLine01.JI_LineNo);
				var entryLine02 = entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(invoiceLine02.JI_LineNo);
				var entryLine03 = entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(invoiceLine03.JI_LineNo);
				AssertEquals("Entry Line 01 Customs Value.", 16080.25m, entryLine01.CL_CustomsValue);
				AssertEquals("Entry Line 02 Customs Value.", 426.65m, entryLine02.CL_CustomsValue);
				AssertEquals("Entry Line 03 Customs Value.", 290.37m, entryLine03.CL_CustomsValue);
			});
		}

		[TestDate(2020, 01, 19)]
		public void TestEntryLineCustomsValueWhenACEAndNonFTZ()
		{
			var tariff99038803 = CreateNewTariffForTest("99038803", "7", 0.25m);
			var tariff8708509900 = CreateNewTariffForTest("8708509900", "7", 0.025m);
			var tariff8708305090 = CreateNewTariffForTest("8708305090", "7", 0.025m);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			invoice.JZ_InvoiceAmount = 104268.60m;
			invoice.JZ_Weight = 1000.000m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.US_DeductADDCVDDuty = YesNoDefaultList.Codes.Yes;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.HongKong;

			var invoiceCharge01 = invoice.GroupCharges.AddNew();
			invoiceCharge01.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			invoiceCharge01.J7_Amount = 2500;
			invoiceCharge01.J7_IsDutiable = true;
			invoiceCharge01.J7_IsGSTApplicable = true;
			invoiceCharge01.J7_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			var invoiceLine01 = CreateInvoiceLineForTest(invoice, "01", "8708.50.9900", 1603.80m, 5.0m);
			var invoiceLine02 = CreateInvoiceLineForTest(invoice, "02", "8708.30.5090", 9081.6m, 30.0m);
			var invoiceLine03 = CreateInvoiceLineForTest(invoice, "03", "8708.30.5090", 1351.90m, 5.0m);
			var invoiceLine04 = CreateInvoiceLineForTest(invoice, "04", "8708.50.9900", 1591.35m, 5.0m);
			var invoiceLine05 = CreateInvoiceLineForTest(invoice, "05", "8708.30.5090", 610.0m, 200.0m);
			var invoiceLine06 = CreateInvoiceLineForTest(invoice, "06", "8708.50.9900", 248.0m, 200.0m);
			var invoiceLine07 = CreateInvoiceLineForTest(invoice, "07", "8708.30.5090", 2463.0m, 15.0m);
			var invoiceLine08 = CreateInvoiceLineForTest(invoice, "08", "8708.50.9900", 2078.40m, 8.0m);
			var invoiceLine09 = CreateInvoiceLineForTest(invoice, "09", "8708.50.9900", 698.740m, 2.0m);
			var invoiceLine10 = CreateInvoiceLineForTest(invoice, "10", "8708.30.5090", 248.80m, 20.0m);
			var invoiceLine11 = CreateInvoiceLineForTest(invoice, "11", "8708.50.9900", 14898.02m, 46m);
			var invoiceLine12 = CreateInvoiceLineForTest(invoice, "12", "8708.50.9900", 10275.60m, 30m);
			var invoiceLine13 = CreateInvoiceLineForTest(invoice, "13", "8708.50.9900", 10201.20m, 30m);
			var invoiceLine14 = CreateInvoiceLineForTest(invoice, "14", "8708.30.5090", 617.000m, 100m);
			var invoiceLine15 = CreateInvoiceLineForTest(invoice, "15", "8708.30.5090", 460.00m, 1000m);
			var invoiceLine16 = CreateInvoiceLineForTest(invoice, "16", "8708.30.5090", 37.0000m, 100m);
			var invoiceLine17 = CreateInvoiceLineForTest(invoice, "17", "8708.30.5090", 124.000m, 100m);
			var invoiceLine18 = CreateInvoiceLineForTest(invoice, "18", "8708.30.5090", 3110.00m, 100m);
			var invoiceLine19 = CreateInvoiceLineForTest(invoice, "19", "8708.30.5090", 3334.00m, 100m);
			var invoiceLine20 = CreateInvoiceLineForTest(invoice, "20", "8708.30.5090", 13881.6m, 480m);
			var invoiceLine21 = CreateInvoiceLineForTest(invoice, "21", "8708.30.5090", 1180.0m, 1000m);
			var invoiceLine22 = CreateInvoiceLineForTest(invoice, "22", "8708.30.5090", 5213.320m, 14m);
			var invoiceLine23 = CreateInvoiceLineForTest(invoice, "23", "8708.30.5090", 6425.230m, 19m);
			var invoiceLine24 = CreateInvoiceLineForTest(invoice, "24", "8708.30.5090", 238.82m, 2.00m);
			var invoiceLine25 = CreateInvoiceLineForTest(invoice, "25", "8708.30.5090", 6.00m, 100.00m);
			var invoiceLine26 = CreateInvoiceLineForTest(invoice, "26", "8708.30.5090", 4254.120m, 12m);
			var invoiceLine27 = CreateInvoiceLineForTest(invoice, "27", "8708.30.5090", 3545.100m, 10m);
			var invoiceLine28 = CreateInvoiceLineForTest(invoice, "28", "8708.30.5090", 6120.000m, 40m);
			var invoiceLine29 = CreateInvoiceLineForTest(invoice, "29", "8708.30.5090", 372.000m, 600m);

			Factory.Save();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				var entryHeader = declaration.ActiveEntryHeaders[0];
				var line = invoiceLine01;
				var description = line.JI_Description;
				AssertEquals(description + " - Duty", 31.33m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 313.25m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 1.57m, line.HMFAmount);
				AssertEquals(description + " - MPF", 4.34m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 1253m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine02;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 177.43m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 1774.25m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 8.87m, line.HMFAmount);
				AssertEquals(description + " - MPF", 24.58m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 7097m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine03;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 26.40m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 264m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 1.32m, line.HMFAmount);
				AssertEquals(description + " - MPF", 3.66m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 1056m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine04;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 31.10m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 311m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 1.56m, line.HMFAmount);
				AssertEquals(description + " - MPF", 4.31m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 1244m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine05;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 11.93m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 119.25m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.60m, line.HMFAmount);
				AssertEquals(description + " - MPF", 1.65m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 477m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine06;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 4.85m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 48.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.24m, line.HMFAmount);
				AssertEquals(description + " - MPF", 0.67m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 194m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine07;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 48.13m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 481.25m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 2.41m, line.HMFAmount);
				AssertEquals(description + " - MPF", 6.67m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 1925m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine08;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 40.60m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 406.00m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 2.03m, line.HMFAmount);
				AssertEquals(description + " - MPF", 5.63m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 1624m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine09;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 13.65m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 136.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.68m, line.HMFAmount);
				AssertEquals(description + " - MPF", 1.89m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 546m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine10;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 4.85m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 48.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.24m, line.HMFAmount);
				AssertEquals(description + " - MPF", 0.67m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 194m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine11;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 291.05m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 2910.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 14.55m, line.HMFAmount);
				AssertEquals(description + " - MPF", 40.33m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 11642m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine12;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 200.75m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 2007.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 10.04m, line.HMFAmount);
				AssertEquals(description + " - MPF", 27.82m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 8030m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine13;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 199.28m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 1992.75m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 9.96m, line.HMFAmount);
				AssertEquals(description + " - MPF", 27.61m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 7971m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine14;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 12.05m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 120.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.60m, line.HMFAmount);
				AssertEquals(description + " - MPF", 1.67m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 482m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine15;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 8.98m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 89.75m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.45m, line.HMFAmount);
				AssertEquals(description + " - MPF", 1.24m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 359m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine16;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 0.73m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 7.25m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.04m, line.HMFAmount);
				AssertEquals(description + " - MPF", 0.10m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 29m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine17;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 2.43m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 24.25m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.12m, line.HMFAmount);
				AssertEquals(description + " - MPF", 0.34m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 97m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine18;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 60.75m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 607.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 3.04m, line.HMFAmount);
				AssertEquals(description + " - MPF", 8.42m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 2430m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine19;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 65.13m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 651.25m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 3.26m, line.HMFAmount);
				AssertEquals(description + " - MPF", 9.02m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 2605m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine20;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 271.18m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 2711.75m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 13.56m, line.HMFAmount);
				AssertEquals(description + " - MPF", 37.57m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 10847m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine21;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 23.05m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 230.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 1.15m, line.HMFAmount);
				AssertEquals(description + " - MPF", 3.19m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 922m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine22;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 101.85m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 1018.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 5.09m, line.HMFAmount);
				AssertEquals(description + " - MPF", 14.11m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 4074m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine23;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 125.53m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 1255.25m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 6.28m, line.HMFAmount);
				AssertEquals(description + " - MPF", 17.39m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 5021m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine24;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 4.68m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 46.75m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.23m, line.HMFAmount);
				AssertEquals(description + " - MPF", 0.65m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 187m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine25;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 0.13m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 1.25m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.01m, line.HMFAmount);
				AssertEquals(description + " - MPF", 0.02m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 5m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine26;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 83.10m, line.US_Duty);
				AssertEquals(description + " - HMF", 4.16m, line.HMFAmount);
				AssertEquals(description + " - MPF", 11.51m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 3324m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine27;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 69.25m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 692.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 3.46m, line.HMFAmount);
				AssertEquals(description + " - MPF", 9.60m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 2770m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine28;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 119.55m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 1195.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 5.98m, line.HMFAmount);
				AssertEquals(description + " - MPF", 16.56m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 4782m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);

				line = invoiceLine29;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 7.28m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 72.75m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 0.36m, line.HMFAmount);
				AssertEquals(description + " - MPF", 1.01m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value", 291m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);
			});
		}

		[TestDate(2020, 01, 21)]
		public void TestEntryLineCustomsValueWhenIncludeCottonFee()
		{
			var tariff99038815 = CreateNewTariffForTest("99038815", "7", 0.15m);
			var tariff6211421081 = CreateNewTariffForTest("6211421081", "7", 0.081m);
			var tariff6211431091 = CreateNewTariffForTest("6211431091", "7", 0.16m);

			tariff99038815.UE_ISOCountryofOriginEditCode = Core.Constants.CountryCodes.China;
			tariff99038815.UE_AdditionalTariffNumberIndicator = true;

			tariff6211421081.UE_NumberOfReportingUnits = 2;
			tariff6211421081.UE_Unit1 = "DOZ";
			tariff6211421081.UE_Unit2 = "KG";
			tariff6211421081.UE_QuotaIndicator = true;
			tariff6211421081.UE_TextileCategoryNumber = "359";
			tariff6211421081.UE_IsBaseRate = false;

			var dutyRate = tariff6211421081.DutyRates.AddNew();
			dutyRate.UD_DutyElement = "5";
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			dutyRate.UD_TaxFeeComputationCode = "2";
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.01356520m;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			invoice.JZ_InvoiceAmount = 59936.40m;
			invoice.JZ_Weight = 17200.000m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.US_DeductADDCVDDuty = YesNoDefaultList.Codes.Yes;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var invoiceCharge01 = invoice.Charges.AddNew();
			invoiceCharge01.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			invoiceCharge01.J7_Amount = 4590m;
			invoiceCharge01.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceCharge01.J7_IsIncludedInITOT = true;
			invoiceCharge01.J7_IsGSTApplicable = true;

			var invoiceLine01 = invoice.InvoiceLines.AddNew();
			invoiceLine01.US_SupTariff = "9903.88.15";
			invoiceLine01.JI_Tariff = "6211.42.1081";
			invoiceLine01.JI_LinePrice = 11372.40m;
			invoiceLine01.JI_CustomsQuantity = 810.00m;
			invoiceLine01.JI_CustomsSecondQuantity = 2610.00m;
			invoiceLine01.JI_InvoiceQuantity = 9720.00m;
			invoiceLine01.JI_Weight = 3263.547m;
			invoiceLine01.JI_NetWeight = 2610.00m;
			invoiceLine01.US_CottonFeeExempt = "N";
			invoiceLine01.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLine01.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine01.US_TransactionsRelated = "N";
			invoiceLine01.JI_Description = "Invoice Line 01";

			var invoiceLine02 = invoice.InvoiceLines.AddNew();
			invoiceLine02.US_SupTariff = "9903.88.15";
			invoiceLine02.JI_Tariff = "6211.43.1091";
			invoiceLine02.JI_LinePrice = 48564.00m;
			invoiceLine02.JI_CustomsQuantity = 7100.00;
			invoiceLine02.JI_CustomsSecondQuantity = 13145.00m;
			invoiceLine02.JI_InvoiceQuantity = 85200.00m;
			invoiceLine02.JI_Weight = 13936.453m;
			invoiceLine02.JI_NetWeight = 13145.00m;
			invoiceLine02.US_CottonFeeExempt = "Y";
			invoiceLine02.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLine02.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine02.US_TransactionsRelated = "N";
			invoiceLine02.JI_Description = "Invoice Line 02";
			Factory.Save();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CombineAssertions(() =>
			{
				var entryHeader = declaration.ActiveEntryHeaders[0];
				var line = invoiceLine01;
				var description = line.JI_Description;
				AssertEquals(description + " - Duty", 686.07m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 1270.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 10.59m, line.HMFAmount);
				AssertEquals(description + " - MPF", 29.34m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Cotton", 35.41m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton).CY_FeeAmount);
				AssertEquals(description + " - Customs Value (AddInfo)", 8470.00m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);
				AssertEquals(description + " - Customs Value", 8469.58m, line.JI_CustomsValue);

				line = invoiceLine02;
				description = line.JI_Description;
				AssertEquals(description + " - Duty", 5457.60m, line.US_Duty);
				AssertEquals(description + " - Sup Duty", 5116.50m, line.US_SupDuty);
				AssertEquals(description + " - HMF", 42.64m, line.HMFAmount);
				AssertEquals(description + " - MPF", 118.16m, line.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals(description + " - Customs Value (AddInfo)", 34110m, ((ICusEntryLine)(entryHeader.MergedLines.FindNonSecondaryLineByLineNumber(line.JI_LineNo))).CL_CustomsValue);
				AssertEquals(description + " - Customs Value", 34110.01m, line.JI_CustomsValue);
			});
		}

		[TestDate(2021, 04, 12)]
		public void TestCustomsValueForDDPWithA99SupTariff()
		{
			#region Setup Tariff and ADD Case

			var tariff7606123096 = CreateNewTariffForTest("7606123096", "7", 0.03m);
			var tariff99038501 = CreateNewTariffForTest("99038501", "7", 0.1m);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99038501 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038501", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ALUM PRD,NT 19,EX PRD EXCL", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038501);
			helper.CreateTaxOrFee(Core.Constants.USCustoms.FeeCodes.HMF, 0.125m, Core.Constants.CountryCodes.UnitedStates, new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01), "Harbor Maintenance Fee");
			helper.CreateTaxOrFee(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 0.3464m, Core.Constants.CountryCodes.UnitedStates, 27.23m, 528.33m, "AVL", new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01), "Merchandise Processing Fee");

			var adCaseNumber = Factory.New<USCACCase>();
			adCaseNumber.U5_CaseNumber = "A479500001";
			adCaseNumber.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			adCaseNumber.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			adCaseNumber.U5_ISOCountryCode = "SI";
			adCaseNumber.CaseTariffs.AddNew().U9_TariffNumber = tariff7606123096.UE_Tariff;
			var caseRate = adCaseNumber.CaseRates.AddNew();
			caseRate.U6_AddedDate = new ZDateTime(2021, 01, 01);
			caseRate.U6_EffectiveDate = new ZDateTime(2021, 01, 01);
			caseRate.U6_AdValoremRate = 0.1343m;

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;
			declaration.JE_ExportDate = new ZDateTime(2021, 03, 29);
			declaration.JE_DateOfArrival = new ZDateTime(2021, 04, 08);
			declaration.US_EntryDate = new ZDateTime(2021, 04, 08);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV08042102";
			invoice.JZ_InvoiceAmount = 83967.02m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.JZ_RN_NKCountryOfExport = Core.Constants.CountryCodes.Slovenia;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Slovenia;

			var insuranceCharge = invoice.Charges.AddNew("ONS", 143.52m);
			insuranceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			insuranceCharge.J7_IsGSTApplicable = true;
			var freightCharge = invoice.Charges.AddNew("OFT", 2190m);
			freightCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			freightCharge.J7_IsGSTApplicable = true;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Slovenia;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Slovenia;
			invoiceLine.JI_Tariff = tariff7606123096.UE_Tariff;
			invoiceLine.US_SupTariff = tariff99038501.UE_Tariff;
			invoiceLine.JI_LinePrice = 81633.5m;
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A479500001";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			CombineAssertions("Deduct ADD/CVD Duty is 'Y'", () =>
			{
				invoice.US_DeductADDCVDDuty = YesNoDefaultList.Codes.Yes;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("US_CustomsValue in JI_AddInfo", 64328m, invoiceLine.US_CustomsValue);
				var entryLine = declaration.FormalEntry.EntryLines.First(x => x.CL_AdValoremTariff == tariff99038501.UE_Tariff);
				AssertEquals("CL_CustomsValue on tariff 99038501", 64328.37m, entryLine.CL_CustomsValue);
				AssertEquals("Sup duty on invoice line", 6432.8m, invoiceLine.US_SupDuty);
				AssertEquals("Normal duty on invoice line", 1929.84m, invoiceLine.US_Duty);
				AssertEquals("MPF fee amount on invoice line", 222.83m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertEquals("HMF fee amount on invoice line", 80.41m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
				AssertEquals("DDD charge apportioned on invoice line", 17305.13m, invoiceLine.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency()));
			});

			CombineAssertions("Deduct ADD/CVD Duty is 'N'", () =>
			{
				invoice.US_DeductADDCVDDuty = YesNoDefaultList.Codes.No;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("US_CustomsValue in JI_AddInfo", 71942m, invoiceLine.US_CustomsValue);
				var entryLine = declaration.FormalEntry.EntryLines.First(x => x.CL_AdValoremTariff == tariff99038501.UE_Tariff);
				AssertEquals("CL_CustomsValue on tariff 99038501", 71941.9m, entryLine.CL_CustomsValue);
				AssertEquals("Sup duty on invoice line", 7194.2m, invoiceLine.US_SupDuty);
				AssertEquals("Normal duty on invoice line", 2158.26m, invoiceLine.US_Duty);
				AssertEquals("MPF fee amount on invoice line", 249.21m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertEquals("HMF fee amount on invoice line", 89.93m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
				AssertEquals("DDD charge apportioned on invoice line", 9691.6m, invoiceLine.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency()));
			});
		}

		[TestDate(2022, 02, 18)]
		public void TestDDPCalculationForDerivedParentChildLine()
		{
			#region Setup fee and tariff data

			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			testHelper.CreateTaxOrFee("499", 0.3464, "US", 27.75, 538.40, "AVL", new ZDateTime("2021-10-01"), new ZDateTime("2022-12-31"), "Merchandise Processing Fee");

			var tariff99038803 = Factory.New<USCTariff>();
			tariff99038803.UE_Tariff = "99038803";
			tariff99038803.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff99038803.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff99038803.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff99038803.UE_Column1RateAdValorem = 0.25m;

			var tariff8206000000 = Factory.New<USCTariff>();
			tariff8206000000.UE_Tariff = "8206000000";
			tariff8206000000.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff8206000000.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff8206000000.UE_DutyComputationCode = ComputationCodeList.Codes.Derived;
			tariff8206000000.UE_Column1RateAdValorem = 1m;

			var tariff8205595510 = Factory.New<USCTariff>();
			tariff8205595510.UE_Tariff = "8205595510";
			tariff8205595510.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff8205595510.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff8205595510.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff8205595510.UE_Column1RateAdValorem = 0.053m;

			Factory.Save();

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceCharge = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 1000m, "USD");
			invoiceCharge.J7_IsIncludedInITOT = true;

			var invoiceLineOne = invoice.JobComInvoiceLines.AddNew();
			invoiceLineOne.US_SupTariff = tariff99038803.UE_Tariff;
			invoiceLineOne.JI_Tariff = tariff8206000000.UE_Tariff;
			invoiceLineOne.JI_LinePrice = 0m;
			var invoiceLineTwo = invoice.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.US_SupTariff = ZString.Empty;
			invoiceLineTwo.JI_Tariff = tariff8205595510.UE_Tariff;
			invoiceLineTwo.JI_LinePrice = 10000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CombineAssertions("Validate Duty After Merge", () =>
			{
				AssertEquals("Sup Duty on invoice line one", 1721.5m, invoiceLineOne.US_SupDuty);
				AssertEquals("Normal Duty on invoice line one", 364.96m, invoiceLineOne.US_Duty);
				AssertEquals("Payable MPF on invoice line one", 0m, invoiceLineOne.US_PayableMPF);
				AssertEquals("Calculated MPF on invoice line one", 0m, invoiceLineOne.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertEquals("Apportioned DDD on invoice line one", 0m, invoiceLineOne.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0].J7_Amount);

				AssertEquals("Sup Duty on invoice line two", 0m, invoiceLineTwo.US_SupDuty);
				AssertEquals("Normal Duty on invoice line two", 0m, invoiceLineTwo.US_Duty);
				AssertEquals("Payable MPF on invoice line two", 27.75m, invoiceLineTwo.US_PayableMPF);
				AssertEquals("Calculated MPF on invoice line two", 23.85m, invoiceLineTwo.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertEquals("Apportioned DDD on invoice line two", 2114.21m, invoiceLineTwo.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0].J7_Amount);
			});
		}

		[TestDate(2019, 09, 15)]
		public void TestTotalAmountOfDutyFeesTaxesWhenUS_SupTariffHasInvalidValue()
		{
			var tariff7307995045 = CreateNewTariffForTest("7307995045", "7", 0.043m);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryFilerCode = "SV9";
			declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_Weight = 500m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var invoiceLine01 = invoice.InvoiceLines.AddNew();
			invoiceLine01.JI_Tariff = "7307.99.5045";
			invoiceLine01.JI_LinePrice = 10000m;
			invoiceLine01.JI_CustomsQuantity = 500m;
			invoiceLine01.JI_Description = "Invoice Line 01";
			Factory.Save();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertForTestTotalAmountOfDutyFeesTaxesWhenUS_SupTariffHasInvalidValue();

			invoiceLine01.US_SupTariff = "1";
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertForTestTotalAmountOfDutyFeesTaxesWhenUS_SupTariffHasInvalidValue();
		}

		[TestDate(2021, 07, 25)]
		public void TestDiffForTotalEnteredValueAndCustomsValueIsWithinOneDollar()
		{
			#region Setup Rate and Tariff

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			CreateNewTariffForTest("9615196000", ComputationCodeList.Codes.AdValorem, 0.11m);
			CreateNewTariffForTest("6110303059", ComputationCodeList.Codes.AdValorem, 0.32m);
			CreateNewTariffForTest("99038815", ComputationCodeList.Codes.AdValorem, 0.075m);

			void AddInvoiceLine(JobComInvoiceHeader invoiceHeader, ZString tariff, ZString supTariff, ZDecimal linePrice, ZDecimal weight, ZString weightUQ)
			{
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = tariff;
				invoiceLine.US_SupTariff = supTariff;
				invoiceLine.JI_LinePrice = linePrice;
				invoiceLine.JI_Weight = weight;
				invoiceLine.JI_WeightUQ = weightUQ;
			}

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_DateOfExport = new ZDateTime(2021, 07, 25);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			invoice.JZ_InvoiceAmount = 100415.52;
			invoice.JZ_Weight = 8640m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoice.Charges.RemoveAndDeleteAll();

			var discountCharge = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.Discount, 3514.54m);
			discountCharge.J7_IsIncludedInITOT = false;
			discountCharge.J7_IsDutiable = false;
			discountCharge.J7_IsGSTApplicable = false;
			var freightCharge = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 1500m);
			freightCharge.J7_IsIncludedInITOT = true;
			freightCharge.J7_IsDutiable = true;
			freightCharge.J7_IsGSTApplicable = true;

			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 402m, 34.589m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 3522m, 303.042m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 402m, 34.589m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 3522m, 303.042m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 26.8m, 2.306m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 234.8m, 20.203m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 840m, 72.276m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 7548m, 649.446m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 840m, 72.276m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 7548m, 649.446m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 840m, 72.276m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 7548m, 649.446m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 656m, 56.444m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 5776m, 496.981m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 656m, 56.444m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 5776m, 496.981m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 39.36m, 3.387m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 346.56m, 29.819m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 384m, 33.04m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 3360m, 289.103m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 384m, 33.04m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 3360m, 289.103m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 384m, 33.04m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 3360m, 289.103m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 804m, 69.178m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 7224m, 621.571m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 804m, 69.178m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 7224m, 621.571m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 804m, 69.178m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 7224m, 621.571m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 632m, 54.379m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 5560m, 478.396m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 632m, 54.379m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 5560m, 478.396m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "9615.19.6000", ZString.Empty, 632m, 54.379m, Core.Constants.Weight.Kilograms);
			AddInvoiceLine(invoice, "6110.30.3059", "9903.88.15", 5560m, 478.396m, Core.Constants.Weight.Kilograms);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var apportionedDDD = invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0];
			AssertEquals("Apportioned DDD charge", apportionedDDD.J7_Amount, 25880.66m);
			var ensEntry = declaration.FormalEntry;
			AssertEquals("TotalEnteredValue", ensEntry.TotalEnteredValue, 71020m);
			var sumCustomsValue = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().Sum(x => x.JI_CustomsValue);
			AssertEquals("Sum of customs value", sumCustomsValue, 71020.32m);
			AssertEquals("Diff should be within 1 dollor", true, Math.Abs(ensEntry.TotalEnteredValue - sumCustomsValue) <= 1m);
		}

		[TestDate(2025, 02, 28)]
		public void TestCustomsValueForDDPWithSupplementaryTariffOnCombinedLines()
		{
			#region Setup Tariff

			var tariff8215993500 = CreateNewTariffForTest("8215993500", "7", 0.068m);
			var tariff99030120 = CreateNewTariffForTest("99030120", "7", 0.1m);
			var tariff99038815 = CreateNewTariffForTest("99038815", "7", 0.075m);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030120 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030120", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030120);
			var tariffView99038815 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038815", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038815);
			helper.CreateTaxOrFee(Core.Constants.USCustoms.FeeCodes.HMF, 0.125m, Core.Constants.CountryCodes.UnitedStates, new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01), "Harbor Maintenance Fee");
			helper.CreateTaxOrFee(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 0.3464m, Core.Constants.CountryCodes.UnitedStates, 27.23m, 528.33m, "AVL", new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01), "Merchandise Processing Fee");

			Factory.Save();

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV25022801";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var combineParentLine = invoice.JobComInvoiceLines.AddNew();
			combineParentLine.SupTariffFormatted = "9903.88.15";
			combineParentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			combineParentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var combineChildLine = invoice.JobComInvoiceLines.AddNew();
			combineChildLine.JI_ParentID = combineParentLine.PK;
			combineChildLine.JI_FormattedTariff = "8215.99.3500";
			combineChildLine.SupTariffFormatted = "9903.01.20";
			combineChildLine.JI_LinePrice = 5000m;
			combineChildLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			combineChildLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_CustomsValue on combine parent line is 0", 0m, combineParentLine.US_CustomsValue);
			AssertEquals("US_CustomsValue on combine child = (5000 - 25.67) / (1+0.075+0.1+0.068+0.00125)", 3997m, combineChildLine.US_CustomsValue);
		}

		[TestDate(2025, 02, 28)]
		public void TestCustomsValueForDDPWithAdditionalSupplementaryTariffsOnOneLine()
		{
			#region Setup Tariff

			var tariff8215993500 = CreateNewTariffForTest("8215993500", "7", 0.068m);
			var tariff99030120 = CreateNewTariffForTest("99030120", "7", 0.1m);
			var tariff99038815 = CreateNewTariffForTest("99038815", "7", 0.075m);
			var tariff99038825 = CreateNewTariffForTest("99038825", "7", 0.25m);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030120 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030120", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030120);
			var tariffView99038815 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038815", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038815);
			var tariffView99038825 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038825", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038825);
			helper.CreateTaxOrFee(Core.Constants.USCustoms.FeeCodes.HMF, 0.125m, Core.Constants.CountryCodes.UnitedStates, new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01), "Harbor Maintenance Fee");
			helper.CreateTaxOrFee(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 0.3464m, Core.Constants.CountryCodes.UnitedStates, 27.23m, 528.33m, "AVL", new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01), "Merchandise Processing Fee");

			Factory.Save();

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV25022801";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "8215.99.3500";
			invoiceLine.SupTariffFormatted = "9903.01.20";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.15";
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.88.25";
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_CustomsValue = (5000 - 25.67) / (1+0.25+0.075+0.1+0.068+0.00125)", 3328m, invoiceLine.US_CustomsValue);
		}

		public void TestCustomsValueForDDPWithAdditionalSupTariffWhenSupGoodsValueIsProvided()
		{
			#region Setup Tariff

			var tariff8215993500 = CreateNewTariffForTest("8215993500", "7", 0.068m);
			var tariff99030120 = CreateNewTariffForTest("99030120", "7", 0.1m);
			var tariff99038815 = CreateNewTariffForTest("99038815", "7", 0.075m);
			var tariff99038825 = CreateNewTariffForTest("99038825", "7", 0.25m);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030120 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030120", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030120);
			var tariffView99038815 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038815", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038815);
			var tariffView99038825 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038825", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038825);
			helper.CreateTaxOrFee(Core.Constants.USCustoms.FeeCodes.HMF, 0.125m, Core.Constants.CountryCodes.UnitedStates, new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01), "Harbor Maintenance Fee");
			helper.CreateTaxOrFee(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 0.3464m, Core.Constants.CountryCodes.UnitedStates, 27.23m, 528.33m, "AVL", new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01), "Merchandise Processing Fee");

			Factory.Save();

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV25022801";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "8215.99.3500";
			invoiceLine.SupTariffFormatted = "9903.01.20";
			invoiceLine.US_SupGoodsValue = 200m;
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.15";
			invoiceLine.US_SupAdditionalTariff1GoodsValue = 300m;
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.88.25";
			invoiceLine.US_SupAdditionalTariff2GoodsValue = 500m;
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_CustomsValue = (5000 - 25.67 - 200 * 0.1 - 300 * 0.075 - 500 * 0.25) / (1+0.068+0.00125)", 4494m, invoiceLine.US_CustomsValue);
		}

		public void TestCustomsValueForDDPWithNoSupTariffOnDerivedLines()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8206000000", "9", 1, "PCS");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8203204000", "7", 0.12, "DOZ");

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV25031501";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var parentInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			parentInvoiceLine.JI_FormattedTariff = "8206.00.0000";
			parentInvoiceLine.JI_LinePrice = 0m;
			var childInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = parentInvoiceLine.PK;
			childInvoiceLine.JI_FormattedTariff = "8203.20.4000";
			childInvoiceLine.JI_LinePrice = 5000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_CustomsValue on parent line is zero", 0m, parentInvoiceLine.US_CustomsValue);
			AssertEquals("US_CustomsValue on child line = (5000 - 25.67) / (1+0.12+0.00125)", 4436m, childInvoiceLine.US_CustomsValue);
		}

		public void TestCustomsValueForDDPWithNormalSupTariffOnDerivedLines()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8206000000", "9", 1, "PCS");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8203204000", "7", 0.12, "DOZ");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV25031501";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var parentInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			parentInvoiceLine.JI_FormattedTariff = "8206.00.0000";
			parentInvoiceLine.SupTariffFormatted = "9903.01.24";
			parentInvoiceLine.JI_LinePrice = 0m;
			var childInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = parentInvoiceLine.PK;
			childInvoiceLine.JI_FormattedTariff = "8203.20.4000";
			childInvoiceLine.JI_LinePrice = 5000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_CustomsValue on parent line is zero", 0m, parentInvoiceLine.US_CustomsValue);
			AssertEquals("US_CustomsValue on child line = (5000 - 25.67) / (1+0.12+0.2+0.00125)", 3765m, childInvoiceLine.US_CustomsValue);
		}

		public void TestCustomsValueForDDPWithAdditionalSupTariffOnDerivedLines()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8206000000", "9", 1, "PCS");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8203204000", "7", 0.12, "DOZ");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038823", "7", 0.25m, "");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);
			var tariffView99038823 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038823", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038823);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV25031501";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var parentInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			parentInvoiceLine.JI_FormattedTariff = "8206.00.0000";
			parentInvoiceLine.SupTariffFormatted = "9903.01.24";
			parentInvoiceLine.SupFormattedAdditionalTariff1 = "9903.88.23";
			parentInvoiceLine.JI_LinePrice = 0m;
			var childInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = parentInvoiceLine.PK;
			childInvoiceLine.JI_FormattedTariff = "8203.20.4000";
			childInvoiceLine.JI_LinePrice = 5000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_CustomsValue on parent line is zero", 0m, parentInvoiceLine.US_CustomsValue);
			AssertEquals("US_CustomsValue on child line = (5000 - 25.67) / (1+0.12+0.2+0.25+0.00125)", 3166m, childInvoiceLine.US_CustomsValue);
		}

		[TestDate(2025, 04, 11)]
		public void TestCustomsValueForDDPWithAdditionalSupTariffForADCVD()
		{
			#region Setup Tariffs

			var caseADNumber = Factory.New<USCACCase>();
			caseADNumber.U5_CaseNumber = "A122857070";
			caseADNumber.U5_ISOCountryCode = "CA";
			caseADNumber.U5_CaseStatusDate = ZDateTime.Today.AddDays(-10);
			var caseADRate = caseADNumber.CaseRates.AddNew();
			caseADRate.U6_CaseNumber = "A122857070";
			caseADRate.U6_AdValoremRate = 0.0766m;
			caseADRate.U6_EffectiveDate = ZDateTime.Today.AddDays(-10);
			var caseADTariff = caseADNumber.CaseTariffs.AddNew();
			caseADTariff.U9_CaseNumber = "A122857070";
			caseADTariff.U9_TariffNumber = "4407130000";

			var caseCVDNumber = Factory.New<USCACCase>();
			caseCVDNumber.U5_CaseNumber = "C122858078";
			caseCVDNumber.U5_ISOCountryCode = "CA";
			caseCVDNumber.U5_CaseStatusDate = ZDateTime.Today.AddDays(-10);
			var caseCVDRate = caseCVDNumber.CaseRates.AddNew();
			caseCVDRate.U6_CaseNumber = "C122858078";
			caseCVDRate.U6_AdValoremRate = 0.0674m;
			caseCVDRate.U6_EffectiveDate = ZDateTime.Today.AddDays(-10);
			var caseCVDTariff = caseCVDNumber.CaseTariffs.AddNew();
			caseCVDTariff.U9_TariffNumber = "C122858078";
			caseCVDTariff.U9_TariffNumber = "4407130000";

			var tariff = USCTariffTest.CreateNewTariffIfNotExist(Factory, "4407130000", "1", 0m, "M3");
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_DutyElement = "5";
			dutyRate.UD_TaxFeeClassCode = "105";
			dutyRate.UD_TaxFeeComputationCode = "1";
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.1737m;

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030126", "0", 0m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030114", "0", 0m, "");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030126 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030126", new ZDateTime(2025, 04, 07), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF CANADA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030126);
			var tariffView99030114 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030114", new ZDateTime(2025, 03, 04), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF CANADA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030114);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV25031501";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.US_DeductADDCVDDuty = YesNoDefaultList.Codes.Yes;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "4407.13.0000";
			invoiceLine.SupTariffFormatted = "9903.01.26";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.01.14";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 237.89m;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XC;
			invoiceLine.US_SPI = SpecialProgramList.Codes.S;
			invoiceLine.US_LumberExportPrice = 50260.34m;
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			invoiceLine.LicenceAndPermits.AddNew("11", "P88888888");
			invoiceLine.US_ADDCaseNo = "A122857070";
			invoiceLine.US_ADDDepositRateIndicator = "A";
			invoiceLine.US_CVDCaseNo = "C122858078";
			invoiceLine.US_CVDDepositRateIndicator = "A";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Softwood Lumber fee amount = 0.1737 * 237.89", 41.32m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber));
			AssertEquals("US_CustomsValue = (10000 - 41.32) / (1+0.0766+0.0674)", 8705m, invoiceLine.US_CustomsValue);
			AssertEquals("JI_CustomsValue = 10000 - 41.32 - 8705 * 0.0766 - 8705 * 0.0674", 8705.16m, invoiceLine.JI_CustomsValue);
		}

		[TestDate(2025, 05, 01)]
		public void TestCustomsValueForDDPWithAdditionalTariffWhenSupTariffIsNA()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8536908585", "7", 0m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038801", "7", 0.25m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99038801 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038801", new ZDateTime(2025, 01, 01), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038801);
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2025, 03, 04), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV25031501";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.HongKong;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_FormattedTariff = "8536.90.8585";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.01";
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.01.24";
			invoiceLine.SupTariffFormatted = "N/A";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Payable MPF", 25.67m, invoiceLine.US_PayableMPF);
			AssertEquals("US_CustomsValue = (10000 - 25.67) / (1+0.25+0.2+0.00125)", 6873m, invoiceLine.US_CustomsValue);
			AssertEquals("JI_CustomsValue = 10000 - 25.67 - 6873 * 0.25 - 6873 * 0.2 - 6873 * 0.00125", 6872.89m, invoiceLine.JI_CustomsValue);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}

		void AssertForTestTotalAmountOfDutyFeesTaxesWhenUS_SupTariffHasInvalidValue()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Invoice Header FOB Amount.", 9555.99m, invoice.JZ_Calc_FOBAmount);
				AssertEquals("Invoice Header CIF Amount.", 9555.99m, invoice.JZ_Calc_CIFAmount);
				AssertEquals("Invoice Header DDD Amount.", 444.01m, invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0].J7_Amount);

				AssertEquals("Invoice Line 01 FOB Amount.", 9555.99m, invoice.InvoiceLines[0].FOBValueInLocalCurrency);
				AssertEquals("Invoice Line 01 DDD Amount.", 444.01m, invoice.InvoiceLines[0].ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0].J7_Amount);
				AssertEquals("Invoice Line 01 Duty.", 410.91m, invoice.InvoiceLines[0].US_Duty);
				AssertEquals("Invoice Line 01 Supper Duty.", 0m, invoice.InvoiceLines[0].US_SupDuty);

				var entryHeader = declaration.ActiveEntryHeaders[0];
				AssertEquals("Entry Header DDD Amount.", 444.01m, entryHeader.CH_TotalPaid);

				var cL_CustomsValueSum = entryHeader.MergedLines.Cast<CusEntryLine>().Sum(x => x.CL_CustomsValue);
				AssertEquals("Sum Entry Line Customs Value.", 9555.99m, cL_CustomsValueSum);
			});
		}

		JobComInvoiceLine CreateInvoiceLineForTest(JobComInvoiceHeader invoice, ZString lineNo, ZString tariff, ZDecimal price, ZDecimal quantity)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9903.88.03";
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_LinePrice = price;
			invoiceLine.JI_CustomsQuantity = quantity;
			invoiceLine.JI_Description = "Invoice Line " + lineNo;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.HongKong;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			return invoiceLine;
		}

		USCTariff CreateNewTariffForTest(ZString tariffCode, ZString dutyComputationCode, ZDecimal column1RateAdValorem)
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_DateFrom = ZDateTime.MinSmallDateTimeValue;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTimeValue;
			tariff.UE_DutyComputationCode = dutyComputationCode;
			tariff.UE_Column1RateAdValorem = column1RateAdValorem;
			return tariff;
		}

		ZDecimal CalculateMPFDifference(JobDeclaration declaration)
		{
			var result = ZDecimal.Zero;
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				if (invoiceLine.US_PayableMPF != ZDecimal.Zero)
				{
					result += invoiceLine.US_PayableMPF;
				}
				if (invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) != null)
				{
					result -= invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount;
				}
			}
			return result;
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
	}
}
