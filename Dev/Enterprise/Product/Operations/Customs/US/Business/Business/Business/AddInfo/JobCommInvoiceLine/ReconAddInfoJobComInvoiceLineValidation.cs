using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public ReconAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine addInfo)
			: base(addInfo)
		{
		}

		protected new JobComInvoiceLine Parent
		{
			get { return base.Parent.InvoiceLine; }
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return Parent; }
		}

		protected override void CheckUS_TaxRate()
		{
			base.CheckUS_TaxRate();

			if (Parent.US_TaxRate <= 0m && Parent.US_TaxRateS.EqualsIgnoringCase(AppendixBTaxRateList.Codes.Specify))
			{
				Parent.US_TaxRateInfo.AddMessageError(TaxRateShouldBeEntered);
			}
		}

		protected override void CheckUS_R_OrigTaxRate()
		{
			base.CheckUS_R_OrigTaxRate();

			if (Parent.US_R_OrigTaxRate <= 0m && Parent.US_R_OrigTaxRateS.EqualsIgnoringCase(AppendixBTaxRateList.Codes.Specify))
			{
				Parent.US_R_OrigTaxRateInfo.AddMessageError(TaxRateShouldBeEntered);
			}
		}

		protected override void CheckUS_R_OrigTaxApply()
		{
			base.CheckUS_R_OrigTaxApply();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_R_OrigTaxApplyInfo, Parent.AddInfoLookups.TaxApplyList);
			CheckTaxApply(Parent.US_R_OrigTaxApplyInfo, Parent.US_R_OrigTaxCode, Parent.US_R_OrigTaxRateS, Parent.OriginalImportTariff);
		}

		protected override void CheckUS_R_OrigTaxCode()
		{
			base.CheckUS_R_OrigTaxCode();

			if (Parent.IsOriginalTaxRateOverridden)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_R_OrigTaxCodeInfo, "tax code");
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.US_R_OrigTaxCodeInfo, Parent.AddInfoLookups.OrigTaxCodeList);
		}

		protected override void CheckUS_TaxApply()
		{
			base.CheckUS_TaxApply();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TaxApplyInfo, Parent.AddInfoLookups.TaxApplyList);
			CheckTaxApply(Parent.US_TaxApplyInfo, Parent.US_TaxCode, Parent.US_TaxRateS, Parent.ImportTariff);
		}

		protected override void CheckUS_TaxCode()
		{
			base.CheckUS_TaxCode();

			if (Parent.IsTaxRateOverridden)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TaxCodeInfo, "tax code");
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.US_TaxCodeInfo, Parent.AddInfoLookups.TaxCodeList);
		}

		protected override void CheckUS_TaxRateS()
		{
			base.CheckUS_TaxRateS();
			if (Parent.US_TaxRateS.IsEmpty)
			{
				if (Parent.IsTaxApplicable)
				{
					Parent.US_TaxRateSInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(TaxRate));
				}
			}
			else if (Parent.IsTaxRateReduced)
			{
				// do nothing
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_TaxRateSInfo, Parent.AddInfoLookups.TaxRateList);
			}
		}

		const string TaxRate = "tax rate";

		protected override void CheckUS_R_OrigTaxRateS()
		{
			base.CheckUS_R_OrigTaxRateS();

			if (Parent.US_R_OrigTaxRateS.IsEmpty)
			{
				if (Parent.IsOrigTaxRateApplicable)
				{
					Parent.US_R_OrigTaxRateSInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(TaxRate));
				}
			}
			else if (Parent.IsOriginalTaxRateReduced)
			{
				// do nothing
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_R_OrigTaxRateSInfo, Parent.AddInfoLookups.OrigTaxRateList);
			}
		}

		protected override void CheckUS_TaxQty()
		{
			base.CheckUS_TaxQty();
			CheckUnitOfOverriddenTaxRateAgainstCustomsUQs(Parent.US_TaxQtyInfo, Parent.JI_CustomsUnitQty, Parent.JI_CustomsSecondUnitQty, Parent.US_TaxRateS);
		}

		protected override void CheckUS_R_OrigTaxQty()
		{
			base.CheckUS_R_OrigTaxQty();
			CheckUnitOfOverriddenTaxRateAgainstCustomsUQs(Parent.US_R_OrigTaxQtyInfo, Parent.US_R_OrigFirstUQ, Parent.US_R_OrigSecondUQ, Parent.US_R_OrigTaxRateS);
		}

		protected override void CheckUS_SPI()
		{
			base.CheckUS_SPI();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SPIInfo, Lookups.SPIList);
		}

		protected override void CheckUS_R_OrigFirstQty()
		{
			base.CheckUS_R_OrigFirstQty();
			if (Parent.OriginalImportTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.US_R_OrigFirstQtyInfo, Parent.US_R_OrigFirstUQ,
					() => Parent.OriginalImportTariff.RequiresFirstQuantity(), QuantityCode.FirstQuantity);
			}
		}

		protected override void CheckUS_R_OrigSecondQty()
		{
			base.CheckUS_R_OrigSecondQty();
			if (Parent.OriginalImportTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.US_R_OrigSecondQtyInfo, Parent.US_R_OrigSecondUQ,
					() => Parent.OriginalImportTariff.RequiresSecondQuantity(), QuantityCode.SecondQuantity);
			}
		}

		protected override void CheckUS_R_OrigThirdQty()
		{
			base.CheckUS_R_OrigThirdQty();
			if (Parent.OriginalImportTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.US_R_OrigThirdQtyInfo, Parent.US_R_OrigThirdUQ,
					() => Parent.OriginalImportTariff.RequiresThirdQuantity(), QuantityCode.ThirdQuantity);
			}
		}

		protected override void CheckUS_SupQty1()
		{
			base.CheckUS_SupQty1();
			if (Parent.ImportSupTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.US_SupQty1Info, Parent.US_SupUQ1,
					() => Parent.ImportSupTariff.RequiresFirstQuantity(), QuantityCode.FirstQuantity);
			}
		}

		protected override void CheckUS_R_OrigSupQty1()
		{
			base.CheckUS_R_OrigSupQty1();
			if (Parent.OriginalImportSupTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.US_R_OrigSupQty1Info, Parent.US_R_OrigSupUQ1,
					delegate
					{ return Parent.OriginalImportSupTariff.RequiresFirstQuantity(); }, QuantityCode.FirstQuantity);
			}
		}

		protected override void CheckUS_SupQty2()
		{
			base.CheckUS_SupQty2();
			if (Parent.ImportSupTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.US_SupQty2Info, Parent.US_SupUQ2,
					() => Parent.ImportSupTariff.RequiresSecondQuantity(), QuantityCode.SecondQuantity);
			}
		}

		protected override void CheckUS_R_OrigSupQty2()
		{
			base.CheckUS_R_OrigSupQty2();
			if (Parent.OriginalImportSupTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.US_R_OrigSupQty2Info, Parent.US_R_OrigSupUQ2,
					() => Parent.OriginalImportSupTariff.RequiresSecondQuantity(), QuantityCode.SecondQuantity);
			}
		}

		protected override void CheckUS_SupQty3()
		{
			base.CheckUS_SupQty3();
			if (Parent.ImportSupTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.US_SupQty3Info, Parent.US_SupUQ3,
					() => Parent.ImportSupTariff.RequiresThirdQuantity(), QuantityCode.ThirdQuantity);
			}
		}

		protected override void CheckUS_R_OrigSupQty3()
		{
			base.CheckUS_R_OrigSupQty3();
			if (Parent.OriginalImportSupTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.US_R_OrigSupQty3Info, Parent.US_R_OrigSupUQ3,
					() => Parent.OriginalImportSupTariff.RequiresThirdQuantity(), QuantityCode.ThirdQuantity);
			}
		}

		protected override void CheckUS_R_OrigSPI()
		{
			base.CheckUS_R_OrigSPI();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_R_OrigSPIInfo, Parent.AddInfoLookups.ReconOrigSPIList);
		}

		protected override void CheckUS_R_OrigTariff()
		{
			base.CheckUS_R_OrigTariff();
			TariffValidator.Validate(Parent, Parent.OriginalImportTariff, Parent.US_R_OrigTariffInfo);
			TariffValidator.Ensure98_99IsNotEntered(Parent.US_R_OrigTariffInfo, Parent.OriginalImportTariff);
			TariffValidator.ValidateBasedOnAdditionalTariffNumberIndicator(Parent, Parent.OriginalImportTariff, Parent.US_R_OrigTariffInfo, Parent.HasSecondaryTariffLines);

			USCTariff tariff = Parent.ParentTariffLine != null ? Parent.ParentTariffLine.OriginalImportTariff : Parent.OriginalImportTariff;
			TariffValidator.CheckSTNRule(Parent, tariff, Parent.US_R_OrigTariffInfo, false);
		}

		protected override void CheckUS_SupTariff()
		{
			base.CheckUS_SupTariff();
			TariffValidator.Validate(Parent, Parent.ImportSupTariff, Parent.US_SupTariffInfo);
			TariffValidator.ValidateBasedOnAdditionalTariffNumberIndicator(Parent, Parent.ImportSupTariff, Parent.US_SupTariffInfo, true);
			TariffValidator.Ensure98_99IsEntered(Parent.US_SupTariffInfo);
		}

		protected override void CheckUS_R_OrigSupTariff()
		{
			base.CheckUS_R_OrigSupTariff();
			TariffValidator.Validate(Parent, Parent.OriginalImportSupTariff, Parent.US_R_OrigSupTariffInfo);
			TariffValidator.ValidateBasedOnAdditionalTariffNumberIndicator(Parent, Parent.OriginalImportSupTariff, Parent.US_R_OrigSupTariffInfo, true);
			TariffValidator.Ensure98_99IsEntered(Parent.US_R_OrigSupTariffInfo);
		}

		protected override void CheckUS_UC_NKCountryOfOrigin()
		{
			if (Parent.US_UC_NKCountryOfOrigin.IsEmpty)
			{
				Parent.US_UC_NKCountryOfOriginInfo.AddMessageError(CountryOfOriginRequiresForDutyCalculations);
			}
			else
			{
				base.CheckUS_UC_NKCountryOfOrigin();
			}
		}
		internal const string CountryOfOriginRequiresForDutyCalculations = "Please enter a Country of Origin.  A Country of Origin is required for duty calculations.";

		protected override void CheckUS_R_HTSChanged4ValueInd()
		{
			base.CheckUS_R_HTSChanged4ValueInd();
			if (InvoiceLine.Declaration != null && InvoiceLine.Declaration.ReconDeclaration != null &&
				ReconIssueCodeList.IsValueAndClassification(InvoiceLine.Declaration.ReconDeclaration.US_IssueCode) && Parent.US_R_HTSChanged4ValueInd)
			{
				Parent.US_R_HTSChanged4ValueIndInfo.AddMessageError(HTSChanged4ValueIndNotNeededForValueAndClassification);
			}
		}
		internal const string HTSChanged4ValueIndNotNeededForValueAndClassification = "HTS changed due to value should not be ticked when recon. issue is value & classification.";

		protected override void CheckUS_R_OrigEntryLineNo()
		{
			string validPattern = @"^\d{1,3}$|^\*\d{1,2}$";

			if (Parent.US_R_OrigEntryLineNo != ZString.Empty && !Regex.IsMatch(Parent.US_R_OrigEntryLineNo, validPattern))
			{
				Parent.US_R_OrigEntryLineNoInfo.AddMessageError(InvalidOrignalEntryLineNumber);
			}
			base.CheckUS_R_OrigEntryLineNo();
		}
		const string InvalidOrignalEntryLineNumber = "The Orignal Entry Line Number should be a numeric format string with up to 3 digits or alphanumeric characters preceding with an asterisk followed by up to 2 digits.";
	}
}
