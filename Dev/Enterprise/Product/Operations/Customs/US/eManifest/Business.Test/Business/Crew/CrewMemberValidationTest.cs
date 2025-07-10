using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class CrewMemberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCP_DateOfBirth()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(crewMember.CP_DateOfBirthInfo);
			crewMember.Certificates.AddNew().XZ_Type = CrewACEIdTypes.Codes.Id;
			ValidationTestHelper.AssertFieldIsNotMandatory(crewMember.CP_DateOfBirthInfo);
		}

		public void TestCheckCP_FullName()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(crewMember.CP_FullNameInfo, "You have not entered a value");
			crewMember.Certificates.AddNew().XZ_Type = CrewACEIdTypes.Codes.ProximityCardId;
			ValidationTestHelper.AssertFieldIsNotMandatory(crewMember.CP_FullNameInfo, "You have not entered a value");
			const string messageError = "You have not entered a First or Last name of the crew member/passenger.";
			crewMember.CP_FullName = "Bill";
			AssertHasMessageError(crewMember.CP_FullNameInfo, messageError);
			crewMember.CP_FullName = "Bill Turner";
			AssertNoMessageError(crewMember.CP_FullNameInfo, messageError);
			crewMember.CP_FullName = "Bill Bootstrap Turner";
			AssertNoMessageError(crewMember.CP_FullNameInfo, messageError);
		}

		public void TestCheckCP_Gender()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(crewMember.CP_GenderInfo, "?", Constants.Genders.Woman);
			crewMember.Certificates.AddNew().XZ_Type = CrewACEIdTypes.Codes.Id;
			ValidationTestHelper.AssertInvalidCodeMessageError(crewMember.CP_GenderInfo, "?", Constants.Genders.Woman);
		}

		public void TestCheckCP_RN_NKNationality()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(crewMember.CP_RN_NKNationalityInfo, "??", Constants.CountryCodes.UnitedStates);
			crewMember.Certificates.AddNew().XZ_Type = CrewACEIdTypes.Codes.ProximityCardId;
			ValidationTestHelper.AssertInvalidCodeMessageError(crewMember.CP_RN_NKNationalityInfo, "??", Constants.CountryCodes.UnitedStates);
		}

		public void TestCheckCP_Type()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(crewMember.CP_TypeInfo, "??", CrewTypes.Codes.ResponsibleParty);
		}

		public void TestCheckAtLeastOneDocumentEntered()
		{
			const string messageError = "At least one travel document should be entered on the Travel Documents tab.";
			crewMember.Validation.ValidateCP_Type();
			AssertNoMessageError("No child collections should be validated if validate all has not been run", crewMember.CP_TypeInfo, messageError);
			crewMember.Validation.ValidateAll();
			AssertHasMessageError("Travel documents validation when validate all has been run", crewMember.CP_TypeInfo, messageError);
			crewMember.Certificates.AddNew();
			crewMember.Validation.ValidateCP_Type();
			AssertNoMessageError("Travel document added", crewMember.CP_TypeInfo, messageError);
			crewMember.Certificates.DeleteAll();
			crewMember.Validation.ValidateCP_Type();
			AssertHasMessageError("Travel documents deleted", crewMember.CP_TypeInfo, messageError);
		}

		public void TestCheckIfThereIsCrewMemberHoldingDriversLicense()
		{
			crewMember.CP_Type = CrewTypes.Codes.CrewMember;
			var passenger = crewMember.Trip.CrewMembers.AddNew();
			passenger.CP_Type = CrewTypes.Codes.Passenger;
			var cert = passenger.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.DrivingLicenseNational;
			cert.XZ_RefNumber = "ZZX00123";
			var responsible = crewMember.Trip.CrewMembers.AddNew();
			responsible.CP_Type = CrewTypes.Codes.ResponsibleParty;
			cert = responsible.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			const string messageError = "At least one crew member should have a driver license; please specify details on the Travel Documents tab.";
			responsible.Validation.ValidateCP_Type();
			AssertNoMessageError("Should NOT be validated if validate all has not been run", responsible.CP_TypeInfo, messageError);
			responsible.Validation.ValidateAll();
			AssertHasMessageError("The notification should be added to the responsible party, passengers drivers license doesn't matter", responsible.CP_TypeInfo, messageError);
			crewMember.Validation.ValidateAll();
			AssertNoMessageError("No message error on the crew member", crewMember.CP_TypeInfo, messageError);
			passenger.Validation.ValidateAll();
			AssertNoMessageError("No message error on the passenger", passenger.CP_TypeInfo, messageError);
			cert = crewMember.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.CommercialDriversLicense;
			cert.XZ_RefNumber = "AAB00123";
			responsible.Validation.ValidateCP_Type();
			AssertNoMessageError("Drivers license added to the crew member", responsible.CP_TypeInfo, messageError);
			cert.Delete();
			cert = responsible.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.EnhancedDriversLicense;
			cert.XZ_RefNumber = "AAB00123";
			responsible.Validation.ValidateCP_Type();
			AssertNoMessageError("Drivers license added to the responsible party", responsible.CP_TypeInfo, messageError);
			cert.XZ_Type = CrewACEIdTypes.Codes.Id;
			responsible.Validation.ValidateCP_Type();
			AssertNoMessageError("No error if responsible party is pre-registered", responsible.CP_TypeInfo, messageError);
		}

		public void TestCheckIfThereIsDriverHoldingHazmatEndorsementIfRequired()
		{
			var trip = crewMember.Trip;
			trip.Shipments.AddNew().Commodities.AddNew().UNDGs.AddNew();
			AssertEquals("Pre-Condition: HasHazmatShipments", true, trip.HasHazmatShipments);
			crewMember.CP_Type = CrewTypes.Codes.CrewMember;
			var passenger = trip.CrewMembers.AddNew();
			passenger.CP_Type = CrewTypes.Codes.Passenger;
			var cert = passenger.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.DrivingLicenseNational;
			cert.XZ_RefNumber = "ZZX00123";
			cert = passenger.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.HazmatEndorsement;
			cert.XZ_RefNumber = "AAAA1234123";
			var responsible = trip.CrewMembers.AddNew();
			responsible.CP_Type = CrewTypes.Codes.ResponsibleParty;
			cert = responsible.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.EnhancedDriversLicense;
			cert.XZ_RefNumber = "AAB00123";
			const string messageError = "At least one driver must have hasmat endorsement to transport hazmat shipments; please specify details on the Travel Documents tab.";
			responsible.Validation.ValidateCP_Type();
			AssertNoMessageError("Should NOT be validated if validate all has not been run", responsible.CP_TypeInfo, messageError);
			responsible.Validation.ValidateAll();
			AssertHasMessageError("The notification should be added to the responsible party, passengers hasmat endorsement doesn't matter", responsible.CP_TypeInfo, messageError);
			crewMember.Validation.ValidateAll();
			AssertNoMessageError("No message error on the crew member", crewMember.CP_TypeInfo, messageError);
			passenger.Validation.ValidateAll();
			AssertNoMessageError("No message error on the passenger", passenger.CP_TypeInfo, messageError);
			cert = crewMember.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.HazmatEndorsement;
			cert.XZ_RefNumber = "ZZZZ1234123";
			responsible.Validation.ValidateCP_Type();
			AssertHasMessageError("Crew member should keep both drivers license and hazmat endorsement", responsible.CP_TypeInfo, messageError);
			cert = crewMember.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.CommercialDriversLicense;
			cert.XZ_RefNumber = "AAB00123";
			responsible.Validation.ValidateCP_Type();
			AssertNoMessageError("Drivers license and hazmat endorsement added to the crew member", responsible.CP_TypeInfo, messageError);
			cert.Delete();
			trip.Shipments.DeleteAll();
			AssertEquals("Pre-Condition: no hazmat shipments", false, trip.HasHazmatShipments);
			responsible.Validation.ValidateCP_Type();
			AssertNoMessageError("No hazmat endorsement required if no hazmat shipments", responsible.CP_TypeInfo, messageError);
			trip.Shipments.AddNew().Commodities.AddNew().UNDGs.AddNew();
			AssertEquals("Pre-Condition: has hazmat shipments", true, trip.HasHazmatShipments);
			cert = responsible.Certificates.AddNew();
			cert.XZ_Type = TravelDocumentTypes.Codes.HazmatEndorsement;
			cert.XZ_RefNumber = "BB234AA34123";
			responsible.Validation.ValidateCP_Type();
			AssertNoMessageError("Drivers license and hazmat endorsement added to the responsible party", responsible.CP_TypeInfo, messageError);
			cert.XZ_Type = CrewACEIdTypes.Codes.ProximityCardId;
			responsible.Validation.ValidateCP_Type();
			AssertNoMessageError("No error if responsible party is pre-registered", responsible.CP_TypeInfo, messageError);
		}

		public void TestCheckUSAddressOrganisationPkForPassenger()
		{
			crewMember.CP_Type = CrewTypes.Codes.Passenger;
			var usAddress = crewMember.USAddress;
			usAddress.Validation.ValidateOrganisationPK();
			AssertNoNotifications(usAddress.OrganisationPKInfo);
			var org = Factory.New<OrgHeader>();
			usAddress.OrganisationPK = org.PK;
			AssertNoNotifications(usAddress.OrganisationPKInfo);
		}

		public void TestCheckUSAddressOrganisationPkForResponsibleParty()
		{
			crewMember.CP_Type = CrewTypes.Codes.ResponsibleParty;
			AssertUSAddressOrganisationPKValidation();
		}

		public void TestCheckUSAddressOrganisationPkForCrewMember()
		{
			crewMember.CP_Type = CrewTypes.Codes.CrewMember;
			AssertUSAddressOrganisationPKValidation();
		}

		public void TestCheckUSAddressCountry()
		{
			const string messageError = "Invalid country code. United States must be entered for this address.";
			var usAddress = crewMember.USAddress;
			usAddress.E2_AddressOverride = true;
			usAddress.E2_RN_NKCountryCode = Constants.CountryCodes.Canada;
			AssertHasError(usAddress.E2_RN_NKCountryCodeInfo, messageError);
			usAddress.E2_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			AssertNoError(usAddress.E2_RN_NKCountryCodeInfo, messageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var trip = Factory.New<Trip>();
			crewMember = trip.CrewMembers.AddNew();
		}

		CrewMember crewMember;

		void AssertUSAddressOrganisationPKValidation()
		{
			const string messageError = "Please specify a valid US Address for this crew member.";
			var usAddress = crewMember.USAddress;
			usAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError("US Address is mandatory for crew member", usAddress.OrganisationPKInfo, messageError);
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Address1 = "Address";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_PostCode = "012345";
			org.MainAddress.OA_RL_NKRelatedPortCode = Constants.CountryCodes.Canada;
			usAddress.OrganisationPK = org.PK;
			AssertHasMessageError("Should be an US organisation", usAddress.OrganisationPKInfo, messageError);
			org.MainAddress.OA_RL_NKRelatedPortCode = Constants.CountryCodes.UnitedStates;
			usAddress.OrganisationPK = ZGuid.Empty;
			usAddress.OrganisationPK = org.PK;
			AssertNoMessageError("Valid US organisation specified", usAddress.OrganisationPKInfo, messageError);
			OrgValidationTest.AssertJobDocAddressUsesOrgValidationIfOrgSpecified(usAddress.OrganisationPKInfo);
		}
	}
}
