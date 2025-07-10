using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.AgencyRequirementsValidator;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusUSClassificationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCD_APHISIndicatorWhenTariffIsFlagAQX()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "AQX";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			var validation = details.Validation;
			validation.ValidateCD_APHISIndicator();
			AssertHasWarning(details.CD_APHISIndicatorInfo, CusUSClassificationValidation.APHISDataMayBeRequired);

			details.CD_APHISIndicator = "D";
			AssertNoWarning(details.CD_APHISIndicatorInfo, CusUSClassificationValidation.APHISDataMayBeRequired);
			AssertNoWarnings(details.CD_APHISIndicatorInfo);
		}

		public void TestCheckCD_HFCIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "EH1";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_HFCIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_HFCIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_HFCIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_HFCIndicatorInfo, ListValidation.InvalidCodeMessageError);

			details.CD_HFCIndicator = ZString.Empty;
			var errorText = string.Format(RequirementConstants.PGA.PGARequiredButBlank, GovernmentAgencyProgramCodeList.Codes.HFC);
			AssertNoMessageErrorContaining(details.CD_HFCIndicatorInfo, errorText);

			tariff.UE_PGACodes = "EH2";
			details.Validation.ValidateCD_HFCIndicator();
			AssertHasMessageErrorContaining(details.CD_HFCIndicatorInfo, errorText);

			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.HFC);
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_HFCIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_HFCIndicatorInfo, errorText);

			details.CD_HFCDisclaimReason = ZString.Empty;
			details.CD_HFCIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(details.CD_HFCDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.CD_HFCIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_HFCIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_HFCIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_HFCIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCD_FlavorContentCreditIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1AL1";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			var validation = details.Validation;
			validation.ValidateCD_FlavorContentCreditIndicator();
			AssertNoWarnings(details.CD_FlavorContentCreditIndicatorInfo);

			details.CD_FlavorContentCreditIndicator = true;
			AssertHasWarningContaining(details.CD_FlavorContentCreditIndicatorInfo, "The Flavor Content Credit Indicator usually only applies to spirits.");

			details.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			validation.ValidateCD_FlavorContentCreditIndicator();
			AssertNoWarnings(details.CD_FlavorContentCreditIndicatorInfo);

			details.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.Avocado;
			validation.ValidateCD_FlavorContentCreditIndicator();
			AssertHasWarningContaining(details.CD_FlavorContentCreditIndicatorInfo, "The Flavor Content Credit Indicator usually only applies to spirits.");

			details.CD_FlavorContentCreditIndicator = false;
			AssertNoWarnings(details.CD_FlavorContentCreditIndicatorInfo);
		}

		public void TestCheckCD_LaceyActIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1AL1";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_LaceyActIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_LaceyActIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_LaceyActIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_LaceyActIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "Lacey Act", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_LaceyActIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_LaceyActIndicatorInfo, errorText);

			details.CD_LaceyActIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_LaceyActDisclaimReason = ZString.Empty;
			AssertHasMessageError(details.CD_LaceyActDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		public void TestCheckCD_FWSIndicator()
		{
			using (ZZCustomsFunctionality.TemporarilySetupFWSEffective())
			{
				CreateTariffWithPGACondition("0000000001", Universal.Constants.TariffTypes.ScheduleB, GovernmentAgencyProgramCodeList.Codes.FWS, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "1000000001";
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
				tariff.UE_PGACodes = "FW2";

				var product = Factory.NewWithValidTestData<OrgSupplierPart>();
				var pivot = product.PivotsForBinding.AddNew();
				var details = pivot.Details;
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.FWSLines.AddNew();
				pivot.CI_TariffNum = tariff.UE_Tariff;

				details.CD_FWSIndicator = "~";
				AssertHasMessageErrorContaining(details.CD_FWSIndicatorInfo, ListValidation.InvalidCodeMessageError);
				details.CD_FWSIndicator = "D";
				AssertNoMessageErrorContaining(details.CD_FWSIndicatorInfo, ListValidation.InvalidCodeMessageError);

				var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "FWS", "Declared");
				tariff.UE_PGACodes = ZString.Empty;
				details.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
				AssertHasWarning(details.CD_FWSIndicatorInfo, errorText);

				details.CD_FWSIndicator = OGAIndicatorList.Codes.Disclaimed;
				details.CD_FWSDisclaimReason = ZString.Empty;
				AssertHasMessageError(details.CD_FWSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				details.CD_FWSIndicator = "~";
				AssertHasMessageErrorContaining(details.CD_FWSIndicatorInfo, ListValidation.InvalidCodeMessageError);
				details.CD_FWSIndicator = "D";
				AssertNoMessageErrorContaining(details.CD_FWSIndicatorInfo, ListValidation.InvalidCodeMessageError);

				pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
				pivot.CI_TariffNum = "0000000001";
				details.CD_FWSIndicator = ZString.Empty;
				AssertHasMessageErrorContaining(details.CD_FWSIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "FWS"));
			}
		}

		public void TestCheckCD_NMFS370Indicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "NM2";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = tariff.UE_Tariff;
			var nmfsLine = pivot.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			details.CD_NMFS370Indicator = "~";
			AssertHasMessageErrorContaining(details.CD_NMFS370IndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_NMFS370Indicator = "D";
			AssertNoMessageErrorContaining(details.CD_NMFS370IndicatorInfo, ListValidation.InvalidCodeMessageError);
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "370", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_NMFS370Indicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_NMFS370IndicatorInfo, errorText);

			details.CD_NMFS370Indicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_NMFS370DisclaimReason = ZString.Empty;
			AssertEquals(true, details.Lookups.PGADisclaimReasonList.Count > 0);
			AssertHasMessageError(details.CD_NMFS370DisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			AssertEquals("A, B", pivot.Lookups.NMFS370DisclaimReasonList.CodesAsString);
			details.CD_NMFS370DisclaimReason = "A";
			AssertNoMessageError(details.CD_NMFS370DisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		public void TestCheckCD_NMFSCOAIndicator()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, TariffConditionValueTypes.Codes.PGA);
			Factory.Save();
			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "1000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, zzTariff.PK, "test", true, false, ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, GovernmentAgencyProgramCodeList.Codes.COA);
			Factory.Save();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			pivot.CI_TariffNum = tariff.UE_Tariff;

			details.CD_NMFSCOAIndicator = "D";
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "COA");
			AssertHasMessageErrorContaining(details.CD_NMFSCOAIndicatorInfo, errorText);

			var nmfsLine = pivot.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.COA;

			details.CD_NMFSCOAIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_NMFSCOAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_NMFSCOAIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_NMFSCOAIndicatorInfo, ListValidation.InvalidCodeMessageError);

			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "COA", "Declared");
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NMFSCOAACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				details.CD_NMFSCOAIndicator = ZString.Empty;
				AssertHasMessageErrorContaining(details.CD_NMFSCOAIndicatorInfo, errorText);
			}

			details.CD_NMFSCOAIndicator = ZString.Empty;
			AssertNoMessageErrorContaining(details.CD_NMFSCOAIndicatorInfo, errorText);
		}

		public void TestCheckCD_NMFSAMRIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "NM3";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = tariff.UE_Tariff;

			details.CD_NMFSAMRIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_NMFSAMRIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_NMFSAMRIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_NMFSAMRIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var nmfsLine = pivot.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "AMR", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_NMFSAMRIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_NMFSAMRIndicatorInfo, errorText);

			details.CD_NMFSAMRIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_NMFSAMRDisclaimReason = ZString.Empty;
			AssertHasMessageError(details.CD_NMFSAMRDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			AssertEquals("A, B", pivot.Lookups.NMFSAMRDisclaimReasonList.CodesAsString);
			details.CD_NMFSAMRDisclaimReason = "A";
			AssertNoMessageError(details.CD_NMFSAMRDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		public void TestCheckCD_NMFSHMSIndicator()
		{
			CreateTariffWithPGACondition("0000000001", Universal.Constants.TariffTypes.ScheduleB, GovernmentAgencyProgramCodeList.NMFS, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "NM5";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = tariff.UE_Tariff;

			details.CD_NMFSHMSIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_NMFSHMSIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_NMFSHMSIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_NMFSHMSIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var nmfsLine = pivot.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "HMS", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_NMFSHMSIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_NMFSHMSIndicatorInfo, errorText);

			details.CD_NMFSHMSIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_NMFSHMSDisclaimReason = ZString.Empty;
			AssertHasMessageError(details.CD_NMFSHMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			AssertEquals("A, B", pivot.Lookups.NMFSHMSDisclaimReasonList.CodesAsString);
			details.CD_NMFSHMSDisclaimReason = "A";
			AssertNoMessageError(details.CD_NMFSHMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.CD_NMFSHMSIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_NMFSHMSIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_NMFSHMSIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_NMFSHMSIndicatorInfo, ListValidation.InvalidCodeMessageError);

			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			pivot.CI_TariffNum = "0000000001";
			details.CD_NMFSHMSIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(details.CD_NMFSHMSIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "NMFS"));
		}

		public void TestCheckCD_NMFSSIMPIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "NM8";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = tariff.UE_Tariff;

			details.CD_NMFSSIMPIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_NMFSSIMPIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_NMFSSIMPIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_NMFSSIMPIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var nmfsLine = pivot.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "SIMP", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_NMFSSIMPIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_NMFSSIMPIndicatorInfo, errorText);
		}

		public void TestCheckCD_ACEFDAIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FD1FD2FD3FD4";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_ACEFDAIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_ACEFDAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_ACEFDAIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_ACEFDAIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "PGA FDA", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_ACEFDAIndicatorInfo, errorText);

			details.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_ACEFDADisclaimReason = ZString.Empty;
			AssertHasMessageError(details.CD_ACEFDADisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		public void TestCheckCD_AMSIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "AM4";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";
			var ams = pivot.AMSLines.AddNew();
			ams.US_Program = AMSProgramList.Codes.OR1;

			details.CD_AMSIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_AMSIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_AMSIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_AMSIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "AMS", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_AMSIndicatorInfo, errorText);

			details.CD_AMSIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_AMSDisclaimReason = ZString.Empty;
			AssertHasMessageError(details.CD_AMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.CD_AMSIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_AMSIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_AMSIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_AMSIndicatorInfo, ListValidation.InvalidCodeMessageError);

			ams.US_Program = AMSProgramList.Codes.MO1;
			details.CD_AMSIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageErrorContaining(details.CD_AMSIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCD_NOPIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "AM7";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";
			var nop = pivot.AMSLines.AddNew();
			nop.US_Program = AMSProgramList.Codes.MO1;

			details.CD_NOPIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_NOPIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_NOPIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_NOPIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "NOP", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_NOPIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_NOPIndicatorInfo, errorText);

			details.CD_NOPIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_NOPDisclaimReason = ZString.Empty;
			AssertHasMessageError(details.CD_NOPDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			nop.US_Program = AMSProgramList.Codes.OR2;
			details.Validation.ValidateCD_NOPIndicator();
			AssertHasMessageError(details.CD_NOPIndicatorInfo, string.Format(RequirementConstants.PGA.PGADisclaimedButEntered, GovernmentAgencyProgramCodeList.Codes.NOP));
			nop.US_Program = AMSProgramList.Codes.MO1;

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.CD_NOPIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_NOPIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_NOPIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_NOPIndicatorInfo, ListValidation.InvalidCodeMessageError);

			nop.US_Program = AMSProgramList.Codes.OR1;
			details.CD_AMSIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageErrorContaining(details.CD_AMSIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCD_TTBIndicator()
		{
			CreateTariffWithPGACondition("0000000001", Universal.Constants.TariffTypes.Export, GovernmentAgencyProgramCodeList.Codes.TTB, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "TB2";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_TTBIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_TTBIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_TTBIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_TTBIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "TTB", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_TTBIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_TTBIndicatorInfo, errorText);

			details.CD_TTBIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_TTBDisclaimReason = ZString.Empty;
			AssertHasMessageError(details.CD_TTBDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.CD_TTBIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_TTBIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_TTBIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_TTBIndicatorInfo, ListValidation.InvalidCodeMessageError);

			pivot.CI_TariffNum = "0000000001";
			details.CD_TTBIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(details.CD_TTBIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "TTB"));
		}

		public void TestCheckCD_ITARExemptionNo()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00
				};

			var usClass = Factory.New<CusUSClassification>();
			ClassificationValidatorHelper.CheckLicensType(usClass.CD_ITARExemptionNoInfo, usClass.CD_LicenceTypeInfo,
				requireDDTCDataList, System.Array.Empty<ZString>(),
				"123.11B",
				ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage,
				ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered,
				Factory);
		}

		public void TestCheckCD_DDTCRegoNo()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			var requireDDTCDataList = new ZString[]
			{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
			};

			var allowedDDTCDataList = new ZString[]
			{
				USAESLicenseCode.Codes.S00
			};

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_TariffNum = "1000000001";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.CD_DDTCIndicator = "D";

			var usClass = Factory.New<CusUSClassification>();
			ClassificationValidatorHelper.CheckLicensType(details.CD_DDTCRegoNoInfo, details.CD_LicenceTypeInfo, requireDDTCDataList, allowedDDTCDataList, "REG123",
				ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage,
				ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered,
				Factory);
		}

		public void TestCheckCD_MilitaryEquipInd()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
				};

			var usClass = Factory.New<CusUSClassification>();
			ClassificationValidatorHelper.CheckLicensType(usClass.CD_MilitaryEquipIndInfo, usClass.CD_LicenceTypeInfo, requireDDTCDataList, System.Array.Empty<ZString>(),
				YesNoDefaultList.Codes.Yes,
				ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage,
				ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered,
				Factory);
		}

		public void TestCheckCD_PartyCertInd()
		{
			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00
				};

			var usClass = Factory.New<CusUSClassification>();
			ClassificationValidatorHelper.CheckLicensType(usClass.CD_PartyCertIndInfo, usClass.CD_LicenceTypeInfo, requireDDTCDataList, System.Array.Empty<ZString>(),
				YesNoDefaultList.Codes.Yes,
				ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage,
				ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered,
				Factory);
		}

		public void TestCheckCD_DDTCUSMLCategoryCode()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
				};
			var usClass = Factory.New<CusUSClassification>();
			ClassificationValidatorHelper.CheckLicensType(usClass.CD_DDTCUSMLCategoryCodeInfo, usClass.CD_LicenceTypeInfo, requireDDTCDataList, System.Array.Empty<ZString>(),
				USMLCategoryCodes.Codes.Ammunition,
				ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage,
				ClassificationValidator.DDTCUSMLCategoryCodeShouldNotBeEntered,
				Factory);
		}

		public void TestCheckCD_DDTCJurisdictionNumber()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESJurisdictionNumber, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				OrgSupplierPart product = Factory.New<OrgSupplierPart>();
				CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
				pivot.CD_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.AircraftAndAssociatedEquipment;
				Assert("CD_DDTCJurisdictionNumber is read-only when CD_DDTCUSMLCategoryCode is not equal to 21", pivot.CD_DDTCJurisdictionNumberInfo.ReadOnly);

				pivot.CD_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.MiscellaneousArticles;
				pivot.CD_DDTCJurisdictionNumber = "";
				AssertHasMessageError(pivot.CD_DDTCJurisdictionNumberInfo, CusUSClassificationValidation.JurisdictionNumberInvalid);

				pivot.CD_DDTCJurisdictionNumber = "1CJ1234567";
				AssertHasMessageError(pivot.CD_DDTCJurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				pivot.CD_DDTCJurisdictionNumber = "CJ12345678";
				AssertHasMessageError(pivot.CD_DDTCJurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				pivot.CD_DDTCJurisdictionNumber = "CJ 1234567";
				AssertHasMessageError(pivot.CD_DDTCJurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				pivot.CD_DDTCJurisdictionNumber = "CJ1234567";
				AssertNoMessageError(pivot.CD_DDTCJurisdictionNumberInfo, CusUSClassificationValidation.JurisdictionNumberInvalid);

				pivot.CD_DDTCJurisdictionNumber = "CJ1234-67";
				AssertHasMessageError(pivot.CD_DDTCJurisdictionNumberInfo, CusUSClassificationValidation.JurisdictionNumberInvalid);

				pivot.CD_DDTCJurisdictionNumber = "CJ 1234-67";
				AssertNoMessageError(pivot.CD_DDTCJurisdictionNumberInfo, CusUSClassificationValidation.JurisdictionNumberInvalid);
			}
		}

		public void TestCheckCD_ActiveIngredientPercentage()
		{
			var usClass = Factory.New<CusUSClassification>();
			ClassificationValidatorHelper.CheckPercentageActiveIngredient(usClass.CD_ActiveIngredientPercentageInfo);
		}

		public void TestCheckCD_ADCVDStat()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			var usClass = Factory.New<CusUSClassification>();
			usClass.CD_ParentID = pivot.PK;
			usClass.CD_ParentTableCode = "CI";
			usClass.CD_ADDCaseNo = "A1";
			usClass.CD_ADCVDStat = YesNoDefaultList.Codes.No;
			AssertHasMessageError(usClass.CD_ADCVDStatInfo, CusUSClassificationValidation.ValidCD_CVDStatement);

			usClass.CD_ADCVDStat = ADDCVDNonReimbursementList.Codes.Declared;
			Assert(!usClass.CD_ADCVDStatInfo.HasMessageError(ACEImportAddInfoJobComInvoiceLineValidation.EmptyADDStatement));

			usClass.CD_ADDCaseNo = ZString.Empty;
			usClass.CD_CVDCaseNo = ZString.Empty;
			usClass.CD_ADCVDStat = ADDCVDNonReimbursementList.Codes.Declared;
			AssertHasMessageError(usClass.CD_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithoutADDCase);

			usClass.CD_ADCVDStat = ZString.Empty;
			AssertNoMessageError(usClass.CD_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithoutADDCase);
		}

		public void TestCheckUS_RX_NK98InvCurrPerUnitCurr()
		{
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CD_RX_NK9802ValuePerUnitCurr = Core.Constants.CurrencyCodes.UnitedStates;
			AssertNoMessageErrorContaining(pivot.CD_RX_NK9802ValuePerUnitCurrInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_RX_NK9802ValuePerUnitCurr = "Z!Z";
			AssertHasMessageErrorContaining(pivot.CD_RX_NK9802ValuePerUnitCurrInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_RX_NK9802ValuePerUnitCurr = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.CD_RX_NK9802ValuePerUnitCurrInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(pivot.CD_RX_NK9802ValuePerUnitCurrInfo, CusUSClassificationValidation.CurrencyIsRequired);
			pivot.CD_9802ValuePerUnit = 1.2m;
			AssertHasMessageError(pivot.CD_RX_NK9802ValuePerUnitCurrInfo, CusUSClassificationValidation.CurrencyIsRequired);
			pivot.CD_RX_NK9802ValuePerUnitCurr = Core.Constants.CurrencyCodes.UnitedStates;
			AssertNoMessageError(pivot.CD_RX_NK9802ValuePerUnitCurrInfo, CusUSClassificationValidation.CurrencyIsRequired);
		}

		public void TestCheckUS_RX_NKPerUnitCostCurr()
		{
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CD_RX_NKPerUnitCostCurr = Core.Constants.CurrencyCodes.UnitedStates;
			AssertNoMessageErrorContaining(pivot.CD_RX_NKPerUnitCostCurrInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_RX_NKPerUnitCostCurr = "Z!Z";
			AssertHasMessageErrorContaining(pivot.CD_RX_NKPerUnitCostCurrInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_RX_NKPerUnitCostCurr = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.CD_RX_NKPerUnitCostCurrInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(pivot.CD_RX_NKPerUnitCostCurrInfo, CusUSClassificationValidation.CurrencyIsRequired);
			pivot.CD_PerUnitCost = 1.2m;
			AssertHasMessageError(pivot.CD_RX_NKPerUnitCostCurrInfo, CusUSClassificationValidation.CurrencyIsRequired);
			pivot.CD_RX_NKPerUnitCostCurr = Core.Constants.CurrencyCodes.UnitedStates;
			AssertNoMessageError(pivot.CD_RX_NKPerUnitCostCurrInfo, CusUSClassificationValidation.CurrencyIsRequired);
		}

		public void TestListValidation()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;

			var product = Factory.New<OrgSupplierPart>();
			var cusClassPartPivot = product.PivotsForBinding.AddNew();

			var pivot = cusClassPartPivot.Details;

			AssertListValidation(pivot.CD_UC_NKCountryOfOriginInfo, Core.Constants.CountryCodes.Australia);
			AssertListValidation(pivot.CD_SPIInfo, new SpecialProgramList()[0].Code);

			pivot.CD_UC_NKCountryOfOrigin = ZString.Empty;
			AssertListValidation(pivot.CD_SPIInfo, new PrimarySpecProgramIndicatorList()[0].Code);
			AssertListValidation(pivot.CD_RulingTypeInfo, pivot.Lookups.CD_RulingTypeList[0].Code);
			AssertListValidation(pivot.CD_ZoneStatusInfo, pivot.Lookups.CD_ZoneStatusList[0].Code);
			AssertListValidation(pivot.CD_TSCAIndicatorInfo, pivot.Lookups.CD_TSCAIndicatorList[0].Code);
			AssertListValidation(pivot.CD_ReconIssueInfo, pivot.Parent.USClassificationLookups.OtherReconIssueList[0].Code);

			var message = "Please enter a valid Product Claim Code. The code you have selected is not in the Product Claim codes List.";

			pivot.CD_ProductClaim = "~";
			AssertHasMessageErrorContaining(pivot.CD_ProductClaimInfo, message);

			pivot.CD_ProductClaim = pivot.Lookups.ProductClaimList[0].Code;
			AssertNoMessageErrorContaining(pivot.CD_ProductClaimInfo, message);
		}

		public void TestCheckCD_TaxApplicability()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";

			AssertEquals("PreCondition", true, tariff.IsTaxRequired);

			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "00000000";

			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			pivot.CD_TaxApplicability = ZString.Empty;
			AssertHasMessageError(pivot.CD_TaxApplicabilityInfo, USCTariff.TaxIsRequired);

			pivot.CD_TaxApplicability = TaxApplyList.Codes.No;
			AssertHasMessageError(pivot.CD_TaxApplicabilityInfo, USCTariff.TaxIsRequired);

			pivot.CD_TaxApplicability = TaxApplyList.Codes.Yes;
			AssertNoMessageError(pivot.CD_TaxApplicabilityInfo, USCTariff.TaxIsRequired);

			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			AssertHasMessageError(pivot.CD_TaxApplicabilityInfo, USCTariff.TaxRateMustBeOverridenForDiffTaxCode);

			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			AssertNoMessageError(pivot.CD_TaxApplicabilityInfo, USCTariff.TaxRateMustBeOverridenForDiffTaxCode);
		}

		public void TestCheckCD_TaxApplicability_ParentParentNotLoad()
		{
			var lookup = Factory.New<CusClassification>();
			lookup.CC_LookupCode = "LOOK434";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";

			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "00000000";

			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;

			pivot.Details.CD_ParentID = ZGuid.NewZGuid(); //make Parent.Parent not load

			var usClass = Factory.New<CusUSClassification>();
			usClass.CD_ParentID = pivot.PK;
			usClass.CD_ParentTableCode = "CI";
			AssertNoExceptionThrown("Shouldn't be NullRef in CheckCD_TaxApplicability", () => pivot.CI_CC = classification.PK);
		}

		public void TestCheckUS_TaxCode()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			AssertEquals("PreCondition", false, tariff.IsTaxRequired);

			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "00000000";

			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			AssertNoMessageErrors(pivot.CD_TaxCodeInfo);

			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			AssertHasMessageErrors(pivot.CD_TaxCodeInfo);

			pivot.CD_TaxApplicability = TaxApplyList.Codes.Yes;
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertNoMessageErrors(pivot.CD_TaxCodeInfo);

			pivot.CD_TaxApplicability = TaxApplyList.Codes.No;
			AssertNoMessageErrors(pivot.CD_TaxCodeInfo);
			AssertEquals(true, pivot.CD_TaxCodeInfo.ReadOnly);

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			AssertEquals("Tax is now required", true, tariff.IsTaxRequired);
			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			pivot.CD_TaxCode = "001";
			AssertHasMessageErrors("Tax code defaults back to Wines, change to an invalide code", pivot.CD_TaxCodeInfo);
			AssertEquals(false, pivot.CD_TaxCodeInfo.ReadOnly);
		}

		[TestDate(2010, 08, 05)]
		public void TestTaxCodeAndRateValidation()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoMessageErrors(pivot.CD_TaxCodeInfo);

			USCACCaseTariff tariff1 = Factory.New<USCACCaseTariff>();
			tariff1.U9_TariffNumber = "2403102050";
			Factory.Save();

			pivot.CI_TariffNum = "2403.10.2050";
			AssertEquals("", pivot.CD_TaxApplicability);
			AssertNoMessageErrors(pivot.CD_TaxApplicabilityInfo);
			AssertEquals(false, pivot.CD_TaxApplicabilityInfo.ReadOnly);

			AssertEquals("", pivot.CD_TaxCode);
			AssertNoMessageErrors(pivot.CD_TaxCodeInfo);
			AssertEquals(true, pivot.CD_TaxCodeInfo.ReadOnly);

			AssertEquals("", pivot.CD_TaxRateDesc);
			AssertNoMessageErrors(pivot.CD_TaxRateDescInfo);
			AssertEquals(true, pivot.CD_TaxRateDescInfo.ReadOnly);

			pivot.CD_TaxApplicability = TaxApplyList.Codes.No;
			AssertEquals("", pivot.CD_TaxCode);
			AssertNoMessageErrors(pivot.CD_TaxCodeInfo);
			AssertEquals(true, pivot.CD_TaxCodeInfo.ReadOnly);

			AssertEquals("", pivot.CD_TaxRateDesc);
			AssertNoMessageErrors(pivot.CD_TaxRateDescInfo);
			AssertEquals(true, pivot.CD_TaxRateDescInfo.ReadOnly);

			pivot.CD_TaxApplicability = TaxApplyList.Codes.Yes;
			AssertEquals("Code should default from Tariff", "018", pivot.CD_TaxCode);
			AssertNoMessageErrors(pivot.CD_TaxCodeInfo);
			AssertEquals("Code remains read only", true, pivot.CD_TaxCodeInfo.ReadOnly);

			AssertEquals("Rate should default from Tariff", "$2.41822574/KG", pivot.CD_TaxRateDesc);
			AssertNoMessageErrors(pivot.CD_TaxRateDescInfo);
			AssertEquals("Rate remains read only", true, pivot.CD_TaxRateDescInfo.ReadOnly);

			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			AssertNoMessageErrors(pivot.CD_TaxCodeInfo);
			AssertEquals("Code should now be enterable", false, pivot.CD_TaxCodeInfo.ReadOnly);

			AssertNoMessageErrors(pivot.CD_TaxRateDescInfo);
			AssertEquals("Rate should now be enterable", false, pivot.CD_TaxRateDescInfo.ReadOnly);

			pivot.CI_TariffNum = "0101.10.0010";
			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			AssertHasMessageErrors("Assert field still errors when override is used and value is blank.", pivot.CD_TaxCodeInfo);

			pivot.CD_TaxRateDesc = "";
			pivot.Details.Validation.ValidateCD_TaxRateDesc();

			AssertHasMessageErrors("Assert field still errors when override is used and value left blank.", pivot.CD_TaxRateDescInfo);
		}

		public void TestCheckCD_TaxRate()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoMessageErrors(pivot.CD_TaxCodeInfo);

			var tariff1 = Factory.New<USCACCaseTariff>();
			tariff1.U9_TariffNumber = "2403102050";
			Factory.Save();

			pivot.CI_TariffNum = "2403.10.2050";
			pivot.CD_TaxRateDesc = AppendixBTaxRateList.Codes.Specify;
			pivot.CD_TaxRate = 100m;
			AssertNoMessageError(pivot.CD_TaxRateInfo, USAddInfoValidation.TaxRateShouldBeEntered);

			pivot.CD_TaxRate = 0m;
			AssertHasMessageError(pivot.CD_TaxRateInfo, USAddInfoValidation.TaxRateShouldBeEntered);

			pivot.CD_TaxRateDesc = AppendixBTaxRateList.CBMAEligible;
			pivot.CD_TaxRate = 100m;
			AssertNoMessageError(pivot.CD_TaxRateInfo, USAddInfoValidation.TaxRateShouldBeEntered);

			pivot.CD_TaxRate = 0m;
			AssertHasMessageError(pivot.CD_TaxRateInfo, USAddInfoValidation.TaxRateShouldBeEntered);

			pivot.CD_TaxRateDesc = "8.8$/L";
			AssertEquals(8.8m, pivot.CD_TaxRate);
			AssertNoMessageError(pivot.CD_TaxRateInfo, USAddInfoValidation.TaxRateShouldBeEntered);
		}

		public void TestCheckCD_TTBRateDesignationCodeAndRateValidation()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();

			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;

			pivot.CD_TTBRateDesignationCode = "B01010";
			AssertNoMessageErrors(pivot.CD_TTBRateDesignationCodeInfo);
			AssertEquals(0.13634690m, pivot.CD_CBMADefaultTaxRate);

			pivot.CD_TTBRateDesignationCode = "ABCDEF";
			AssertHasMessageErrorContaining(pivot.CD_TTBRateDesignationCodeInfo, ListValidation.InvalidCodeMessageError);

			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			AssertNoMessageError(pivot.CD_TTBRateDesignationCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertEquals(0m, pivot.CD_CBMADefaultTaxRate);
		}

		public void TestCheckCD_CBMADefaultTaxRate()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();

			AssertNoMessageErrors(pivot.CD_CBMADefaultTaxRateInfo);

			pivot.CD_CBMADefaultTaxRate = 100m;
			AssertNoMessageError(pivot.CD_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			pivot.CD_CBMADefaultTaxRate = -1m;
			AssertHasMessageErrorContaining(pivot.CD_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckADDCVD_DepositRateIndicators()
		{
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AXXAAABBB";

			var rate = addCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDateTime.Today;
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;

			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "CXXAAABBB";

			var rate2 = cvdCase.CaseRates.AddNew();
			rate2.U6_EffectiveDate = ZDateTime.Today;
			rate2.U6_AdValoremRate = 0.4563m;
			rate2.U6_SpecificRate = 0.456m;

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "00000000";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			pivot.CD_ADDCaseNo = "AXXAAABBB";
			pivot.CD_ADDDepositRateInd = "~";
			AssertHasMessageErrorContaining(pivot.CD_ADDDepositRateIndInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_ADDDepositRateInd = pivot.USClassificationLookups.AntidumpingDutyDepositRates[0].Code;
			AssertNoMessageErrorContaining(pivot.CD_ADDDepositRateIndInfo, ListValidation.InvalidCodeMessageError);

			pivot.CD_CVDCaseNo = "CXXAAABBB";
			pivot.CD_CVDDepositRateInd = "~";
			AssertHasMessageErrorContaining(pivot.CD_CVDDepositRateIndInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_CVDDepositRateInd = pivot.USClassificationLookups.CountervailingDutyDepositRates[0].Code;
			AssertNoMessageErrorContaining(pivot.CD_CVDDepositRateIndInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2009, 12, 12)]
		public void TestCheckUS_UC_NKCountryOfExport()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_UC_NKCountryOfExport = "~";
			AssertHasMessageError(pivot.CD_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);

			pivot.CD_UC_NKCountryOfExport = "AU";
			AssertNoMessageError(pivot.CD_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_ADDCaseNo()
		{
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AXXAAABBB";
			addCase.U5_ISOCountryCode = Core.Constants.CountryCodes.Andorra;
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;

			USCACCaseTariff tariff1 = Factory.New<USCACCaseTariff>();
			tariff1.U9_TariffNumber = "0000000000";
			tariff1.U9_CaseNumber = addCase.U5_CaseNumber;
			Factory.Save();

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "0000000000";
			pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Andorra;

			pivot.CD_ADDCaseNo = "~";
			AssertHasMessageErrorContaining(pivot.CD_ADDCaseNoInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_ADDCaseNo = "AXXAAABBB";
			AssertNoMessageErrorContaining(pivot.CD_ADDCaseNoInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_CVDCaseNo()
		{
			var uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "CXXAAABBB";
			uscCase.U5_ISOCountryCode = Core.Constants.CountryCodes.Andorra;
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;

			USCACCaseTariff tariff1 = Factory.New<USCACCaseTariff>();
			tariff1.U9_TariffNumber = "0000000000";
			tariff1.U9_CaseNumber = uscCase.U5_CaseNumber;
			Factory.Save();

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "0000000000";
			pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Andorra;

			pivot.CD_CVDCaseNo = "~";
			AssertHasMessageErrorContaining(pivot.CD_CVDCaseNoInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_CVDCaseNo = "CXXAAABBB";
			AssertNoMessageErrorContaining(pivot.CD_CVDCaseNoInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCD_NHTSAIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1DT2";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_NHTSAIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_NHTSAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_NHTSAIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_NHTSAIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "NHTSA");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_NHTSAIndicatorInfo, errorText);

			details.CD_NHTSADisclaimReason = ZString.Empty;
			details.CD_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_NHTSADisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageErrors(details.CD_NHTSADisclaimReasonInfo);
		}

		public void TestCheckCD_CPSCIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "CP1CP2";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_CPSCIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_CPSCIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_CPSCIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_CPSCIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "CPSC");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_CPSCIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_CPSCIndicatorInfo, errorText);

			details.CD_CPSCDisclaimReason = ZString.Empty;
			details.CD_CPSCIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_CPSCDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageErrors(details.CD_CPSCDisclaimReasonInfo);

			tariff.UE_PGACodes = "CP1CP2";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				details.CD_CPSCIndicator = ZString.Empty;
				AssertHasMessageErrorContaining(details.CD_CPSCIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, GovernmentAgencyProgramCodeList.Codes.CPSC));
				details.CD_CPSCIndicator = "C";
				details.CD_CPSCDisclaimReason = ZString.Empty;
				AssertHasMessageErrorContaining(details.CD_CPSCDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				details.CD_CPSCIndicator = ZString.Empty;
				AssertNoMessageErrorContaining(details.CD_CPSCIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, GovernmentAgencyProgramCodeList.Codes.CPSC));
				details.CD_CPSCIndicator = "C";
				details.CD_CPSCDisclaimReason = ZString.Empty;
				AssertNoMessageErrorContaining(details.CD_CPSCDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			}
		}

		public void TestCheckCD_ODSIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP2";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_ODSIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_ODSIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_ODSIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_ODSIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.ODS);
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_ODSIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_ODSIndicatorInfo, errorText);

			details.CD_ODSDisclaimReason = ZString.Empty;
			details.CD_ODSIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(details.CD_ODSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		public void TestCheckCD_TSCAClaimIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP8";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_TSCAClaimIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_TSCAClaimIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_TSCAClaimIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_TSCAClaimIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.TSCA);
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_TSCAClaimIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_TSCAClaimIndicatorInfo, errorText);

			details.CD_TSCADisclaimReason = ZString.Empty;
			details.CD_TSCAClaimIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(details.CD_TSCADisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		public void TestCheckCD_PSTIndicator()
		{
			CreateTariffWithPGACondition("0000000001", Universal.Constants.TariffTypes.Export, GovernmentAgencyProgramCodeList.Codes.EPA, UniversalReferenceConstants.TariffConditionValue.Values.Optional);
			CreateTariffWithPGACondition("0000000002", Universal.Constants.TariffTypes.ScheduleB, GovernmentAgencyProgramCodeList.Codes.EPA, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP6";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_PSTIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_PSTIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.PST);
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_PSTIndicatorInfo, errorText);

			details.CD_PSTDisclaimReason = ZString.Empty;
			details.CD_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(details.CD_PSTDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.CD_PSTIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_PSTIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_PSTIndicator = "C";
			AssertNoMessageErrorContaining(details.CD_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);

			pivot.CI_TariffNum = "0000000001";
			details.CD_PSTIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(details.CD_PSTIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "EPA"));

			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			pivot.CI_TariffNum = "0000000002";
			details.CD_PSTIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(details.CD_PSTIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "EPA"));

			details.CD_PSTIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_PSTIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "EPA"));
		}

		public void TestCheckCD_VNEIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP4";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_VNEIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_VNEIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_VNEIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_VNEIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.VNE);
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_VNEIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_VNEIndicatorInfo, errorText);

			details.CD_VNEDisclaimReason = ZString.Empty;
			details.CD_VNEIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(details.CD_VNEDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		[TestDate(2015, 12, 23)]
		public void TestCheckCD_ATFIndicator()
		{
			CreateTariffWithPGACondition("0000000001", Universal.Constants.TariffTypes.ScheduleB, GovernmentAgencyProgramCodeList.Codes.ATF, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_ATFIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_ATFIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_ATFIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_ATFIndicatorInfo, ListValidation.InvalidCodeMessageError);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.CD_ATFIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_ATFIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_ATFIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_ATFIndicatorInfo, ListValidation.InvalidCodeMessageError);

			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			pivot.CI_TariffNum = "0000000001";
			details.CD_ATFIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(details.CD_ATFIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "ATF"));
		}

		public void TestCheckCD_DEAIndicator()
		{
			CreateTariffWithPGACondition("0000000001", Universal.Constants.TariffTypes.Export, GovernmentAgencyProgramCodeList.Codes.DEA, UniversalReferenceConstants.TariffConditionValue.Values.Optional);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "DE1";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			details.CD_DEAIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_DEAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_DEAIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_DEAIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "DEA");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_DEAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_DEAIndicatorInfo, errorText);

			details.CD_DEADisclaimReason = ZString.Empty;
			details.CD_DEAIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_DEADisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageErrors(details.CD_DEADisclaimReasonInfo);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.CD_DEAIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_DEAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_DEAIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_DEAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(details.CD_DEAIndicatorInfo, string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequiredWhenndicatorIsNotBlank, "DEA"));
			details.CD_DEAIndicator = "C";
			AssertNoMessageErrorContaining(details.CD_DEAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(details.CD_DEAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(details.CD_DEAIndicatorInfo, string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequiredWhenndicatorIsNotBlank, "DEA"));

			pivot.CI_TariffNum = "0000000001";
			details.CD_DEAIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(details.CD_DEAIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "DEA"));

			details.CD_DEAIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_DEAIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "DEA"));
		}

		[TestDate(2024, 01, 01)]
		public void TestCheckCD_TTBDEAEPAIndDisclaim()
		{
			CreateTariffWithPGACondition("1100000003", Universal.Constants.TariffTypes.Export, GovernmentAgencyProgramCodeList.Codes.TTB, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);
			CreateTariffWithPGACondition("1100000002", Universal.Constants.TariffTypes.Export, GovernmentAgencyProgramCodeList.Codes.DEA, UniversalReferenceConstants.TariffConditionValue.Values.Optional);
			CreateTariffWithPGACondition("1100000001", Universal.Constants.TariffTypes.Export, GovernmentAgencyProgramCodeList.Codes.EPA, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			var tariffDEA = Factory.New<USCTariff>();
			tariffDEA.UE_Tariff = "1100000002";
			tariffDEA.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffDEA.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffDEA.UE_PGACodes = "DE1";

			var tariffTTB = Factory.New<USCTariff>();
			tariffTTB.UE_Tariff = "1100000003";
			tariffTTB.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffTTB.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffTTB.UE_PGACodes = "TB2";

			var tariffEPA = Factory.New<USCTariff>();
			tariffEPA.UE_Tariff = "1100000001";
			tariffEPA.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffEPA.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffEPA.UE_PGACodes = "FW1EP6";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;

			details.CD_TTBIndicator = "";
			AssertEquals("No TTB Tariff yet", "", details.CD_TTBIndicator);
			pivot.CI_TariffNum = tariffTTB.UE_Tariff;
			AssertEquals("Default D as mandatory", "D", details.CD_TTBIndicator);
			AssertNoMessageErrorContaining(details.CD_TTBIndicatorInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);
			details.CD_TTBIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageErrorContaining(details.CD_TTBIndicatorInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);

			details.CD_DEAIndicator = "";
			AssertEquals("No DEA Tariff yet", "", details.CD_DEAIndicator);
			pivot.CI_TariffNum = tariffDEA.UE_Tariff;
			AssertEquals("Option O ", "", details.CD_DEAIndicator);
			AssertNoMessageErrorContaining(details.CD_DEAIndicatorInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);
			details.CD_DEAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageErrorContaining(details.CD_DEAIndicatorInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);

			details.CD_PSTIndicator = "";
			AssertEquals("No EPA Tariff yet", "", details.CD_PSTIndicator);
			pivot.CI_TariffNum = tariffEPA.UE_Tariff;
			AssertEquals("Default D as mandatory", "D", details.CD_PSTIndicator);
			AssertNoMessageErrorContaining(details.CD_PSTIndicatorInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);
			details.CD_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageErrorContaining(details.CD_PSTIndicatorInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);
		}

		public void TestCheckCD_APHISIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "AQ2";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = tariff.UE_Tariff;

			details.CD_APHISIndicator = OGAIndicatorList.Codes.Disclaimed;
			details.CD_APHISDisclaimReason = "A";
			AssertNoMessageError(details.CD_APHISDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			details.CD_APHISDisclaimReason = ZString.Empty;
			details.Validation.ValidateCD_APHISDisclaimReason();
			AssertHasMessageError(details.CD_APHISDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);

			details.CD_APHISIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_APHISIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_APHISIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_APHISIndicatorInfo, ListValidation.InvalidCodeMessageError);

			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "APHIS", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_APHISIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_APHISIndicatorInfo, errorText);
		}

		void AssertListValidation(ZPropertyInfo info, ZString validValue)
		{
			info.Value = (ZString)"~";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

			info.Value = validValue;
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
		}

		void CreateTariffWithPGACondition(ZString tariff, ZString tariffType, ZString conditionValueType, ZString conditionValue)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var newTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, tariffType);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.PGA);
			var shbTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, newTariffType.PK, tariff, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, shbTariff.PK, "PGA Data requirements", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var refCusConditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, conditionValueType);
			helper.CreateOrGetExistingRefCusConditionValue(refCusConditionValueType.PK, condition.PK, conditionValue);
			Factory.Save();
		}

		[TestDate(2024, 8, 16)]
		public void TestCheckCD_OMCDisclaimReason()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "CP1CP2";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1000000001";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				details.CD_OMCIndicator = "C";
				details.CD_OMCDisclaimReason = ZString.Empty;
				AssertHasMessageErrorContaining(details.CD_OMCDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				details.CD_OMCIndicator = "C";
				details.CD_OMCDisclaimReason = ZString.Empty;
				AssertNoMessageErrorContaining(details.CD_OMCDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			}
		}
	}
}
