using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbPersonLanguageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLanguage()
		{
			GlbPersonLanguage lang = Factory.New<GlbPersonLanguage>();

			lang.G7_Language = "XXX";
			AssertHasErrors(lang.G7_LanguageInfo);

			lang.G7_Language = Core.SharedConstants.Languages.EnglishAmerican;
			AssertNoErrors(lang.G7_LanguageInfo);

			lang.G7_Language = "";
			AssertHasErrors(lang.G7_LanguageInfo);
		}

		public void TestSkillLevel()
		{
			GlbPersonLanguage lang = Factory.New<GlbPersonLanguage>();

			lang.G7_SkillLevel = "XXX";
			AssertHasErrors(lang.G7_SkillLevelInfo);

			lang.G7_SkillLevel = "NAT";
			AssertNoErrors(lang.G7_SkillLevelInfo);

			lang.G7_SkillLevel = "";
			AssertHasErrors(lang.G7_SkillLevelInfo);
		}
	}
}
