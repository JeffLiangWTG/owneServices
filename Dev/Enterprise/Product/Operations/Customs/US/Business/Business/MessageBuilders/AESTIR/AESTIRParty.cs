using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class AESTIRParty : IAESTIRParty
	{
		public static AESTIRParty New(USOrganisation organisation, string[] uSACodeTypesInSignificantOrder, USOrganisationDocAddress alternativeDocAddress = null)
		{
			AESTIRParty result = null;
			if (organisation != null)
			{
				var docAddress = organisation.USOrganisationDocAddress;
				if (!docAddress.IsDeleted && docAddress.E2_AddressOverride)
				{
					(var firstName, var middleInitial, var lastName) = ContactNameHelper.GetSplitContactName(docAddress.E2_Contact);
					if (alternativeDocAddress != null)
					{
						result = new AESTIRParty(docAddress, alternativeDocAddress, firstName, middleInitial, lastName, organisation.ZO_Phone, uSACodeTypesInSignificantOrder);
					}
					else
					{
						result = new AESTIRParty(docAddress, firstName, middleInitial, lastName, uSACodeTypesInSignificantOrder);
					}
				}
				else if (organisation.IsValid)
				{
					if (alternativeDocAddress != null)
					{
						result = new AESTIRParty(organisation, alternativeDocAddress, organisation.FirstName, organisation.MiddleName.Left(1), organisation.LastName, organisation.ZO_Phone, uSACodeTypesInSignificantOrder);
					}
					else
					{
						result = new AESTIRParty(organisation.Organisation, organisation.Address, organisation.FirstName, organisation.MiddleName.Left(1), organisation.LastName, organisation.ZO_Phone, uSACodeTypesInSignificantOrder);
					}
				}
			}
			return result;
		}

		public static AESTIRParty New(OrgHeader organisation, ContactType contactType, ZString transportMode, string[] uSACodeTypesInSignificantOrder)
		{
			AESTIRParty result = null;
			OrgAddress address = null;
			if (organisation != null && !organisation.IsDeleted)
			{
				address = organisation.MainAddress;
			}
			if (address != null && !address.IsDeleted)
			{
				ZString firstName = ZString.Empty;
				ZString middleInitial = ZString.Empty;
				ZString lastName = ZString.Empty;
				ZString contactPhone = ZString.Empty;
				OrgContact contact = new DefaultContactFinder(organisation, false).DefaultContact(contactType, transportMode);
				if (contact != null)
				{
					var contactName = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(contact.OC_ContactName, ABICharacterTypeString.Constants.Alphabetic, OrgContact.Schema.OC_ContactNameMaxLength);
					ZString[] contactNames = contactName.Split(' ');
					if (contactNames.Length == 1)
					{
						lastName = contactNames[0];
					}
					else if (contactNames.Length > 1)
					{
						firstName = contactNames[0];
						lastName = contactNames[contactNames.Length - 1];

						if (contactNames.Length > 2)
						{
							middleInitial = contactNames[1].Left(1);
						}
					}

					contactPhone = contact.PhoneFallbackToOrganisation;
				}

				result = new AESTIRParty(organisation, organisation.MainAddress, firstName, middleInitial, lastName, contactPhone, uSACodeTypesInSignificantOrder);
			}
			return result;
		}

		AESTIRParty(OrgHeader org, OrgAddress address, ZString firstName, ZString middleInitial, ZString lastName, ZString phone, string[] uSACodeTypesInSignificantOrder)
		{
			var companyNameOverride = address.OA_CompanyNameOverride;
			var companyName = companyNameOverride.IsEmpty ? org.OH_FullNameTruncated : companyNameOverride;
			var countryCode = OrganisationValidation.GetCountryCode(address);

			Argument.NotNull(uSACodeTypesInSignificantOrder, "USACodeTypesInSignificantOrder");
			var decider = new SEDIdentificationAndNumberDecider(org, address, uSACodeTypesInSignificantOrder);

			PopulatePartyInfo(companyName, countryCode, address.OA_Address1, address.OA_Address2, address.OA_City, address.OA_PostCode, address.OA_State, firstName, middleInitial, lastName, phone, decider, address.Factory);
		}

		AESTIRParty(USOrganisationDocAddress overrideDocAddress, ZString firstName, ZString middleInitial, ZString lastName, string[] uSACodeTypesInSignificantOrder)
		{
			var countryCode = overrideDocAddress.E2_RN_NKCountryCode;

			Argument.NotNull(uSACodeTypesInSignificantOrder, "USACodeTypesInSignificantOrder");
			var decider = new SEDIdentificationAndNumberDecider(overrideDocAddress, uSACodeTypesInSignificantOrder);

			PopulatePartyInfo(overrideDocAddress.E2_CompanyName, countryCode, overrideDocAddress.E2_Address1, overrideDocAddress.E2_Address2, overrideDocAddress.E2_City,
				overrideDocAddress.E2_Postcode, overrideDocAddress.E2_State, firstName, middleInitial, lastName, overrideDocAddress.E2_Phone, decider, overrideDocAddress.Factory);
		}

		AESTIRParty(USOrganisation organisation, USOrganisationDocAddress docAddress, ZString firstName, ZString middleInitial, ZString lastName, ZString phone, string[] uSACodeTypesInSignificantOrder)
		{
			var companyNameOverride = organisation.Address.OA_CompanyNameOverride;
			var companyName = companyNameOverride.IsEmpty ? organisation.Organisation.OH_FullNameTruncated : companyNameOverride;

			Argument.NotNull(uSACodeTypesInSignificantOrder, "USACodeTypesInSignificantOrder");
			var decider = new SEDIdentificationAndNumberDecider(organisation.Organisation, organisation.Address, uSACodeTypesInSignificantOrder);

			PopulatePartyInfo(companyName, docAddress.E2_RN_NKCountryCode, docAddress.E2_Address1, docAddress.E2_Address2, docAddress.E2_City,
				docAddress.E2_Postcode, docAddress.E2_State, firstName, middleInitial, lastName, phone, decider, docAddress.Factory);
		}

		AESTIRParty(USOrganisationDocAddress overrideDocAddress, USOrganisationDocAddress docAddress, ZString firstName, ZString middleInitial, ZString lastName, ZString phone, string[] uSACodeTypesInSignificantOrder)
		{
			var companyName = overrideDocAddress.E2_CompanyName;

			Argument.NotNull(uSACodeTypesInSignificantOrder, "USACodeTypesInSignificantOrder");
			var decider = new SEDIdentificationAndNumberDecider(overrideDocAddress, uSACodeTypesInSignificantOrder);

			PopulatePartyInfo(companyName, docAddress.E2_RN_NKCountryCode, docAddress.E2_Address1, docAddress.E2_Address2, docAddress.E2_City,
				docAddress.E2_Postcode, docAddress.E2_State, firstName, middleInitial, lastName, phone, decider, docAddress.Factory);
		}

		void PopulatePartyInfo(ZString companyName, ZString countryCode, ZString address1, ZString address2, ZString city, ZString postCode, ZString stateCode, ZString firstName, ZString middleInitial, ZString lastName, ZString phone, SEDIdentificationAndNumberDecider decider, BusinessObjectFactory factory)
		{
			fPartyName = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(companyName, ABICharacterTypeString.Constants.Special, 30);
			fAddressLine1 = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(address1.TrimStart(), ABICharacterTypeString.Constants.Special, 32);
			fAddressLine2 = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(address2.TrimStart(), ABICharacterTypeString.Constants.Special, 32);
			fCity = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(city, ABICharacterTypeString.Constants.Alphabetic, 25);
			fCountryCode = countryCode;
			fPostalCode = GetPostalCode(postCode, fCountryCode);

			switch (fCountryCode)
			{
				case Core.Constants.CountryCodes.PuertoRico:
					fStateCode = fCountryCode;
					fCountryCode = Core.Constants.CountryCodes.UnitedStates;
					break;
				case Core.Constants.CountryCodes.UnitedStates:
					fStateCode = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(stateCode, ABICharacterTypeString.Constants.Alphabetic, 2);
					break;
				case Core.Constants.CountryCodes.Mexico:

					var relatedState = new RefCountryStates.Loader(factory).LoadRefCountryStatesFromCodeOrDesc(stateCode, Core.Constants.CountryCodes.Mexico);
					if (relatedState != null)
					{
						fStateCode = relatedState.GetCustomsCodeFor(RefCusMapTypeList.Codes.EXPSTA, Core.Constants.CountryCodes.UnitedStates);
					}

					if (fStateCode.IsEmpty || fStateCode.Length > 2)
					{
						fStateCode = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(stateCode, ABICharacterTypeString.Constants.Alphabetic, 2).ToString();
					}
					break;
			}

			if (!lastName.IsEmpty)
			{
				fContactFirstName = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(firstName, ABICharacterTypeString.Constants.Alphabetic, 13);
				fContactMiddleInitial = middleInitial;
				fContactLastName = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(lastName, ABICharacterTypeString.Constants.Alphabetic, 20);
			}

			fContactPhoneNumber = PhoneNumberCalculator.GetUnformattedPhoneNumber(phone, fCountryCode != Core.Constants.CountryCodes.UnitedStates);

			fPartyIDType = decider.IdentificationType;
			if (!fPartyIDType.IsEmpty)
			{
				fPartyID = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(decider.IdentificationNumber, ABICharacterTypeString.Constants.Alphanumeric, 11);
				if (fPartyID.Length < 11)
				{
					fPartyID = fPartyID.PadRight(11, '0');
				}
			}
		}

		ZString GetPostalCode(ZString postalCode, ZString countryCode)
		{
			ZString result = postalCode.Replace(" ", "");

			if (countryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				result = result.Replace("-", "");
			}

			return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(result, ABICharacterTypeString.Constants.Alphanumeric, 9);
		}

		#region IAESTIRParty Members

		// N01 Record
		public ZString PartyID
		{
			get { return fPartyID; }
		}
		ZString fPartyID;

		public ZString PartyIDType
		{
			get { return fPartyIDType; }
		}
		ZString fPartyIDType;

		public ZString PartyName
		{
			get { return fPartyName; }
		}
		ZString fPartyName;

		public ZString ContactFirstName
		{
			get { return fContactFirstName; }
		}
		ZString fContactFirstName;

		public ZString ContactMiddleInitial
		{
			get { return fContactMiddleInitial; }
		}
		ZString fContactMiddleInitial;

		public ZString ContactLastName
		{
			get { return fContactLastName; }
		}
		ZString fContactLastName;

		// N02 Record
		public ZString AddressLine1
		{
			get { return fAddressLine1; }
		}
		ZString fAddressLine1;

		public ZString AddressLine2
		{
			get { return fAddressLine2; }
		}
		ZString fAddressLine2;

		public ZString ContactPhoneNumber
		{
			get { return fContactPhoneNumber; }
		}
		ZString fContactPhoneNumber;

		// N03 Record
		public ZString City
		{
			get { return fCity; }
		}
		ZString fCity;

		public ZString StateCode
		{
			get { return fStateCode; }
		}
		ZString fStateCode;

		public ZString CountryCode
		{
			get { return fCountryCode; }
		}
		ZString fCountryCode;

		public ZString PostalCode
		{
			get { return fPostalCode; }
		}
		ZString fPostalCode;

		#endregion
	}
}
