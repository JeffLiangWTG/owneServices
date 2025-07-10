using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class CusPersonValidationTest : TestCaseWithFactory
	{
		public void TestCheckIdentificationNumber()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Persons.AddNew();
			var glbPerson = Factory.New<GlbPerson>();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			cusPerson.Person.PER_DriversLicenseNumber = "";
			cusPerson.Person.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.SouthAfrica;
			cusPerson.Person.PER_DriversLicenseNumber = string.Empty;

			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, cusPerson.PersonNationality);
			AssertEquals(string.Empty, cusPerson.PersonIdentificationNumber);
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Valid ZA Identification Number is mandatory when the Person's Nationality is ZA");

			cusPerson.Person.PER_DriversLicenseNumber = "A1234567";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Valid ZA Identification Number is mandatory when the Person's Nationality is ZA");
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Identification Number must not be provided when the Person's Nationality is not ZA");

			var cusPerson2 = manifest.Persons.AddNew();
			var glbPerson2 = Factory.New<GlbPerson>();
			cusPerson2.CPN_PER_Person = glbPerson2.PK;
			cusPerson2.Person.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.UnitedKingdom;
			cusPerson2.Person.PER_DriversLicenseNumber = string.Empty;
			cusPerson2.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson2.CPN_PER_PersonInfo, "Valid ZA Identification Number is mandatory when the Person's Nationality is ZA");
			AssertNoMessageError(cusPerson2.CPN_PER_PersonInfo, "Identification Number must not be provided when the Person's Nationality is not ZA");

			cusPerson2.Person.PER_DriversLicenseNumber = "A1234567";
			cusPerson2.Validation.ValidateCPN_PER_Person();
			AssertHasMessageError(cusPerson2.CPN_PER_PersonInfo, "Identification Number must not be provided when the Person's Nationality is not ZA");
		}

		public void TestCheckTravelDocumentTypeInZA()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Persons.AddNew();
			cusPerson.TravelDocumentTypeInZA = "";
			AssertHasMessageErrorContaining(cusPerson.TravelDocumentTypeInZAInfo, MandatoryValidation.YouHaveNotEntered);
			cusPerson.TravelDocumentTypeInZA = "X";
			AssertHasMessageErrorContaining(cusPerson.TravelDocumentTypeInZAInfo, ListValidation.InvalidCodeMessageError);
			cusPerson.TravelDocumentTypeInZA = ZaTravelDocumentTypes.Codes.DocumentForTravelPurposes;
			AssertNoNotifications(cusPerson.TravelDocumentTypeInZAInfo);
		}

		public void TestCheckOccupationInZA()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Persons.AddNew();
			cusPerson.OccupationInZA = "";
			AssertHasMessageErrorContaining(cusPerson.OccupationInZAInfo, MandatoryValidation.YouHaveNotEntered);
			cusPerson.OccupationInZA = "X";
			AssertHasMessageErrorContaining(cusPerson.OccupationInZAInfo, ListValidation.InvalidCodeMessageError);
			cusPerson.OccupationInZA = ZaOccupations.Codes.Other;
			AssertNoNotifications(cusPerson.OccupationInZAInfo);
		}

		public void TestCheckTravellerTypeInZA()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Persons.AddNew();
			cusPerson.TravellerTypeInZA = "";
			AssertHasMessageErrorContaining(cusPerson.TravellerTypeInZAInfo, MandatoryValidation.YouHaveNotEntered);
			cusPerson.TravellerTypeInZA = "X";
			AssertHasMessageErrorContaining(cusPerson.TravellerTypeInZAInfo, ListValidation.InvalidCodeMessageError);
			cusPerson.TravellerTypeInZA = ZaTravellerTypes.Codes.ForeignContractWorker;
			AssertNoNotifications(cusPerson.TravellerTypeInZAInfo);
		}

		public void TestCheckReasonForMovementInZA()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Persons.AddNew();
			cusPerson.ReasonForMovementInZA = "";
			AssertHasMessageErrorContaining(cusPerson.ReasonForMovementInZAInfo, MandatoryValidation.YouHaveNotEntered);
			cusPerson.ReasonForMovementInZA = "X";
			AssertHasMessageErrorContaining(cusPerson.ReasonForMovementInZAInfo, ListValidation.InvalidCodeMessageError);
			cusPerson.ReasonForMovementInZA = ZaReasonForMovement.Codes.Transit;
			AssertNoNotifications(cusPerson.ReasonForMovementInZAInfo);
		}

		public void TestCheckCPN_PER_Person_CountryOfIssueForPassportIsRequired()
		{
			var glbPerson = Factory.New<GlbPerson>();
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var cusPerson = manifest.Persons.AddNew();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter the Passport Country/Region Of Issue against the person");
			glbPerson.PER_PassportPlaceOfIssue = "X";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter the Passport Country/Region Of Issue against the person");
		}
	}
}
