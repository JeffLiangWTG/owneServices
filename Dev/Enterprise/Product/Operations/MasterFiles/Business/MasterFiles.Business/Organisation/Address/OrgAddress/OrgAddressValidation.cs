using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAddressValidation : AutoOrgAddressValidation
	{
		public OrgAddressValidation(AutoOrgAddress parent)
			: base(parent)
		{
			phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());
		}

		public new OrgAddress Parent
		{
			get { return (OrgAddress)base.Parent; }
		}

		AddressValidation addressValidation;
		AddressValidation AddressValidation => addressValidation ?? (addressValidation = new AddressValidation());

		#region OA_CompanyNameOverride

		protected override void CheckOA_CompanyNameOverride()
		{
			base.CheckOA_CompanyNameOverride();
			CheckEnglishCharactersForEnglishAddresses(Parent.OA_CompanyNameOverrideInfo);
			CheckAWBFormat(Parent.OA_CompanyNameOverrideInfo, AWBCharacterSet.Text);
		}

		#endregion

		#region OA_Address1

		protected override void CheckOA_Address1()
		{
			base.CheckOA_Address1();
			var info = Parent.OA_Address1Info;
			if (!info.HasErrors())
			{
				MandatoryValidation.CheckEntered(info);
			}
			if (!info.HasErrors())
			{
				if (Parent.OA_Address1.Length < OrgAddress.Schema.OA_Address1MinimumLength)
				{
					info.AddError(Res.GetString("D07508F1-5006-4A46-8EB4-A70FF36995B1", "Address 1 must have at least {0} characters.", OrgAddress.Schema.OA_Address1MinimumLength));
				}
			}
			CheckEnglishCharactersForEnglishAddresses(info);
			CheckAWBFormat(info, AWBCharacterSet.Text);
			CheckTSAKnown(info);
			CheckMID(info);
		}

		#endregion

		#region OA_Address2

		protected override void CheckOA_Address2()
		{
			base.CheckOA_Address2();
			var info = Parent.OA_Address2Info;
			var header = Parent.Header;
			if ((!ShouldValidateAddress() || !AddressValidation.IsWebVerified(Parent.ValidationStatus)) && header != null && header.RequiredFieldsForOrg.RequireAddress2)
			{
				MandatoryValidation.CheckEntered(info);
			}
			CheckEnglishCharactersForEnglishAddresses(info);
			CheckAWBFormat(info, AWBCharacterSet.Text);
			CheckTSAKnown(info);
			CheckMID(info);
		}

		#endregion

		#region OA_City

		protected override void CheckOA_City()
		{
			base.CheckOA_City();
			var header = Parent.Header;
			var info = Parent.OA_CityInfo;

			if ((!ShouldValidateAddress() || !AddressValidation.IsWebVerified(Parent.ValidationStatus)) && header != null && header.RequiredFieldsForOrg.RequireCity)
			{
				MandatoryValidation.CheckEntered(info);
			}

			CheckEnglishCharactersForEnglishAddresses(info);
			CheckAWBFormat(info, AWBCharacterSet.Text);
			CheckTSAKnown(info);
			CheckMID(info);
		}

		#endregion

		#region OA_JobLoadingDuration

		protected override void CheckOA_JobLoadingDuration()
		{
			base.CheckOA_JobLoadingDuration();
			if (Parent.OA_JobLoadingDuration < 0)
			{
				Parent.OA_JobLoadingDurationInfo.AddError(Res.GetString("5862190D-E250-46B4-B8FA-790C9155D770", "Job loading during timespan should be greater than or equal to zero."));
			}
		}

		#endregion

		#region OA_State

		protected override void CheckOA_State()
		{
			base.CheckOA_State();
			var info = Parent.OA_StateInfo;

			RefCountry country = Parent.Country;
			AddressValidation.CheckState(info, country, Parent.ValidationSection, Parent.ValidationStatus);

			CheckEnglishCharactersForEnglishAddresses(info);
			CheckAWBFormat(info, AWBCharacterSet.Text);
			CheckTSAKnown(info);
			CheckMID(info);
		}

		#endregion

		#region OA_RN_NKCountryCode

		protected override void CheckOA_RN_NKCountryCode()
		{
			base.CheckOA_RN_NKCountryCode();
			var info = Parent.OA_RN_NKCountryCodeInfo;

			if (!info.HasErrors())
			{
				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info);
			}

			CheckAWBFormat(info, AWBCharacterSet.Alpha);
			CheckTSAKnown(info);
			CheckMID(info);
		}

		#endregion

		#region OA_Postcode

		protected override void CheckOA_PostCode()
		{
			base.CheckOA_PostCode();
			var info = Parent.OA_PostCodeInfo;

			RefCountry country = Parent.Country ?? (Parent.RelatedPortCode != null ? Parent.RelatedPortCode.Country : null);
			AddressValidation.CheckPostCode(info, country, Parent.ValidationSection, Parent.ValidationStatus);

			CheckAWBFormat(info, AWBCharacterSet.Text);
			CheckTSAKnown(info);
			CheckMID(info);
		}

		#endregion

		#region OA_Language

		protected override void CheckOA_Language()
		{
			base.CheckOA_Language();
			var info = Parent.OA_LanguageInfo;
			CheckNoDuplicateLocalAddress();
			if (!info.HasErrors())
			{
				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info);

				if (!Parent.IsEnglishOnlyOrEmpty && Parent.IsEnglish)
				{
					info.AddError(Res.GetString("4efc0518-1228-456c-9ecb-ac26e2d31f1c", "Non-English Characters detected in this address. Please select the proper language for this address."));
				}

				if (Parent.IsMainAddress && !Parent.IsEnglish && Parent.Header.IsOrgTypeInvolvedInOverseasTransactions)
				{
					info.AddWarning(Res.GetString("a9071496-052b-4872-b56b-9537fdebd73b", "The main address of an organization that is involved in overseas dealings (Consignee, Consignor, Shipping Provider, Forwarder) should be English as this is the main language accepted in the logistics and forwarding industry."));
				}

				foreach (OrgAddressCapabilityWrapper capability in Parent.AddressCapability)
				{
					capability.ValidateMain();
				}
			}
		}

		void CheckNoDuplicateLocalAddress()
		{
			Parent.CheckNoDuplicateLocalAddress(Parent, Parent.OA_LanguageInfo);
		}

		#endregion

		#region OA_DeliveryRoute

		protected override void CheckOA_DeliveryRoute()
		{
			base.CheckOA_DeliveryRoute();
			var info = Parent.OA_DeliveryRouteInfo;
			if (!info.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(info);
			}
		}

		#endregion

		#region OA_DeliveryRouteSequence

		protected override void CheckOA_DeliveryRouteSequence()
		{
			base.CheckOA_DeliveryRoute();
			if (!Parent.OA_DeliveryRoute.IsEmpty && Parent.OA_DeliveryRouteSequence < 1)
			{
				Parent.OA_DeliveryRouteSequenceInfo.AddError((Res.GetString("C263FBCB-C04B-413E-9B70-8367D63F69F8", "Delivery Route Sequence number must be greater than 0.")));
			}
			else if (Parent.OA_DeliveryRoute.IsEmpty && Parent.OA_DeliveryRouteSequence != 0)
			{
				Parent.OA_DeliveryRouteSequenceInfo.AddError((Res.GetString("EF9C40CF-43A9-42A5-A071-DCB7B3EEB609", "Delivery Route must be specified to have a Delivery Route Sequence.")));
			}
		}

		#endregion

		#region OA_AuthorityToLeave

		protected override void CheckOA_AuthorityToLeave()
		{
			base.CheckOA_AuthorityToLeave();
			var info = Parent.OA_AuthorityToLeaveInfo;
			ListValidation.ErrorIfInvalidCode(info);
			MandatoryValidation.CheckEntered(info);
		}

		#endregion

		#region OA_Code

		protected override void CheckOA_Code()
		{
			base.CheckOA_Code();
			var info = Parent.OA_CodeInfo;
			if (!info.HasErrors())
			{
				MandatoryValidation.CheckEntered(info);
				var header = Parent.Header;
				if (header != null && !header.HasUniqueAddressCode(Parent))
				{
					info.AddError(shortCodeNotUnique);
				}
			}

			CheckEnglishCharactersForEnglishAddresses(Parent.OA_CodeInfo);
		}

		#endregion

		#region OA_Email

		[BusinessObjectTestExclude()] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		protected override void CheckOA_Email()
		{
			base.CheckOA_Email();
			AddressValidation.CheckEmail(Parent.OA_EmailInfo);

			var header = Parent.Header;
			if (header != null && Parent.IsMainAddress)
			{
				if (header.RequiredFieldsForOrg.RequireEmailAddress)
				{
					MandatoryValidation.CheckEntered(Parent.OA_EmailInfo);
				}
				else if (header.RequiredFieldsForOrg.RequireFaxEmailOrWeb)
				{
					if (Parent.OA_Fax.IsEmpty && Parent.OA_Email.IsEmpty && header.MainWebURL.PU_URL.IsEmpty)
					{
						Parent.OA_EmailInfo.AddError(Res.GetString("edeb58e9-a2ca-43e4-a4fa-aed845801426", "Either a fax number, email Address or web Address must be entered for this organization."));
					}
					else
					{
						if (Parent.OA_Fax.IsEmpty && Parent.OA_FaxInfo.HasErrors())
						{
							Parent.Validation.ValidateOA_Fax();
						}
						if (header.MainWebURL.PU_URL.IsEmpty && header.MainWebURL.PU_URLInfo.HasErrors())
						{
							header.MainWebURL.Validation.ValidatePU_URL();
						}
					}
				}
			}
		}

		#endregion

		#region OA_Phone_Formatted

		public void ValidateOA_Phone_Formatted()
		{
			ValidateCalculatedProperty(Parent.OA_Phone_FormattedInfo);
		}

		protected virtual void CheckOA_Phone_Formatted()
		{
			var header = Parent.Header;
			if (header != null)
			{
				if (!Parent.IgnorePhoneNumberError)
				{
					if (Parent.PK == header.MainAddress.PK)
					{
						if (header.RequiredFieldsForOrg.RequirePhoneNumber)
						{
							MandatoryValidation.CheckEntered(Parent.OA_Phone_FormattedInfo);
						}
						if (header.PhoneOrBusinessNumberRequiredForOrg && Parent.OA_Phone_Formatted.IsEmpty && header.PrimaryRegistrationNumber.Number.IsEmpty)
						{
							Parent.OA_Phone_FormattedInfo.AddError(Res.GetString("ddf3ccb0-94cb-40e6-8112-48f55949e3c1", "Either a Phone number or the Business Registration number must be entered for this organization."));
						}
					}
					ValidatePhoneNumber(Parent.OA_Phone_FormattedInfo, Parent.OA_PhoneInfo, Parent.OA_Phone_IsManuallyVerifiedInfo);
				}
			}
		}

		#endregion

		#region OA_Mobile_Formatted

		public void ValidateOA_Mobile_Formatted()
		{
			ValidateCalculatedProperty(Parent.OA_Mobile_FormattedInfo);
		}

		protected virtual void CheckOA_Mobile_Formatted()
		{
			ValidatePhoneNumber(Parent.OA_Mobile_FormattedInfo, Parent.OA_MobileInfo, Parent.OA_Mobile_IsManuallyVerifiedInfo);
		}

		#endregion

		#region OA_Fax_Formatted

		public void ValidateOA_Fax_Formatted()
		{
			ValidateCalculatedProperty(Parent.OA_Fax_FormattedInfo);
		}

		protected virtual void CheckOA_Fax_Formatted()
		{
			var header = Parent.Header;
			if (header != null)
			{
				if (!Parent.IgnoreFaxNumberError)
				{
					if (Parent.PK == header.MainAddress.PK)
					{
						if (header.RequiredFieldsForOrg.RequireFaxNumber)
						{
							MandatoryValidation.CheckEntered(Parent.OA_Fax_FormattedInfo);
						}
						else if (header.RequiredFieldsForOrg.RequireFaxEmailOrWeb)
						{
							if (Parent.OA_Fax_Formatted.IsEmpty && Parent.OA_Email.IsEmpty && header.MainWebURL.PU_URL.IsEmpty)
							{
								Parent.OA_Fax_FormattedInfo.AddError(Res.GetString("edeb58e9-a2ca-43e4-a4fa-aed845801426", "Either a fax number, email Address or web Address must be entered for this organization."));
							}
							else
							{
								if (Parent.OA_Email.IsEmpty && Parent.OA_EmailInfo.HasErrors())
								{
									Parent.Validation.ValidateOA_Email();
								}
								if (header.MainWebURL.PU_URL.IsEmpty && header.MainWebURL.PU_URLInfo.HasErrors())
								{
									header.MainWebURL.Validation.ValidatePU_URL();
								}
							}
						}
					}
					ValidatePhoneNumber(Parent.OA_Fax_FormattedInfo, Parent.OA_FaxInfo, Parent.OA_Fax_IsManuallyVerifiedInfo);
				}
			}
		}

		#endregion

		#region OA_RL_NKRelatedPortCode

		protected override void CheckOA_RL_NKRelatedPortCode()
		{
			ValidateOA_RL_NKRelatedPortCode();

			var info = Parent.OA_RL_NKRelatedPortCodeInfo;
			if (Parent.IsMainAddress)
			{
				MandatoryValidation.CheckEntered(info);
			}

			ListValidation.ErrorIfInvalidCode(info);
			if (Parent.IsMainAddress && Parent.Header != null && !Parent.OA_RL_NKRelatedPortCode.IsEmpty && !Parent.Header.OH_RL_NKClosestPort.IsEmpty && Parent.OA_RL_NKRelatedPortCode != Parent.Header.OH_RL_NKClosestPort)
			{
				Parent.OA_RL_NKRelatedPortCodeInfo.AddError(OrgHeaderValidationReal.OH_RL_NKClosestPortMismatchMessage);
			}
		}

		#endregion

		#region OA_AIREquipmentNeeded

		protected override void CheckOA_AIREquipmentNeeded()
		{
			base.CheckOA_AIREquipmentNeeded();
			if (!DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				var info = Parent.OA_AIREquipmentNeededInfo;
				ListValidation.ErrorIfInvalidCode(info);
				MandatoryValidation.CheckEntered(info);
			}
		}

		#endregion

		#region OA_FCLEquipmentNeeded

		protected override void CheckOA_FCLEquipmentNeeded()
		{
			base.CheckOA_FCLEquipmentNeeded();
			if (!DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				var info = Parent.OA_FCLEquipmentNeededInfo;
				ListValidation.ErrorIfInvalidCode(info);
				MandatoryValidation.CheckEntered(info);
			}
		}

		#endregion

		#region OA_LCLEquipmentNeeded

		protected override void CheckOA_LCLEquipmentNeeded()
		{
			base.CheckOA_LCLEquipmentNeeded();
			if (!DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				var info = Parent.OA_LCLEquipmentNeededInfo;
				ListValidation.ErrorIfInvalidCode(info);
				MandatoryValidation.CheckEntered(info);
			}
		}

		#endregion

		#region OA_IsActive

		protected override void CheckOA_IsActive()
		{
			base.CheckOA_IsActive();
			if (Parent.Header != null && Parent.Header.AddressesActive.Count == 0)
			{
				Parent.OA_IsActiveInfo.AddError(Res.GetString("54c170ed-24d8-4c9f-a37f-bd5896e261d9", "Organization should have at least one active address."));
			}

			if (!Parent.OA_IsActive && (Parent.CustomsCodes?.Any(o => o.OK_OA_PremisesAddress == Parent.PK) ?? false))
			{
				Parent.OA_IsActiveInfo.AddError(Res.GetString("918CB830-21DB-4FE5-BFAC-4179B57A9588", "Addresses set in 'Registration Numbers / Codes' cannot be inactive."));
			}

			if (!Parent.OA_IsActive && Parent.IsMainAddress)
			{
				Parent.OA_IsActiveInfo.AddError(Res.GetString("37d5c84f-2ddf-44b6-a6d0-33e3d4cd3e35", "Main address cannot be inactive."));
			}
		}

		#endregion

		#region OA_ValidationStatus

		protected override void CheckOA_ValidationStatus()
		{
			base.CheckOA_ValidationStatus();

			if (ShouldValidateAddress() && Parent.OA_ValidationStatus == AddressValidationStatus.Invalid && !Parent.IgnoreValidationStatusError)
			{
				var shouldSuppressError =
					Parent.OA_SuppressAddressValidationError &&
					OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled || Parent.IsTemporaryOrgAddress;

				var message = Res.GetString(
					"5b1cccd7-c2fc-4083-9010-4a6e7efb198f",
					"There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				if (shouldSuppressError)
				{
					Parent.OA_ValidationStatusInfo.AddWarning(message);
				}
				else
				{
					Parent.OA_ValidationStatusInfo.AddError(message);
				}
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOA_ValidationStatus();
			ValidateOA_Phone_Formatted();
			ValidateOA_Mobile_Formatted();
			ValidateOA_Fax_Formatted();
		}

		void CheckTSAKnown(ZPropertyInfo info)
		{
			if (info.HasChanges && Parent.IsTSAKnownAddress)
			{
				info.AddWarning(Res.GetString("21DD36A9-A242-4481-A6FF-321063E24D0E", "The address is a US TSA Known Shipper address. Changing it may cause TSA record inconsistency. All previous approved TSA Known Shipper linked to this address will be changed to not approved when you click the save button. Refer to the TSA Known Shipper tab which shows the details that have been last reviewed with TSA."));
			}
		}

		void CheckMID(ZPropertyInfo info)
		{
			if (info.HasChanges && Parent.IsMIDAddress)
			{
				info.AddWarning(Res.GetString("8AA56B56-9FA4-4253-86BF-E493F20BEA3D", "An address related to a Manufacturer ID Number (MID) has been modified. The change to the address may affect the MID number."));
			}
		}

		#region Implementation

		readonly ZString shortCodeNotUnique = Res.GetString("619c3f16-0eae-4367-943c-f0a198cb26e0", "The Address Short Code must be unique for each Address (whether active or inactive) within an organization.");

		PhoneNumberFormatAndValidation phoneNumberValidator;

		PhoneNumberFormatAndValidation PhoneNumberValidator
		{
			get
			{
				if (phoneNumberValidator == null)
				{
					phoneNumberValidator = new PhoneNumberFormatAndValidation();
				}

				return phoneNumberValidator;
			}
		}

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk;

		protected void CheckPhoneNumber(ZPropertyInfo numberToCheck)
		{
			if (Parent.Header != null)
			{
				var relatedPortCode = Parent.RelatedPortCode;
				RefUNLOCO homePort = relatedPortCode ?? Parent.Header.ClosestPort;
				PhoneNumberValidator.PerformNumberValidation(numberToCheck, homePort, true);
			}
		}

		void ValidatePhoneNumber(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty)
		{
			PhoneNumberFormatterAndValidator.Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, !Parent.DefaultCountryCodeForPhoneNumbers.IsEmpty ? Parent.DefaultCountryCodeForPhoneNumbers : (Parent.Header != null ? Parent.Header.CountryCode : ZString.Empty));
		}

		void CheckEnglishCharactersForEnglishAddresses(ZPropertyInfo info)
		{
			if (Parent.IsEnglish)
			{
				EnglishStrictCharactersValidation.ErrorIfNotEnglish(info);
			}
		}

		bool ShouldValidateAddress()
		{
			return
				Parent.OA_IsActive &&
				Parent.Country != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Parent.Country.PK.ToGuid(), Parent.ValidationSection) &&
				!HasNoPermissionToModifyAddress();
		}

		bool HasNoPermissionToModifyAddress()
		{
			var webAddressValidation = (ISupportWebAddressValidation)Parent;
			return webAddressValidation.GetReadOnlySecurity(Parent.OA_Address1Info) &&
				webAddressValidation.GetReadOnlySecurity(Parent.OA_Address2Info) &&
				webAddressValidation.GetReadOnlySecurity(Parent.OA_CityInfo) &&
				webAddressValidation.GetReadOnlySecurity(Parent.OA_PostCodeInfo) &&
				webAddressValidation.GetReadOnlySecurity(Parent.OA_StateInfo) &&
				webAddressValidation.GetReadOnlySecurity(Parent.OA_RN_NKCountryCodeInfo);
		}

		#region AWB

		enum AWBCharacterSet
		{
			Alpha,
			Text
		}

		void CheckAWBFormat(ZPropertyInfo info, AWBCharacterSet format)
		{
			var value = (ZString)info.Value;

			if (!value.IsEmpty && Parent.AddressCapability.GetCapabilityEnabled(OrgAddressType.AWB.Code))
			{
				if (!Regex.IsMatch(value, GetAWBRegex(format)))
				{
					info.AddError(GetAWBErrorMessage(format));
				}
				else if (!IsValidAWBAddressFormat(info))
				{
					info.AddError(Res.GetString("7c09cebd-56de-4bb2-972f-7c484ea8aa84", "{0} is not valid for AWB address.", info.HumanReadableName));
				}
			}
		}

		bool IsValidAWBAddressFormat(ZPropertyInfo info)
		{
			var value = (ZString)info.Value;

			if (info.Name == Parent.OA_CompanyNameOverrideInfo.Name
					|| info.Name == Parent.OA_Address1Info.Name
					|| info.Name == Parent.OA_Address2Info.Name
					|| info.Name == Parent.OA_CityInfo.Name)
			{
				var stringValue = value.ToString();

				return stringValue != Core.Constants.AWB.AWBAddressNotAllowed.ToBeAnnounced
					&& stringValue.ToCharArray().Any(x => x != '.')
					&& stringValue.ToUpper(CultureInfo.InvariantCulture) != "X"
					&& stringValue != "-";
			}
			else if (info.Name == Parent.OA_StateInfo.Name)
			{
				return value.KeepAlphanumericCharacters().Length >= 1;
			}
			else if (info.Name == Parent.OA_PostCodeInfo.Name)
			{
				return value.KeepAlphanumericCharacters().Length >= 1;
			}

			return true;
		}

		string GetAWBRegex(AWBCharacterSet format)
		{
			switch (format)
			{
				case AWBCharacterSet.Alpha:
					return (NoResString)@"^[a-zA-Z]*$";
				case AWBCharacterSet.Text:
					return (NoResString)@"^[a-zA-Z0-9\-\. ]*$";
				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unexpected format {0}", format));
			}
		}

		string GetAWBErrorMessage(AWBCharacterSet format)
		{
			var charactersAllowed = "";

			switch (format)
			{
				case AWBCharacterSet.Alpha:
					charactersAllowed = Res.GetString("e699b81f-42cd-4d7d-9045-f927394ff82b", "alphabetical characters");
					break;
				case AWBCharacterSet.Text:
					charactersAllowed = Res.GetString("7d60165a-4eb9-4b13-b07b-8fe15059aa9a", "alphanumeric characters and prescribed special characters");
					break;
				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unexpected format {0}", format));
			}

			return Res.GetString("ada40094-d265-4285-bf47-11ceef86d034", "This address is marked as an AWB address, and must only contain {0} in this field.", charactersAllowed);
		}

		#endregion

		#endregion
	}
}
