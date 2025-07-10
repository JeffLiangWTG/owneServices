using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(CusPersonValidation))]
	sealed class CusPersonValidationTest : TestCaseWithFactory
	{
		public void TestCheckPersonDescription()
		{
			var message = "The entered Onboard does not have an Identification Number nor a Passport Number. Press F3 to edit the entered person.";
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Person ?? manifest.Persons.AddNew();
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertEquals("CheckCPN_PER_PersonIsNotEmpty", false, cusPerson.CPN_PER_PersonInfo.Notifications.GetErrors().ContainsNotificationContaining(MandatoryValidation.MustBeEntered));

			cusPerson.CPN_PER_Person = glbPerson.PK;
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_DriversLicenseNumber = "DLN001";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_DriversLicenseNumber = ZString.Empty;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_Passport = "PSP001";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, message);
		}

		public void TestCheckPersonLanguage()
		{
			var message = "Please enter a Preferred Language";
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Person ?? manifest.Persons.AddNew();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_PreferredLanguage = Enterprise.Core.SharedConstants.Languages.English;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, message);
		}

		public void TestCheckIdentificationNumbe()
		{
			var message = "Please enter an Identification Number against the person";
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Person ?? manifest.Persons.AddNew();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_DriversLicenseNumber = "DLN001";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_DriversLicenseNumber = ZString.Empty;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_Passport = "PSP001";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, message);
		}

		public void TestCheckPersonPassport()
		{
			var message = "Please enter Passport Details against the person";
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Person ?? manifest.Persons.AddNew();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_Passport = "PSP001";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_Passport = ZString.Empty;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_DriversLicenseNumber = "DLN001";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, message);
		}

		public void TestCheckPersonPassportExpiry()
		{
			var message = "Please enter a Passport Expiry Date against the person";
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			var manifest = Factory.New<AsycudaManifestHeader>();
			var cusPerson = manifest.Person ?? manifest.Persons.AddNew();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_PassportExpiryDate = ZDate.BrettsBirthday;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_PassportExpiryDate = ZDate.Empty;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, message);

			glbPerson.PER_DriversLicenseNumber = "DLN001";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, message);
		}
	}
}
