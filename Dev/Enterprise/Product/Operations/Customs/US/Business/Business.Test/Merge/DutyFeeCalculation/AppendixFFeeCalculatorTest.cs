using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AppendixFFeeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2009, 1, 1)]
		public void TestCalculateWithKAnd50()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "2402.20.9000";

			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.JI_CustomsUnitQty = "K";

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Other_1;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Fee calculated with overriden tax rate", 3.15m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.OtherExcise).CY_FeeAmount);
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateWithOverriddenTaxRate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "2204216000";
			AssertNotNull("PreCondition", invoiceLine.ImportTariff);

			invoiceLine.JI_CustomsQuantity = 150m;
			AssertNotNull(invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Wines));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Fee calculated normally with a primary rate", 62.21000m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Wines).CY_FeeAmount);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_TaxRate = 0.9999m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Fee calculated with overriden tax rate", 149.99m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Wines).CY_FeeAmount);

			invoiceLine.US_TaxRate = 0m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Fee calculated with overriden tax rate", 0m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Wines));

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Fee calculated normally with a primary rate", 62.21000m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Wines).CY_FeeAmount);
		}

		public void TestWhenTaxIsNotApplicableForImport()
		{
			SetUpTariffsForTaxRelatedFields();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalIRTTaxes);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(50m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalIRTTaxes);
		}

		[TestDate(2009, 1, 1)]
		public void TestWhenTwoTobaccoTariffs()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2403102050";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Tax, Duty and MPF", 241.82m + 32.8m + 25m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);

			invoiceLine.JI_Tariff = "2403102080";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Tax, Duty and MPF", 241.82m + 32.8m + 25m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		public void TestTaxWithSupplementaryTariff()
		{
			SetUpTariffsForTaxRelatedFields();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801001015";
			invoiceLine.JI_Tariff = "00000002";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_TaxRateT = RateTypeList.Codes.Primary;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(70m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);

			invoiceLine.US_TaxRateT = RateTypeList.Codes.Secondary;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(80m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAmountPayable);
		}

		[TestDate(2013, 01, 01)]
		public void TestTaxCalculationWhenRateUQDoesNotMatchCustomsUQs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3303003000";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Other_4;
			invoiceLine.US_TaxQty = 10m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(35.66m, entry.OtherExciseTax);

			Factory.Save();

			var message = new MessageBuilders.EntrySummaryMessageBuilder(entry, US.Messaging.Business.UpdateActionCode.Add, false).PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);

			AssertEquals("DEF $3.5663227/L", entry.US7501DocPrintingData[0].US_FEEPercentAsString);
		}

		[TestDate(2025, 05, 01)]
		public void TestTaxCalculationFromCustomsValueWithSupplementaryTariff()
		{
			#region Setup Tariffs

			var tariff = USCTariffTest.CreateNewTariffIfNotExist(Factory, "2402108050", "5", 0.014m, "K");
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_DutyElement = "5";
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			dutyRate.UD_TaxFeeFlag = "1";

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030121", "0", 0m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030121 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030121", new ZDateTime(2025, 02, 04), new ZDateTime(2079, 06, 06), "ALL IMPORTS OF ARTICLES THAT ARE PRODUCTS OF CHINA AND HONG KONG", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030121);
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2025, 03, 04), new ZDateTime(2079, 06, 06), "ALL IMPORTS OF ARTICLES THAT ARE PRODUCTS OF CHINA AND HONG KONG", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "2402.10.8050";
			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Tobacco_2;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Tobacco tax amount", 527.5m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));

			invoiceLine.FeeCusCodes.RemoveAndDeleteAll();
			invoiceLine.SupTariffFormatted = "9903.01.21";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Tobacco tax amount", 527.5m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));

			invoiceLine.FeeCusCodes.RemoveAndDeleteAll();
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.01.24";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Tobacco tax amount", 527.5m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
		}

		void SetUpTariffsForTaxRelatedFields()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_Unit1 = "L";

			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			dutyRate2.UD_TaxFeeFlag = "2";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "00000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff3.UE_Unit1 = "PFL";

			USCTariffDutyRate dutyRate3 = tariff3.DutyRates.AddNew();
			dutyRate3.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate3.UD_TaxFeeFlag = "2";
			dutyRate3.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate3.UD_TaxFeeSpecificRate = 0.7m;
			dutyRate3.UD_TaxFeeAdvalorem = 0.8m;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
