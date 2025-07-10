using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.AgencyRequirementsValidator;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class HTICusUSClassificationValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2015, 12, 23)]
		public void TestCheckCD_DDTCIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_TariffNum = "1000000001";
			details.CD_DDTCIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_DDTCIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_DDTCIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_DDTCIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCD_OMCIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "OM1OM2";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";
			details.CD_OMCIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_OMCIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_OMCIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_OMCIndicatorInfo, ListValidation.InvalidCodeMessageError);
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "OMC");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_OMCIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_OMCIndicatorInfo, errorText);
			details.CD_OMCDisclaimReason = ZString.Empty;
			details.CD_OMCIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_OMCDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageErrors(details.CD_OMCDisclaimReasonInfo);

			tariff.UE_PGACodes = "OM1OM2";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				details.CD_OMCIndicator = "";
				AssertNoMessageErrorContaining(details.CD_OMCIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, GovernmentAgencyProgramCodeList.Codes.OMC));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				details.CD_OMCIndicator = "";
				AssertHasMessageErrorContaining(details.CD_OMCIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, GovernmentAgencyProgramCodeList.Codes.OMC));
			}
		}

		public void TestCheckCD_AMMVProperties()
		{
			#region SetUp Data
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var concurrencyErrorMsg = "You can enter either Assist/AMMV Unit or Assist/AMMV Percent, not both.";
			#endregion
			pivot.CD_AMMVPercentage = 4m;
			pivot.CD_AMMVPerUnit = ZDecimal.Zero;
			AssertNoMessageErrors(pivot.CD_AMMVPerUnitInfo);
			pivot.CD_AMMVPerUnit = 2m;
			AssertHasMessageError(pivot.CD_AMMVPerUnitInfo, concurrencyErrorMsg);
			pivot.CD_AMMVPercentage = ZDecimal.Zero;
			AssertNoMessageErrors(pivot.CD_AMMVPercentageInfo);
			pivot.CD_AMMVPercentage = 4m;
			AssertHasMessageError(pivot.CD_AMMVPercentageInfo, concurrencyErrorMsg);
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.Validation.ValidateCD_AMMVPercentage();
			AssertNoMessageErrors(pivot.CD_AMMVPercentageInfo);
			details.Validation.ValidateCD_AMMVPerUnit();
			AssertNoMessageErrors(pivot.CD_AMMVPerUnitInfo);
			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			details.Validation.ValidateCD_AMMVPercentage();
			AssertNoMessageErrors(pivot.CD_AMMVPercentageInfo);
			details.Validation.ValidateCD_AMMVPerUnit();
			AssertNoMessageErrors(pivot.CD_AMMVPerUnitInfo);
		}
	}
}
