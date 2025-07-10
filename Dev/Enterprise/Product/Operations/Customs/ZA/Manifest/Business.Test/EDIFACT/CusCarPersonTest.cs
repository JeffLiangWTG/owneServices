using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	class CusCarPersonTest : TestCaseWithFactory
	{
		public void TestAdditionalInformationTwoFor16A()
		{
			var glb = Factory.New<GlbPerson>();
			glb.PER_FullName = "Vic";
			glb.PER_Gender = "M";
			glb.PER_Passport = "123";
			glb.PER_PassportExpiryDate = ZDateTime.BrettsBirthday.AddYears(40).Date;
			glb.PER_BirthDate = ZDateTime.BrettsBirthday.Date;
			glb.PER_DriversLicenseNumber = "DL123";
			glb.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.UnitedKingdom;
			glb.PER_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			glb.PER_PassportPlaceOfIssue = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			var person = Factory.New<CusPerson>();
			person.CPN_PER_Person = glb.PK;
			AssertEquals("M     ZAGBAU", new CusCarPerson(person, PartyType.Consignor_CZ).AdditionalInformationTwoFor16A);
		}

		public void TestAdditionalInformationTwo()
		{
			var glb = Factory.New<GlbPerson>();
			glb.PER_FullName = "Vic";
			glb.PER_Gender = "M";
			glb.PER_Passport = "123";
			glb.PER_PassportExpiryDate = ZDateTime.BrettsBirthday.AddYears(40).Date;
			glb.PER_BirthDate = ZDateTime.BrettsBirthday.Date;
			glb.PER_DriversLicenseNumber = "DL123";
			glb.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.UnitedKingdom;
			glb.PER_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			glb.PER_PassportPlaceOfIssue = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			var person = Factory.New<CusPerson>();
			person.CPN_PER_Person = glb.PK;
			AssertEquals("M     ZAFGBRAUS", new CusCarPerson(person, PartyType.Consignor_CZ).AdditionalInformationTwo);
		}
	}
}
