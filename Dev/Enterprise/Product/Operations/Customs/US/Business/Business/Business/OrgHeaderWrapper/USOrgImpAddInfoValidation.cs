//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSOrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSOrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USOrgImpAddInfoValidation : AutoUSOrgImpAddInfoValidation
	{
		public USOrgImpAddInfoValidation(AutoUSOrgImpAddInfo parent)
			: base(parent)
		{
		}

		new OrgImpAddInfo Parent
		{
			get { return (OrgImpAddInfo)base.Parent; }
		}

		protected override void CheckZO_ImporterType()
		{
			base.CheckZO_ImporterType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_ImporterTypeInfo, Parent.Lookups.ZO_ImporterTypeList);
		}

		protected override void CheckZO_ProducerFirmType()
		{
			base.CheckZO_ProducerFirmType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_ProducerFirmTypeInfo, Parent.Lookups.ProducerFirmTypes);
		}

		protected override void CheckZO_SubmitterFirmType()
		{
			base.CheckZO_SubmitterFirmType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_SubmitterFirmTypeInfo, Parent.Lookups.SubmitterFirmTypes);
		}

		protected override void CheckZO_IsEINNumberVerifiedIndicator()
		{
			base.CheckZO_IsEINNumberVerifiedIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_IsEINNumberVerifiedIndicatorInfo, Parent.Lookups.ZO_YesNoList);
		}

		protected override void CheckZO_SPDNumberOfDays()
		{
			base.CheckZO_SPDNumberOfDays();

			if (!Parent.ZO_SPDNumberOfDays.IsEmpty)
			{
				if (Parent.ZO_SPDNumberOfDays < 0)
				{
					Parent.ZO_SPDNumberOfDaysInfo.AddError(NumberOfDaysCannotBeNegative);
				}
				else if (Parent.ZO_SPDNumberOfDays > 10)
				{
					Parent.ZO_SPDNumberOfDaysInfo.AddError(NumberOfDaysCannotBeGreaterThan10);
				}
				else
				{
					int systemNoOfDaysToAdd = USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty).NumberOfDays;
					if (Parent.ZO_SPDNumberOfDays == systemNoOfDaysToAdd)
					{
						Parent.ZO_SPDNumberOfDaysInfo.AddWarning(OrgNumberOfDaysDoesNotDiffer);
					}
				}
			}
		}
		internal const string NumberOfDaysCannotBeNegative = "Statement Print Date Number of Days cannot be negative.";
		internal const string NumberOfDaysCannotBeGreaterThan10 = "Number of Days cannot be more than 10 days, as Statement would then be considered overdue.";
		internal const string OrgNumberOfDaysDoesNotDiffer = "Organisation override of Statement Print Date Number of Days does not differ from System wide Registry value.";

		protected override void CheckZO_PayMethod()
		{
			base.CheckZO_PayMethod();

			ListValidation.ErrorIfInvalidCode(Parent.ZO_PayMethodInfo, Parent.Lookups.PayMethodList);

			if (Parent.ZO_PayMethod == ACHPaymentTypeList.Codes.ImporterCheck && !PaymentTypeList.IsPaidByImporter(Parent.ZO_PaymentType))
			{
				Parent.ZO_PayMethodInfo.AddError(ImporterCheckWhilePaymentTypeIndicatesBrokerPays);
			}

			ValidateZO_AccountNo();
		}
		internal const string ImporterCheckWhilePaymentTypeIndicatesBrokerPays = "This option is only valid for payment type, 3, 5, 7, 8.";

		protected override void CheckZO_DoNotAutoGenerateSDCR()
		{
			base.CheckZO_DoNotAutoGenerateSDCR();

			if (Parent.ZO_DoNotAutoGenerateSDCR)
			{
				var autoSendSDCR = USCustomsDataRegistry.Instance.AutoSendSDCR.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty).OverrideAllOrByOrganisation;
				if (autoSendSDCR.IsEmpty)
				{
					Parent.ZO_DoNotAutoGenerateSDCRInfo.AddWarning("Automatic generation of Statement Date Change Requests is currently off by default in the system registry. Setting this indicator here is currently superfluous");
				}
				else if (autoSendSDCR == "ALL")
				{
					Parent.ZO_DoNotAutoGenerateSDCRInfo.AddWarning("Automatic generation of Statement Date Change Requests is currently set in the system registry to apply for ALL organisations. Setting this indicator here is currently superfluous.");
				}
			}
		}

		protected override void CheckZO_PaymentType()
		{
			base.CheckZO_PaymentType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_PaymentTypeInfo, Parent.Lookups.PaymentTypes);
			ValidateZO_PayMethod();
			ValidateZO_AccountNo();
		}

		protected override void CheckZO_BrokerToPay()
		{
			base.CheckZO_BrokerToPay();
			ValidateBrokerToPay(Parent.ZO_BrokerToPayInfo, "");
		}

		void ValidateBrokerToPay(ZPropertyInfo propertyInfo, ZString message)
		{
			ListValidation.MessageErrorIfInvalidCode(propertyInfo, Parent.Lookups.ZO_YesNoList);
			if (!Parent.ZO_PaymentType.IsEmpty && propertyInfo.Value.IsEmpty)
			{
				propertyInfo.AddWarning(string.Format("{0} ", message).Trim() + BrokerToPayShouldBeSet);
			}
		}
		internal const string BrokerToPayShouldBeSet = "Broker To Pay should be indicated. This is required for Billing.";

		protected override void CheckZO_KnwImpInd()
		{
			base.CheckZO_KnwImpInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_KnwImpIndInfo, Parent.Lookups.ZO_YesNoList);
		}

		protected override void CheckZO_ReconFilingPort()
		{
			base.CheckZO_ReconFilingPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_ReconFilingPortInfo, Parent.Lookups.ReconPorts);
		}

		protected override void CheckZO_ReconBrokerToPay()
		{
			base.CheckZO_ReconBrokerToPay();
			ValidateBrokerToPay(Parent.ZO_ReconBrokerToPayInfo, "Reconciliation");
		}

		protected override void CheckZO_TaxDeferredInd()
		{
			base.CheckZO_TaxDeferredInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_TaxDeferredIndInfo, Parent.Lookups.TaxDeferredIndicators);
		}

		protected override void CheckZO_MFRRegExempt()
		{
			base.CheckZO_MFRRegExempt();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_MFRRegExemptInfo, Parent.Lookups.FDAPriorNoticeExemptCodeList, (NoResString)MFRRegExemptNotInList);

			if (ExemptionNoLongerValid)
			{
				Parent.ZO_MFRRegExemptInfo.AddWarning(ValidationConstants.PriorNotice.FoodFacilityReasonNowInvalid);
			}
		}
		internal const string MFRRegExemptNotInList = "The code you have selected for MFR Reg. Exemption is not in FDA Prior Notice Exemption List.";

		bool ExemptionNoLongerValid
		{
			get
			{
				return Parent.ZO_MFRRegExempt == FDAPriorNoticeExemptCodeList.Codes.G ||
					Parent.ZO_MFRRegExempt == FDAPriorNoticeExemptCodeList.Codes.I ||
					Parent.ZO_MFRRegExempt == FDAPriorNoticeExemptCodeList.Codes.J ||
					Parent.ZO_MFRRegExempt == FDAPriorNoticeExemptCodeList.Codes.L ||
					Parent.ZO_MFRRegExempt == FDAPriorNoticeExemptCodeList.Codes.M ||
					Parent.ZO_MFRRegExempt == FDAPriorNoticeExemptCodeList.Codes.O;
			}
		}

		protected override void CheckZO_OtherReconIndicator()
		{
			base.CheckZO_OtherReconIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_OtherReconIndicatorInfo, Parent.Lookups.OtherReconIssueList, (NoResString)OtherReconIndicatorNotInList);
		}
		internal const string OtherReconIndicatorNotInList = "The entered value is not a valid Recon. Issue Code.";

		protected override void CheckZO_AccountNo()
		{
			base.CheckZO_AccountNo();

			if (Parent.ZO_PayMethod == ACHPaymentTypeList.Codes.ACHDebit)
			{
				var parent = Parent;
				var isCurrentOrgCustomsDisbursementCreditor = parent.organization.IsCustomsDisbursementCreditor();

				if (isCurrentOrgCustomsDisbursementCreditor && !Parent.ZO_AccountNo.IsEmpty)
				{
					Parent.ZO_AccountNoInfo.AddWarning(ValidationConstants.Statement.PayerUnitNoShouldBeEnteredInRegistry);
				}
				else if (!isCurrentOrgCustomsDisbursementCreditor && Parent.ZO_AccountNo.IsEmpty && Parent.IsPaidByImporter)
				{
					Parent.ZO_AccountNoInfo.AddError(ValidationConstants.Statement.PayerUnitNoIsMandatory);
				}

				if (!Parent.ZO_AccountNo.IsEmpty && Parent.ZO_AccountNo.Length != 6)
				{
					Parent.ZO_AccountNoInfo.AddError(ValidationConstants.Statement.PayerUnitNoLength);
				}
			}
			else if (!Parent.ZO_AccountNo.IsEmpty)
			{
				Parent.ZO_AccountNoInfo.AddError(ValidationConstants.Statement.PayerUnitNoIsNotRequired);
			}
		}

		protected override void CheckZO_GB()
		{
			base.CheckZO_GB();

			GlbBranch branch = Parent.Factory.Load<GlbBranch>(Parent.ZO_GB);

			if (branch != null && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(branch.Company.GC_RN_NKCountryCode) != Core.Constants.CountryCodes.UnitedStates)
			{
				Parent.ZO_GBInfo.AddError(BIRDBranchIsNotUS);
			}
		}
		public const string BIRDBranchIsNotUS = "You should select a US/PR branch in this field.";

		protected override void CheckZO_FIRMS()
		{
			base.CheckZO_FIRMS();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_FIRMSInfo, Parent.Lookups.FIRMSList);
		}

		protected override void CheckZO_OH_NP()
		{
			base.CheckZO_OH_NP();
			if (!Parent.ZO_OH_NP.IsEmpty && Parent.ZO_NPID.IsEmpty)
			{
				Parent.ZO_OH_NPInfo.AddMessageError(_4811PartyMustHaveID);
			}
		}
		internal const string _4811PartyMustHaveID = "This organization does not have EIN/CBN/SSN.";

		protected override void CheckZO_NPID()
		{
			base.CheckZO_NPID();
			if (!Parent.ZO_NPID.IsEmpty && !EmployerIdentificationNumberValidator.IsValidEIN(Parent.ZO_NPID) &&
				!SocialSecurityNumberValidator.IsValidSSN(Parent.ZO_NPID) &&
				!CBPAssignedNumberValidator.IsValidCBPAssignedNumber(Parent.ZO_NPID))
			{
				if (OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.Value.GetBoolFromCode(OrganisationRegistry.RegistrationNumberFormatFields.EINCBNSSN4811PartyID))
				{
					Parent.ZO_NPIDInfo.AddError(Invalid4811PartyIDFormat);
				}
				else
				{
					Parent.ZO_NPIDInfo.AddWarning(Invalid4811PartyIDFormat);
				}
			}
		}
		internal const string Invalid4811PartyIDFormat = "4811 Notify Party ID is not in a valid format. It may be in a form of EIN, CBN, or SSN.";
	}
}
