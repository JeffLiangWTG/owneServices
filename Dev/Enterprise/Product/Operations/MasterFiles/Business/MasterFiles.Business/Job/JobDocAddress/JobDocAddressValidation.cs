using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressValidation : AutoJobDocAddressValidation
	{
		public JobDocAddressValidation(AutoJobDocAddress parent)
			: base(parent)
		{
			phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());
		}

		public new JobDocAddress Parent
		{
			get { return (JobDocAddress)base.Parent; }
		}

		readonly AddressValidation AddressValidation = new AddressValidation();

		protected virtual bool IsAddressOverridenValidationEnabled
		{
			get { return Parent.E2_AddressOverride; }
		}

		#region CheckIsActive

		void CheckIsActive(ZPropertyInfo info, BusinessObjectCollection col, ZGuid value)
		{
			if (!info.HasWarnings())
			{
				bool isActive = false;
				foreach (BusinessObject obj in col)
				{
					if (obj.PK == value)
					{
						isActive = true;
						break;
					}
				}
				if (!isActive)
				{
					info.AddWarning(Res.GetString("4c8be09d-8aa9-4b4f-a26d-a5d86b73b911", "This Contact is inactive."));
				}
			}
		}
		#endregion

		#region E2_OA_Address

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			if (!Parent.E2_AddressOverride)
			{
				var requirement = Parent.Requirement;
				if (requirement != null && requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address != null)
				{
					requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address(this);
				}

				if (!Parent.E2_OA_AddressInfo.HasErrors() && Parent.OrganisationPK.IsValid)
				{
					MandatoryValidation.CheckEntered(Parent.E2_OA_AddressInfo);
				}
			}
		}

		#endregion

		#region E2_Contact

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();

			if (Parent.Organisation != null && Parent.Contact != null)
			{
				CheckIsActive(Parent.E2_ContactInfo, Parent.Organisation.ContactsActive, Parent.ContactPK);
			}

			var requirement = Parent.Requirement;
			if (requirement != null && requirement.ValidateContact != null)
			{
				requirement.ValidateContact(this);
			}
		}

		#endregion

		#region OrganisationPK

		public void ValidateOrganisationPK()
		{
			ValidateCalculatedProperty(Parent.OrganisationPKInfo);
		}

		protected virtual void CheckOrganisationPK()
		{
			if (Parent.IsDeleted)
			{
				using (((IBusinessObjectInternals)Parent).SuppressReportRowDeletedError())
				{
					if (!Parent.OrganisationPK.IsEmpty)
					{
						Parent.OrganisationPKInfo.AddError(Res.GetString("8fddcc30-a53c-4a9a-b763-e3894e456002",
							"This field has been previously cleared and an underlying record has been marked for deletion. Please reopen this form to enter new value."));
					}
				}
				return;
			}

			if (!Parent.E2_AddressOverride)
			{
				TypeValidation.CheckValidGuid(Parent.OrganisationPKInfo, Res.GetString("24dd4199-986f-438d-9871-b50abb16badd", "selection"));
			}

			if (Parent.Requirement != null)
			{
				if (Parent.Requirement.ValidateOrganisationPK != null)
				{
					Parent.Requirement.ValidateOrganisationPK(this);
				}

				if (Parent.Requirement.IsMandatory && !Parent.IsValidAddress && !Parent.OrganisationPKInfo.HasErrors())
				{
					if (Parent.Requirement.ValidateOrganisationPKUponMandatoryRequirement != null)
					{
						Parent.Requirement.ValidateOrganisationPKUponMandatoryRequirement(this);
					}
					else
					{
						Parent.OrganisationPKInfo.AddError(Res.GetString("86bc4ad8-5d3b-4e55-89c4-8e3c75c3fc00", "Please specify a {0} for this job.", Parent.AddressCaption));
					}
				}
			}

			if (Parent.Organisation != null)
			{
				if (!Parent.Organisation.OH_IsActive)
				{
					Parent.OrganisationPKInfo.AddError(Res.GetString("8742629b-2499-43c4-820a-2b745a2ed400", "This Organization is not active."));
				}

				if (Parent.Organisation.OH_Code == "UNMATCHED")
				{
					Parent.OrganisationPKInfo.Add(UnmatchedOrgNotificationType, Res.GetString("db6744cf-09a0-461e-a779-625173adbb6a", "We recommend selecting an existing Organization or creating a new Organization with the details in the '{0}' note.", PredefinedNoteTypes.Instance.UnmatchedOrgDetails));
				}
			}
		}

		protected virtual INotificationType UnmatchedOrgNotificationType => CargoWise.ComponentModel.NotificationType.Warning;

		#endregion

		#region OrganisationNameOrPK

		public void ValidateOrganisationNameOrPK()
		{
			ValidateCalculatedProperty(Parent.OrganisationNameOrPKInfo);
		}

		protected virtual void CheckOrganisationNameOrPK()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				ValidateE2_CompanyName();
				Parent.OrganisationNameOrPKInfo.AddAllNotificationsFrom(Parent.E2_CompanyNameInfo);
			}
			else
			{
				ValidateOrganisationPK();
				Parent.OrganisationNameOrPKInfo.AddAllNotificationsFrom(Parent.OrganisationPKInfo);
			}
		}

		#endregion

		#region ContactPK

		public void ValidateContactPK()
		{
			ValidateCalculatedProperty(Parent.ContactPKInfo);
		}

		protected virtual void CheckContactPK()
		{
			if (!Parent.E2_AddressOverride)
			{
				TypeValidation.CheckValidGuid(Parent.ContactPKInfo);
			}
		}

		#endregion

		#region E2_AddressType

		protected override void CheckE2_AddressType()
		{
			base.CheckE2_AddressType();
			if (Parent.Requirement != null && Parent.Requirement.ValidateAddressType != null)
			{
				Parent.Requirement.ValidateAddressType(this);
			}
			else
			{
				if (!Parent.E2_AddressTypeInfo.HasErrors())
				{
					MandatoryValidation.CheckEntered(Parent.E2_AddressTypeInfo);
					if (Parent.E2_AddressType == DocAddressTypes.None)
					{
						Parent.E2_AddressTypeInfo.AddWarning(Res.GetString("90ceb64d-93c2-41c1-b04e-5008ec5bfb85", "The Address Type is of type (nothing)"));
					}
					else if (!DocAddressTypes.GetCodes(Parent.Factory).Contains(Parent.E2_AddressType))
					{
						Parent.E2_AddressTypeInfo.AddError(Res.GetString("0445934d-b488-4441-9eb7-574332bf55fa", "The Address Type is invalid"));
					}
				}
			}
		}

		#endregion

		#region ResidentialCommercialAddressType

		public void ValidateResidentialCommercialAddressType()
		{
			base.ValidateCalculatedProperty(Parent.ResidentialCommercialAddressTypeInfo);
		}

		protected virtual void CheckResidentialCommercialAddressType()
		{
			var addressTypeError = ResString.GetMultilingualString("9F1386E1-5F04-4F4D-B90E-AB13C7107D10", "You must enter a valid Address Type. Either COM for Commercial, or RES for Residential.");
			MandatoryValidation.CheckEntered(Parent.ResidentialCommercialAddressTypeInfo);
			ListValidation.ErrorIfInvalidCode(addressTypeError, Parent.ResidentialCommercialAddressTypeInfo);
		}

		#endregion

		#region E2_AdditionalAddressInformation

		protected override void CheckE2_AdditionalAddressInformation()
		{
			base.CheckE2_AdditionalAddressInformation();

			if (IsAddressOverridenValidationEnabled)
			{
				if (!Parent.E2_AdditionalAddressInformationInfo.HasErrors())
				{
					base.CheckE2_AdditionalAddressInformation();

					if (Parent.UnrestrictedAdditionalAddressInformation.Length > Parent.E2_AdditionalAddressInformationInfo.MaxLength)
					{
						var message = Res.GetString(
							"a1641bb6-3803-4f0a-8b66-9939f61fe85f",
							"This field contains text which is longer than allowed. Please shorten the text to {0} letters or less before trying to save the address again.",
							Parent.E2_AdditionalAddressInformationInfo.MaxLength);

						Parent.E2_AdditionalAddressInformationInfo.AddError(message);
					}
				}
			}

			if (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.Value
				&& !Parent.E2_AddressOverride
				&& Parent.HasRealAddress
				&& Parent.UnrestrictedAdditionalAddressInformationInfo == Parent.E2_AdditionalAddressInformationInfo
				&& !(Parent.Address?.AdditionalInfos.Any(info => info.OAI_AdditionalInfo.EqualsIgnoringCase(Parent.E2_AdditionalAddressInformation)) ?? false)
				&& !(Parent.Organisation?.OH_OverrideAdditionalAddressInformation ?? false))
			{
				Parent.E2_AdditionalAddressInformationInfo.AddError(Res.GetString("6303C837-4CF6-43D4-AE5A-065C888CC482", "The organization does not allow additional information override"));
			}
		}

		#endregion

		#region E2_Address1

		protected override void CheckE2_Address1()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				base.CheckE2_Address1();
				var parent = this.Parent;
				if (!parent.E2_Address1Info.HasErrors())
				{
					var requirement = parent.Requirement;
					if (requirement != null && requirement.ValidateAddress1 != null)
					{
						requirement.ValidateAddress1(this);
					}
					else
					{
						MandatoryValidation.CheckEntered(parent.E2_Address1Info);
					}
				}
			}
		}

		#endregion

		#region E2_Address2

		protected override void CheckE2_Address2()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				base.CheckE2_Address2();
				if (!Parent.E2_Address2Info.HasErrors())
				{
					var requirement = Parent.Requirement;
					if (requirement != null && requirement.ValidateAddress2 != null)
					{
						requirement.ValidateAddress2(this);
					}
				}
			}
		}

		#endregion

		#region E2_Address1AndE2_Address2

		public void ValidateE2_Address1AndE2_Address2()
		{
			ValidateCalculatedProperty(Parent.E2_Address1AndE2_Address2Info);
		}

		protected virtual void CheckE2_Address1AndE2_Address2()
		{
			var parent = Parent;
			if (IsAddressOverridenValidationEnabled && parent.E2_Address1.IsEmpty)
			{
				parent.E2_Address1AndE2_Address2Info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("3345804E-2BD9-4201-988A-85E4DB491594", "Address")));
			}
		}

		#endregion

		#region E2_City

		protected override void CheckE2_City()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				base.CheckE2_City();
				var parent = this.Parent;
				if (!parent.E2_CityInfo.HasErrors())
				{
					var requirement = parent.Requirement;
					if (requirement != null && requirement.ValidateCity != null)
					{
						requirement.ValidateCity(this);
					}
					else
					{
						var country = parent.Country;
						if ((country == null || !OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country.PK.ToGuid(), Parent.ValidationSection)) && RawDataRegistry.Instance.JobAddressValidation_CityMandatory.Value)
						{
							MandatoryValidation.CheckEntered(parent.E2_CityInfo);
						}
						AddressValidation.CheckPostcodeViaCity(parent.E2_CityInfo, parent.E2_PostcodeInfo, country);
					}
				}
			}
		}

		#endregion

		#region E2_PostCode

		protected override void CheckE2_Postcode()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				base.CheckE2_Postcode();
				var requirement = Parent.Requirement;
				if (requirement != null && requirement.ValidatePostCode != null)
				{
					requirement.ValidatePostCode(this);
				}
				else
				{
					if (RawDataRegistry.Instance.JobAddressValidation_UsePostcodeRules.Value)
					{
						AddressValidation.CheckPostCode(Parent.E2_PostcodeInfo, Parent.Country, Parent.ValidationSection, Parent.ValidationStatus);
					}
				}
			}
		}

		#endregion

		#region E2_State

		protected override void CheckE2_State()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				base.CheckE2_State();
				var requirement = Parent.Requirement;
				if (requirement != null && requirement.ValidateState != null)
				{
					requirement.ValidateState(this);
				}
				else
				{
					if (RawDataRegistry.Instance.JobAddressValidation_UseStateRules.Value)
					{
						AddressValidation.CheckState(Parent.E2_StateInfo, Parent.Country, Parent.ValidationSection, Parent.ValidationStatus);
					}
				}
			}
		}

		#endregion

		#region E2_RN_NKCountryCode

		protected override void CheckE2_RN_NKCountryCode()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				base.CheckE2_RN_NKCountryCode();
				var parent = Parent;

				var requirement = parent.Requirement;
				if (requirement != null && requirement.ValidateCountry != null)
				{
					requirement.ValidateCountry(this);
				}
				else
				{
					if (RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.Value)
					{
						MandatoryValidation.CheckEntered(parent.E2_RN_NKCountryCodeInfo);
					}
				}

				if (!parent.E2_RN_NKCountryCode.IsEmpty && !parent.E2_RN_NKCountryCodeInfo.HasMessageErrors())
				{
					ListValidation.ErrorIfInvalidCode(parent.E2_RN_NKCountryCodeInfo);
				}
			}
		}

		#endregion

		#region E2_CompanyName

		protected override void CheckE2_CompanyName()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				base.CheckE2_CompanyName();
				var parent = this.Parent;
				if (!parent.E2_CompanyNameInfo.HasErrors())
				{
					var requirement = parent.Requirement;
					if (requirement != null && requirement.ValidateCompanyName != null)
					{
						requirement.ValidateCompanyName(this);
					}
					else
					{
						MandatoryValidation.CheckEntered(parent.E2_CompanyNameInfo, Res.GetString("667d5f49-69f6-4a19-915b-da0459a4e01e", "Company Name, or remove the override for this Address"));
					}
				}
			}
		}

		#endregion

		#region E2_Email

		[BusinessObjectTestExclude()] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		protected override void CheckE2_Email()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				base.CheckE2_Email();
				AddressValidation.CheckEmail(Parent.E2_EmailInfo);

				var requirement = Parent.Requirement;
				if (requirement != null && requirement.ValidateEmail != null)
				{
					requirement.ValidateEmail(this);
				}
			}
		}

		#endregion

		#region E2_AddressOverride

		protected override void CheckE2_AddressOverride()
		{
			base.CheckE2_AddressOverride();
			var parent = this.Parent;

			if (parent.E2_AddressOverride && parent.Parent != null)
			{
				SecurityCheckpoint checkpoint = parent.Parent.GetCanOverrideCheckpoint(parent);
				if (checkpoint != null && !checkpoint.IsAllowed)
				{
					parent.E2_AddressOverrideInfo.AddError(checkpoint.ErrorMessageForNotAllowed);
				}
			}

			var requirement = parent.Requirement;
			if (requirement != null && requirement.ValidateDelegateToBeFiredUponValidationOf_E2_AddressOverride != null)
			{
				// This will fire ALL of the registered handlers, hurrah! 
				requirement.ValidateDelegateToBeFiredUponValidationOf_E2_AddressOverride(this);
			}

			if (!parent.E2_AddressOverride)
			{
				ValidateE2_CompanyName();
				ValidateE2_Address1();
				ValidateE2_City();
				ValidateE2_Postcode();
				ValidateE2_RN_NKCountryCode();
				ValidateE2_State();
				ValidateE2_GovRegNum();
				ValidateE2_Email();
				ValidateE2_Fax();
				ValidateE2_Phone();
			}

			ValidateOrganisationPK();
		}

		#endregion

		#region CheckE2_GovRegNumType

		protected override void CheckE2_GovRegNumType()
		{
			using (Parent.CacheRegistrationNumberFromRequirement())
			{
				base.CheckE2_GovRegNumType();
				if (IsAddressOverridenValidationEnabled)
				{
					var requirement = Parent.Requirement;
					if (requirement != null && requirement.ValidateGovRegNumType != null)
					{
						requirement.ValidateGovRegNumType(this);
					}
				}
				ValidateE2_GovRegNum();
			}
		}

		#endregion

		#region E2_GovRegNum

		protected override void CheckE2_GovRegNum()
		{
			using (Parent.CacheRegistrationNumberFromRequirement())
			{
				base.CheckE2_GovRegNum();
				if (IsAddressOverridenValidationEnabled)
				{
					var requirement = Parent.Requirement;
					if (requirement != null && requirement.ValidateGovRegNo != null)
					{
						requirement.ValidateGovRegNo(this);
					}
				}
				ValidateE2_PassportDetails();
				ValidateE2_GovRegNumType();
			}
		}

		#endregion

		#region E2_PassportDetails

		public void ValidateE2_PassportDetails()
		{
			ValidateCalculatedProperty(Parent.E2_PassportDetailsInfo);
		}

		protected void CheckE2_PassportDetails()
		{
			ValidateE2_PassportID();
			ValidateE2_PassportCountryOfIssue();
			ValidateE2_PassportDateOfBirth();
			var parent = this.Parent;
			if (parent.IsPassportIDGovRegNumType)
			{
				parent.E2_PassportDetailsInfo.AddAllNotificationsFrom(parent.E2_PassportIDInfo);
				parent.E2_PassportDetailsInfo.AddAllNotificationsFrom(parent.E2_PassportCountryOfIssueInfo);
				parent.E2_PassportDetailsInfo.AddAllNotificationsFrom(parent.E2_PassportDateOfBirthInfo);
			}
		}

		#endregion

		#region E2_PassportID

		public void ValidateE2_PassportID()
		{
			ValidateCalculatedProperty(Parent.E2_PassportIDInfo);
		}

		protected void CheckE2_PassportID()
		{
			if (!Parent.IsRowDeletedOrDetachedOrNull && Parent.IsPassportDataOverridable)
			{
				var requirement = Parent.Requirement;
				if (requirement != null && requirement.ValidatePassportID != null)
				{
					requirement.ValidatePassportID(this);
				}
				ValidateE2_PassportDetails();
			}
		}

		#endregion

		#region E2_PassportCountryOfIssue

		public void ValidateE2_PassportCountryOfIssue()
		{
			ValidateCalculatedProperty(Parent.E2_PassportCountryOfIssueInfo);
		}

		protected void CheckE2_PassportCountryOfIssue()
		{
			var parent = this.Parent;
			if (parent.IsPassportDataOverridable)
			{
				var requirement = parent.Requirement;
				if (requirement != null && requirement.ValidatePassportCountryOfIssue != null)
				{
					requirement.ValidatePassportCountryOfIssue(this);
				}
				ValidateE2_PassportDetails();
			}
		}

		#endregion

		#region E2_PassportDateOfBirth

		public void ValidateE2_PassportDateOfBirth()
		{
			ValidateCalculatedProperty(Parent.E2_PassportDateOfBirthInfo);
		}

		protected void CheckE2_PassportDateOfBirth()
		{
			var parent = this.Parent;
			if (parent.IsPassportDataOverridable)
			{
				var requirement = parent.Requirement;
				if (requirement != null && requirement.ValidatePassportDateOfBirth != null)
				{
					requirement.ValidatePassportDateOfBirth(this);
				}
				ValidateE2_PassportDetails();
			}
		}

		#endregion

		#region E2_ValidationStatus

		public static string ValidationStatusNotVerifiedMessage => Res.GetString("JobDocAddressValidation|ValidationStatus", "This Booking must have valid Addresses entered for all Instructions.");

		protected override void CheckE2_ValidationStatus()
		{
			base.CheckE2_ValidationStatus();

			if (Parent.E2_AddressOverride
				&& ShouldValidateAddress()
				&& Parent.E2_ValidationStatus == AddressValidationStatus.Invalid
				&& !Parent.IgnoreValidationStatusError)
			{
				var message = Res.GetString(
					"c0d525b2-e739-4795-9d17-d454aa3579f7",
					"There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				if (ShouldSuppressError() || ShouldSuppressErrorForAddressTypeAndController())
				{
					Parent.E2_ValidationStatusInfo.AddWarning(message);
				}
				else
				{
					Parent.E2_ValidationStatusInfo.AddError(message);
				}
			}

			if (Parent.Parent is IDtbBookingInstruction instruction && instruction.Booking != null && instruction.Booking.IsSendingXUSToCTO)
			{
				if (Parent.ValidationStatus != AddressValidationStatus.Verified)
				{
					Parent.E2_ValidationStatusInfo.AddMessageError(ValidationStatusNotVerifiedMessage);
				}
			}
		}

		protected virtual bool ShouldSuppressError()
		{
			return Parent.E2_AddressOverride &&
				(Parent.E2_SuppressAddressValidationError && OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled);
		}

		bool ShouldSuppressErrorForAddressTypeAndController()
		{
			var controllerBizO = Parent.Parent;

			var result = false;
			var isNotRunningFromServiceTask = string.IsNullOrEmpty(Env.Instance.ServiceTaskCode);

			if (isNotRunningFromServiceTask && controllerBizO != null)
			{
				var orgDataReg = OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.Value
					.Cast<AddressListElement>()
					.GroupBy(item => item.ControllerName)
					.ToDictionary(g => g.Key, g => g.Select(item => item.AddressType.ToString()).ToHashSet());

				if (orgDataReg.Count > 0)
				{
					if (orgDataReg.TryGetValue(Constants.AVSRegistryConstants.ControllerNames.All, out var addressTypes))
					{
						result = addressTypes.Contains(Constants.AVSRegistryConstants.AddressTypes.All) || addressTypes.Contains(Parent.DocAddressType.ToString());
					}

					if (!result)
					{
						var controllerName = ObjectFactory.Get<IControllerNameFinder>().GetControllerNameForType(controllerBizO.GetType());
						if (controllerName != null && orgDataReg.TryGetValue(controllerName, out addressTypes))
						{
							result = addressTypes.Contains(Constants.AVSRegistryConstants.AddressTypes.All) || addressTypes.Contains(Parent.DocAddressType.ToString());
						}
					}
				}
			}

			return result;
		}

		bool ShouldValidateAddress()
		{
			return
				!Globals.IsWeb &&
				Parent.Country != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Parent.Country.PK.ToGuid(), Parent.ValidationSection);
		}

		#endregion

		#region E2_ScreeningStatus

		protected override void CheckE2_ScreeningStatus()
		{
			base.CheckE2_ScreeningStatus();
			MandatoryValidation.CheckEntered(Parent.E2_ScreeningStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.E2_ScreeningStatusInfo, Parent.Lookups.ScreeningStatusesList);
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return base.ShouldValidateFKToCancelledRecord(info) && info.Name != JobDocAddressSchema.Constants.E2_ParentID;
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			using (Parent.CacheRegistrationNumberFromRequirement())
			{
				ValidateOrganisationPK();
				base.ValidateAll();
				ValidateContactPK();
				ValidateOrganisationNameOrPK();
				ValidateE2_PassportCountryOfIssue();
				ValidateE2_PassportDateOfBirth();
				ValidateE2_PassportID();
				ValidateE2_PassportDetails();
				ValidateE2_ValidationStatus();
				ValidateResidentialCommercialAddressType();
			}
		}

		#endregion

		#region Phone Numbers

		void ValidatePhoneNumber(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty)
		{
			PhoneNumberFormatterAndValidator.Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, Parent.DefaultCountryCodeForPhoneNumbers);
		}

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk;

		#region E2_Mobile_Formatted

		public void ValidateE2_Mobile_Formatted()
		{
			ValidateCalculatedProperty(Parent.E2_Mobile_FormattedInfo);
		}

		protected virtual void CheckE2_Mobile_Formatted()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				ValidatePhoneNumber(Parent.E2_Mobile_FormattedInfo, Parent.E2_MobileInfo, Parent.E2_Mobile_IsManuallyVerifiedInfo);

				var requirement = Parent.Requirement;
				if (requirement != null && requirement.ValidateMobileFormatted != null)
				{
					requirement.ValidateMobileFormatted(this);
				}
			}
		}

		#endregion

		#region E2_Fax_Formatted

		public void ValidateE2_Fax_Formatted()
		{
			ValidateCalculatedProperty(Parent.E2_Fax_FormattedInfo);
		}

		protected virtual void CheckE2_Fax_Formatted()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				ValidatePhoneNumber(Parent.E2_Fax_FormattedInfo, Parent.E2_FaxInfo, Parent.E2_Fax_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region E2_Phone_Formatted

		public void ValidateE2_Phone_Formatted()
		{
			ValidateCalculatedProperty(Parent.E2_Phone_FormattedInfo);
		}

		protected virtual void CheckE2_Phone_Formatted()
		{
			if (IsAddressOverridenValidationEnabled)
			{
				var parent = Parent;
				var requirement = parent.Requirement;
				if (requirement != null && requirement.ValidatePhoneFormatted != null)
				{
					requirement.ValidatePhoneFormatted(this);
				}
				ValidatePhoneNumber(Parent.E2_Phone_FormattedInfo, Parent.E2_PhoneInfo, Parent.E2_Phone_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#endregion
	}
}
