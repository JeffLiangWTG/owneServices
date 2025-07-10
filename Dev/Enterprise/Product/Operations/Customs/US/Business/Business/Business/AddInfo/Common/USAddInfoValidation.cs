//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USAddInfoValidation : AutoUSAddInfoValidation
	{
		public USAddInfoValidation(AutoUSAddInfo parent)
			: base(parent)
		{
			if (!parent.GetType().IsSubclassOf(typeof(AddInfo)))
			{
				throw new ArgumentException("Parent is not a subclass of AddInfo");
			}
		}

		protected override void CheckUS_CottonFeeExempt()
		{
			base.CheckUS_CottonFeeExempt();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CottonFeeExemptInfo, Parent.Lookups.US_YesNoList);
		}

		protected void CheckUS_ZoneStatusCommonForImport()
		{
			if ((Parent.Declaration != null) && (Parent.Declaration.IsConsumptionFTZ || Parent.Declaration.IsFTZAdmission))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ZoneStatusInfo, Parent.Lookups.US_ZoneStatusList);

				if (Parent.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign)
				{
					ValidateUS_PrivilegedStatusDate();
				}
			}
		}

		protected void CheckUS_PrivilegedStatusDateCommonForImport()
		{
			base.CheckUS_PrivilegedStatusDate();

			if (Parent.US_PrivilegedStatusDate > ZDateTime.Today)
			{
				Parent.US_PrivilegedStatusDateInfo.AddMessageError(PrivilegedStatusDateCannotBeFuture);
			}
		}
		internal const string PrivilegedStatusDateCannotBeFuture = "Privileged Status Date cannot be in the future.";

		protected override void CheckUS_InBondTOLState()
		{
			base.CheckUS_InBondTOLState();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InBondTOLStateInfo, Parent.Lookups.USStateList, (NoResString)InBondTOLStateShouldBeInList);
		}
		internal const string InBondTOLStateShouldBeInList = "Please enter a valid In-Bond TOL State Code. The code you have selected is not in the In-Bond TOL State codes List.";

		protected override void CheckUS_InBondExportTransMode()
		{
			base.CheckUS_InBondExportTransMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InBondExportTransModeInfo, Parent.Lookups.ExportingTransportTypeList, (NoResString)InBondExportTransModeShouldBeInList);
		}
		internal const string InBondExportTransModeShouldBeInList = "Please enter a valid In-Bond Export Transport Mode Code. The code you have selected is not in the In-Bond Export Transport Mode codes List.";

		protected override void CheckUS_SecondarySPI()
		{
			base.CheckUS_SecondarySPI();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SecondarySPIInfo, Parent.Lookups.ProductClaimList, (NoResString)GetErrorMessageForSecondarySPI());
		}

		string GetErrorMessageForSecondarySPI()
		{
			return string.Format(CultureInfo.InvariantCulture
				, "Please enter a valid {0} Code. The code you have selected is not in the {0} codes List."
				, GetHumanReadableNameForSecondarySPI());
		}

		protected virtual string GetHumanReadableNameForSecondarySPI()
		{
			return "Secondary SPI";
		}

		protected override void CheckUS_PrivilegedStatusDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckUS_MissingDocument1()
		{
			base.CheckUS_MissingDocument1();
			if (!Parent.US_MissingDocument1.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_MissingDocument1Info, Parent.Lookups.US_MissingDocumentList);
				if (Parent.US_MissingDocument1 == MissingDocumentList.Codes.Lots)
				{
					Parent.US_MissingDocument1Info.AddMessageError("Record the code number for the first missing document and insert code '99' in Missing Document 2 field, to indicate more than one additional document is missing.");
				}
			}
		}

		protected override void CheckUS_MissingDocument2()
		{
			base.CheckUS_MissingDocument2();
			if (!Parent.US_MissingDocument2.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_MissingDocument2Info, Parent.Lookups.US_MissingDocumentList);
			}
		}

		internal const string InBondEntryTypeShouldBeEnteredInInBondType = "The selected entry type is an IT (In-Bond) entry type. You should enter it at Declaration > IT (In-Bond) Type.";
		internal const string PaperBasedEntryType = "This is paper-based and not automated. Messages will be rejected.";

		protected virtual bool ShouldValidateUS_EntryType
		{
			get { return false; }
		}

		void CheckRestrictedCountry(ZPropertyInfo info)
		{
			var countryOfOrigin = Parent.Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, info.Value.ToString());
			if (countryOfOrigin != null)
			{
				if (countryOfOrigin.IsRestrictedCountry(Parent.US_EstimatedEntryDate))
				{
					info.AddMessageError(countryOfOrigin.UC_Name + " is a restricted country");
				}
			}
		}

		protected override void CheckUS_UC_NKCountryOfOrigin()
		{
			base.CheckUS_UC_NKCountryOfOrigin();
			if (Parent.US_UC_NKCountryOfOrigin != USCCountry.Unknown)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_UC_NKCountryOfOriginInfo, Parent.Lookups.USCountryList);
			}
			CheckRestrictedCountry(Parent.US_UC_NKCountryOfOriginInfo);
		}

		protected override void CheckUS_UC_NKCountryOfExport()
		{
			base.CheckUS_UC_NKCountryOfExport();
			CheckRestrictedCountry(Parent.US_UC_NKCountryOfExportInfo);
		}

		internal const string InvalidTheFirstTwoCharacters = "The first two digits indicating an industry code should be between 02 and 97";
		internal const string InvalidTheThirdCharacter = "The third character indicating a class should be a letter";
		internal const string InvalidFourthAndFifthCharacters = "The fourth and fifth characters indicating a subclass and PIC should be a letter or '.' or '-'";
		internal const string InvalidSixthAndSeventhCharacters = "The sixth and seventh characters indicating product should be a letter or number";

		protected override void CheckUS_PercentageActiveIngredient()
		{
			base.CheckUS_PercentageActiveIngredient();
			ClassificationValidator.CheckPercentageActiveIngredient(Parent.US_PercentageActiveIngredientInfo);
		}

		internal const string removedDDTCITARExemptionNo = "123.17A";

		protected void CheckUS_DDTCITARExemptionNoCore(ZPropertyInfo dDTCITARExemptionNoInfo, ZString licenseType)
		{
			var declaration = Parent.Declaration;
			var validateforRemovedDDTCITARExemptionNo = declaration != null &&
				(declaration.JE_EntrySubmittedDate.IsEmpty || declaration.JE_EntrySubmittedDate > new ZDateTime(2013, 09, 10));

			if (validateforRemovedDDTCITARExemptionNo || !dDTCITARExemptionNoInfo.Value.Equals(removedDDTCITARExemptionNo))
			{
				ListValidation.MessageErrorIfInvalidCode(dDTCITARExemptionNoInfo, Parent.Lookups.US_DDTCITARExemptionCodes);
			}
			ClassificationValidator.CheckLicNo(dDTCITARExemptionNoInfo, USAESLicenseCode.IsDDTCITARExemptionRequired(licenseType), false,
			ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage, ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
		}

		protected void CheckUS_DDTCRegistrationNoCore(ZPropertyInfo dDTCRegistrationNoInfo, ZString licenseType)
		{
			ClassificationValidator.CheckLicNo(dDTCRegistrationNoInfo, USAESLicenseCode.IsDDTCRegistrationNumberRequired(licenseType), USAESLicenseCode.IsDDTCRegistrationNumberAllowed(licenseType),
			ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage, ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
		}

		protected void CheckUS_DDTCMilitaryEquipmentIndicatorCore(ZPropertyInfo dDTCMilitaryEquipmentIndicatorInfo, ZString licenseType)
		{
			ListValidation.MessageErrorIfInvalidCode(dDTCMilitaryEquipmentIndicatorInfo, Parent.Lookups.US_YesNoList);
			ClassificationValidator.CheckLicNo(dDTCMilitaryEquipmentIndicatorInfo, USAESLicenseCode.IsDDTCDataRequired(licenseType), false,
			ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage, ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
		}

		protected void CheckUS_DDTCPartyCertificationIndicatorCore(ZPropertyInfo dDTCPartyCertificationIndicatorInfo, ZString licenseType)
		{
			ListValidation.MessageErrorIfInvalidCode(dDTCPartyCertificationIndicatorInfo, Parent.Lookups.US_YesNoList);
			ClassificationValidator.CheckLicNo(dDTCPartyCertificationIndicatorInfo, USAESLicenseCode.IsDDTCPartyCertIndicatorRequired(licenseType), false,
			ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage, ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
		}

		protected void CheckUS_DDTCUSMLCategoryCodeCore(ZPropertyInfo dDTCUSMLCategoryCodeInfo, ZString licenseType)
		{
			ListValidation.MessageErrorIfInvalidCode(dDTCUSMLCategoryCodeInfo, Parent.Lookups.US_USMLCategoryCodes);
			ClassificationValidator.CheckLicNo(dDTCUSMLCategoryCodeInfo, USAESLicenseCode.IsDDTCDataRequired(licenseType), false,
			ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage, ClassificationValidator.DDTCUSMLCategoryCodeShouldNotBeEntered);
		}

		protected void CheckUS_JurisdictionNumberCore(ZPropertyInfo jurisdictionNumberInfo, ZString jurisdictionNumber, ZString categoryCodeCore)
		{
			if (ZZCustomsFunctionality.IsAESJurisdictionNumberEffective)
			{
				if (categoryCodeCore == USMLCategoryCodes.Codes.MiscellaneousArticles && !Regex.IsMatch(jurisdictionNumber, @"^CJ((\d{7})|(\s\d{4}-\d{2}))$"))
				{
					jurisdictionNumberInfo.AddMessageError(JurisdictionNumberInvalid);
				}
			}
		}
		public const string JurisdictionNumberInvalid = "Category XXI Determination Number format is incorrect. Format should be CJ NNNN-NN or CJNNNNNNN, where N is a number.";

		protected void CheckUnitOfOverriddenTaxRateAgainstCustomsUQs(ZPropertyInfo overriddenQuantityInfo, ZString firstUQ, ZString secondUQ, ZString rateDesc)
		{
			if (!rateDesc.IsEmpty)
			{
				ZString uq = AppendixBTaxRateList.GetUQ(rateDesc);

				if (!uq.IsEmpty && overriddenQuantityInfo.Value.IsEmpty && AppendixBTaxRateList.DoesUQMatchNoneOfCustomsUQsCore(uq, firstUQ, secondUQ))
				{
					overriddenQuantityInfo.AddMessageError(string.Format(UnitOfQuantityDoesNotMatch, uq));
				}
			}
		}

		public const string UnitOfQuantityDoesNotMatch = "The system is unable to determine quantity in {0}. Please enter a quantity so the system can calculate the Tax.";

		protected void CheckTaxApply(ZPropertyInfo taxApplyPropertyInfo, ZString selectedTaxCode, ZString selectedTaxRate, USCTariff importTariff)
		{
			if (importTariff != null)
			{
				importTariff.CheckTaxApply(taxApplyPropertyInfo, selectedTaxCode);
			}
			else
			{
				var taxApply = (ZString)taxApplyPropertyInfo.Value;

				if (taxApply == TaxApplyList.Codes.Yes)
				{
					if (selectedTaxCode.IsEmpty)
					{
						taxApplyPropertyInfo.AddMessageError(SelectOverrideAndIndicateTaxCode);
					}
					else if (selectedTaxRate.IsEmpty)
					{
						taxApplyPropertyInfo.AddMessageError(SelectOverrideAndIndicateTaxRate);
					}
				}
			}
		}

		new AddInfo Parent => (AddInfo)base.Parent;

		public const string TaxRateShouldBeEntered = "Tax rate should be entered in terms of the Customs Quantity (IR Tax = Tax Rate * Customs Quantity)";

		internal const string SelectOverrideAndIndicateTaxCode = "The system cannot determine the appropriate Tax Code. If Tax applies, please select 'O' for Override and select the correct Tax Code.";
		internal const string SelectOverrideAndIndicateTaxRate = "The system cannot determine the appropriate Tax Rate. If Tax applies, please select 'O' for Override and select the correct Tax Rate.";
	}
}
