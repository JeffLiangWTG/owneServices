using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class OrgHelperTest : TestCaseWithFactory
	{
		public void TestUsePassport()
		{
			var onOff = new List<bool> { true, false };
			CombineAssertions("UsePassport ON/OFF", () =>
			{
				onOff.ForEach(onOrOff =>
				{
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAUSEPASSPORT, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, onOrOff))
					{
						AssertEquals($"UsePassportForImporter is on: {onOrOff}", onOrOff, OrgHelper.UsePassport);
					}
				});
			});
		}

		public void TestGetPassportNumber()
		{
			var auCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			var gbCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			var zaCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("No Passports exist", ZString.Empty, OrgHelper.GetPassportNumber(org));
			org.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, gbCountry, "");
			AssertEquals("Empty Passport", ZString.Empty, OrgHelper.GetPassportNumber(org));
			org.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, zaCountry, "");
			org.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, auCountry, ""); // Should be skipped
			org.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, gbCountry, "GB12345678");
			AssertEquals("GB Passport", "GB12345678", OrgHelper.GetPassportNumber(org));
			org.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, auCountry, "AU12345678");
			AssertEquals("AU Passport", "AU12345678", OrgHelper.GetPassportNumber(org));
			org.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, zaCountry, "ZA12345678");
			AssertEquals("ZA Passport", "ZA12345678", OrgHelper.GetPassportNumber(org));
		}
	}
}
