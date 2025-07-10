using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class CusRefTariffVersionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCRT_RN_NKCountryCode()
		{
			var version = Factory.New<CusRefTariffVersion>();
			version.CRT_Version = "V1";
			version.CRT_Description = "V1 desc";
			version.CRT_EffectiveDate = new ZDate(2020, 3, 8);

			var info = version.CRT_RN_NKCountryCodeInfo;
			CombineAssertions(() =>
			{
				version.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;
				AssertHasError("Duplicate error", info, CusRefTariffVersionValidation.DuplicateVersionError);

				version.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;
				AssertNoErrors("No error", info);
			});
		}

		public void TestCheckCRT_EffectiveDate()
		{
			var version = Factory.New<CusRefTariffVersion>();
			version.CRT_Version = "V1";
			version.CRT_Description = "V1 desc";
			version.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;

			var info = version.CRT_EffectiveDateInfo;
			CombineAssertions(() =>
			{
				version.CRT_EffectiveDate = new ZDate(2020, 3, 8);
				AssertHasError("Duplicate error", info, CusRefTariffVersionValidation.DuplicateVersionError);

				version.CRT_EffectiveDate = new ZDate(2020, 3, 9);
				AssertNoErrors("No error", info);
				Factory.Save();

				version.CRT_EffectiveDate = new ZDate(2020, 3, 8);
				AssertEquals("The version already in DB", true, version.IsInDatabase);
				AssertHasError("Duplicate error for Version already in DB", info, CusRefTariffVersionValidation.DuplicateVersionError);

				version.CRT_EffectiveDate = new ZDate(2020, 3, 9);
				AssertNoErrors("No error for Version already in DB", info);
			});
		}

		public void TestCheckCRT_Code()
		{
			var version = Factory.New<CusRefTariffVersion>();
			version.CRT_Version = "";
			var info = version.CRT_VersionInfo;

			CombineAssertions(() =>
			{
				AssertHasError(info, MandatoryValidation.MustBeEnteredMessage(info.HumanReadableName));
				version.CRT_Version = "HS2020";
				AssertHasError(info, "There's a version with the code existed in DB. The Code should be unique.");
				version.CRT_Version = "HS2021";
				AssertNoErrors(info);
			});
		}

		public void TestCheckCRT_Description()
		{
			var version = Factory.New<CusRefTariffVersion>();
			version.CRT_Description = "";
			var info = version.CRT_DescriptionInfo;

			CombineAssertions(() =>
			{
				AssertHasError(info, MandatoryValidation.MustBeEnteredMessage(info.HumanReadableName));

				version.CRT_Description = "HS2020 desc";
				AssertHasError(info, "There's a version with the description existed in DB. The Description should be unique.");
				version.CRT_Description = "HS2021 desc";
				AssertNoErrors(info);

				version.CRT_Version = "HS2021";
				version.CRT_EffectiveDate = new ZDate(2021, 3, 8);
				Factory.Save();

				version.CRT_Description = "HS2020 desc";
				AssertEquals(true, version.IsInDatabase);
				AssertHasError(info, "There's a version with the description existed in DB. The Description should be unique.");
				version.CRT_Description = "HS2021 desc0";
				AssertNoErrors(info);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			versionInDB = Factory.New<CusRefTariffVersion>();
			versionInDB.CRT_Version = "HS2020";
			versionInDB.CRT_Description = "HS2020 desc";
			versionInDB.CRT_EffectiveDate = new ZDate(2020, 3, 8);
			versionInDB.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;
			Factory.Save();
		}

		CusRefTariffVersion versionInDB;
	}
}
