using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ACEConstituentElementAddInfoValidation : USConstituentElementAddInfoValidation
	{
		public ACEConstituentElementAddInfoValidation(ConstituentElementAddInfo parent) : base(parent)
		{
		}

		protected override bool IsNameRequired
		{
			get
			{
				var fda = ConstituentElement?.Parent as ACEFDA;
				var lacey = ConstituentElement?.LaceyAct;
				return Parent.US_PGANameOfTheConstituentElement.IsEmpty && ((lacey != null && !lacey.US_UnknownBreakdownTotal) || (fda != null && fda.IsProductConstituentElementRequired));
			}
		}

		protected override bool IsQtyRequired
		{
			get
			{
				var fda = ConstituentElement?.Parent as ACEFDA;
				if (fda != null)
				{
					var isRequiredForFDA = fda.US_ProgramCode == FDAProgramCodeList.Codes.VME && fda.US_ProcessingCode == FDAProcessingCodeList.Codes.VME_ADR && Parent.US_PGAPercentOfConstituentElement.IsEmpty;
					isRequiredForFDA |= fda.US_ProgramCode == FDAProgramCodeList.Codes.DRU && FDAIntendedUseCodesHelper.IsActivePharmaceuticalIngredients(fda.US_IntendedUseCode);
					return isRequiredForFDA;
				}
				else
				{
					var lacey = ConstituentElement?.LaceyAct;
					return lacey != null && !lacey.US_UnknownBreakdownTotal;
				}
			}
		}

		protected override void CheckUS_PGAQuantityOfConstituentElement()
		{
			base.CheckUS_PGAQuantityOfConstituentElement();

			if (IsCertifyCargoReleaseValidationMode)
			{
				var fda = ConstituentElement?.Parent as ACEFDA;
				if (fda != null)
				{
					if (fda.US_ProgramCode == FDAProgramCodeList.Codes.DRU)
					{
						var intendedUseCode = fda.US_IntendedUseCode;
						if (FDAIntendedUseCodesHelper.IsFinishedDosageFormDrugs(intendedUseCode) && Parent.US_PGAQuantityOfConstituentElement.IsEmpty && Parent.US_PGAPercentOfConstituentElement.IsEmpty)
						{
							Parent.US_PGAQuantityOfConstituentElementInfo.AddMessageError(QuantityOrPercentIsRequired);
						}
						else if (FDAIntendedUseCodesHelper.IsInvestigationalOrResearch(intendedUseCode) && Parent.US_PGAQuantityOfConstituentElement.IsEmpty && !Parent.US_PGAPercentOfConstituentElement.IsEmpty)
						{
							Parent.US_PGAQuantityOfConstituentElementInfo.AddWarning(QuantityAndPercentIsRequiredIfNotFinished);
						}
					}
				}
			}
		}
		internal const string QuantityAndPercentIsRequiredIfNotFinished = "If Active Pharmaceutical Ingredients (not Finished) both the quantity, unit of measure AND Percent of Constituent Element are required.";

		protected override void CheckUS_PGAPercentOfConstituentElement()
		{
			base.CheckUS_PGAPercentOfConstituentElement();

			if (IsCertifyCargoReleaseValidationMode)
			{
				var fda = ConstituentElement?.Parent as ACEFDA;
				if (fda != null)
				{
					if (fda.US_ProgramCode == FDAProgramCodeList.Codes.DRU)
					{
						var intendedUseCode = fda.US_IntendedUseCode;
						if (FDAIntendedUseCodesHelper.IsFinishedDosageFormDrugs(intendedUseCode) && Parent.US_PGAQuantityOfConstituentElement.IsEmpty && Parent.US_PGAPercentOfConstituentElement.IsEmpty)
						{
							Parent.US_PGAPercentOfConstituentElementInfo.AddMessageError(QuantityOrPercentIsRequired);
						}
						else if (FDAIntendedUseCodesHelper.IsActivePharmaceuticalIngredients(intendedUseCode) && Parent.US_PGAPercentOfConstituentElement.IsEmpty)
						{
							Parent.US_PGAPercentOfConstituentElementInfo.AddMessageError(PercentRequiredIfActiveIngredient);
						}
						else if (FDAIntendedUseCodesHelper.IsInvestigationalOrResearch(intendedUseCode) && !Parent.US_PGAQuantityOfConstituentElement.IsEmpty && Parent.US_PGAPercentOfConstituentElement.IsEmpty)
						{
							Parent.US_PGAPercentOfConstituentElementInfo.AddWarning(QuantityAndPercentIsRequiredIfNotFinished);
						}
					}
					else if (fda.US_ProgramCode == FDAProgramCodeList.Codes.VME && fda.US_ProcessingCode == FDAProcessingCodeList.Codes.VME_ADR)
					{
						if (Parent.US_PGAQuantityOfConstituentElement.IsEmpty && Parent.US_PGAPercentOfConstituentElement.IsEmpty)//Either the quantity and Unit of Measurement should be entered OR the Percent Constituent Element should be entered.
						{
							Parent.US_PGAPercentOfConstituentElementInfo.AddMessageError(QuantityOrPercentIsRequired);
						}
					}
					ValidateUS_PGAQuantityOfConstituentElement();
				}
			}
		}
		internal const string QuantityOrPercentIsRequired = "Either the Quantity and Unit of Measurement should be entered OR the Percent of Constituent Element should be entered.";
		internal const string PercentRequiredIfActiveIngredient = "Percent of the Active Ingredient is required for Drugs reporting.";

		protected override void CheckUS_SpeciesName()
		{
			base.CheckUS_SpeciesName();
			if (IsCertifyCargoReleaseValidationMode && ConstituentElement?.LaceyAct != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SpeciesNameInfo);
				if (Parent.US_SpecialUseDesignation)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_SpeciesNameInfo, Parent.Lookups.SpeciesNameCodeList);
				}
			}
		}

		protected override void CheckUS_GenusName()
		{
			base.CheckUS_GenusName();
			if (!Parent.US_SpecialUseDesignation && IsCertifyCargoReleaseValidationMode && ConstituentElement?.LaceyAct != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_GenusNameInfo);
			}
		}

		protected override void CheckUS_PGAPercentOfConstituentElementIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.US_PGAPercentOfConstituentElementInfo, 7, 4);
		}

		protected override void CheckUS_SpecialUseDesignation()
		{
			base.CheckUS_SpecialUseDesignation();
			ValidateUS_GenusName();
		}

		protected override void CheckUS_UnknownBreakdownCountryCode()
		{
			base.CheckUS_UnknownBreakdownCountryCode();

			var lacey = ConstituentElement?.LaceyAct;
			if (IsCertifyCargoReleaseValidationMode && lacey != null)
			{
				if (!Parent.US_UnknownBreakdownCountryCode.IsEmpty && Parent.US_UnknownBreakdownCountryCode != USCCountry.Unknown)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_UnknownBreakdownCountryCodeInfo, Parent.Lookups.USCountryList);
				}

				if (!lacey.US_UnknownBreakdownTotal)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UnknownBreakdownCountryCodeInfo);
				}
			}
		}
		protected override void CheckUS_OA_ProducerAddress()
		{
			base.CheckUS_OA_ProducerAddress();

			var fda = ConstituentElement?.Parent as ACEFDA;

			if (fda != null && fda.US_ProgramCode == FDAProgramCodeList.Codes.DRU &&
				(fda.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._080000 || fda.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._130000) &&
				Parent.US_OA_ProducerAddress.IsEmpty &&
				IsCertifyCargoReleaseValidationMode)
			{
				Parent.US_OA_ProducerAddressInfo.AddWarning(ProducerRequired);
			}
		}
		internal const string ProducerRequired = "Producer Address will be mandatory for Drugs with Intended Codes '080' or '130' in the future.";

		protected override bool IsCertifyCargoReleaseValidationMode
		{
			get
			{
				var result = true;
				var invoiceLine = InvoiceLine;

				if (ConstituentElement?.Parent is ACEFDA)
				{
					result = invoiceLine == null || invoiceLine.IsACECargoCertificationMode;
				}
				else
				{
					var declaration = invoiceLine?.Declaration;
					result = declaration == null || declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		internal override string NameRequired
		{
			get
			{
				var fda = ConstituentElement?.Parent as ACEFDA;
				return fda != null ? "Name Of Active Ingredient is mandatory." : base.NameRequired;
			}
		}
	}
}
