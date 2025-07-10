using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEImportAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_DisclaimSanctions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType1 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Fishing);
			var conditionType2 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Mining);
			Factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0301930000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType1.PK, tariff1.PK, "Fishing Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7102310000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition2 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType2.PK, tariff2.PK, "Mining Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Sanctions, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var header = declaration.Invoices.AddNew();
				var invoiceLine1 = header.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "0301930000";
				invoiceLine1.US_UC_NKCountryOfOrigin = "RU";
				invoiceLine1.AddInfoValidation.ValidateUS_DisclaimSanctions();
				AssertHasMessageError(invoiceLine1.US_DisclaimSanctionsInfo, string.Format(ACEImportAddInfoJobComInvoiceLineValidation.SanctionsDataRequirementMessage, "Fishing"));

				invoiceLine1.US_DisclaimSanctions = true;
				AssertNoMessageError(invoiceLine1.US_DisclaimSanctionsInfo, string.Format(ACEImportAddInfoJobComInvoiceLineValidation.SanctionsDataRequirementMessage, "Fishing"));

				invoiceLine1.US_DisclaimSanctions = false;
				var fishing = invoiceLine1.FishingInformations.AddNew();
				fishing.US_MethodOfHarvest = "HFC";
				invoiceLine1.AddInfoValidation.ValidateUS_DisclaimSanctions();
				AssertNoMessageError(invoiceLine1.US_DisclaimSanctionsInfo, string.Format(ACEImportAddInfoJobComInvoiceLineValidation.SanctionsDataRequirementMessage, "Fishing"));

				invoiceLine1.JI_Tariff = "7102310000";
				invoiceLine1.AddInfoValidation.ValidateUS_DisclaimSanctions();
				AssertHasMessageError(invoiceLine1.US_DisclaimSanctionsInfo, string.Format(ACEImportAddInfoJobComInvoiceLineValidation.SanctionsDataRequirementMessage, "Mining"));
				invoiceLine1.US_DisclaimSanctions = true;
				AssertNoMessageError(invoiceLine1.US_DisclaimSanctionsInfo, string.Format(ACEImportAddInfoJobComInvoiceLineValidation.SanctionsDataRequirementMessage, "Mining"));

				invoiceLine1.US_DisclaimSanctions = false;
				var mining = invoiceLine1.MiningInformations.AddNew();
				mining.CY_Data = "CA";
				invoiceLine1.AddInfoValidation.ValidateUS_DisclaimSanctions();
				AssertNoMessageError(invoiceLine1.US_DisclaimSanctionsInfo, string.Format(ACEImportAddInfoJobComInvoiceLineValidation.SanctionsDataRequirementMessage, "Mining"));
			}
		}

		public void TestValidateCustomsValueAgainstFWSUSDValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var messageError = "The rounded total of all FWS Values";
			var header = invoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			invoiceLine.JI_LinePrice = 100.10m;
			declaration.US_EnableCRL = false;
			AssertEquals("ENS enabled", true, declaration.US_EnableENS);
			AssertEquals("CRL enabled", false, declaration.US_EnableCRL);
			declaration.US_EntryFilerCode = "XJ5";
			header.US_InvCurrPGAValue = 30;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			invoiceLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrors(invoiceLine);
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			header.US_InvCurrPGAValue = 130;
			declaration.DoMerge();
			invoiceLine.AddInfoValidation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageError);
			header.US_InvCurrPGAValue = 100m;
			declaration.DoMerge();
			invoiceLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageError);
			var secondHeader = invoiceLine.FWSHeaders.AddNew();
			secondHeader.US_InvCurrPGAValue = 100m;
			invoiceLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageError);
			declaration.DoMerge();
			invoiceLine.AddInfoValidation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageError);
			var invoiceLine2 = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.JI_LinePrice = 200m;
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			declaration.DoMerge();
			invoiceLine.AddInfoValidation.ValidateAll();
			invoiceLine2.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageError);
			AssertNoRowMessageErrorContaining(invoiceLine2, messageError);
			var fwsHeader2 = invoiceLine2.FWSHeaders.AddNew();
			fwsHeader2.US_InvCurrPGAValue = 250m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			invoiceLine.AddInfoValidation.ValidateAll();
			invoiceLine2.AddInfoValidation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageError);
			AssertNoRowMessageErrorContaining(invoiceLine2, messageError);
		}

		public void TestReRunJI_ParentIDWhenVIsSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SetInd = "X";
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			var invoiceLine8 = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals("PreCondition", "V", invoiceLine8.US_SetInd);
			Assert(!invoiceLine8.JI_ParentIDInfo.HasMessageError(JobComInvoiceLineValidation.MaxSecondaryLinesCount));
		}

		public void TestFWSIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			var tariff2_FW2 = Factory.New<USCTariff>();
			using (ZZCustomsFunctionality.TemporarilySetupFWSEffective(false))
			{
				tariff.UE_Tariff = "1000000001";
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
				tariff.UE_PGACodes = "FW1";
				tariff2_FW2.UE_Tariff = "1000000002";
				tariff2_FW2.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff2_FW2.UE_DateTo = ZDateTime.Today.AddDays(10);
				tariff2_FW2.UE_PGACodes = "FW2";
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				AssertNoMessageErrors(invoiceLine.US_FWSIndInfo);
			}

			using (ZZCustomsFunctionality.TemporarilySetupFWSEffective())
			{
				invoiceLine.US_SupTariff = tariff2_FW2.UE_Tariff;
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "FWS");
				AssertNoMessageError(invoiceLine.US_FWSIndInfo, errorText);
				invoiceLine.US_FWSInd = ZString.Empty;
				AssertHasMessageError(invoiceLine.US_FWSIndInfo, errorText);
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.JI_Tariff = tariff2_FW2.UE_Tariff;
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertHasMessageError(invoiceLine.US_FWSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.AddInfoValidation.ValidateUS_FWSInd();
				AssertNoMessageError(invoiceLine.US_FWSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_FWSDisclaimReason = PGADisclaimReasonList.Codes.D;
				invoiceLine.AddInfoValidation.ValidateUS_FWSDisclaimReason();
				AssertNoMessageError(invoiceLine.US_FWSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
				var req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.FWS);
				errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.FWS, "Declared");
				var tariff2_Empty = Factory.New<USCTariff>();
				tariff2_Empty.UE_Tariff = "1000000003";
				tariff2_Empty.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff2_Empty.UE_DateTo = ZDateTime.Today.AddDays(10);
				tariff2_Empty.UE_PGACodes = "";
				invoiceLine.JI_Tariff = tariff2_Empty.UE_Tariff;
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertHasWarning(invoiceLine.US_FWSIndInfo, errorText);
				tariff.UE_PGACodes = "FW1";
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertNoWarning(invoiceLine.US_FWSIndInfo, errorText);
				invoiceLine.US_FWSInd = ZString.Empty;
				AssertNoWarning(invoiceLine.US_FWSIndInfo, errorText);
				errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, "FWS", EntryTypeList.Codes.ReWarehouse);
				declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
				declaration.US_CertifyCargoRelease = true;
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertHasMessageErrorContaining(invoiceLine.US_FWSIndInfo, errorText);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertNoMessageError(invoiceLine.US_FWSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
				declaration.US_EntryType = ZString.Empty;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertHasMessageError(invoiceLine.US_FWSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertHasMessageError(invoiceLine.US_FWSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
				invoiceLine.US_FWSInd = ZString.Empty;
				AssertNoMessageError(invoiceLine.US_FWSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
				var hasLinesErrorMessage = ZString.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "FWS");
				var requiresLinesErrorMessage = ZString.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "FWS");
				declaration.US_CertifyCargoRelease = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertHasMessageErrorContaining(invoiceLine.US_FWSIndInfo, requiresLinesErrorMessage);
				invoiceLine.FWSHeaders.AddNew();
				invoiceLine.AddInfoValidation.ValidateUS_FWSInd();
				AssertNoMessageErrorContaining(invoiceLine.US_FWSIndInfo, requiresLinesErrorMessage);
				AssertNoMessageErrorContaining(invoiceLine.US_FWSIndInfo, hasLinesErrorMessage);
				invoiceLine.US_FWSInd = ZString.Empty;
				AssertHasMessageErrorContaining(invoiceLine.US_FWSIndInfo, hasLinesErrorMessage);
			}
		}

		[TestDate(2015, 10, 30)]
		public void TestCheckUS_TTBInd()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2402108850";
			tariff.UE_DateFrom = ZDate.Today.AddMonths(-1);
			tariff.UE_DateTo = ZDate.Today.AddMonths(1);
			tariff.UE_PGACodes = "TB2";
			var tariff2_TB1 = Factory.New<USCTariff>();
			tariff2_TB1.UE_Tariff = "2402108001";
			tariff2_TB1.UE_DateFrom = ZDate.Today.AddMonths(-1);
			tariff2_TB1.UE_DateTo = ZDate.Today.AddMonths(1);
			tariff2_TB1.UE_PGACodes = "TB1";
			var tariff2_TB3 = Factory.New<USCTariff>();
			tariff2_TB3.UE_Tariff = "2402108002";
			tariff2_TB3.UE_DateFrom = ZDate.Today.AddMonths(-1);
			tariff2_TB3.UE_DateTo = ZDate.Today.AddMonths(1);
			tariff2_TB3.UE_PGACodes = "TB3";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_TTBInd = "~";
			var xLineMessageError = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "TTB");
			AssertHasMessageError(invoiceLine.US_TTBIndInfo, xLineMessageError);
			AssertNoMessageError(invoiceLine.US_TTBIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_SetInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_TTBIndInfo, xLineMessageError);
			AssertHasMessageError(invoiceLine.US_TTBIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_TTBIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_TTBInd = ZString.Empty;
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "TTB");
			AssertHasMessageError(invoiceLine.US_TTBIndInfo, errorText);
			tariff.UE_PGACodes = "TB1";
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.US_TTBInd = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_TTBIndInfo, errorText);
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_TTBDisclaimReason = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_TTBDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(invoiceLine.US_TTBDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_TTBDisclaimReason = ZString.Empty;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_TTBDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			var req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.TTB);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "TTB");
			AssertHasMessageError(invoiceLine.US_TTBIndInfo, errorText);
			AssertHasMessageError(req.IndicatorInfo, errorText);
			invoiceLine.TTBLines.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_TTBInd();
			AssertNoMessageError(invoiceLine.US_TTBIndInfo, errorText);
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "TTB");
			AssertHasMessageError(invoiceLine.US_TTBIndInfo, errorText);
			invoiceLine.TTBLines.RemoveAndDeleteAll();
			invoiceLine.US_TTBInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_TTBIndInfo, errorText);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "TTB", "Declared");
			invoiceLine.JI_Tariff = "1234554321";
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_TTBIndInfo, errorText);
			invoiceLine.US_TTBInd = ZString.Empty;
			AssertNoWarning(invoiceLine.US_TTBIndInfo, errorText);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, "TTB", EntryTypeList.Codes.TemporaryImportationBond);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_TTBIndInfo, errorText);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			tariff.UE_PGACodes = "TB2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_TTBDisclaimReason = "~";
			var messageErrorText = ListValidation.InvalidCodeMessageError;
			AssertHasMessageErrorContaining(invoiceLine.US_TTBIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
			AssertHasMessageErrorContaining(invoiceLine.US_TTBDisclaimReasonInfo, messageErrorText);
			tariff.UE_PGACodes = "TB1";
			invoiceLine.JI_Tariff = tariff2_TB1.UE_Tariff;
			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.C;
			invoiceLine.AddInfoValidation.ValidateUS_TTBDisclaimReason();
			AssertNoMessageErrorContaining(invoiceLine.US_TTBIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
			AssertNoMessageErrorContaining(invoiceLine.US_TTBDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.AddInfoValidation.ValidateUS_TTBDisclaimReason();
			AssertHasMessageErrorContaining(invoiceLine.US_TTBDisclaimReasonInfo, messageErrorText);
			invoiceLine.JI_Tariff = tariff2_TB3.UE_Tariff;
			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.AddInfoValidation.ValidateUS_TTBDisclaimReason();
			AssertNoMessageErrorContaining(invoiceLine.US_TTBDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.C;
			invoiceLine.AddInfoValidation.ValidateUS_TTBDisclaimReason();
			AssertNoMessageErrorContaining(invoiceLine.US_TTBDisclaimReasonInfo, messageErrorText);
		}

		[TestDate(2015, 10, 31)]
		public void TestCheckUS_DDTCInd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertUS_DDTCInd(invoiceLine, AssertNoMessageError, AssertHasMessageError);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_DDTCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_DDTCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_DDTCInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_DDTCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
		}

		public void TestPGAIndicatorsOnXLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = "01";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "SET X Test";
			invoiceLine.US_SetInd = "";
			var errorAPHIS = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "APHIS");
			AssertNoMessageErrorContaining(invoiceLine.US_APHISIndInfo, errorAPHIS);
			var errorDDTC = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "DDTC");
			AssertNoMessageErrorContaining(invoiceLine.US_DDTCIndInfo, errorDDTC);
			var errorSIMP = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "NMFS SIMP");
			AssertNoMessageErrorContaining(invoiceLine.US_NMFSSIMPIndInfo, errorSIMP);
			var errorHMS = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "NMFS HMS");
			AssertNoMessageErrorContaining(invoiceLine.US_NMFSHMSIndInfo, errorHMS);
			var errorAMR = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "NMFS AMR");
			AssertNoMessageErrorContaining(invoiceLine.US_NMFSAMRIndInfo, errorAMR);
			var error370 = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "NMFS 370");
			AssertNoMessageErrorContaining(invoiceLine.US_NMFS370IndInfo, error370);
			invoiceLine.US_SetInd = "X";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_APHISIndInfo, errorAPHIS);
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_DDTCIndInfo, errorDDTC);
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSSIMPIndInfo, errorSIMP);
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSHMSIndInfo, errorHMS);
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSAMRIndInfo, errorAMR);
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_NMFS370IndInfo, error370);
		}

		[TestDate(2015, 10, 30)]
		public void TestCheckUS_DDTCArrivalDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertUS_DDTCArrivalDate(invoiceLine, AssertNoMessageError, AssertHasMessageError);
		}

		[TestDate(2015, 10, 30)]
		public void TestCheckUS_DDTCExemptionCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertUS_DDTCExemptionCode(invoiceLine, AssertNoMessageError, AssertHasMessageError);
		}

		[TestDate(2015, 10, 30)]
		public void TestCheckUS_DDTCLicenseNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertUS_DDTCLicenseNo(invoiceLine, AssertNoMessageError, AssertHasMessageError);
		}

		[TestDate(2015, 10, 30)]
		public void TestCheckUS_DDTCLicenseType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertUS_DDTCLicenseType(invoiceLine, AssertNoMessageError, AssertHasMessageError);
		}

		public void TestADCVDQuantityMandatory()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A462105011";
			acCase.U5_CaseStatus = "AC";
			acCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			acCase.CaseTariffs.AddNew();
			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_Unit = "T";
			caseRate.U6_SpecificRate = 0.52m;
			var acCase2 = Factory.New<USCACCase>();
			acCase2.U5_CaseNumber = "C462105011";
			acCase2.U5_CaseStatus = "AC";
			acCase2.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			acCase2.CaseTariffs.AddNew();
			var caseRate2 = acCase2.CaseRates.AddNew();
			caseRate2.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate2.U6_Unit = "KG";
			caseRate2.U6_SpecificRate = 0.52m;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A462105011";
			invoiceLine.US_CVDCaseNo = "C462105011";
			invoiceLine.US_ADDQty = ZDecimal.Zero;
			invoiceLine.US_CVDQty = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.US_ADDQtyInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADCVDQuantityShouldBeEntered);
			AssertHasMessageError(invoiceLine.US_CVDQtyInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADCVDQuantityShouldBeEntered);
		}

		[TestDate(2015, 10, 30)]
		public void TestCheckUS_DDTCRegistrationNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_DDTCInd = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.US_DDTCRegistrationNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_DDTCRegistrationNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.ValidationModes = ValidationModes.None;
			invoiceLine.AddInfoValidation.ValidateUS_DDTCRegistrationNo();
			AssertNoMessageErrorContaining(invoiceLine.US_DDTCRegistrationNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.RecalculateValidationModesOnDeclaration();
			invoiceLine.AddInfoValidation.ValidateUS_DDTCRegistrationNo();
			AssertHasMessageErrorContaining(invoiceLine.US_DDTCRegistrationNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_DDTCRegistrationNo = "A";
			AssertNoMessageErrorContaining(invoiceLine.US_DDTCRegistrationNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_DDTCExemptionCode = ACEDDTCExemptionCodes.Codes._12318A2;
			invoiceLine.US_DDTCRegistrationNo = "";
			AssertNoMessageErrorContaining(invoiceLine.US_DDTCRegistrationNoInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(invoiceLine.US_DDTCRegistrationNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ADCVDStat()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A1";
			AssertHasMessageError(invoiceLine.US_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.EmptyADDStatement);
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			AssertNoMessageError(invoiceLine.US_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.EmptyADDStatement);
			invoiceLine.US_ADDCaseNo = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithoutADDCase);
			invoiceLine.US_ADCVDStat = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithoutADDCase);
			declaration.US_EnableENS = true;
			declaration.US_EntryType = declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.US_ADDCaseNo = "1";
			invoiceLine.US_ADCVDStat = "D";
			AssertHasMessageError(invoiceLine.US_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithEntryTypes);
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa;
			invoiceLine.US_ADCVDStat = "D";
			AssertNoMessageError(invoiceLine.US_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithEntryTypes);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			invoiceLine.US_ADCVDStat = "A";
			AssertNoMessageError(invoiceLine.US_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithEntryTypes);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			invoiceLine.US_ADCVDStat = "D";
			AssertNoMessageError(invoiceLine.US_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithEntryTypes);
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			invoiceLine.US_ADCVDStat = "A";
			AssertNoMessageError(invoiceLine.US_ADCVDStatInfo, ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithEntryTypes);
		}

		public void TestCheckAntiDumpingOrCountervailingAgainstEntryType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "24";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "123";
			AssertHasMessageError(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDEnteredForAnInvalidEntryType);
			declaration.US_EntryType = "06";
			invoiceLine.US_ADDCaseNo = "123";
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDEnteredForAnInvalidEntryType);
		}

		public void TestCheckADD_DecID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_ADDDecIDInfo, ACEImportAddInfoJobComInvoiceLineValidation.StatementMadeWithoutDeclarationID);
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AXXAAABBB";
			addCase.U5_ISOCountryCode = "AU";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.IC;
			addCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			invoiceLine.US_ADDCaseNo = "AXXAAABBB";
			AssertHasMessageError(invoiceLine.US_ADDDecIDInfo, ACEImportAddInfoJobComInvoiceLineValidation.StatementMadeWithoutDeclarationID);
			invoiceLine.US_ADDDecID = "36";
			AssertNoMessageError(invoiceLine.US_ADDDecIDInfo, ACEImportAddInfoJobComInvoiceLineValidation.StatementMadeWithoutDeclarationID);
		}

		public void TestUS_MiscPermitNo_Steel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7219320042";
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			Assert(!invoiceLine.US_MiscPermitNoInfo.GetMessageErrors().GetMessageErrors().Contains("Steel"));
		}

		public void TestBondWaivedWhenAD_CVDIsInvolved()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A1";
			AssertHasMessageError(invoiceLine.US_ADDCaseNoInfo, ACEImportAddInfoJobComInvoiceLineValidation.BondCannotBeWaivedWhenAD_CVDInvolved);
			invoiceLine.US_CVDCaseNo = "A1";
			AssertHasMessageError(invoiceLine.US_CVDCaseNoInfo, ACEImportAddInfoJobComInvoiceLineValidation.BondCannotBeWaivedWhenAD_CVDInvolved);
			declaration.US_BondWaiverCode = ZString.Empty;
			invoiceLine.US_ADDCaseNo = "A1";
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, ACEImportAddInfoJobComInvoiceLineValidation.BondCannotBeWaivedWhenAD_CVDInvolved);
			invoiceLine.US_CVDCaseNo = "A1";
			AssertNoMessageError(invoiceLine.US_CVDCaseNoInfo, ACEImportAddInfoJobComInvoiceLineValidation.BondCannotBeWaivedWhenAD_CVDInvolved);
		}

		public void TestValidateADDCVDCases()
		{
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AXXAAABBB";
			addCase.U5_ISOCountryCode = "AU";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.IC;
			addCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var sus = addCase.LiqSuspensions.AddNew();
			sus.UN_EffectiveDate = ZDateTime.BrettsBirthday;
			sus.UN_Action = "START";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_ADDCaseNo = "AXXAAABBB";
			AssertHasMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			AssertHasMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.IC, ACCaseStatusList.Descriptions.IC));
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_ADDCaseNo = "AXXAAABBB";
			AssertNoMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.IC, ACCaseStatusList.Descriptions.IC));
			AssertNoMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			addCase.U5_CaseNumber = "CXXAAABBB";
			addCase.U5_ManufacturerMID = "MXFTR345F";
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			string errorMessage1 = string.Format(FormalImportAddInfoJobComInvoiceLineValidation.CVDADDManufacturerIDDiffer, "Countervailing");
			string errorMessage2 = string.Format(FormalImportAddInfoJobComInvoiceLineValidation.CVDADDManufacturerIDDiffer, "Antidumping");
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "HXFTR345F");
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			AssertHasWarningContaining(invoiceLine.US_CVDCaseNoInfo, errorMessage1);
			manufacturer.MainAddress.CustomsCodes.DeleteAll();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MXFTR345F");
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			AssertNoWarningContaining(invoiceLine.US_CVDCaseNoInfo, errorMessage1);
			addCase.U5_ManufacturerMID = ZString.Empty;
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			AssertNoWarningContaining(invoiceLine.US_CVDCaseNoInfo, errorMessage1);
		}

		public void TestCheckIsBondedADD_CVD()
		{
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AXXAAABBB";
			addCase.U5_ISOCountryCode = "AU";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var addCaseRate = addCase.CaseRates.AddNew();
			addCaseRate.U6_AdValoremRate = 0.05m;
			addCaseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "CXXAAABBB";
			cvdCase.U5_ISOCountryCode = "AU";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var cvdCaseRate = cvdCase.CaseRates.AddNew();
			cvdCaseRate.U6_AdValoremRate = 0.04m;
			cvdCaseRate.U6_SpecificRate = 0.01m;
			cvdCaseRate.U6_Unit = "KG";
			cvdCaseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "AXXAAABBB";
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			AssertNotNull("PreCondition", invoiceLine.AntidumpingDutyCase);
			AssertNotNull("PreCondition", invoiceLine.CountervailingDutyCase);
			invoiceLine.US_IsBondedADD = true;
			AssertHasMessageError(invoiceLine.US_IsBondedADDInfo, ACEImportAddInfoJobComInvoiceLineValidation.OneContinuousBondIsNotAllowedWhenSpecificOrMoreThan5PercentAdValoremRate);
			invoiceLine.US_IsBondedCVD = true;
			Assert(!invoiceLine.US_IsBondedCVDInfo.HasMessageError(ACEImportAddInfoJobComInvoiceLineValidation.OneContinuousBondIsNotAllowedWhenSpecificOrMoreThan5PercentAdValoremRate));
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			AssertHasMessageError(invoiceLine.US_IsBondedCVDInfo, ACEImportAddInfoJobComInvoiceLineValidation.OneContinuousBondIsNotAllowedWhenSpecificOrMoreThan5PercentAdValoremRate);
		}

		public void TestCheckUS_SetInd()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_OGACodes = "AAAFD4";
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1234567891";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff2.UE_OGACodes = "AAAFD2";
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.JI_Tariff = "1234567890";
			var firstVLine = invoice.InvoiceLines.AddNew();
			var secondVline = invoice.InvoiceLines.AddNew();
			firstVLine.JI_ParentID = invoiceLine.PK;
			firstVLine.JI_Tariff = "1234567891";
			firstVLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			secondVline.JI_ParentID = invoiceLine.PK;
			secondVline.JI_Tariff = "1234567891";
			secondVline.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("Precondition : Second V line has line number greater than first V line", true, secondVline.JI_LineNo > firstVLine.JI_LineNo);
			AssertHasMessageError("There is a error in first V line due to different tariff number from X line", firstVLine.US_SetIndInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertNoMessageError("There is no error in second V line despite of having different tariff number from X line", secondVline.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			firstVLine.JI_Tariff = "";
			secondVline.JI_ParentID = firstVLine.PK;
			secondVline.JI_Tariff = "1234567891";
			AssertHasMessageError("There is a error in parent V line due to different tariff number from X line", firstVLine.US_SetIndInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertHasMessageError("There is a error in child V line due to different tariff number from X line", secondVline.US_SetIndInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			secondVline.JI_Tariff = "1234567890";
			firstVLine.Validation.ValidateAll();
			AssertNoMessageError("There is no error in parent V line due to child V line has same number from X line", firstVLine.US_SetIndInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertNoMessageError("There is no error in child V line due to same tariff number from X line", secondVline.US_SetIndInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
		}

		public void TestApplicableTariffsWithUS_UC_NKCountryOfOrigin()
		{
			var startDate = ZDateTime.Today.AddMonths(-5);
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "44444444";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff4444 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "44444444", startDate, endDate);
			var tariff6666 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "66666666", startDate, endDate);
			var tariff8888 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "88888888", startDate, endDate);
			var tariff7777 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "77777777", startDate, endDate);
			var attribute1 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff4444);
			var attribute4 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff6666);
			var attribute6 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff8888);
			var attribute7 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff7777);
			var relationship2 = helper.CreateTariffRelationship(tariff6666.PK, tariffType.PK, tariff4444.ZZ1_TariffCode);
			var relationship3 = helper.CreateTariffRelationship(tariff8888.PK, tariffType.PK, tariff4444.ZZ1_TariffCode);
			var relationship5 = helper.CreateTariffRelationship(tariff7777.PK, tariffType.PK, tariff4444.ZZ1_TariffCode);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Japan, startDate.Date, endDate.Date);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);
			var rate6666 = helper.CreateRate(tariff6666, rateCode.PK, startDate, endDate, "0");
			var rate8888 = helper.CreateRate(tariff8888, rateCode.PK, startDate, endDate, "0");
			var rate7777 = helper.CreateRate(tariff7777, rateCode.PK, startDate, endDate, "0");
			helper.CreateCusApplicability(rate7777, tradeGroup, startDate, endDate);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			invoiceLine.JI_Tariff = tariff4444.ZZ1_TariffCode;
			AssertEquals(tariff4444.ZZ1_TariffCode, invoiceLine.JI_Tariff);
			AssertEquals("DropDownList has 1 item", 1, invoiceLine.Lookups.SupTariffsList.Count);
			AssertEquals(tariff7777.ZZ1_TariffCode, invoiceLine.US_SupTariff);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			AssertEquals("DropDownList has 0 item", 0, invoiceLine.Lookups.SupTariffsList.Count);
		}

		public void TestLaceyActIndicators()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1AL1";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_LaceyIndicator = "~";
			AssertHasMessageError(invoiceLine.US_LaceyIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_LaceyIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyIndicator = ZString.Empty;
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "Lacey Act");
			AssertHasMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.B;
			AssertNoMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.C;
			AssertNoMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.D;
			AssertNoMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			tariff.UE_PGACodes = "FW1AL2";
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			invoiceLine.US_LaceyIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_LaceyIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_LaceyDisclaimReason = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.C;
			AssertNoMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.D;
			AssertNoMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertHasMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.B;
			AssertNoMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyDisclaimReason = ZString.Empty;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_LaceyDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			var req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.Lacey);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "Lacey Act");
			AssertHasMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			AssertHasMessageError(req.IndicatorInfo, errorText);
			invoiceLine.LaceyActLines.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_LaceyIndicator();
			AssertNoMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			tariff.UE_OGACodes = "FW1";
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "Lacey Act");
			AssertHasMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			invoiceLine.LaceyActLines.RemoveAndDeleteAll();
			invoiceLine.US_LaceyIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "Lacey Act", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_LaceyIndicatorInfo, errorText);
			invoiceLine.US_LaceyIndicator = ZString.Empty;
			AssertNoWarning(invoiceLine.US_LaceyIndicatorInfo, errorText);
			tariff.UE_PGACodes = "AL1";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			invoiceLine.US_LaceyIndicator = ZString.Empty;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "Lacey Act");
			AssertHasMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = false;
			AssertNoMessageError(invoiceLine.US_LaceyIndicatorInfo, errorText);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, "Lacey", EntryTypeList.Codes.InformalFreeDutiable);
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_LaceyIndicatorInfo, errorText);
		}

		[TestDate(2015, 11, 2)]
		public void TestCheckUS_FTZCurrentTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.AddInfoValidation.ValidateUS_FTZCurrentTariff();
			AssertNoNotifications(invoiceLine.US_FTZCurrentTariffInfo);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2402108850";
			tariff.UE_DateFrom = ZDate.Today.AddMonths(-2);
			tariff.UE_DateTo = ZDate.Today.AddMonths(-1);
			invoiceLine.JI_Tariff = "2402108850";
			invoiceLine.AddInfoValidation.ValidateUS_FTZCurrentTariff();
			AssertHasWarningContaining(invoiceLine.US_FTZCurrentTariffInfo, string.Format(ACEImportAddInfoJobComInvoiceLineValidation.FTZCurrentTariffShouldBeEntered, invoiceLine.JI_FormattedTariff));
			tariff.UE_DateTo = ZDate.Today.AddMonths(1);
			invoiceLine.AddInfoValidation.ValidateUS_FTZCurrentTariff();
			AssertNoWarningContaining(invoiceLine.US_FTZCurrentTariffInfo, string.Format(ACEImportAddInfoJobComInvoiceLineValidation.FTZCurrentTariffShouldBeEntered, invoiceLine.JI_FormattedTariff));
			invoiceLine.US_FTZCurrentTariff = "2402108850";
			AssertHasMessageErrorContaining(invoiceLine.US_FTZCurrentTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.FTZCurrentTariffShouldNotEqualToJI_Tariff);
			var newTariff = Factory.New<USCTariff>();
			newTariff.UE_Tariff = "2402108851";
			newTariff.UE_DateFrom = ZDate.Today.AddMonths(-2);
			newTariff.UE_DateTo = ZDate.Today.AddMonths(-1);
			invoiceLine.US_FTZCurrentTariff = "2402108851";
			AssertNoMessageErrorContaining(invoiceLine.US_FTZCurrentTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.FTZCurrentTariffShouldNotEqualToJI_Tariff);
			AssertHasMessageErrorContaining(invoiceLine.US_FTZCurrentTariffInfo, TariffValidator.GetTariffFoundButNotValid("Tariff", ZDate.Today.ToShortDateString()));
			newTariff.UE_DateTo = ZDate.Today.AddMonths(1);
			invoiceLine.AddInfoValidation.ValidateUS_FTZCurrentTariff();
			AssertNoMessageErrors(invoiceLine.US_FTZCurrentTariffInfo);
			AssertHasWarning(invoiceLine.US_FTZCurrentTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.FTZTariffUsedWhenTariffExpired);
			tariff.UE_DateTo = ZDate.Today.AddMonths(-1);
			invoiceLine.AddInfoValidation.ValidateUS_FTZCurrentTariff();
			AssertNoWarning(invoiceLine.US_FTZCurrentTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.FTZTariffUsedWhenTariffExpired);
		}

		public void TestCheckUS_FDAContactInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_FDAContactPhoneNo = "";
			AssertHasMessageErrorContaining(invoiceLine.US_FDAContactPhoneNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_FDAContactPhoneNo = "10122232323";
			AssertNoMessageErrorContaining(invoiceLine.US_FDAContactPhoneNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_FDAContactPhoneNo = "0122232323";
			AssertNoMessageErrorContaining(invoiceLine.US_FDAContactPhoneNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FDAContactPhoneNo = "";
			AssertNoMessageErrorContaining(invoiceLine.US_FDAContactPhoneNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_FDAContactEmail = "123456";
			AssertHasMessageErrorContaining(invoiceLine.US_FDAContactEmailInfo, "Invalid email format");
			invoiceLine.US_FDAContactEmail = "Kevin@WISETECHGLOBAL.COM";
			AssertNoMessageErrorContaining(invoiceLine.US_FDAContactEmailInfo, "Invalid email format");
			AssertNoMessageErrorContaining(invoiceLine.US_FDAContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_FDAContactEmail = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_FDAContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_FDAContactName = "XIAO TIAN";
			AssertNoMessageErrorContaining(invoiceLine.US_FDAContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_FDAContactName = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_FDAContactNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_NMFSSIMInd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_NMFSSIMPInd = "!";
			AssertHasMessageError(invoiceLine.US_NMFSSIMPIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_NMFSSIMPIndInfo, ListValidation.InvalidCodeMessageError);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101099";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now.AddYears(1);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var warning = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "SIMP", "Declared");
			var pGARequiredButBlank = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "SIMP");
			var pGALineRequired = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "SIMP");
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_NMFSSIMPIndInfo, warning);
			Assert(!invoiceLine.US_NMFSSIMPIndInfo.HasMessageError(pGARequiredButBlank));
			AssertHasMessageError(invoiceLine.US_NMFSSIMPIndInfo, pGALineRequired);
			tariff.UE_PGACodes = "NM8";
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_NMFSSIMPIndInfo, warning);
			Assert(!invoiceLine.US_NMFSSIMPIndInfo.HasMessageError(pGARequiredButBlank));
			AssertHasMessageError(invoiceLine.US_NMFSSIMPIndInfo, pGALineRequired);
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_NMFSSIMPIndInfo, warning);
			Assert(!invoiceLine.US_NMFSSIMPIndInfo.HasMessageError(pGARequiredButBlank));
			AssertNoMessageError(invoiceLine.US_NMFSSIMPIndInfo, pGALineRequired);
			invoiceLine.US_NMFSSIMPInd = ZString.Empty;
			AssertNoWarning(invoiceLine.US_NMFSSIMPIndInfo, warning);
			AssertNoMessageError(invoiceLine.US_NMFSSIMPIndInfo, pGALineRequired);
		}

		public void TestCheckUS_NMFSSIMPIndByEffectiveDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101099";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now.AddYears(1);
			tariff.UE_PGACodes = "NM8";
			var pgaGARequiredButBlank = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "SIMP");
			var pgaLineRequired = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "SIMP");
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine2.US_NMFSSIMPInd);
			invoiceLine2.US_NMFSSIMPInd = ZString.Empty;
			AssertHasMessageError(invoiceLine2.US_NMFSSIMPIndInfo, pgaGARequiredButBlank);
			invoiceLine2.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_NMFSSIMPIndInfo, pgaGARequiredButBlank);
			AssertHasMessageError(invoiceLine2.US_NMFSSIMPIndInfo, pgaLineRequired);
		}

		public void TestCheckUS_ADDCaseNoWithCustomsRule()
		{
			var date = new ZDateTime(2023, 11, 30);

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_StartDate = date.Date.AddMonths(-1);
			customsRule.CPH_EndDate = date.Date.AddMonths(1);
			customsRule.CPH_PermitDescription = "TEST";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ensEntry = declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.US_DutyCalcDate = date;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var expected = string.Format(ACEImportAddInfoJobComInvoiceLineValidation.CaseNumberFlaggedError, customsRule.HumanReadableName);
			invoiceLine.US_ADDCaseNo = "ADD";
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, expected);

			var adcRule = customsRule.Rules.AddNew();
			adcRule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			adcRule.CPR_ValueFrom = CustomsRuleRuleADCEligibleCodeList.Codes.NotApplicableSelected;
			Factory.Save();

			var addInfoValidation = invoiceLine.AddInfoValidation;
			addInfoValidation.ValidateUS_ADDCaseNo();
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, expected);

			adcRule.CPR_ValueFrom = CustomsRuleRuleADCEligibleCodeList.Codes.CaseNumberEntered;
			Factory.Save();
			addInfoValidation.ValidateUS_ADDCaseNo();
			AssertHasMessageError(invoiceLine.US_ADDCaseNoInfo, expected);

			invoiceLine.US_ADDCaseNo = "";
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, expected);
		}

		public void TestCheckUS_CVDCaseNoWithCustomsRule()
		{
			var date = new ZDateTime(2023, 11, 30);

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_StartDate = date.Date.AddMonths(-1);
			customsRule.CPH_EndDate = date.Date.AddMonths(1);
			customsRule.CPH_PermitDescription = "TEST";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ensEntry = declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.US_DutyCalcDate = date;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var expected = string.Format(ACEImportAddInfoJobComInvoiceLineValidation.CaseNumberFlaggedError, customsRule.HumanReadableName);
			invoiceLine.US_CVDCaseNo = "CVD";
			AssertNoMessageError(invoiceLine.US_CVDCaseNoInfo, expected);

			var adcRule = customsRule.Rules.AddNew();
			adcRule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			adcRule.CPR_ValueFrom = CustomsRuleRuleADCEligibleCodeList.Codes.NotApplicableSelected;
			Factory.Save();

			var addInfoValidation = invoiceLine.AddInfoValidation;
			addInfoValidation.ValidateUS_CVDCaseNo();
			AssertNoMessageError(invoiceLine.US_CVDCaseNoInfo, expected);

			adcRule.CPR_ValueFrom = CustomsRuleRuleADCEligibleCodeList.Codes.CaseNumberEntered;
			Factory.Save();
			addInfoValidation.ValidateUS_CVDCaseNo();
			AssertHasMessageError(invoiceLine.US_CVDCaseNoInfo, expected);

			invoiceLine.US_CVDCaseNo = "";
			AssertNoMessageError(invoiceLine.US_CVDCaseNoInfo, expected);
		}

		public void TestCheckUS_ADD_NA()
		{
			var date = new ZDateTime(2023, 11, 30);

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_StartDate = date.Date.AddMonths(-1);
			customsRule.CPH_EndDate = date.Date.AddMonths(1);
			customsRule.CPH_PermitDescription = "TEST";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ensEntry = declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.US_DutyCalcDate = date;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var expected = string.Format(ACEImportAddInfoJobComInvoiceLineValidation.CaseNAFlaggedError, customsRule.HumanReadableName);
			invoiceLine.US_ADD_NA = true;
			AssertNoMessageError(invoiceLine.US_ADD_NAInfo, expected);

			var adcRule = customsRule.Rules.AddNew();
			adcRule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			adcRule.CPR_ValueFrom = CustomsRuleRuleADCEligibleCodeList.Codes.CaseNumberEntered;
			Factory.Save();

			var addInfoValidation = invoiceLine.AddInfoValidation;
			addInfoValidation.ValidateUS_ADD_NA();
			AssertNoMessageError(invoiceLine.US_ADD_NAInfo, expected);

			adcRule.CPR_ValueFrom = CustomsRuleRuleADCEligibleCodeList.Codes.NotApplicableSelected;
			Factory.Save();
			addInfoValidation.ValidateUS_ADD_NA();
			AssertHasMessageError(invoiceLine.US_ADD_NAInfo, expected);

			invoiceLine.US_ADD_NA = false;
			AssertNoMessageError(invoiceLine.US_ADD_NAInfo, expected);
		}

		public void TestCheckUS_CVD_NA()
		{
			var date = new ZDateTime(2023, 11, 30);

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_StartDate = date.Date.AddMonths(-1);
			customsRule.CPH_EndDate = date.Date.AddMonths(1);
			customsRule.CPH_PermitDescription = "TEST";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ensEntry = declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.US_DutyCalcDate = date;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var expected = string.Format(ACEImportAddInfoJobComInvoiceLineValidation.CaseNAFlaggedError, customsRule.HumanReadableName);
			invoiceLine.US_CVD_NA = true;
			AssertNoMessageError(invoiceLine.US_CVD_NAInfo, expected);

			var adcRule = customsRule.Rules.AddNew();
			adcRule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			adcRule.CPR_ValueFrom = CustomsRuleRuleADCEligibleCodeList.Codes.CaseNumberEntered;
			Factory.Save();

			var addInfoValidation = invoiceLine.AddInfoValidation;
			addInfoValidation.ValidateUS_CVD_NA();
			AssertNoMessageError(invoiceLine.US_CVD_NAInfo, expected);

			adcRule.CPR_ValueFrom = CustomsRuleRuleADCEligibleCodeList.Codes.NotApplicableSelected;
			Factory.Save();
			addInfoValidation.ValidateUS_CVD_NA();
			AssertHasMessageError(invoiceLine.US_CVD_NAInfo, expected);

			invoiceLine.US_CVD_NA = false;
			AssertNoMessageError(invoiceLine.US_CVD_NAInfo, expected);
		}

		public void TestCheckUS_SupTariffWithTRFRule()
		{
			var date = new ZDateTime(2023, 11, 30);

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_StartDate = date.Date.AddMonths(-1);
			customsRule.CPH_EndDate = date.Date.AddMonths(1);
			customsRule.CPH_PermitDescription = "TEST";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ensEntry = declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.US_DutyCalcDate = date;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var expected = string.Format(CustomsRuleHelper.TRFRuleMessageError, customsRule.HumanReadableName);
			invoiceLine.US_SupTariff = "3333333333";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, expected);

			var trfRule = customsRule.Rules.AddNew();
			trfRule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			trfRule.CPR_ValueFrom = "33";
			Factory.Save();
			invoiceLine.US_SupTariff = "4444444444";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, expected);
			invoiceLine.US_SupTariff = "3333333333";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, expected);

			trfRule.CPR_ValueFrom = "333";
			Factory.Save();
			invoiceLine.US_SupTariff = "4444444444";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, expected);
			invoiceLine.US_SupTariff = "3333333333";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, expected);

			trfRule.CPR_ValueFrom = "3333";
			Factory.Save();
			invoiceLine.US_SupTariff = "4444444444";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, expected);
			invoiceLine.US_SupTariff = "3333333333";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, expected);

			trfRule.CPR_ValueFrom = "3333333332";
			trfRule.CPR_ValueTo = "3333333334";
			Factory.Save();
			invoiceLine.US_SupTariff = "3333333331";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, expected);
			invoiceLine.US_SupTariff = "3333333332";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, expected);
			invoiceLine.US_SupTariff = "3333333333";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, expected);
			invoiceLine.US_SupTariff = "3333333334";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, expected);
			invoiceLine.US_SupTariff = "3333333335";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, expected);
		}

		public void TestCheckUS_SupTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN"); // TariffType
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038801", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffAttribute = helper.CreateTariffAttribute("TYPE", "232", tariff);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffAttribute2 = helper.CreateTariffAttribute("TYPE", "201", tariff2);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038803", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffAttribute3 = helper.CreateTariffAttribute("TYPE", "301", tariff3);
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038804", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._01, "Steel Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "44091020";
			tariff1.UE_PermitLicenseIndicator = "01";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(10);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "44091020";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "The Steel Import License number is required.");
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "The Steel Import License number is required.");
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "99038801";
			var childLine = invoiceLine2.AddSecondaryInvoiceLine();
			childLine.US_SupTariff = "99038801";
			childLine.JI_Tariff = "7304416045";
			childLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasWarningContaining(childLine.US_SupTariffInfo, "Prov/Prog. Tariff should not repeat on the same line.");
			invoiceLine2.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasWarningContaining(invoiceLine2.US_SupTariffInfo, "Prov/Prog. Tariff should not repeat on the same line.");
			invoiceLine2.JI_Tariff = "44091020";
			invoiceLine2.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoWarningContaining(invoiceLine2.US_SupTariffInfo, "Prov/Prog. Tariff should not repeat on the same line.");
			childLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoWarningContaining(childLine.US_SupTariffInfo, "Prov/Prog. Tariff should not repeat on the same line.");
			var expectedMessage = "For multiple trade remedy tariff numbers, Section 301 tariff numbers should be listed first, then Section 232 or Section 201 tariff numbers.";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "99038801";
			var childLine31 = invoiceLine3.AddSecondaryInvoiceLine();
			childLine31.US_SupTariff = "99038803";
			childLine31.JI_Tariff = "7304416045";
			invoiceLine3.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasWarningContaining(invoiceLine3.US_SupTariffInfo, expectedMessage);
			childLine31.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasWarningContaining(childLine31.US_SupTariffInfo, expectedMessage);
			invoiceLine3.US_SupTariff = "99038802";
			childLine31.US_SupTariff = "99038803";
			invoiceLine3.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasWarningContaining(invoiceLine3.US_SupTariffInfo, expectedMessage);
			invoiceLine3.US_SupTariff = "99038804";
			childLine31.US_SupTariff = "99038803";
			invoiceLine3.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoWarningContaining(invoiceLine3.US_SupTariffInfo, expectedMessage);
			childLine31.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoWarningContaining(childLine31.US_SupTariffInfo, expectedMessage);

			expectedMessage = "Per CSMS # 61152419 , when tariff number 9903.45.29 is used,  the appropriate importer certification documentation must be sent to ABI using DIS.";
			invoiceLine3.US_SupTariff = CusEntryLine.Tariff99034529;
			AssertHasWarning(invoiceLine3.US_SupTariffInfo, expectedMessage);

			invoiceLine3.US_SupTariff = "99038804";
			AssertNoWarning(invoiceLine3.US_SupTariffInfo, expectedMessage);

			expectedMessage = "Per CSMS # 62284627 , when tariff number 9903.91.09 is used, the appropriate importer certification documentation must be sent to ABI using DIS.";
			invoiceLine3.US_SupTariff = CusEntryLine.Tariff99039109;
			AssertHasWarning(invoiceLine3.US_SupTariffInfo, expectedMessage);

			invoiceLine3.US_SupTariff = "99038804";
			AssertNoWarning(invoiceLine3.US_SupTariffInfo, expectedMessage);

			var invoice3 = declaration.Invoices.AddNew();
			var xInvoiceLine = invoice3.InvoiceLines.AddNew();
			xInvoiceLine.US_SetInd = "X";
			xInvoiceLine.JI_Tariff = "3920992000";
			xInvoiceLine.US_SupTariff = "99038802";
			xInvoiceLine.SupFormattedAdditionalTariff1 = "99030120";

			var vParentInvoiceLine = invoice3.InvoiceLines.AddNew();
			vParentInvoiceLine.JI_ParentID = xInvoiceLine.PK;
			vParentInvoiceLine.US_SetInd = "V";
			vParentInvoiceLine.JI_Tariff = "3920992000";
			vParentInvoiceLine.US_SupTariff = "99038802";
			vParentInvoiceLine.SupFormattedAdditionalTariff1 = "99030120";

			xInvoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoWarningContaining(xInvoiceLine.US_SupTariffInfo, "Prov/Prog. Tariff should not repeat on the same line.");

			vParentInvoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoWarningContaining(vParentInvoiceLine.US_SupTariffInfo, "Prov/Prog. Tariff should not repeat on the same line.");
		}

		public void TestCheckUS_SupTariffForSteelProductWhenEntryTypeQuotaOrNonQuota()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var progTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038804", new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06));
			var progTariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038809", new ZDateTime(2019, 05, 10), new ZDateTime(2019, 05, 31));
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate1 = helper.CreateRate(progTariff1, rateCode.PK, new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06), "0");
			var rate2 = helper.CreateRate(progTariff2, rateCode.PK, new ZDateTime(2019, 05, 10), new ZDateTime(2019, 05, 31), "0");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "CN", new ZDateTime(2000, 01, 01), new ZDateTime(2079, 06, 06));
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.China, new ZDate(2000, 01, 01), new ZDate(2079, 06, 06));
			var applicability1 = helper.CreateCusApplicability(rate1, tradeGroup, new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06));
			var applicability2 = helper.CreateCusApplicability(rate2, tradeGroup, new ZDateTime(2019, 05, 10), new ZDateTime(2019, 05, 31));
			var relationship1 = helper.CreateTariffRelationship(progTariff1.PK, tariffType.PK, "8517620010");
			var relationship2 = helper.CreateTariffRelationship(progTariff2.PK, tariffType.PK, "8517620010");
			helper.CreateTariffAttribute("RULE", "A99", progTariff1);
			helper.CreateTariffAttribute("RULE", "A99", progTariff2);

			var tariff7208103000 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7208103000", startDate, endDate);
			var tariff7219900060 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7219900060", startDate, endDate);
			var tariff99038181 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038181", startDate, endDate);
			var tariff99038180 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038180", startDate, endDate);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff7219900060);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038181);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038180);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038181);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038180);

			var dataGroupingUnitedStates = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroupEuropeanUnionEUN = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "EUN", startDate, endDate);
			helper.AddCountry(tradeGroupEuropeanUnionEUN, Core.Constants.CountryCodes.Germany, startDate.Date, endDate.Date);
			var tradeGroupJapan = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "JP", startDate, endDate);
			helper.AddCountry(tradeGroupJapan, Core.Constants.CountryCodes.Japan, startDate.Date, endDate.Date);
			var tradeGroupUnitedKingdom = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "GB", startDate, endDate);
			helper.AddCountry(tradeGroupUnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom, startDate.Date, endDate.Date);
			var tradeGroupArgentina = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "AR", startDate, endDate);
			helper.AddCountry(tradeGroupArgentina, Core.Constants.CountryCodes.Argentina, startDate.Date, endDate.Date);

			var rateType = helper.CreateNewOrGetExistingRateType(dataGroupingUnitedStates.ZZZ_DataGrouping, Universal.Constants.RateTypes.Duty);
			var rate99038181 = helper.CreateRefCusRate(tariff99038181.PK, rateCode.PK, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupEuropeanUnionEUN, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupJapan, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupUnitedKingdom, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupArgentina, startDate, endDate);
			helper.CreateTariffRelationship(tariff99038181.PK, tariffType.PK, tariff7208103000.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff99038181.PK, tariffType.PK, tariff7219900060.ZZ1_TariffCode);

			var rate99038180 = helper.CreateRefCusRate(tariff99038180.PK, rateCode.PK, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupEuropeanUnionEUN, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupJapan, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupUnitedKingdom, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupArgentina, startDate, endDate);
			helper.CreateTariffRelationship(tariff99038180.PK, tariffType.PK, tariff7208103000.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff99038180.PK, tariffType.PK, tariff7219900060.ZZ1_TariffCode);

			var tariff8517620010 = Factory.New<USCTariff>();
			tariff8517620010.UE_Tariff = "8517620010";
			tariff8517620010.UE_DateFrom = new ZDateTime(2019, 01, 01);
			tariff8517620010.UE_DateTo = new ZDateTime(2079, 06, 06);
			var tariff99038804 = Factory.New<USCTariff>();
			tariff99038804.UE_Tariff = "99038804";
			tariff99038804.UE_DateFrom = new ZDateTime(2018, 09, 24);
			tariff99038804.UE_DateTo = new ZDateTime(2079, 06, 06);
			var tariff99038809 = Factory.New<USCTariff>();
			tariff99038809.UE_Tariff = "99038809";
			tariff99038809.UE_DateFrom = new ZDateTime(2019, 05, 10);
			tariff99038809.UE_DateTo = new ZDateTime(2019, 05, 31);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2019, 05, 13);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DateOfExport = new ZDateTime(2019, 05, 10);
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			invoiceLine.JI_Tariff = "8517620010";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			invoiceLine.US_ProductExclusion = "02";
			invoiceLine.US_SupTariff = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.ProvTariffIsRequired);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_ProductExclusion = "02";
			invoiceLine.US_ExclusionNumber = "SPR223355";
			invoiceLine.JI_Tariff = "7208103000";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsRequired);
		}

		public void TestCheckUS_SupTariffForSteelProduct()
		{
			var gbCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			gbCountry.RN_EconomicGrouping = ZString.Empty;

			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff7208103000 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7208103000", startDate, endDate);
			var tariff7219900060 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7219900060", startDate, endDate);
			var tariff99038181 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038181", startDate, endDate);
			var tariff99038180 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038180", startDate, endDate);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff7219900060);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038181);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038180);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038181);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038180);

			var dataGroupingUnitedStates = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroupEuropeanUnionEUN = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "EUN", startDate, endDate);
			helper.AddCountry(tradeGroupEuropeanUnionEUN, Core.Constants.CountryCodes.Germany, startDate.Date, endDate.Date);
			var tradeGroupJapan = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "JP", startDate, endDate);
			helper.AddCountry(tradeGroupJapan, Core.Constants.CountryCodes.Japan, startDate.Date, endDate.Date);
			var tradeGroupUnitedKingdom = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "GB", startDate, endDate);
			helper.AddCountry(tradeGroupUnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom, startDate.Date, endDate.Date);
			var tradeGroupArgentina = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "AR", startDate, endDate);
			helper.AddCountry(tradeGroupArgentina, Core.Constants.CountryCodes.Argentina, startDate.Date, endDate.Date);

			var rateType = helper.CreateNewOrGetExistingRateType(dataGroupingUnitedStates.ZZZ_DataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodes.Codes.Duty, rateType.PK);

			var rate99038181 = helper.CreateRefCusRate(tariff99038181.PK, rateCode.PK, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupEuropeanUnionEUN, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupJapan, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupUnitedKingdom, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupArgentina, startDate, endDate);
			helper.CreateTariffRelationship(tariff99038181.PK, tariffType.PK, tariff7208103000.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff99038181.PK, tariffType.PK, tariff7219900060.ZZ1_TariffCode);

			var rate99038180 = helper.CreateRefCusRate(tariff99038180.PK, rateCode.PK, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupEuropeanUnionEUN, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupJapan, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupUnitedKingdom, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupArgentina, startDate, endDate);
			helper.CreateTariffRelationship(tariff99038180.PK, tariffType.PK, tariff7208103000.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff99038180.PK, tariffType.PK, tariff7219900060.ZZ1_TariffCode);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CombineAssertions("Regular tariff 7208103000 has NO GAE attribute", () =>
			{
				invoiceLine.JI_Tariff = "7208103000";
				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_SupTariff = "99038180";
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_SupTariff = "99038180";
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Argentina;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);
			});

			CombineAssertions("Regular tariff 7219900060 has GAE attribute", () =>
			{
				invoiceLine.JI_Tariff = "7219900060";
				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_SupTariff = "99038180";
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_SupTariff = "99038180";
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.");

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Argentina;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);
			}
			);
		}

		public void TestCheckUS_ProductExclusionForSteelProduct()
		{
			var gbCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			gbCountry.RN_EconomicGrouping = ZString.Empty;

			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff7208103000 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7208103000", startDate, endDate);
			var tariff7219900060 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7219900060", startDate, endDate);
			var tariff99038181 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038181", startDate, endDate);
			var tariff99038180 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038180", startDate, endDate);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff7219900060);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038181);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038180);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038181);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038180);

			var dataGroupingUnitedStates = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroupEuropeanUnionEUN = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "EUN", startDate, endDate);
			helper.AddCountry(tradeGroupEuropeanUnionEUN, Core.Constants.CountryCodes.Germany, startDate.Date, endDate.Date);
			var tradeGroupJapan = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "JP", startDate, endDate);
			helper.AddCountry(tradeGroupJapan, Core.Constants.CountryCodes.Japan, startDate.Date, endDate.Date);
			var tradeGroupUnitedKingdom = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "GB", startDate, endDate);
			helper.AddCountry(tradeGroupUnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom, startDate.Date, endDate.Date);
			var tradeGroupArgentina = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "AR", startDate, endDate);
			helper.AddCountry(tradeGroupArgentina, Core.Constants.CountryCodes.Argentina, startDate.Date, endDate.Date);

			var rateType = helper.CreateNewOrGetExistingRateType(dataGroupingUnitedStates.ZZZ_DataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodes.Codes.Duty, rateType.PK);

			var rate99038181 = helper.CreateRefCusRate(tariff99038181.PK, rateCode.PK, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupEuropeanUnionEUN, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupJapan, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupUnitedKingdom, startDate, endDate);
			helper.CreateCusApplicability(rate99038181, tradeGroupArgentina, startDate, endDate);
			helper.CreateTariffRelationship(tariff99038181.PK, tariffType.PK, tariff7208103000.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff99038181.PK, tariffType.PK, tariff7219900060.ZZ1_TariffCode);

			var rate99038180 = helper.CreateRefCusRate(tariff99038180.PK, rateCode.PK, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupEuropeanUnionEUN, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupJapan, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupUnitedKingdom, startDate, endDate);
			helper.CreateCusApplicability(rate99038180, tradeGroupArgentina, startDate, endDate);
			helper.CreateTariffRelationship(tariff99038180.PK, tariffType.PK, tariff7208103000.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff99038180.PK, tariffType.PK, tariff7219900060.ZZ1_TariffCode);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CombineAssertions("Regular tariff 7208103000 has NO GAE attribute", () =>
			{
				invoiceLine.JI_Tariff = "7208103000";
				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertHasMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertHasMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertHasMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_SupTariff = "99038180";
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertHasMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertHasMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_SupTariff = "99038180";
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertHasMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Argentina;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);
			});

			CombineAssertions("Regular tariff 7219900060 has GAE attribute", () =>
			{
				invoiceLine.JI_Tariff = "7219900060";
				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertHasMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_SupTariff = "99038180";
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertHasMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_SupTariff = "99038180";
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "Tariff number is required");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.");

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Argentina;
				invoiceLine.US_SupTariff = ZString.Empty;
				invoiceLine.US_ProductExclusion = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);

				invoiceLine.US_SupTariff = "99038181";
				invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
				invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, "requires either a Prov/Prog Tariff Number or a Product Exclusion Number.");
				AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.OnlyEitherProvTariffOrProductExclusionCodeAllowed);
			});
		}

		public void TestCheckUS_ProductExclusion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN"); // TariffType
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "9903100102", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffAttribute = helper.CreateTariffAttribute("TYPE", "232", tariff);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_ProductExclusion = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);

			invoiceLine.US_ProductExclusion = "02";
			AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);

			invoiceLine.JI_Tariff = "7201101001";
			invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
			AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);

			invoiceLine.JI_Tariff = "7301101001";
			invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
			AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);

			invoiceLine.US_ProductExclusion = "03";
			AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);

			invoiceLine.JI_Tariff = "7601101001";
			invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
			AssertNoMessageErrorContaining(invoiceLine.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);

			invoiceLine.US_SupTariff = "9801100101";
			invoiceLine.AddInfoValidation.ValidateUS_ProductExclusion();
			invoiceLine.US_ProductExclusion = "02";
			AssertNoMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertHasMessageError(invoiceLine.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);
		}

		public void TestCheckUS_ControlledGroupName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			invoiceLine.US_ControlledGroupName = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_ControlledGroupName = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			invoiceLine.AddInfoValidation.ValidateUS_ControlledGroupName();
			AssertHasMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.AddInfoValidation.ValidateUS_ControlledGroupName();
			AssertHasMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.AddInfoValidation.ValidateUS_ControlledGroupName();
			AssertNoMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableENS = true;
			invoiceLine.US_ControlledGroupName = "~TEST~";
			AssertNoMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			invoiceLine.US_ControlledGroupName = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_ControlledGroupName = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			invoiceLine.AddInfoValidation.ValidateUS_ControlledGroupName();
			AssertNoMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.AddInfoValidation.ValidateUS_ControlledGroupName();
			AssertNoMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.AddInfoValidation.ValidateUS_ControlledGroupName();
			AssertNoMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableENS = true;
			invoiceLine.US_ControlledGroupName = "~TEST~";
			AssertNoMessageErrorContaining(invoiceLine.US_ControlledGroupNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_AllocationQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			invoiceLine.US_AllocationQuantity = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_ControlledGroupName = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			invoiceLine.AddInfoValidation.ValidateUS_AllocationQuantity();
			AssertHasMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.AddInfoValidation.ValidateUS_AllocationQuantity();
			AssertHasMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.AddInfoValidation.ValidateUS_AllocationQuantity();
			AssertNoMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableENS = true;
			invoiceLine.US_AllocationQuantity = 1m;
			AssertNoMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			invoiceLine.US_AllocationQuantity = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_ControlledGroupName = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			invoiceLine.AddInfoValidation.ValidateUS_AllocationQuantity();
			AssertNoMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.AddInfoValidation.ValidateUS_AllocationQuantity();
			AssertNoMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.AddInfoValidation.ValidateUS_AllocationQuantity();
			AssertNoMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableENS = true;
			invoiceLine.US_AllocationQuantity = 1m;
			AssertNoMessageErrorContaining(invoiceLine.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CBMADefaultTaxRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			invoiceLine.US_CBMADefaultTaxRate = -1m;
			AssertNoMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_ControlledGroupName = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			invoiceLine.AddInfoValidation.ValidateUS_CBMADefaultTaxRate();
			AssertNoMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_CBMADefaultTaxRate = -1m;
			invoiceLine.AddInfoValidation.ValidateUS_CBMADefaultTaxRate();
			AssertHasMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.AddInfoValidation.ValidateUS_CBMADefaultTaxRate();
			AssertNoMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.US_EnableENS = true;
			invoiceLine.US_CBMADefaultTaxRate = 0m;
			AssertNoMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			invoiceLine.US_CBMADefaultTaxRate = -1m;
			AssertNoMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_ControlledGroupName = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			invoiceLine.AddInfoValidation.ValidateUS_CBMADefaultTaxRate();
			AssertNoMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_CBMADefaultTaxRate = -1m;
			invoiceLine.AddInfoValidation.ValidateUS_CBMADefaultTaxRate();
			AssertNoMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.AddInfoValidation.ValidateUS_CBMADefaultTaxRate();
			AssertNoMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.US_EnableENS = true;
			invoiceLine.US_CBMADefaultTaxRate = 0m;
			AssertNoMessageErrorContaining(invoiceLine.US_CBMADefaultTaxRateInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckUS_ExclusionNumber()
		{
			#region Create Reference Data
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USProductExclusionTypes, "Product Exclusion Types", dataGrouping.ZZZ_DataGrouping);
			var attributeEXCMSK = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USProductExclusionCodeMask, "Exclusion Code Mask", codeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeEXCERT = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USProductExclusionCodeErrorText, "Exclusion Code Error Text", codeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var codeList02 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, AdditionalDeclarationTypeCodeList.Codes._02, AdditionalDeclarationTypeCodeList.Descriptions._02, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList02.PK, attributeEXCMSK.ZXE_Name, @"((STL|SPR|STX)\d{6}|INJS\d{5})");
			var errorMessage02 = @"Format should be STLNNNNNN or SPRNNNNNN or STXNNNNNN, where NNNNNN represent 6 digits, or format should be INJSNNNNN, where NNNNN represent 5 digits.";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList02.PK, attributeEXCERT.ZXE_Name, errorMessage02);
			var codeList03 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, AdditionalDeclarationTypeCodeList.Codes._03, AdditionalDeclarationTypeCodeList.Descriptions._03, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList03.PK, attributeEXCMSK.ZXE_Name, @"((ALU|APR)\d{6}|INJA\d{5})");
			var errorMessage03 = @"Format should be ALUNNNNNN or APRNNNNNN, where NNNNNN represent 6 digits, or format should be INJANNNNN, where NNNNN represent 5 digits.";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList03.PK, attributeEXCERT.ZXE_Name, errorMessage03);
			Factory.Save();
			#endregion
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_ExclusionNumber = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_ExclusionNumberInfo, ACEImportAddInfoJobComInvoiceLineValidation.ProductExclustionNumberNotAllowed);
			var fullErrorMessage02 = ACEImportAddInfoJobComInvoiceLineValidation.InvalidProductExclusionNumberMessagePrefix + errorMessage02;
			var fullErrorMessage03 = ACEImportAddInfoJobComInvoiceLineValidation.InvalidProductExclusionNumberMessagePrefix + errorMessage03;
			invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageErrorContaining(invoiceLine.US_ExclusionNumberInfo, ACEImportAddInfoJobComInvoiceLineValidation.ProductExclustionNumberNotAllowed);
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			invoiceLine.US_ExclusionNumber = "STL00000 ";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "STL00000A";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "STL000001";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "SPR00000 ";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "SPR00000A";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "SPR000001";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "STX00000 ";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "STX00000A";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "STX000001";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "INJS00001";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "ALU000001";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "APR000001";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._03;
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "ALU00000 ";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "ALU00000A";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "ALU100000";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "APR00000 ";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "APR00000A";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertHasMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "APR100000";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = "INJA00001";
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
			invoiceLine.US_ExclusionNumber = ZString.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_ExclusionNumber();
			AssertHasMessageErrorContaining(invoiceLine.US_ExclusionNumberInfo, fullErrorMessage03);
		}

		public void TestCheckUS_FPI()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNoNotifications(invoiceLine.US_FPIInfo);
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.AddInfoValidation.ValidateUS_FPI();
			AssertHasMessageErrorContaining(invoiceLine.US_FPIInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			invoiceLine.US_FPI = "~";
			AssertHasWarningContaining(invoiceLine.US_FPIInfo, "The FPI as assigned by TTB should be in the format TTB-FP-XXXXXXX");
			invoiceLine.US_FPI = "TTB-FP-0123456";
			AssertNoNotifications(invoiceLine.US_FPIInfo);
			invoiceLine.US_FPI = "X11021";
			AssertHasWarningContaining(invoiceLine.US_FPIInfo, "The FPI as assigned by TTB should be in the format TTB-FP-XXXXXXX");
			AssertNoWarningContaining(invoiceLine.US_FPIInfo, ACEImportAddInfoJobComInvoiceLineValidation.ForeignProducerIdentifierFormat);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			invoiceLine.AddInfoValidation.ValidateUS_FPI();
			AssertHasWarningContaining(invoiceLine.US_FPIInfo, ACEImportAddInfoJobComInvoiceLineValidation.ForeignProducerIdentifierFormat);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			invoiceLine.US_FPI = "~";
			AssertHasWarningContaining(invoiceLine.US_FPIInfo, "The format of the Foreign Producer Identifier is:\r\n");
			invoiceLine.US_FPI = "TB-FP-0123456";
			AssertHasWarningContaining(invoiceLine.US_FPIInfo, "The format of the Foreign Producer Identifier is:\r\n");
			invoiceLine.US_FPI = "B11021";
			AssertNoNotifications(invoiceLine.US_FPIInfo);
		}

		public void TestCheckUS_Prim_NA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNoNotifications(invoiceLine.US_Prim_NAInfo);

			invoiceLine.US_Prim_NA = true;
			AssertNoMessageError(invoiceLine.US_Prim_NAInfo, ACEImportAddInfoJobComInvoiceLineValidation.PrimaryNACannotBeTicked);

			invoiceLine.US_RN_NKPrimCtry = "CA";
			invoiceLine.AddInfoValidation.ValidateUS_Prim_NA();
			AssertHasMessageError(invoiceLine.US_Prim_NAInfo, ACEImportAddInfoJobComInvoiceLineValidation.PrimaryNACannotBeTicked);

			invoiceLine.US_RN_NKPrimCtry = ZString.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_Prim_NA();
			AssertNoMessageError(invoiceLine.US_Prim_NAInfo, ACEImportAddInfoJobComInvoiceLineValidation.PrimaryNACannotBeTicked);
		}

		public void TestCheckUS_RN_NKPrimCtry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNoNotifications(invoiceLine.US_RN_NKPrimCtryInfo);

			invoiceLine.US_RN_NKPrimCtry = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_RN_NKPrimCtryInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.Canada;
			AssertNoMessageErrorContaining(invoiceLine.US_RN_NKPrimCtryInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_RN_NKPrimCtry = ZString.Empty;
			AssertNoNotifications(invoiceLine.US_RN_NKPrimCtryInfo);
		}

		public void TestCheckUS_Sec_NA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNoNotifications(invoiceLine.US_Sec_NAInfo);

			invoiceLine.US_Sec_NA = true;
			AssertNoMessageError(invoiceLine.US_Sec_NAInfo, ACEImportAddInfoJobComInvoiceLineValidation.SecondaryNACannotBeTicked);

			invoiceLine.US_RN_NKSecCtry = "CA";
			invoiceLine.AddInfoValidation.ValidateUS_Sec_NA();
			AssertHasMessageError(invoiceLine.US_Sec_NAInfo, ACEImportAddInfoJobComInvoiceLineValidation.SecondaryNACannotBeTicked);

			invoiceLine.US_RN_NKSecCtry = ZString.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_Sec_NA();
			AssertNoMessageError(invoiceLine.US_Sec_NAInfo, ACEImportAddInfoJobComInvoiceLineValidation.SecondaryNACannotBeTicked);
		}

		public void TestCheckUS_RN_NKMeltCtryUS_RN_NKCertOriginWhenCombined()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.CTORG);
			var meltConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.MELT);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			Factory.Save();

			var tariff7206100000 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7206100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime); // CTORG
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff7206100000.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff98138183 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "98138183", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7206100001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime); // No CTORG
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);

			var tariff99038001 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var meltConditionFor99038001 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, meltConditionType.PK, tariff99038001.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(meltConditionFor99038001, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate99038001 = helper.CreateRate(tariff99038001, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038001, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(tariff99038001.PK, tariffType.PK, "7206");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99038001";

			invoiceLine.US_RN_NKCertOrigin = "CN";
			invoiceLine.US_RN_NKMeltCtry = "CN";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "7206100001";
			invoiceLine2.US_UC_NKCountryOfOrigin = "FR";
			invoiceLine2.US_RN_NKCertOrigin = "#$";
			invoiceLine2.US_RN_NKMeltCtry = "DE";
			invoiceLine2.US_SupTariff = "98138183";
			invoiceLine2.JI_ParentID = invoiceLine.PK;

			invoiceLine.AddInfoValidation.ValidateUS_RN_NKCertOrigin();
			AssertHasMessageError(invoiceLine.US_RN_NKCertOriginInfo, ACEImportAddInfoJobComInvoiceLineValidation.CertificateOfOriginShouldBeEnteredOnNormalInvoiceLine);
			invoiceLine.US_RN_NKCertOrigin = "";
			AssertNoMessageError(invoiceLine.US_RN_NKCertOriginInfo, ACEImportAddInfoJobComInvoiceLineValidation.CertificateOfOriginShouldBeEnteredOnNormalInvoiceLine);

			invoiceLine2.AddInfoValidation.ValidateUS_RN_NKCertOrigin();
			AssertNoMessageError(invoiceLine2.US_RN_NKCertOriginInfo, ACEImportAddInfoJobComInvoiceLineValidation.CertificateOfOriginShouldBeEnteredOnNormalInvoiceLine);

			invoiceLine.AddInfoValidation.ValidateUS_RN_NKMeltCtry();
			AssertHasMessageError(invoiceLine.US_RN_NKMeltCtryInfo, ACEImportAddInfoJobComInvoiceLineValidation.MeltCtryShouldBeEnteredOnNormalInvoiceLine);
			invoiceLine.US_RN_NKMeltCtry = "";
			AssertNoMessageError(invoiceLine.US_RN_NKMeltCtryInfo, ACEImportAddInfoJobComInvoiceLineValidation.MeltCtryShouldBeEnteredOnNormalInvoiceLine);

			invoiceLine2.AddInfoValidation.ValidateUS_RN_NKMeltCtry();
			AssertNoMessageError(invoiceLine2.US_RN_NKMeltCtryInfo, ACEImportAddInfoJobComInvoiceLineValidation.MeltCtryShouldBeEnteredOnNormalInvoiceLine);
		}

		public void TestCheckUS_RN_NKCertOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7206100001";
			invoiceLine.US_UC_NKCountryOfOrigin = "FR";
			invoiceLine.US_RN_NKCertOrigin = "#$";
			invoiceLine.US_SupTariff = "98138183";

			invoiceLine.AddInfoValidation.ValidateUS_RN_NKCertOrigin();
			AssertHasMessageError(invoiceLine.US_RN_NKCertOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_RN_NKMeltCtry()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.MELT);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariff7206100000 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7206100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime); // MELT
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff7206100000.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff98138183 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "98138183", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition98138183 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff98138183.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7206100001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime); // No MELT
			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var mandatoryMessage = "You have not entered a Melt Ctry.";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7206100001";
			invoiceLine.US_UC_NKCountryOfOrigin = "FR";
			invoiceLine.US_RN_NKMeltCtry = "#$";
			invoiceLine.US_SupTariff = "98138183";

			invoiceLine.AddInfoValidation.ValidateUS_RN_NKMeltCtry();
			AssertHasMessageError(invoiceLine.US_RN_NKMeltCtryInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_RN_NKMeltCtry = "US";
			AssertNoWarning(invoiceLine.US_RN_NKMeltCtryInfo, mandatoryMessage);

			invoiceLine.JI_Tariff = "7206100000";
			invoiceLine.US_RN_NKMeltCtry = "";
			AssertHasWarning(invoiceLine.US_RN_NKMeltCtryInfo, mandatoryMessage);
			invoiceLine.US_RN_NKMeltCtry = "ZZ";
			AssertNoWarning(invoiceLine.US_RN_NKMeltCtryInfo, mandatoryMessage);
			invoiceLine.US_RN_NKMeltCtry = "US";
			AssertNoWarning(invoiceLine.US_RN_NKMeltCtryInfo, mandatoryMessage);
		}

		public void TestCheckUS_RN_NKMeltCtry_TariffCode99038185()
		{
			#region Setup Preliminary Data
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.MELT);

			var usTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var usTradeGroupCountry = helper.AddCountry(usTradeGroup, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var caTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Canada, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var caTradeGroupCountry = helper.AddCountry(caTradeGroup, Core.Constants.CountryCodes.Canada, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var mxTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Mexico, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var mxTradeGroupCountry = helper.AddCountry(mxTradeGroup, Core.Constants.CountryCodes.Mexico, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "All Countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.China, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Canada, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Mexico, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			#endregion

			TestCase("99038185", "MSG");
			TestCase("99038186", "WAR");
			TestCase("99038187", null);

			void TestCase(ZString tariffCode, ZString messageType)
			{
				#region Setup Data
				var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, severity: messageType);
				Factory.Save();
				var applicability = helper.CreateCusApplicability(condition, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				helper.CreateExcludedTradeGroup(caTradeGroup, applicability);
				helper.CreateExcludedTradeGroup(usTradeGroup, applicability);
				helper.CreateExcludedTradeGroup(mxTradeGroup, applicability);
				Factory.Save();
				#endregion

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				var excludeErrorMessage = "The code you have selected is in the exclusion list.";
				var emptyErrorMessage = "You have not entered a Melt Ctry.";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.US_UC_NKCountryOfOrigin = "UA";
				invoiceLine.US_SupTariff = tariffCode;
				if (messageType == "MSG")
				{
					invoiceLine.US_RN_NKMeltCtry = "CA";
					AssertHasMessageError(invoiceLine.US_RN_NKMeltCtryInfo, excludeErrorMessage);
					invoiceLine.US_RN_NKMeltCtry = "US";
					AssertHasMessageError(invoiceLine.US_RN_NKMeltCtryInfo, excludeErrorMessage);
					invoiceLine.US_RN_NKMeltCtry = "MX";
					AssertHasMessageError(invoiceLine.US_RN_NKMeltCtryInfo, excludeErrorMessage);
					invoiceLine.US_RN_NKMeltCtry = "CN";
					AssertNoMessageErrors(invoiceLine.US_RN_NKMeltCtryInfo);
					invoiceLine.US_RN_NKMeltCtry = "";
					AssertHasMessageError(invoiceLine.US_RN_NKMeltCtryInfo, emptyErrorMessage);
				}
				else
				{
					invoiceLine.US_RN_NKMeltCtry = "CA";
					AssertHasWarning(invoiceLine.US_RN_NKMeltCtryInfo, excludeErrorMessage);
					invoiceLine.US_RN_NKMeltCtry = "US";
					AssertHasWarning(invoiceLine.US_RN_NKMeltCtryInfo, excludeErrorMessage);
					invoiceLine.US_RN_NKMeltCtry = "MX";
					AssertHasWarning(invoiceLine.US_RN_NKMeltCtryInfo, excludeErrorMessage);
					invoiceLine.US_RN_NKMeltCtry = "CN";
					AssertNoWarnings(invoiceLine.US_RN_NKMeltCtryInfo);
					invoiceLine.US_RN_NKMeltCtry = "";
					AssertHasWarning(invoiceLine.US_RN_NKMeltCtryInfo, emptyErrorMessage);
				}
			}
		}

		public void TestCheckUS_RN_NKSecCtry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNoNotifications(invoiceLine.US_RN_NKSecCtryInfo);

			invoiceLine.US_RN_NKSecCtry = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_RN_NKSecCtryInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_RN_NKSecCtry = Core.Constants.CountryCodes.Canada;
			AssertNoMessageErrorContaining(invoiceLine.US_RN_NKSecCtryInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_RN_NKSecCtry = ZString.Empty;
			AssertNoNotifications(invoiceLine.US_RN_NKSecCtryInfo);
		}

		public void TestCheckUS_RN_NKCastCtry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNoNotifications(invoiceLine.US_RN_NKSecCtryInfo);

			invoiceLine.US_RN_NKCastCtry = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_RN_NKCastCtryInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.Canada;
			AssertNoMessageErrorContaining(invoiceLine.US_RN_NKCastCtryInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_RN_NKCastCtry = ZString.Empty;
			AssertNoNotifications(invoiceLine.US_RN_NKCastCtryInfo);
		}

		public void TestSupTariffWith99038809AgnistWithExportDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var progTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038804", new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06));
			var progTariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038809", new ZDateTime(2019, 05, 10), new ZDateTime(2019, 05, 31));
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate1 = helper.CreateRate(progTariff1, rateCode.PK, new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06), "0");
			var rate2 = helper.CreateRate(progTariff2, rateCode.PK, new ZDateTime(2019, 05, 10), new ZDateTime(2019, 05, 31), "0");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "CN", new ZDateTime(2000, 01, 01), new ZDateTime(2079, 06, 06));
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.China, new ZDate(2000, 01, 01), new ZDate(2079, 06, 06));
			var applicability1 = helper.CreateCusApplicability(rate1, tradeGroup, new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06));
			var applicability2 = helper.CreateCusApplicability(rate2, tradeGroup, new ZDateTime(2019, 05, 10), new ZDateTime(2019, 05, 31));
			var relationship1 = helper.CreateTariffRelationship(progTariff1.PK, hsnTariffType.PK, "8517620010");
			var relationship2 = helper.CreateTariffRelationship(progTariff2.PK, hsnTariffType.PK, "8517620010");
			helper.CreateTariffAttribute("RULE", "A99", progTariff1);
			helper.CreateTariffAttribute("RULE", "A99", progTariff2);
			Factory.Save();
			var tariff8517620010 = Factory.New<USCTariff>();
			tariff8517620010.UE_Tariff = "8517620010";
			tariff8517620010.UE_DateFrom = new ZDateTime(2019, 01, 01);
			tariff8517620010.UE_DateTo = new ZDateTime(2079, 06, 06);
			var tariff99038804 = Factory.New<USCTariff>();
			tariff99038804.UE_Tariff = "99038804";
			tariff99038804.UE_DateFrom = new ZDateTime(2018, 09, 24);
			tariff99038804.UE_DateTo = new ZDateTime(2079, 06, 06);
			var tariff99038809 = Factory.New<USCTariff>();
			tariff99038809.UE_Tariff = "99038809";
			tariff99038809.UE_DateFrom = new ZDateTime(2019, 05, 10);
			tariff99038809.UE_DateTo = new ZDateTime(2019, 05, 31);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2019, 05, 13);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "8517620010";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation._99038809CannotBeSelected);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DateOfExport = new ZDateTime(2019, 05, 08);
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "8517620010";
			invoiceLine.US_SupTariff = "99038804";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation._99038809CannotBeSelected);
			invoiceLine.US_SupTariff = "99038809";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation._99038809CannotBeSelected);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DateOfExport = new ZDateTime(2019, 05, 10);
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "8517620010";
			invoiceLine.US_SupTariff = "99038809";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation._99038809CannotBeSelected);
			invoiceLine.US_SupTariff = "99038804";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation._99038809CannotBeSelected);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			invoiceLine.US_ProductExclusion = "02";
			invoiceLine.US_SupTariff = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.ProvTariffIsRequired);
			invoiceLine.US_ProductExclusion = ZString.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, ACEImportAddInfoJobComInvoiceLineValidation.ProvTariffIsRequired);
		}

		public void TestCheckUS_SupTariffFA99()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffHelper = new USCTariffTestCase(Factory);
			var startDate = new ZDate(2016, 01, 01);
			var endDate = new ZDate(2079, 01, 01);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var tariff73 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "7301100000", startDate, endDate);
			var tariff9903 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038001", startDate, endDate);
			var tariff99038002 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038002", startDate, endDate);
			tariffHelper.CreateNewTariffIfNotExists("7301100000", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("99038001", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("99038002", startDate, endDate, "KG", "", "", "");
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate = helper.CreateRate(tariff9903, rateCode.PK, startDate, endDate, "0.25");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var applicability = helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate);
			var relationship = helper.CreateTariffRelationship(tariff9903.PK, hsnTariffType.PK, "73");
			var tariffAttribute = helper.CreateTariffAttribute("RULE", "A99", tariff9903);
			var tariffAttribute8002 = helper.CreateTariffAttribute("RULE", "A99", tariff99038002);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_CertifyCargoRelease = false;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7301100000";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertEquals("99038001", invoiceLine.US_SupTariff);
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
			invoiceLine.US_SupTariff = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
			invoiceLine.US_ProductExclusion = "02";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
			invoiceLine.US_ProductExclusion = string.Empty;
			invoiceLine.US_SupTariff = "12341234";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Only certain tariffs can be selected");
			invoiceLine.US_SupTariff = "99038001";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Only certain tariffs can be selected");
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Prov/Prog Tariff number not allowed with the Tariff/Country combination.");
			invoiceLine.US_SupTariff = "99038002";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Prov/Prog Tariff number not allowed with the Tariff/Country combination.");
		}

		public void TestSupTariffHasAutoCondition()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, UniversalReferenceConstants.TariffConditionValueTypes.Codes.Entry);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.AUTO);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "98130035", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition98130035 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition98130035.PK, EntryTypeList.Codes.TemporaryImportationBond);

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8401100000";
			invoiceLine.US_SupTariff = "98130035";
			AssertEquals("Has Auto Condition", true, invoiceLine.SupTariffMatchesAutoCondition);

			invoiceLine.US_SupTariff = "98130000";
			AssertEquals("No Auto Condition", false, invoiceLine.SupTariffMatchesAutoCondition);
		}

		public void TestSmeltAndCastInformationRequired()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, UniversalReferenceConstants.TariffConditionTypes.Codes.SMELT);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7601103000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			#endregion

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USSmelt, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				declaration.US_EnableENS = true;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "7601103000";
				invoiceLine.AddInfoValidation.ValidateUS_Prim_NA();
				AssertNoMessageError(invoiceLine.US_Prim_NAInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.AddInfoValidation.ValidateUS_RN_NKPrimCtry();
				AssertNoMessageError(invoiceLine.US_RN_NKPrimCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.AddInfoValidation.ValidateUS_Sec_NA();
				AssertNoMessageError(invoiceLine.US_Sec_NAInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.AddInfoValidation.ValidateUS_RN_NKSecCtry();
				AssertNoMessageError(invoiceLine.US_RN_NKSecCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.AddInfoValidation.ValidateUS_RN_NKCastCtry();
				AssertNoMessageError(invoiceLine.US_RN_NKCastCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.US_Prim_NA = true;
				AssertHasMessageError(invoiceLine.US_Prim_NAInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.Australia;
				AssertHasMessageError(invoiceLine.US_RN_NKPrimCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.US_Sec_NA = true;
				AssertHasMessageError(invoiceLine.US_Sec_NAInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.US_RN_NKSecCtry = Core.Constants.CountryCodes.Australia;
				AssertHasMessageError(invoiceLine.US_RN_NKSecCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.Australia;
				AssertHasMessageError(invoiceLine.US_RN_NKCastCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USSmelt, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				declaration.US_EnableENS = true;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "7601103000";
				invoiceLine.AddInfoValidation.ValidateUS_Prim_NA();
				AssertHasMessageError(invoiceLine.US_Prim_NAInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.AddInfoValidation.ValidateUS_RN_NKPrimCtry();
				AssertHasMessageError(invoiceLine.US_RN_NKPrimCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.AddInfoValidation.ValidateUS_Sec_NA();
				AssertHasMessageError(invoiceLine.US_Sec_NAInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.AddInfoValidation.ValidateUS_RN_NKSecCtry();
				AssertHasMessageError(invoiceLine.US_RN_NKSecCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.AddInfoValidation.ValidateUS_RN_NKCastCtry();
				AssertHasMessageError(invoiceLine.US_RN_NKCastCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.US_Prim_NA = true;
				AssertNoMessageError(invoiceLine.US_Prim_NAInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.Australia;
				AssertNoMessageError(invoiceLine.US_RN_NKPrimCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.US_Sec_NA = true;
				AssertNoMessageError(invoiceLine.US_Sec_NAInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.US_RN_NKSecCtry = Core.Constants.CountryCodes.Australia;
				AssertNoMessageError(invoiceLine.US_RN_NKSecCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.Australia;
				AssertNoMessageError(invoiceLine.US_RN_NKCastCtryInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);
			}
		}

		public void TestCheckUS_SupTariffForAluminumSmelt()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var allTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "All Countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.HongKong, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var russiaTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(russiaTradeGroup, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, UniversalReferenceConstants.TariffConditionTypes.Codes.SMELT);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, UniversalReferenceConstants.TariffConditionValueTypes.Codes.Entry);

			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);

			Factory.Save();

			var tariff7601 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7601103000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff7601.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff99038501 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038501", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038501);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038501);
			helper.CreateTariffRelationship(tariff99038501.PK, tariffType.PK, "7601");
			var rate99038501 = helper.CreateRate(tariff99038501, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038501, allTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff99038567 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038567", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038567);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038567);
			helper.CreateTariffRelationship(tariff99038567.PK, tariffType.PK, "7601");
			var rate99038567 = helper.CreateRate(tariff99038567, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038567, russiaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition99038567 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff99038567.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition99038567.PK, "~06");
			helper.CreateCusApplicability(condition99038567, russiaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			#endregion

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USSmelt, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				declaration.US_EnableENS = true;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "7601103000";
				invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
				invoiceLine1.US_SupTariff = "99038501";
				AssertNoMessageError(invoiceLine1.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);

				invoiceLine1.US_SupTariff = "99038567";
				AssertHasMessageError(invoiceLine1.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);

				invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
				invoiceLine1.US_SupTariff = "99038501";
				AssertHasMessageError(invoiceLine1.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);

				invoiceLine1.US_SupTariff = "99038567";
				AssertNoMessageError(invoiceLine1.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);

				invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
				invoiceLine1.US_RN_NKSecCtry = Core.Constants.CountryCodes.Russia;
				invoiceLine1.US_SupTariff = "99038501";
				AssertHasMessageError(invoiceLine1.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);

				invoiceLine1.US_SupTariff = "99038567";
				AssertNoMessageError(invoiceLine1.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);

				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.US_SupTariff = "99038501";
				invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
				var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_ParentID = invoiceLine2.PK;
				invoiceLine3.JI_Tariff = "7601103000";
				invoiceLine3.US_SupTariff = "99038567";
				invoiceLine2.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageError(invoiceLine2.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
				invoiceLine3.AddInfoValidation.ValidateUS_SupTariff();
				AssertHasMessageError(invoiceLine3.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);

				invoiceLine3.US_RN_NKCastCtry = Core.Constants.CountryCodes.Russia;
				invoiceLine3.AddInfoValidation.ValidateUS_SupTariff();
				AssertNoMessageError(invoiceLine3.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
			}
		}

		public void TestCheckUS_SupTariffFA99WhenIsCombinedLines()
		{
			SetUpRefData();
			string messageError = "Only certain tariffs can be selected as secondary tariff numbers where their primary tariff numbers are applicable for a 'A99' rule.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.JI_Tariff = "8401100000";
			invoiceLine.US_SupTariff = "9801001010";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, messageError);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.US_SupTariff = "9801001010";
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_ParentID = invoiceLine.PK;
			invoiceLine1.US_SupTariff = "99030221";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.JI_Tariff = "8401100000";
			invoiceLine2.US_SupTariff = "99038802";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			invoiceLine1.AddInfoValidation.ValidateUS_SupTariff();
			invoiceLine2.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, messageError);
			AssertHasMessageError(invoiceLine1.US_SupTariffInfo, messageError);
			AssertHasMessageError(invoiceLine2.US_SupTariffInfo, messageError);
			invoiceLine2.US_SupTariff = "99038801";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			invoiceLine1.AddInfoValidation.ValidateUS_SupTariff();
			invoiceLine2.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, messageError);
			AssertNoMessageError(invoiceLine1.US_SupTariffInfo, messageError);
			AssertNoMessageError(invoiceLine2.US_SupTariffInfo, messageError);
		}

		public void TestCheckUS_SupTariff_NotAllowed_NoParentChildRelationship()
		{
			SetUpRefData();
			string messageError = "Only certain tariffs can be selected as secondary tariff numbers where their primary tariff numbers are applicable for a 'A99' rule.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8401100000";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.US_SupTariff = "99038801";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, messageError);
			invoiceLine.US_SupTariff = "99038802";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, messageError);
			invoiceLine.JI_Tariff = "4101201010";
			invoiceLine.US_SupTariff = "99038801";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, "Prov/Prog Tariff number not allowed with the Tariff/Country combination.");
			invoiceLine.JI_Tariff = "";
			invoiceLine.US_SupTariff = "99038801";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, "Prov/Prog Tariff number not allowed with the Tariff/Country combination.");
		}

		public void TestCheckUS_SupTariff_NotAllowed_WithParentChildRelationship()
		{
			SetUpRefData();
			string messageError = "Only certain tariffs can be selected as secondary tariff numbers where their primary tariff numbers are applicable for a 'A99' rule.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineParent = invoice.JobComInvoiceLines.AddNew();
			invoiceLineParent.US_IsParent = true;
			invoiceLineParent.JI_Tariff = "8401100000";
			invoiceLineParent.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLineParent.US_SupTariff = "99038801";
			AssertNoMessageError(invoiceLineParent.US_SupTariffInfo, messageError);
			var invoiceLineChild1 = invoiceLineParent.AddSecondaryInvoiceLine();
			invoiceLineChild1.JI_Tariff = "4101201010";
			invoiceLineChild1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLineChild1.US_SupTariff = "";
			invoiceLineParent.JI_Tariff = "8401100000";
			invoiceLineParent.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLineParent.US_SupTariff = "99038801";
			AssertNoMessageError(invoiceLineParent.US_SupTariffInfo, messageError);
			invoiceLineParent.JI_Tariff = "";
			invoiceLineParent.US_SupTariff = "99038801";
			AssertHasMessageError(invoiceLineParent.US_SupTariffInfo, "Prov/Prog Tariff number not allowed with the Tariff/Country combination.");
			invoiceLineChild1.JI_Tariff = "8401100000";
			invoiceLineParent.JI_Tariff = "";
			invoiceLineParent.US_SupTariff = "99038801";
			AssertNoMessageError(invoiceLineParent.US_SupTariffInfo, messageError);
			invoiceLineParent.US_SupTariff = "99038802";
			AssertHasMessageError(invoiceLineParent.US_SupTariffInfo, messageError);
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffHelper = new USCTariffTestCase(Factory);
			var startDate = new ZDate(2016, 01, 01);
			var endDate = new ZDate(2079, 01, 01);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "8401100000", startDate, endDate);
			helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "4101201010", startDate, endDate);
			var tariff9903 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038801", startDate, endDate);
			tariffHelper.CreateNewTariffIfNotExists("8401100000", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("4101201010", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("99038801", startDate, endDate, "KG", "", "", "");
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate = helper.CreateRate(tariff9903, rateCode.PK, startDate, endDate, "0.25");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var applicability = helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate);
			var relationship = helper.CreateTariffRelationship(tariff9903.PK, hsnTariffType.PK, "84");
			var tariffAttribute = helper.CreateTariffAttribute("RULE", "A99", tariff9903);
			Factory.Save();
		}

		void AssertUS_DDTCInd(JobComInvoiceLine invoiceLine, Action<ZPropertyInfo, string> assertNoMessage, Action<ZPropertyInfo, string> assertHasMessage)
		{
			var declaration = invoiceLine.Declaration;
			var invalidCodeMessageError = ListValidation.GetNotificationMessage(invoiceLine.US_DDTCIndInfo).ToString();
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			assertNoMessage(invoiceLine.US_DDTCIndInfo, invalidCodeMessageError);
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Disclaimed;
			assertHasMessage(invoiceLine.US_DDTCIndInfo, invalidCodeMessageError);
			declaration.ValidationModes = ValidationModes.None;
			invoiceLine.AddInfoValidation.ValidateUS_DDTCInd();
			assertHasMessage(invoiceLine.US_DDTCIndInfo, invalidCodeMessageError);
			declaration.RecalculateValidationModesOnDeclaration();
			invoiceLine.AddInfoValidation.ValidateUS_DDTCInd();
			assertHasMessage(invoiceLine.US_DDTCIndInfo, invalidCodeMessageError);
			invoiceLine.US_DDTCInd = ZString.Empty;
			assertNoMessage(invoiceLine.US_DDTCIndInfo, invalidCodeMessageError);
		}

		void AssertUS_DDTCArrivalDate(JobComInvoiceLine invoiceLine, Action<ZPropertyInfo, string> assertNoMessage, Action<ZPropertyInfo, string> assertHasMessage)
		{
			var declaration = invoiceLine.Declaration;
			invoiceLine.US_DDTCArrivalDate = ZDateTime.Empty;
			invoiceLine.US_DDTCInd = ZString.Empty;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("DDTC Anticipated Arrival Date");
			assertNoMessage(invoiceLine.US_DDTCArrivalDateInfo, messageError);
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			assertHasMessage(invoiceLine.US_DDTCArrivalDateInfo, messageError);
			declaration.ValidationModes = ValidationModes.None;
			invoiceLine.AddInfoValidation.ValidateUS_DDTCArrivalDate();
			assertNoMessage(invoiceLine.US_DDTCArrivalDateInfo, messageError);
			declaration.RecalculateValidationModesOnDeclaration();
			invoiceLine.AddInfoValidation.ValidateUS_DDTCArrivalDate();
			assertHasMessage(invoiceLine.US_DDTCArrivalDateInfo, messageError);
			invoiceLine.US_DDTCArrivalDate = ZDateTime.BrettsBirthday;
			assertNoMessage(invoiceLine.US_DDTCArrivalDateInfo, messageError);
		}

		void AssertUS_DDTCExemptionCode(JobComInvoiceLine invoiceLine, Action<ZPropertyInfo, string> assertNoMessage, Action<ZPropertyInfo, string> assertHasMessage)
		{
			var declaration = invoiceLine.Declaration;
			invoiceLine.US_DDTCInd = ZString.Empty;
			invoiceLine.US_DDTCExemptionCode = "!@";
			var invalidCodeMessageError = ListValidation.GetNotificationMessage(invoiceLine.US_DDTCExemptionCodeInfo).ToString();
			assertNoMessage(invoiceLine.US_DDTCExemptionCodeInfo, invalidCodeMessageError);
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			assertHasMessage(invoiceLine.US_DDTCExemptionCodeInfo, invalidCodeMessageError);
			declaration.ValidationModes = ValidationModes.None;
			invoiceLine.AddInfoValidation.ValidateUS_DDTCExemptionCode();
			assertNoMessage(invoiceLine.US_DDTCExemptionCodeInfo, invalidCodeMessageError);
			declaration.RecalculateValidationModesOnDeclaration();
			invoiceLine.AddInfoValidation.ValidateUS_DDTCExemptionCode();
			assertHasMessage(invoiceLine.US_DDTCExemptionCodeInfo, invalidCodeMessageError);
			invoiceLine.US_DDTCExemptionCode = ACEDDTCExemptionCodes.Codes._12318A2;
			assertNoMessage(invoiceLine.US_DDTCExemptionCodeInfo, invalidCodeMessageError);
			invoiceLine.US_DDTCExemptionCode = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_DDTCExemptionCodeInfo);
		}

		void AssertUS_DDTCLicenseNo(JobComInvoiceLine invoiceLine, Action<ZPropertyInfo, string> assertNoMessage, Action<ZPropertyInfo, string> assertHasMessage)
		{
			var declaration = invoiceLine.Declaration;
			invoiceLine.US_DDTCInd = ZString.Empty;
			assertNoMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			assertHasMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
			declaration.ValidationModes = ValidationModes.None;
			invoiceLine.AddInfoValidation.ValidateUS_DDTCLicenseNo();
			assertNoMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
			declaration.RecalculateValidationModesOnDeclaration();
			invoiceLine.AddInfoValidation.ValidateUS_DDTCLicenseNo();
			assertHasMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCExemptionCode = ACEDDTCExemptionCodes.Codes._12318A2;
			assertNoMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCExemptionCode = ZString.Empty;
			assertHasMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCLicenseType = DDTCLicenseTypeCodes.Codes.S61;
			assertNoMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
			assertHasMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.DDTCLicenseNoIsRequired);
			invoiceLine.US_DDTCLicenseType = ZString.Empty;
			assertHasMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
			assertNoMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.DDTCLicenseNoIsRequired);
			invoiceLine.US_DDTCLicenseNo = "A";
			assertNoMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCExemptionCode = ACEDDTCExemptionCodes.Codes._12318A2;
			assertHasMessage(invoiceLine.US_DDTCLicenseNoInfo, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
		}

		void AssertUS_DDTCLicenseType(JobComInvoiceLine invoiceLine, Action<ZPropertyInfo, string> assertNoMessage, Action<ZPropertyInfo, string> assertHasMessage)
		{
			var declaration = invoiceLine.Declaration;
			invoiceLine.US_DDTCInd = ZString.Empty;
			assertNoMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			assertHasMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
			declaration.ValidationModes = ValidationModes.None;
			invoiceLine.AddInfoValidation.ValidateUS_DDTCLicenseType();
			assertNoMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
			declaration.RecalculateValidationModesOnDeclaration();
			invoiceLine.AddInfoValidation.ValidateUS_DDTCLicenseType();
			assertHasMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCExemptionCode = ACEDDTCExemptionCodes.Codes._12318A2;
			assertNoMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCExemptionCode = ZString.Empty;
			assertHasMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCLicenseNo = "A";
			assertNoMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
			assertHasMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.DDTCLicenseTypeIsRequired);
			invoiceLine.US_DDTCLicenseNo = ZString.Empty;
			assertHasMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
			assertNoMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.DDTCLicenseTypeIsRequired);
			invoiceLine.US_DDTCLicenseType = DDTCLicenseTypeCodes.Codes.S61;
			assertNoMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
			invoiceLine.US_DDTCExemptionCode = ACEDDTCExemptionCodes.Codes._12318A2;
			assertHasMessage(invoiceLine.US_DDTCLicenseTypeInfo, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
		}
	}
}
