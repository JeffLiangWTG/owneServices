using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	class CusRefTariffLanguageViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZX7_ZX6_NKLanguage()
		{
			var refLanguageType = Factory.New<RefLanguageType>();
			refLanguageType.ZX6_Language = "ZHS";
			refLanguageType.ZX6_Description = "ChineseSimplified";
			var language = Factory.New<CusRefTariffLanguageView>();
			language.ZX7_IsSystem = false;
			CombineAssertions(() =>
			{
				language.ZX7_ZX6_NKLanguage = "AA";
				AssertHasErrorContaining(language.ZX7_ZX6_NKLanguageInfo, ListValidation.InvalidCodeError);
				language.ZX7_ZX6_NKLanguage = "ZHS";
				AssertNoErrorContaining(language.ZX7_ZX6_NKLanguageInfo, ListValidation.InvalidCodeError);
			}

			);
		}

		public void TestCheckZX7_ZX6_NKLanguage_Length()
		{
			var message = "Language must be 2 or 3 characters.";
			var language = Factory.New<CusRefTariffLanguageView>();
			language.ZX7_IsSystem = false;
			CombineAssertions(() =>
			{
				language.Validation.ValidateZX7_ZX6_NKLanguage();
				AssertNoErrorContaining("Empty", language.ZX7_ZX6_NKLanguageInfo, message);
				language.ZX7_ZX6_NKLanguage = "A";
				AssertHasErrorContaining("A", language.ZX7_ZX6_NKLanguageInfo, message);
				language.ZX7_ZX6_NKLanguage = "AA";
				AssertNoErrorContaining("AA", language.ZX7_ZX6_NKLanguageInfo, message);
				language.ZX7_ZX6_NKLanguage = "AAA";
				AssertNoErrorContaining("AAA", language.ZX7_ZX6_NKLanguageInfo, message);
			}

			);
		}
	}
}
