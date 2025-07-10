using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USOrgImpAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZO_KnownImporterIndicator()
		{
			addInfo.ZO_KnwImpInd = "~";
			AssertHasMessageError(addInfo.ZO_KnwImpIndInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_KnwImpInd = "Y";
			AssertNoMessageError(addInfo.ZO_KnwImpIndInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZO_OH_NP()
		{
			var notifyParty = Factory.New<OrgHeader>();
			addInfo.ZO_OH_NP = notifyParty.PK;
			AssertHasMessageError(addInfo.ZO_OH_NPInfo, USOrgImpAddInfoValidation._4811PartyMustHaveID);

			notifyParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12344");
			addInfo.ZO_OH_NP = ZGuid.Empty;
			addInfo.ZO_OH_NP = notifyParty.PK;
			AssertNoMessageError(addInfo.ZO_OH_NPInfo, USOrgImpAddInfoValidation._4811PartyMustHaveID);
		}

		public void TestCheckZO_ReconFilingPort()
		{
			addInfo.ZO_ReconFilingPort = "";
			AssertNoMessageErrorContaining(addInfo.ZO_ReconFilingPortInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_ReconFilingPort = "5";
			AssertHasMessageErrorContaining(addInfo.ZO_ReconFilingPortInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZO_NPID()
		{
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());

			addInfo.ZO_NPID = "1234567NN";
			AssertHasWarningContaining(addInfo.ZO_NPIDInfo, USOrgImpAddInfoValidation.Invalid4811PartyIDFormat);

			addInfo.ZO_NPID = "12-1234567NN";
			AssertNoWarningContaining(addInfo.ZO_NPIDInfo, USOrgImpAddInfoValidation.Invalid4811PartyIDFormat);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());

			addInfo.ZO_NPID = "1234567NN";
			AssertHasErrorContaining(addInfo.ZO_NPIDInfo, USOrgImpAddInfoValidation.Invalid4811PartyIDFormat);

			addInfo.ZO_NPID = "123-12-1234";
			AssertNoErrorContaining(addInfo.ZO_NPIDInfo, USOrgImpAddInfoValidation.Invalid4811PartyIDFormat);
		}

		public void TestCheckZO_ImporterType()
		{
			addInfo.ZO_ImporterType = "~";
			AssertHasMessageError(addInfo.ZO_ImporterTypeInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_ImporterType = addInfo.Lookups.ZO_ImporterTypeList[0].Code;
			AssertNoMessageError(addInfo.ZO_ImporterTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZO_PayMethod()
		{
			addInfo.ZO_PayMethod = "~~~";
			AssertHasError(addInfo.ZO_PayMethodInfo, "Enter a valid selection.");

			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			addInfo.ZO_PayMethod = ACHPaymentTypeList.Codes.ImporterCheck;
			AssertNoError(addInfo.ZO_PayMethodInfo, "Enter a valid selection.");
			AssertHasError(addInfo.ZO_PayMethodInfo, USOrgImpAddInfoValidation.ImporterCheckWhilePaymentTypeIndicatesBrokerPays);

			addInfo.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			AssertNoError(addInfo.ZO_PayMethodInfo, USOrgImpAddInfoValidation.ImporterCheckWhilePaymentTypeIndicatesBrokerPays);
		}

		public void TestCheckZO_PayerUnitNo()
		{
			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			addInfo.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			addInfo.ZO_AccountNo = ZString.Empty;
			AssertHasError(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoIsMandatory);

			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			addInfo.ZO_AccountNo = ZString.Empty;
			AssertNoError(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoIsMandatory);

			addInfo.ZO_AccountNo = "123";
			AssertNoError(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoIsMandatory);
			AssertHasError(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoLength);

			addInfo.ZO_AccountNo = "123456";
			AssertNoError(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoLength);

			addInfo.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHCredit;
			addInfo.ZO_AccountNo = "123";
			AssertHasError(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoIsNotRequired);

			addInfo.ZO_AccountNo = ZString.Empty;
			AssertNoError(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoIsNotRequired);

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "~US";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(usCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DisbursementCreditor.PK.ToGuid());
			addInfo.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			addInfo.ZO_AccountNo = ZString.Empty;
			AssertNoError("Current Organisation is Customs Disbursement Creditor", addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoIsMandatory);

			addInfo.ZO_AccountNo = "123456";
			AssertHasWarning(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoShouldBeEnteredInRegistry);

			addInfo.ZO_AccountNo = ZString.Empty;
			AssertNoWarning(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoShouldBeEnteredInRegistry);
		}

		public void TestCheckZO_BrokerToPay()
		{
			ValidateAndAssertBrokerToPay(addInfo.ZO_BrokerToPayInfo);
		}

		public void TestCheckZO_ReconBrokerToPay()
		{
			ValidateAndAssertBrokerToPay(addInfo.ZO_ReconBrokerToPayInfo);
		}

		void ValidateAndAssertBrokerToPay(ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = new ZString("!");
			AssertHasMessageError(propertyInfo, ListValidation.InvalidCodeMessageError);

			propertyInfo.Value = new ZString("");
			AssertNoMessageError(propertyInfo, ListValidation.InvalidCodeMessageError);
			AssertNoWarningContaining(propertyInfo, USOrgImpAddInfoValidation.BrokerToPayShouldBeSet);

			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			propertyInfo.Value = new ZString("");
			AssertNoMessageError(propertyInfo, ListValidation.InvalidCodeMessageError);
			AssertHasWarningContaining(propertyInfo, USOrgImpAddInfoValidation.BrokerToPayShouldBeSet);

			propertyInfo.Value = (ZString)YesNoDefaultList.Codes.No;
			AssertNoWarningContaining(propertyInfo, USOrgImpAddInfoValidation.BrokerToPayShouldBeSet);

			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			propertyInfo.Value = (ZString)YesNoDefaultList.Codes.No;
			AssertNoWarningContaining(propertyInfo, USOrgImpAddInfoValidation.BrokerToPayShouldBeSet);
		}

		public void TestCheckZO_ProducerFirmType()
		{
			addInfo.ZO_ProducerFirmType = "~";
			AssertHasMessageError(addInfo.ZO_ProducerFirmTypeInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_ProducerFirmType = ProducerFirmTypeList.Codes.G;
			AssertNoMessageError(addInfo.ZO_ProducerFirmTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZO_GB()
		{
			GlbCompany auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			GlbBranch branch = auCompany.Branches.AddNew();

			addInfo.ZO_GB = branch.PK;
			AssertHasError(addInfo.ZO_GBInfo, USOrgImpAddInfoValidation.BIRDBranchIsNotUS);

			addInfo.ZO_GB = CargoWise.Types.ZGuid.Empty;
			AssertNoError(addInfo.ZO_GBInfo, USOrgImpAddInfoValidation.BIRDBranchIsNotUS);

			addInfo.ZO_GB = GlbBranch.CurrentBranch.PK;
			AssertNoError(addInfo.ZO_GBInfo, USOrgImpAddInfoValidation.BIRDBranchIsNotUS);

			var prCompany = Factory.New<GlbCompany>();
			prCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;

			var prBranch = prCompany.Branches.AddNew();
			addInfo.ZO_GB = prBranch.PK;
			AssertNoError(addInfo.ZO_GBInfo, USOrgImpAddInfoValidation.BIRDBranchIsNotUS);
		}

		public void TestCheckZO_SubmitterFirmType()
		{
			addInfo.ZO_SubmitterFirmType = "~";
			AssertHasMessageError(addInfo.ZO_SubmitterFirmTypeInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_SubmitterFirmType = SubmitterFirmTypeList.Codes.S;
			AssertNoMessageError(addInfo.ZO_SubmitterFirmTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZO_IsEINNumberVerifiedIndicator()
		{
			addInfo.ZO_IsEINNumberVerifiedIndicator = "D";
			AssertHasMessageError(addInfo.ZO_IsEINNumberVerifiedIndicatorInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_IsEINNumberVerifiedIndicator = addInfo.Lookups.ZO_YesNoList[0].Code;
			AssertNoMessageError(addInfo.ZO_IsEINNumberVerifiedIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZO_MFRRegExempt()
		{
			addInfo.ZO_MFRRegExempt = "~";
			AssertHasMessageError(addInfo.ZO_MFRRegExemptInfo, USOrgImpAddInfoValidation.MFRRegExemptNotInList);

			addInfo.ZO_MFRRegExempt = addInfo.Lookups.FDAPriorNoticeExemptCodeList[0].Code;
			AssertNoMessageError(addInfo.ZO_MFRRegExemptInfo, USOrgImpAddInfoValidation.MFRRegExemptNotInList);
		}

		public void TestCheckZO_SPDNumberOfDays()
		{
			addInfo.ZO_SPDNumberOfDays = -1;
			AssertHasError(addInfo.ZO_SPDNumberOfDaysInfo, USOrgImpAddInfoValidation.NumberOfDaysCannotBeNegative);

			addInfo.ZO_SPDNumberOfDays = 5;
			AssertNoError(addInfo.ZO_SPDNumberOfDaysInfo, USOrgImpAddInfoValidation.NumberOfDaysCannotBeNegative);

			addInfo.ZO_SPDNumberOfDays = 11;
			AssertHasError(addInfo.ZO_SPDNumberOfDaysInfo, USOrgImpAddInfoValidation.NumberOfDaysCannotBeGreaterThan10);

			addInfo.ZO_SPDNumberOfDays = 10;
			AssertNoError(addInfo.ZO_SPDNumberOfDaysInfo, USOrgImpAddInfoValidation.NumberOfDaysCannotBeGreaterThan10);

			DefaultStatementPrintDate statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			addInfo.ZO_SPDNumberOfDays = 8;
			AssertHasWarning(addInfo.ZO_SPDNumberOfDaysInfo, USOrgImpAddInfoValidation.OrgNumberOfDaysDoesNotDiffer);

			addInfo.ZO_SPDNumberOfDays = 5;
			AssertNoWarning(addInfo.ZO_SPDNumberOfDaysInfo, USOrgImpAddInfoValidation.OrgNumberOfDaysDoesNotDiffer);
		}

		public void TestCheckZO_DoNotAutoGenerateSDCR()
		{
			var autoGenerate = new AutoSendStatementDateChangeRequest
			{
				OverrideAllOrByOrganisation = "ALL"
			};

			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, autoGenerate);
			addInfo.ZO_DoNotAutoGenerateSDCR = true;
			AssertHasWarning(addInfo.ZO_DoNotAutoGenerateSDCRInfo, "Automatic generation of Statement Date Change Requests is currently set in the system registry to apply for ALL organisations. Setting this indicator here is currently superfluous.");

			autoGenerate = new AutoSendStatementDateChangeRequest();
			autoGenerate.OverrideAllOrByOrganisation = "ORG";

			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, autoGenerate);
			addInfo.ZO_DoNotAutoGenerateSDCR = true;
			AssertNoWarnings(addInfo.ZO_DoNotAutoGenerateSDCRInfo);
		}

		public void TestAutoGenerateSDCRAcceptsCompanyLevelFallBack()
		{
			addInfo.ZO_DoNotAutoGenerateSDCR = true;
			AssertHasWarning(addInfo.ZO_DoNotAutoGenerateSDCRInfo, "Automatic generation of Statement Date Change Requests is currently off by default in the system registry. Setting this indicator here is currently superfluous");

			var autoGenerate = new AutoSendStatementDateChangeRequest
			{
				OverrideAllOrByOrganisation = "ALL"
			};

			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, autoGenerate);
			addInfo.ZO_DoNotAutoGenerateSDCR = true;
			AssertHasWarning(addInfo.ZO_DoNotAutoGenerateSDCRInfo, "Automatic generation of Statement Date Change Requests is currently set in the system registry to apply for ALL organisations. Setting this indicator here is currently superfluous.");

			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, autoGenerate);
			addInfo.ZO_DoNotAutoGenerateSDCR = true;
			AssertHasWarning(addInfo.ZO_DoNotAutoGenerateSDCRInfo, "Automatic generation of Statement Date Change Requests is currently set in the system registry to apply for ALL organisations. Setting this indicator here is currently superfluous.");

			((IRegistryItemInternals)USCustomsDataRegistry.Instance.AutoSendSDCR).DeleteValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			autoGenerate.OverrideAllOrByOrganisation = "ORG";
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, autoGenerate);
			addInfo.ZO_DoNotAutoGenerateSDCR = true;
			AssertNoNotifications(addInfo.ZO_DoNotAutoGenerateSDCRInfo);
		}

		public void TestCheckZO_PaymentType()
		{
			addInfo.ZO_PaymentType = "A";
			AssertHasMessageErrors(addInfo.ZO_PaymentTypeInfo);

			addInfo.ZO_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertNoMessageErrors(addInfo.ZO_PaymentTypeInfo);
		}

		public void TestCheckZO_TaxDeferredInd()
		{
			addInfo.ZO_TaxDeferredInd = "6";
			AssertHasMessageErrors(addInfo.ZO_TaxDeferredIndInfo);

			addInfo.ZO_TaxDeferredInd = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			AssertNoMessageErrors(addInfo.ZO_TaxDeferredIndInfo);
		}

		public void TestCheckZO_OtherReconIndicator()
		{
			addInfo.ZO_OtherReconIndicator = "~";
			AssertHasMessageError(addInfo.ZO_OtherReconIndicatorInfo, USOrgImpAddInfoValidation.OtherReconIndicatorNotInList);

			addInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			AssertNoMessageError(addInfo.ZO_OtherReconIndicatorInfo, USOrgImpAddInfoValidation.OtherReconIndicatorNotInList);
		}

		public void TestCheckZO_AccountNo()
		{
			addInfo.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			addInfo.ZO_AccountNo = "123";
			AssertHasError(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoLength);

			addInfo.ZO_AccountNo = "123008";
			AssertNoError(addInfo.ZO_AccountNoInfo, ValidationConstants.Statement.PayerUnitNoLength);
		}

		public void TestNoLongerValidCodes()
		{
			addInfo.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.G;
			AssertHasWarning(addInfo.ZO_MFRRegExemptInfo, ValidationConstants.PriorNotice.FoodFacilityReasonNowInvalid);

			addInfo.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.F;
			AssertNoWarning(addInfo.ZO_TaxDeferredIndInfo, ValidationConstants.PriorNotice.FoodFacilityReasonNowInvalid);

			addInfo.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.I;
			AssertEquals(true, addInfo.ZO_MFRRegExemptInfo.HasNotifications());
			addInfo.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.J;
			AssertEquals(true, addInfo.ZO_MFRRegExemptInfo.HasNotifications());
			addInfo.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.L;
			AssertEquals(true, addInfo.ZO_MFRRegExemptInfo.HasNotifications());
			addInfo.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.M;
			AssertEquals(true, addInfo.ZO_MFRRegExemptInfo.HasNotifications());
			addInfo.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.O;
			AssertEquals(true, addInfo.ZO_MFRRegExemptInfo.HasNotifications());

			addInfo.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.A;
			AssertEquals(false, addInfo.ZO_MFRRegExemptInfo.HasNotifications());
		}

		public void TestCheckZO_FIRMS()
		{
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			addInfo.organization.OH_Code = "111";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "A001", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			addInfo.ZO_FIRMS = "~";
			AssertHasMessageError(addInfo.ZO_FIRMSInfo, ListValidation.InvalidCodeMessageError);
			addInfo.ZO_FIRMS = "A001";
			AssertNoMessageError(addInfo.ZO_FIRMSInfo, ListValidation.InvalidCodeMessageError);
		}

		OrgHeader DisbursementCreditor
		{
			get { return disbursementCreditor ?? (disbursementCreditor = CreateCreditorOrgHeader("~o~")); }
		}
		OrgHeader disbursementCreditor;

		OrgHeader CreateCreditorOrgHeader(string code)
		{
			organisation.OH_FullName = "Test Company Name";
			organisation.MainAddress.OA_Address1 = "184 Bourke Road";
			organisation.MainAddress.OA_City = "Alexandria";
			organisation.MainAddress.OA_State = "NSW";
			organisation.OH_Code = "Z" + code;

			organisation.OH_IsCreditor = true;
			organisation.CompanyData.SetAPTaxApplicable(true);
			organisation.MiscServ.OM_APWHTApplicable = true;

			Factory.Save();
			return organisation;
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			addInfo = new OrgImpAddInfo((ZPropertyInfoString)organisation.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
		}
		OrgImpAddInfo addInfo;
		OrgHeader organisation;
	}
}
