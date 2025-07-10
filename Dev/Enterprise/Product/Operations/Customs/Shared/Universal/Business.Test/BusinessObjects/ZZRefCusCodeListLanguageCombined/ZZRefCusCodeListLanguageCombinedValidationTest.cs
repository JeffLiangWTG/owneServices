using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class ZZRefCusCodeListLanguageCombinedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZXA_ZX6_NKLanguage()
		{
			var languageType = Factory.New<RefLanguageType>();
			languageType.ZX6_Language = "ZHT";
			var language = Factory.New<ZZRefCusCodeListLanguageCombined>();
			var info = language.ZXA_ZX6_NKLanguageInfo;
			language.ZXA_ZX6_NKLanguage = "ZHT";
			AssertNoErrorContaining(info, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(info, ListValidation.InvalidCodeError);

			language.ZXA_ZX6_NKLanguage = "ZHS";
			AssertNoErrorContaining(info, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(info, ListValidation.InvalidCodeError);

			language.ZXA_ZX6_NKLanguage = ZString.Empty;
			AssertHasErrorContaining(info, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(info, ListValidation.InvalidCodeError);
		}

		public void TestCheckZXA_Description()
		{
			var language = Factory.New<ZZRefCusCodeListLanguageCombined>();
			var info = language.ZXA_DescriptionInfo;
			language.ZXA_Description = "Desc.";
			AssertNoErrorContaining(info, MandatoryValidation.MustBeEntered);

			language.ZXA_Description = ZString.Empty;
			AssertHasErrorContaining(info, MandatoryValidation.MustBeEntered);
		}
	}
}
