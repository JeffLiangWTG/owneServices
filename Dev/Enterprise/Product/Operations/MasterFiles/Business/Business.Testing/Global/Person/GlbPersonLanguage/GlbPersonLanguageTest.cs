using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPersonLanguage))]
	sealed class GlbPersonLanguageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLanguageDescription()
		{
			GlbPersonLanguage lang = Factory.New<GlbPersonLanguage>();

			lang.G7_Language = Core.SharedConstants.Languages.EnglishAmerican;
			AssertEquals("English (American)", lang.LanguageDescription);

			lang.G7_Language = "";
			AssertEquals("", lang.LanguageDescription);
		}

		public void TestNewObjectDefaults()
		{
			GlbPersonLanguage lang = Factory.New<GlbPersonLanguage>();

			AssertEquals("Language Empty", "", lang.G7_Language);
			AssertEquals("Skill Level Empty", "", lang.G7_SkillLevel);
		}
	}
}
