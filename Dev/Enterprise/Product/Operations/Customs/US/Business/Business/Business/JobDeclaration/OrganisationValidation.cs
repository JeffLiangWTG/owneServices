using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public enum OrgMatchedCustomsRegNoType { EIN, MID, ECN, FEI, NULL }

	public class OrganisationValidation
	{
		public OrganisationValidation(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public static void ValidatePGAContact(ZPropertyInfo propertyInfo, IPGAContactDetails wrapper)
		{
			if (wrapper != null)
			{
				ValidatePGAContact(propertyInfo, ContactInformationForCustomsIsMissing, wrapper.Name, wrapper.PhoneNumber, wrapper.EmailAddress, wrapper.Fax);
			}
		}

		public static void ValidatePGAContact(ZPropertyInfo propertyInfo, GlbStaff staff)
		{
			if (staff != null)
			{
				ValidatePGAContact(propertyInfo, StaffInformationIsMissing, staff.GS_FullName, staff.GS_WorkPhone, staff.GS_EmailAddress, staff.GS_FaxNum);
			}
		}

		internal const string StaffInformationIsMissing = "Broker {0} is required for PGA reporting. Please enter Broker {0} on the related staff record.";
		internal const string ContactInformationForCustomsIsMissing = "Contact {0} for Customs is required for PGA reporting. Please add Contact {0} as USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact.";
		internal const string ContactName = "Name";
		internal const string WorkPhone = "Work Phone";
		internal const string Email_Fax = "Email/Fax";

		static void ValidatePGAContact(ZPropertyInfo propertyInfo, ZString messageErrorText, ZString name, ZString phone, ZString email, ZString fax)
		{
			ValidatePGAContactDetails(propertyInfo, ContactName, name, null, true, messageErrorText);
			ValidatePGAContactDetails(propertyInfo, WorkPhone, phone, null, true, messageErrorText);
			ValidatePGAContactDetails(propertyInfo, Email_Fax, email, () => string.IsNullOrWhiteSpace(fax), true, messageErrorText);
		}

		public static void ValidatePGAContactDetails(ZPropertyInfo propertyInfo, ZString propertyDescriptor, ZString value, Func<bool> getAdditionalCondition, bool hasRealAddress, string messageErrorText = ContactInformationForCustomsIsMissing)
		{
			var additionalCondition = true;
			if (getAdditionalCondition != null)
			{
				additionalCondition = getAdditionalCondition();
			}

			if (string.IsNullOrWhiteSpace(value) && additionalCondition)
			{
				var messageError = hasRealAddress ? string.Format(messageErrorText, propertyDescriptor) : MandatoryValidation.YouHaveNotEnteredMessage(propertyDescriptor);
				propertyInfo.AddMessageError(messageError);
			}
		}

		public void ValidateSCACCodeForOrganisation(ZPropertyInfo organisationInfo, ZString transportMode)
		{
			if (!organisationInfo.Value.IsEmpty && transportMode != Core.Constants.TransportModes.Air && transportMode != Core.Constants.TransportModes.Mail)
			{
				ValidateCustomsRegNoForOrganisation(organisationInfo, OrgCusCode.CodeTypes.CarrierCode, NoSCACForCarrier);
			}
		}

		internal const string NoSCACForCarrier = "There is no Standard Carrier Alpha Code (SCAC) entered for this organisation.\nTo add a SCAC Code, please select F3 > Config > Registration Numbers/Codes.\nThe SCAC Type = CCC in the drop down list.";
		const int RegistrationNumberLength = 12;

		public static void ValidateMatchedCustomsRegoNoForOrganisation(ZPropertyInfo organisationOrAddressInfo, OrgMatchedCustomsRegNoType customsRegNumberType, ZString missingRegoNumberMessage, bool forceEntered, bool shouldBeRegisteredInCustomsAsImporter)
		{
			var organisation = organisationOrAddressInfo.BizObj.Factory.Load<OrgHeader>((ZGuid)organisationOrAddressInfo.Value);

			if (organisation == null)
			{
				var orgAddress = organisationOrAddressInfo.BizObj.Factory.Load<OrgAddress>((ZGuid)organisationOrAddressInfo.Value);
				organisation = orgAddress != null ? orgAddress.Header : null;
			}

			if (organisation == null)
			{
				if (forceEntered)
				{
					MandatoryValidation.MessageErrorIfNotEntered(organisationOrAddressInfo);
				}
			}
			else if (customsRegNumberType != OrgMatchedCustomsRegNoType.NULL)
			{
				ZString regoNo = ZString.Empty;

				if (customsRegNumberType == OrgMatchedCustomsRegNoType.EIN)
				{
					regoNo = organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber);

					if (!regoNo.IsEmpty && regoNo.StartsWith("-"))
					{
						organisationOrAddressInfo.AddMessageError(string.Format(EINLikeAsEntryptedNo, regoNo));
					}
					else if (regoNo.Length > RegistrationNumberLength)
					{
						organisationOrAddressInfo.AddMessageError(string.Format(RegisrationNumberShouldLessThanLengthLimit, regoNo, RegistrationNumberLength));
					}
				}
				else if (customsRegNumberType == OrgMatchedCustomsRegNoType.ECN)
				{
					regoNo = organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.USACodeTypes.EncryptedConsigneeNumber);
				}
				else if (customsRegNumberType == OrgMatchedCustomsRegNoType.MID)
				{
					ErrorReporter.ReportOnce("MID requested against organisation", "MID is requested against organisation. It is an OrgAddress level information.");
				}

				if (regoNo.IsEmpty)
				{
					organisationOrAddressInfo.AddMessageError(missingRegoNumberMessage);
				}
				else if (shouldBeRegisteredInCustomsAsImporter)
				{
					ValidateOrganisationRegisteredInCustoms(organisationOrAddressInfo, organisation);
				}
			}
			else if (shouldBeRegisteredInCustomsAsImporter)
			{
				var einNumber = organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber);
				if (!einNumber.IsEmpty)
				{
					ValidateOrganisationRegisteredInCustoms(organisationOrAddressInfo, organisation);
				}
			}
		}
		internal const string RegisrationNumberShouldLessThanLengthLimit = "The Importer Number, '{0}', should be less than {1}";
		internal const string EINLikeAsEntryptedNo = "The Importer Number, '{0}' starts with '-', which is in an Encrypted Number format. It  may be used only for Cargo Release. Please enter a correct Importer Number or change the type for this number to 'ECN'.";

		public static void ValidateOrganisationRegisteredInCustoms(ZPropertyInfo organisationInfo, OrgHeader organisation)
		{
			ValidateOrganisationRegisteredInCustoms(organisationInfo, organisation, OrganisationShouldBeRegisteredInCustoms);
		}

		public static void ValidateOrganisationRegisteredInCustoms(ZPropertyInfo organisationInfo, OrgHeader organisation, string message)
		{
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(organisation);

			if (wrapper != null)
			{
				if (wrapper.ZO_IsEINNumberVerifiedIndicator == YesNoDefaultList.Codes.No)
				{
					organisationInfo.AddMessageError(message);
				}
				else if (wrapper.ZO_IsEINNumberVerifiedIndicator == ZString.Empty)
				{
					organisationInfo.AddWarning(message);
				}
			}
		}
		public const string OrganisationShouldBeRegisteredInCustoms = "This organization may not be on the Customs File. The organization can be added to the Customs File by sending a message from the Customs Messaging menu on the Organization Form. Alternatively, if this organization has previously been added to the Customs File, please indicate this on the Organization Record (Organization > Customs Messaging > Importer/Consignee File (CBPF-5106) Add).";

		public void ValidateCustomsRegNoForOrganisation(ZPropertyInfo organisationInfo, ZString codeType, ZString messageToAddIfNotExist)
		{
			if (organisationInfo.Value.IsValid)
			{
				OrgHeader organisation = factory.Load<OrgHeader>((ZGuid)organisationInfo.Value);

				if (organisation != null &&
					organisation.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.UnitedStates).IsEmpty)
				{
					organisationInfo.AddMessageError(messageToAddIfNotExist);
				}
			}
		}

		public static void ValidateManufacturerIDForMainAddress(ZPropertyInfo organisationInfo, bool forceEntered, CargoWise.ComponentModel.INotificationType notificationType)
		{
			OrgHeader org = organisationInfo.BizObj.Factory.Load<OrgHeader>((ZGuid)organisationInfo.Value);
			ValidateManufacturerIDForAddress(org == null ? null : org.MainAddress, organisationInfo, forceEntered, notificationType);
		}

		public static void ValidateManufacturerIDForAddress(ZPropertyInfo addressInfo, bool forceEntered, CargoWise.ComponentModel.INotificationType notificationType)
		{
			OrgAddress address = addressInfo.BizObj.Factory.Load<OrgAddress>((ZGuid)addressInfo.Value);
			ValidateManufacturerIDForAddress(address, addressInfo, forceEntered, notificationType);
		}

		public static void ValidateManufacturerIDForAddress(OrgAddress address, ZPropertyInfo organisationRelatedInfo, bool forceEntered, CargoWise.ComponentModel.INotificationType notificationType)
		{
			BusinessObjectFactory factory = organisationRelatedInfo.BizObj.Factory;

			if (address == null)
			{
				if (forceEntered && organisationRelatedInfo.Value.IsEmpty)
				{
					organisationRelatedInfo.AddNotification(notificationType, MandatoryValidation.YouHaveNotEnteredMessage("Supplier/Manufacturer"));
				}
			}
			else
			{
				ZString manufacturerID = address.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates);

				if (manufacturerID.IsEmpty)
				{
					organisationRelatedInfo.AddNotification(notificationType, string.Format(CultureInfo.CurrentCulture, ManufacturerIDMissing, address.OA_Code));
				}
				else
				{
					ManufacturerIDValidator validator = new ManufacturerIDValidator(factory);
					if (!ManufacturerIDValidator.IsMinMaxAndAlphaNumeric(manufacturerID))
					{
						organisationRelatedInfo.AddNotification(notificationType, ManufacturerIDValidator.Constants.Format);
					}
					else if (!validator.IsISOCountryCodeValid(manufacturerID))
					{
						organisationRelatedInfo.AddNotification(notificationType, ManufacturerIDValidator.Constants.ISOCode);
					}
				}
			}
		}

		public static void ValidateMIDCountryAgainstCountryOfOriginForCanada(ZPropertyInfo countryOfOriginInfo, ZString manufacturerID, ZString countryOfExport)
		{
			var countryOfOrigin = (ZString)countryOfOriginInfo.Value;

			if (countryOfExport == Core.Constants.CountryCodes.Canada && CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfOrigin) && !ManufacturerIDValidator.IsMIDCanadianAndValid(countryOfOrigin, manufacturerID))
			{
				countryOfOriginInfo.AddMessageError(ManufacturerIDValidator.Constants.Canadian);
			}
		}

		public static void ValidateMIDCountryAgainstCountryOfOriginForTextileTariff(ZPropertyInfo countryOfOriginInfo, ZString manufacturerID, USCTariff importTariff, ZDate effectiveDate)
		{
			var countryOfOrigin = (ZString)countryOfOriginInfo.Value;

			if (!countryOfOrigin.IsEmpty && !manufacturerID.IsEmpty && manufacturerID.Left(2) != countryOfOrigin)
			{
				if (importTariff != null && importTariff.Applies(TariffRuleList.Codes.TextileEntryMID, effectiveDate))
				{
					if (!ManufacturerIDValidator.IsMIDCanadianAndValid(countryOfOrigin, manufacturerID))
					{
						countryOfOriginInfo.AddMessageError(ManufacturerIDValidator.Constants.InvalidMIDForTextile);
					}
					else if (CanadaProvinceTerritoryCodes.IsCanadianProvince(manufacturerID.Left(2)) && (CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfOrigin) || countryOfOrigin == Core.Constants.CountryCodes.Canada))
					{
						countryOfOriginInfo.AddWarning(ManufacturerIDValidator.Constants.InvalidMIDForTextileCanadianProvinces);
					}
				}
			}
		}

		public static void ValidatePGAEmail(ZPropertyInfo propertyInfo, IPGAContactDetails pgaContactDetails, bool hasRealAddress = true)
		{
			if (pgaContactDetails != null)
			{
				var email = pgaContactDetails.EmailAddress;
				if (email.IsEmpty)
				{
					var propertyDescriptor = "Email";
					var messageError = hasRealAddress ? string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, propertyDescriptor) : MandatoryValidation.YouHaveNotEnteredMessage(propertyDescriptor);
					propertyInfo.AddMessageError(messageError);
				}
			}
		}

		public static void ValidatePGAAddress(ZPropertyInfo propertyInfo, IAddressDetails address)
		{
			if (address != null)
			{
				if (address.CompanyName.IsEmpty)
				{
					propertyInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, AddressInfoCanNotBeEmpty, "Company Name"));
				}
				if (address.City.IsEmpty)
				{
					propertyInfo.AddMessageError(CityCanNotBeEmpty);
				}
				if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(address.Country) == Core.Constants.CountryCodes.UnitedStates && address.State.IsEmpty)
				{
					propertyInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, AddressInfoCanNotBeEmpty, "State"));
				}
			}
		}

		internal const string AddressInfoCanNotBeEmpty = "{0} can not be empty, Please press F3, go to the Address tab and enter the {0} for selected address.";

		public static bool MIDCountryMatchesOrigin(IOrganisationDetails manufacturer, ZString countryOfOrigin)
		{
			bool result = true;
			if (manufacturer != null)
			{
				ZString matchedCustomsRegoNumber = manufacturer.MatchedCustomsRegoNumber;

				if (!countryOfOrigin.IsEmpty &&
					!matchedCustomsRegoNumber.IsEmpty &&
					 matchedCustomsRegoNumber.Left(2) != countryOfOrigin)
				{
					if (!ManufacturerIDValidator.IsMIDCanadianAndValid(countryOfOrigin, matchedCustomsRegoNumber))
					{
						result = false;
					}
				}
			}

			return result;
		}

		internal const string EIN_SSN_CBNCodeRequired = "This {0} does not have EIN, SSN or CBN number configured. Please press F3 in the field and add one in Config > Registration Numbers/Codes.";
		internal const string EIN_SSN_CBN_EncryptedCodeRequired = "This {0} does not have EIN, SSN, CBN number or Encrypted EIN number configured. Please press F3 in the field and add one in Config > Registration Numbers/Codes. Or you can send Consignee Name and Address Add message to use name and address instead of importer number.";
		internal const string ManufacturerIDMissing = "There is no MID linked to address: {0}. Please press F3 in this field and add an 'MID' in Config > Registration Numbers/Codes.";

		public static void ValidateCityForAddress(ZPropertyInfo organisationRelatedInfo)
		{
			OrgAddress orgAddress = organisationRelatedInfo.BizObj.Factory.Load<OrgAddress>((ZGuid)organisationRelatedInfo.Value);

			if (orgAddress != null && ((IAddressDetails)orgAddress).City.IsEmpty)
			{
				organisationRelatedInfo.AddMessageError(CityCanNotBeEmpty);
			}
		}
		internal const string CityCanNotBeEmpty = "City can not be empty, Please press F3, go to the Address tab and enter the City for selected address.";

		internal static void ValidateACEFDAOrganisationCodes(ZPropertyInfo info, OrgCusCodeForFDA codeForMatching)
		{
			if (!codeForMatching.Number.IsEmpty)
			{
				if (codeForMatching.ID == OrgCusCodeForFDA.DUNSEntityIdentificationCode && (codeForMatching.Number.Length != 9 || !codeForMatching.Number.IsNumbersOnlyOrEmpty))
				{
					info.AddMessageError(DUNSCodeFormat);
				}
				else if (codeForMatching.ID == OrgCusCodeForFDA.FEIEntityIdentificationCode && (codeForMatching.Number.Length < 1 || codeForMatching.Number.Length > 10 || !codeForMatching.Number.IsNumbersOnlyOrEmpty))
				{
					info.AddMessageError(FEICodeFormat);
				}
			}
		}

		internal const string DUNSCodeFormat = "Identification Code is invalid, please check. DUNS Number should be 9 digits in the format: NNNNNNNNN, where N is a number.";
		internal const string FEICodeFormat = "Identification Code is invalid, please check. FEI should be between 1 and 10 digits.";

		internal static void ValidatePostCodeForAddress(ZPropertyInfo propertyInfo, OrgAddress address)
		{
			if (address != null && ((IAddressDetails)address).PostCode.IsEmpty)
			{
				propertyInfo.AddMessageError(PostCodeIsRequired);
			}
		}
		internal const string PostCodeIsRequired = "Post Code is required, please press F3, go to Address tab and enter the Post Code for selected address.";

		public static void ValidateCountryForPGAAddress(ZPropertyInfo propertyInfo, IAddressDetails address, Func<ZString, bool> isCountryValid, ZString errorMessageTextIfInvalidCountry)
		{
			if (address != null)
			{
				var countryCode = address.Country;
				if (!isCountryValid(countryCode))
				{
					propertyInfo.AddMessageError(errorMessageTextIfInvalidCountry);
				}
			}
		}

		public static void ValidateStateForPGAAddress(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, ZString countryCode, ZString state, Func<ZString, bool> shouldCheckStateCodeForPGA = null)
		{
			if (shouldCheckStateCodeForPGA == null)
			{
				shouldCheckStateCodeForPGA = ShouldCheckStateCodeForPGA;
			}

			if (shouldCheckStateCodeForPGA(countryCode))
			{
				var errorMessageText = state.IsEmpty ? StateShouldNotBeEmpty : OrgAddress.ValidateState(factory, state, countryCode).ToString();
				if (!string.IsNullOrEmpty(errorMessageText))
				{
					propertyInfo.AddMessageError(errorMessageText);
				}
			}
		}

		public static void ValidateStateForPGAAddress(ZPropertyInfo propertyInfo, OrgAddress address, Func<ZString, bool> shouldCheckStateCodeForPGA = null)
		{
			if (address != null && address.Country is RefCountry country)
			{
				ValidateStateForPGAAddress(address.Factory, propertyInfo, country.RN_Code, address.State, shouldCheckStateCodeForPGA);
			}
		}
		const string StateShouldNotBeEmpty = "State should not be empty.";

		public static void ValidateStateForExportAddress(ZPropertyInfo propertyInfo, OrgAddress address)
		{
			if (address != null && GetCountryCode(address) != Core.Constants.CountryCodes.PuertoRico)
			{
				var errorMessageText = address.ValidateState();
				if (!errorMessageText.IsEmpty)
				{
					propertyInfo.AddMessageError(errorMessageText);
				}
			}
		}

		public static ZString GetCountryCode(OrgAddress address)
		{
			ZString result = ZString.Empty;
			RefUNLOCO port = (address == null) ? null : address.EffectiveRelatedPortCode;
			if (port != null)
			{
				RefCountry country = port.Country;
				result = (country == null) ? port.RL_Code.Left(2) : country.Code;
			}

			if (result.IsEmpty && address != null)
			{
				result = address.OA_RL_NKRelatedPortCode.Left(2);
			}
			return result;
		}

		public static bool ShouldCheckStateCodeForPGA(ZString countryCode)
		{
			return countryCode == Core.Constants.CountryCodes.UnitedStates
				|| countryCode == Core.Constants.CountryCodes.Mexico
				|| countryCode == Core.Constants.CountryCodes.Canada;
		}

		public static void ValidateCharactorsForAddressDescription(ZPropertyInfo organisationRelatedInfo, OrgAddress address)
		{
			if (address != null)
			{
				if (!address.AddressDescription.IsEmpty)
				{
					ABICharactersValidator.ValidateCharacters(organisationRelatedInfo, address.AddressDescription, true, "Address Description");
				}
				if (!address.OA_Code.IsEmpty)
				{
					ABICharactersValidator.ValidateCharacters(organisationRelatedInfo, address.OA_Code, true, "Address Code");
				}
			}
		}
	}
}
