using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public sealed class ReconAddInfoJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		[TestDate(2010, 08, 05)]
		public void TestTaxCodeAndRateValidation()
		{
			ReconInvoiceLine.JI_Tariff = "2403.10.2050";
			AssertEquals("", ReconInvoiceLine.US_TaxCode);
			AssertNoMessageErrors(ReconInvoiceLine.US_TaxCodeInfo);
			AssertEquals(true, ReconInvoiceLine.US_TaxCodeInfo.ReadOnly);
			AssertEquals("", ReconInvoiceLine.US_TaxRateS);
			AssertNoMessageErrors(ReconInvoiceLine.US_TaxRateSInfo);
			AssertEquals(true, ReconInvoiceLine.US_TaxRateSInfo.ReadOnly);
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			AssertEquals("", ReconInvoiceLine.US_TaxCode);
			AssertNoMessageErrors(ReconInvoiceLine.US_TaxCodeInfo);
			AssertEquals(true, ReconInvoiceLine.US_TaxCodeInfo.ReadOnly);
			AssertEquals("", ReconInvoiceLine.US_TaxRateS);
			AssertNoMessageErrors(ReconInvoiceLine.US_TaxRateSInfo);
			AssertEquals(true, ReconInvoiceLine.US_TaxRateSInfo.ReadOnly);
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertEquals("Code should default from Tariff", "018", ReconInvoiceLine.US_TaxCode);
			AssertNoMessageErrors(ReconInvoiceLine.US_TaxCodeInfo);
			AssertEquals("Code remains read only", false, ReconInvoiceLine.US_TaxCodeInfo.ReadOnly);
			AssertEquals("Rate should default from Tariff", "$2.41822574/KG", ReconInvoiceLine.US_TaxRateS);
			AssertNoMessageErrors(ReconInvoiceLine.US_TaxRateSInfo);
			AssertEquals("Rate remains read only", false, ReconInvoiceLine.US_TaxRateSInfo.ReadOnly);
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			AssertNoMessageErrors(ReconInvoiceLine.US_TaxCodeInfo);
			AssertEquals("Code should now be enterable", false, ReconInvoiceLine.US_TaxCodeInfo.ReadOnly);
			AssertNoMessageErrors(ReconInvoiceLine.US_TaxRateSInfo);
			AssertEquals("Rate should now be enterable", false, ReconInvoiceLine.US_TaxRateSInfo.ReadOnly);
			ReconInvoiceLine.JI_Tariff = "1901.10.0500";
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			ReconInvoiceLine.AddInfoValidation.ValidateUS_TaxCode();
			AssertHasMessageErrors("Assert field still errors when override is used and value is blank.", ReconInvoiceLine.US_TaxCodeInfo);
			ReconInvoiceLine.US_TaxRateS = "";
			ReconInvoiceLine.AddInfoValidation.ValidateUS_TaxRateS();
			AssertHasMessageErrors("Assert field still errors when override is used and value left blank.", ReconInvoiceLine.US_TaxRateSInfo);
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertEquals("Code should be blank", "", ReconInvoiceLine.US_TaxCode);
			ReconInvoiceLine.AddInfoValidation.ValidateUS_TaxCode();
			AssertNoMessageErrors("No message error should be shown as use of Y for tax override is invalid for this tariff and errors there", ReconInvoiceLine.US_TaxCodeInfo);
			AssertEquals("Code remains read only", false, ReconInvoiceLine.US_TaxCodeInfo.ReadOnly);
			ReconInvoiceLine.AddInfoValidation.ValidateUS_TaxRateS();
			AssertEquals("Rate should be blank", "", ReconInvoiceLine.US_TaxRateS);

			var message = MandatoryValidation.YouHaveNotEnteredMessage("tax rate");
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_TaxRateSInfo, message);

			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			ReconInvoiceLine.AddInfoValidation.ValidateUS_TaxRateS();
			AssertNoMessageErrors("No message error should be shown as use of Y for tax override is invalid for this tariff and errors there", ReconInvoiceLine.US_TaxRateSInfo);
			AssertEquals("Rate remains read only", true, ReconInvoiceLine.US_TaxRateSInfo.ReadOnly);
		}

		public void TestCheckUS_SupQty1()
		{
			ReconInvoiceLine.US_SupTariff = "9810001000";
			ReconInvoiceLine.US_SupQty1 = ZDecimal.Zero;
			AssertHasWarning(ReconInvoiceLine.US_SupQty1Info, ValidationConstants.WarnIfStatQTYisZero);
			ReconInvoiceLine.US_SupQty1 = 10m;
			AssertNoWarning(ReconInvoiceLine.US_SupQty1Info, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckUS_R_OrigSupQty1()
		{
			ReconInvoiceLine.US_R_OrigSupTariff = "9810001000";
			ReconInvoiceLine.US_R_OrigSupQty1 = ZDecimal.Zero;
			AssertHasWarning(ReconInvoiceLine.US_R_OrigSupQty1Info, ValidationConstants.WarnIfStatQTYisZero);
			ReconInvoiceLine.US_R_OrigSupQty1 = 10m;
			AssertNoWarning(ReconInvoiceLine.US_R_OrigSupQty1Info, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckUS_SupQty2()
		{
			ReconInvoiceLine.US_SupTariff = "9810001000";
			ReconInvoiceLine.US_SupUQ2 = "KG";
			ReconInvoiceLine.US_SupQty2 = ZDecimal.Zero;
			AssertHasWarning(ReconInvoiceLine.US_SupQty2Info, ValidationConstants.WarnIfStatQTYisZero);
			ReconInvoiceLine.US_SupQty2 = 10m;
			AssertNoWarning(ReconInvoiceLine.US_SupQty2Info, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckUS_R_OrigSupQty2()
		{
			ReconInvoiceLine.US_R_OrigSupTariff = "9810001000";
			ReconInvoiceLine.US_R_OrigSupUQ2 = "KG";
			ReconInvoiceLine.US_R_OrigSupQty2 = ZDecimal.Zero;
			AssertHasWarning(ReconInvoiceLine.US_R_OrigSupQty2Info, ValidationConstants.WarnIfStatQTYisZero);
			ReconInvoiceLine.US_R_OrigSupQty2 = 10m;
			AssertNoWarning(ReconInvoiceLine.US_R_OrigSupQty2Info, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckUS_SupQty3()
		{
			ReconInvoiceLine.US_SupTariff = "9810001000";
			ReconInvoiceLine.US_SupUQ3 = "KG";
			ReconInvoiceLine.US_SupQty3 = ZDecimal.Zero;
			AssertHasWarning(ReconInvoiceLine.US_SupQty3Info, ValidationConstants.WarnIfStatQTYisZero);
			ReconInvoiceLine.US_SupQty3 = 10m;
			AssertNoWarning(ReconInvoiceLine.US_SupQty3Info, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckUS_R_OrigSupQty3()
		{
			ReconInvoiceLine.US_R_OrigSupTariff = "9810001000";
			ReconInvoiceLine.US_R_OrigSupUQ3 = "KG";
			ReconInvoiceLine.US_R_OrigSupQty3 = ZDecimal.Zero;
			AssertHasWarning(ReconInvoiceLine.US_R_OrigSupQty3Info, ValidationConstants.WarnIfStatQTYisZero);
			ReconInvoiceLine.US_R_OrigSupQty3 = 10m;
			AssertNoWarning(ReconInvoiceLine.US_R_OrigSupQty3Info, ValidationConstants.WarnIfStatQTYisZero);
		}

		[TestDate(2009, 1, 1)]
		public void TestCheckOverrideTaxIndicator()
		{
			// When tax is required
			ReconInvoiceLine.US_R_OrigTariff = USCTariff.DistilledSpiritsFeeApplicable;
			ReconInvoiceLine.US_R_OrigTaxApply = ZString.Empty;
			AssertHasMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxIsRequired);
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.No;
			AssertHasMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxIsRequired);
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxIsRequired);
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxIsRequired);
			// When tax is conditional
			ReconInvoiceLine.US_R_OrigTariff = USCTariff.TaxConditional;
			ReconInvoiceLine.US_R_OrigTaxApply = ZString.Empty;
			AssertHasMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxApplicabilityMustBeSelected);
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.No;
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxApplicabilityMustBeSelected);
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxIsRequired);
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxApplicabilityMustBeSelected);
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxIsRequired);
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxApplicabilityMustBeSelected);
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, USCTariff.TaxIsRequired);
			ReconInvoiceLine.US_R_OrigTariff = ZString.Empty;
			ReconInvoiceLine.US_R_OrigTaxApply = ZString.Empty;
			AssertNoMessageErrors(ReconInvoiceLine.US_R_OrigTaxApplyInfo);
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
			AssertHasMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.SelectOverrideAndIndicateTaxCode);
			ReconInvoiceLine.US_R_OrigTaxCode = "016";
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
			AssertHasMessageError(ReconInvoiceLine.US_R_OrigTaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.SelectOverrideAndIndicateTaxRate);
		}

		public void TestListValidationForUS_TaxApply()
		{
			ReconInvoiceLine.US_TaxApply = "~";
			ReconInvoiceLine.US_R_OrigTaxApply = "~";
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_TaxApplyInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_OrigTaxApplyInfo, ListValidation.InvalidCodeMessageError);
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_TaxApplyInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_R_OrigTaxApplyInfo, ListValidation.InvalidCodeMessageError);
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertHasMessageError(ReconInvoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.SelectOverrideAndIndicateTaxCode);
			ReconInvoiceLine.US_TaxCode = "016";
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			ReconInvoiceLine.AddInfoValidation.ValidateUS_TaxApply();
			AssertHasMessageError(ReconInvoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.SelectOverrideAndIndicateTaxRate);
		}

		public void TestCheckUS_TaxRateQuantity()
		{
			ReconInvoiceLine.JI_CustomsUnitQty = "K";
			ReconInvoiceLine.JI_CustomsSecondUnitQty = "";
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			ReconInvoiceLine.US_TaxCode = "016";
			ReconInvoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_1; //"15.3389c/L";
			ReconInvoiceLine.AddInfoValidation.ValidateUS_TaxQty();
			AssertHasMessageError(ReconInvoiceLine.US_TaxQtyInfo, string.Format(USAddInfoValidation.UnitOfQuantityDoesNotMatch, "L"));
			ReconInvoiceLine.US_TaxQty = 120m;
			AssertNoMessageError(ReconInvoiceLine.US_TaxQtyInfo, string.Format(USAddInfoValidation.UnitOfQuantityDoesNotMatch, "L"));
		}

		public void TestCheckUS_OrigTaxRateQuantity()
		{
			ReconInvoiceLine.US_R_OrigFirstUQ = "K";
			ReconInvoiceLine.US_R_OrigSecondUQ = "";
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			ReconInvoiceLine.US_R_OrigTaxCode = "016";
			ReconInvoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Wines_1; //"15.3389c/L";
			ReconInvoiceLine.AddInfoValidation.ValidateUS_R_OrigTaxQty();
			AssertHasMessageError(ReconInvoiceLine.US_R_OrigTaxQtyInfo, string.Format(USAddInfoValidation.UnitOfQuantityDoesNotMatch, "L"));
			ReconInvoiceLine.US_R_OrigTaxQty = 120m;
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigTaxQtyInfo, string.Format(USAddInfoValidation.UnitOfQuantityDoesNotMatch, "L"));
		}

		public void TestListValidationForUS_TaxCode()
		{
			ReconInvoiceLine.US_TaxCode = "~";
			ReconInvoiceLine.US_R_OrigTaxCode = "~";
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_TaxCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_OrigTaxCodeInfo, ListValidation.InvalidCodeMessageError);
			ReconInvoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			ReconInvoiceLine.US_R_OrigTaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_TaxCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_R_OrigTaxCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_TaxRateS()
		{
			var message = MandatoryValidation.YouHaveNotEnteredMessage("tax rate");
			ReconInvoiceLine.JI_CustomsUnitQty = "L";
			ReconInvoiceLine.US_R_OrigFirstUQ = "PFL";
			ReconInvoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			ReconInvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
			ReconInvoiceLine.US_TaxRateS = string.Empty;
			ReconInvoiceLine.AddInfoValidation.ValidateUS_TaxRateS();
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_TaxRateSInfo, message);
			ReconInvoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			ReconInvoiceLine.US_R_OrigTaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			ReconInvoiceLine.US_R_OrigTaxRateS = string.Empty;
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_OrigTaxRateSInfo, message);
			ReconInvoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Tobacco_1;
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_TaxRateSInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_TaxRateSInfo, message);
			ReconInvoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Other_4;
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_OrigTaxRateSInfo, ListValidation.InvalidCodeMessageError);
			ReconInvoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Other_3;
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_TaxRateSInfo, ListValidation.InvalidCodeMessageError);
			ReconInvoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.DistilledSpirits;
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_R_OrigTaxRateSInfo, ListValidation.InvalidCodeMessageError);
			ReconInvoiceLine.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;
			AssertNoMessageErrors(ReconInvoiceLine.US_TaxRateSInfo);
		}

		public void TestCheckUS_SPI()
		{
			ReconInvoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			AssertEquals(false, ReconInvoiceLine.US_SPIInfo.HasMessageErrors());
			ReconInvoiceLine.US_SPI = "XX";
			AssertEquals(true, ReconInvoiceLine.US_SPIInfo.HasMessageErrors());
		}

		public void TestCheckUS_SecondarySPI()
		{
			ReconInvoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals(false, ReconInvoiceLine.US_SecondarySPIInfo.HasMessageErrors());
			ReconInvoiceLine.US_SecondarySPI = "Q";
			AssertEquals(true, ReconInvoiceLine.US_SecondarySPIInfo.HasMessageErrors());
		}

		public void TestCheckUS_R_OrigFirstQty()
		{
			ReconInvoiceLine.US_R_OrigTariff = Tariff.UE_Tariff;
			ReconInvoiceLine.US_R_OrigFirstQty = 0m;
			ReconInvoiceLine.AddInfoValidation.ValidateUS_R_OrigFirstQty();
			AssertHasWarning(ReconInvoiceLine.US_R_OrigFirstQtyInfo, ValidationConstants.WarnIfStatQTYisZero);
			ReconInvoiceLine.US_R_OrigFirstQty = 1m;
			AssertNoWarning(ReconInvoiceLine.US_R_OrigFirstQtyInfo, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckUS_R_OrigSecondQty()
		{
			ReconInvoiceLine.US_R_OrigTariff = Tariff.UE_Tariff;
			ReconInvoiceLine.US_R_OrigSecondQty = 0m;
			ReconInvoiceLine.AddInfoValidation.ValidateUS_R_OrigSecondQty();
			AssertHasWarning(ReconInvoiceLine.US_R_OrigSecondQtyInfo, ValidationConstants.WarnIfStatQTYisZero);
			ReconInvoiceLine.US_R_OrigSecondQty = 1m;
			AssertNoWarning(ReconInvoiceLine.US_R_OrigSecondQtyInfo, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckUS_R_OrigThirdQty()
		{
			ReconInvoiceLine.US_R_OrigTariff = Tariff.UE_Tariff;
			ReconInvoiceLine.US_R_OrigThirdQty = 0m;
			ReconInvoiceLine.AddInfoValidation.ValidateUS_R_OrigThirdQty();
			AssertHasWarning(ReconInvoiceLine.US_R_OrigThirdQtyInfo, ValidationConstants.WarnIfStatQTYisZero);
			ReconInvoiceLine.US_R_OrigThirdQty = 1m;
			AssertNoWarning(ReconInvoiceLine.US_R_OrigThirdQtyInfo, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckUS_R_OrigSPI()
		{
			ReconInvoiceLine.US_R_OrigSPI = "~";
			AssertHasMessageError(ReconInvoiceLine.US_R_OrigSPIInfo, ListValidation.InvalidCodeMessageError);
			ReconInvoiceLine.US_R_OrigSPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertNoMessageError(ReconInvoiceLine.US_R_OrigSPIInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_R_OrigTariff()
		{
			ReconInvoiceLine.InvoiceHeader.ReconOriginalEntry.US_R_DutyRateDate = ZDateTime.Today;
			ReconInvoiceLine.US_R_OrigTariff = "0000000000";
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_OrigTariffInfo, "Tariff unable to be found. ");
			ReconInvoiceLine.US_R_OrigTariff = USCTariff.AdditionalDutyCalculationApplicable;
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_R_OrigTariffInfo, "Tariff unable to be found. ");
			ReconInvoiceLine.US_R_OrigTariff = "98010040";
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_R_OrigTariffInfo, TariffValidator.TariffNumber99ShouldNotBeEnteredHere);
			ReconInvoiceLine.US_R_OrigTariff = "9810001000";
			Assert("PreCondition", !ReconInvoiceLine.OriginalImportTariff.UE_AdditionalTariffNumberIndicator);
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_R_OrigTariffInfo, TariffValidator.TariffNumber99ShouldNotBeEnteredHere);
			ReconInvoiceLine.US_R_OrigTariff = USCTariff.CAFTABenefitsApplicable;
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_OrigTariffInfo, TariffValidator.TariffNumber99ShouldNotBeEnteredHere);
		}

		public void TestCheckUS_SupTariff()
		{
			ReconInvoiceLine.US_SupTariff = "9810001000";
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_SupTariffInfo, TariffValidator.NoSecondaryTariffNumberAllowed);
			ReconInvoiceLine.US_SupTariff = "98010040";
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_SupTariffInfo, TariffValidator.NoSecondaryTariffNumberAllowed);
			ReconInvoiceLine.US_SupTariff = USCTariff.LumberPermitApplicable;
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_SupTariffInfo, TariffValidator.TariffNumber98_99ShouldBeEnteredHere);
			ReconInvoiceLine.US_SupTariff = "98010040";
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_SupTariffInfo, TariffValidator.TariffNumber98_99ShouldBeEnteredHere);
			ReconInvoiceLine.US_SupTariff = ZString.Empty;
			AssertNoMessageErrors(ReconInvoiceLine.US_SupTariffInfo);
		}

		public void TestCheckUS_R_OrigSupTariff()
		{
			ReconInvoiceLine.US_R_OrigSupTariff = "9810001000";
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_OrigSupTariffInfo, TariffValidator.NoSecondaryTariffNumberAllowed);
			ReconInvoiceLine.US_R_OrigSupTariff = "98010040";
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_R_OrigSupTariffInfo, TariffValidator.NoSecondaryTariffNumberAllowed);
			ReconInvoiceLine.US_R_OrigSupTariff = USCTariff.FCCApplicable;
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_OrigSupTariffInfo, TariffValidator.TariffNumber98_99ShouldBeEnteredHere);
			ReconInvoiceLine.US_R_OrigSupTariff = "98010040";
			AssertNoMessageErrorContaining(ReconInvoiceLine.US_R_OrigSupTariffInfo, TariffValidator.TariffNumber98_99ShouldBeEnteredHere);
			ReconInvoiceLine.US_R_OrigSupTariff = ZString.Empty;
			AssertNoMessageErrors(ReconInvoiceLine.US_R_OrigSupTariffInfo);
		}

		public void TestCheckUS_UC_NKCountryOfOrigin()
		{
			ReconInvoiceLine.US_UC_NKCountryOfOrigin = "AU";
			AssertNoMessageError(ReconInvoiceLine.US_UC_NKCountryOfOriginInfo, ReconAddInfoJobComInvoiceLineValidation.CountryOfOriginRequiresForDutyCalculations);
			ReconInvoiceLine.US_UC_NKCountryOfOrigin = "";
			AssertHasMessageError(ReconInvoiceLine.US_UC_NKCountryOfOriginInfo, ReconAddInfoJobComInvoiceLineValidation.CountryOfOriginRequiresForDutyCalculations);
		}

		public void TestCheckUS_R_HTSChanged4ValueInd()
		{
			ReconInvoiceLine.Declaration.ReconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ValueClass9802Recon;
			ReconInvoiceLine.US_R_HTSChanged4ValueInd = true;
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_HTSChanged4ValueIndInfo, ReconAddInfoJobComInvoiceLineValidation.HTSChanged4ValueIndNotNeededForValueAndClassification);
			ReconInvoiceLine.US_R_HTSChanged4ValueInd = false;
			ReconInvoiceLine.Declaration.ReconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ValueClassRecon;
			ReconInvoiceLine.US_R_HTSChanged4ValueInd = true;
			AssertHasMessageErrorContaining(ReconInvoiceLine.US_R_HTSChanged4ValueIndInfo, ReconAddInfoJobComInvoiceLineValidation.HTSChanged4ValueIndNotNeededForValueAndClassification);
			ReconInvoiceLine.US_R_HTSChanged4ValueInd = false;
			ReconInvoiceLine.Declaration.ReconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			ReconInvoiceLine.US_R_HTSChanged4ValueInd = true;
			AssertNoMessageError(ReconInvoiceLine.US_R_HTSChanged4ValueIndInfo, ReconAddInfoJobComInvoiceLineValidation.HTSChanged4ValueIndNotNeededForValueAndClassification);
		}

		public void TestUS_R_OrigEntryLineNo()
		{
			ReconInvoiceLine.US_R_OrigEntryLineNo = "a00";
			AssertHasMessageError("Invalid characters", ReconInvoiceLine.US_R_OrigEntryLineNoInfo, "The Orignal Entry Line Number should be a numeric format string with up to 3 digits or alphanumeric characters preceding with an asterisk followed by up to 2 digits.");
			ReconInvoiceLine.US_R_OrigEntryLineNo = "0*0";
			AssertHasMessageError("Asterisk is not the leading character", ReconInvoiceLine.US_R_OrigEntryLineNoInfo, "The Orignal Entry Line Number should be a numeric format string with up to 3 digits or alphanumeric characters preceding with an asterisk followed by up to 2 digits.");
			ReconInvoiceLine.US_R_OrigEntryLineNo = "*01";
			AssertNoMessageError("Valid line number with asterisk", ReconInvoiceLine.US_R_OrigEntryLineNoInfo, "The Orignal Entry Line Number should be a numeric format string with up to 3 digits or alphanumeric characters preceding with an asterisk followed by up to 2 digits.");
			ReconInvoiceLine.US_R_OrigEntryLineNo = "001";
			AssertNoMessageError("Valid line number without asterisk", ReconInvoiceLine.US_R_OrigEntryLineNoInfo, "The Orignal Entry Line Number should be a numeric format string with up to 3 digits or alphanumeric characters preceding with an asterisk followed by up to 2 digits.");
		}

		ReconDeclaration recon;
		ReconDeclaration Recon => recon ?? (recon = new ReconDeclaration(Factory.New<JobDeclaration>()));

		JobComInvoiceLine reconInvoiceLine;
		JobComInvoiceLine ReconInvoiceLine
		{
			get
			{
				if (reconInvoiceLine == null)
				{
					var reconInvoice = Recon.Invoices.AddNew();
					var reconEntry = Recon.OriginalEntries.AddNew();
					reconInvoice.US_CH_ReconEntry = reconEntry.CH_PK;
					reconInvoice.ReconOriginalEntry.US_R_DutyRateDate = ZDateTime.Now;
					reconInvoiceLine = reconInvoice.InvoiceLines.AddNew();
				}

				return reconInvoiceLine;
			}
		}

		USCTariff tariff;
		USCTariff Tariff
		{
			get
			{
				if (tariff == null)
				{
					tariff = Factory.New<USCTariff>();
					tariff.UE_Tariff = "00000000";
					tariff.UE_Unit1 = "LTR";
					tariff.UE_Unit2 = "LTR";
					tariff.UE_Unit3 = "LTR";
					tariff.UE_DateFrom = new ZDateTime(2008, 9, 11);
					tariff.UE_DateTo = ZDateTime.Now;
				}

				return tariff;
			}
		}
	}
}
