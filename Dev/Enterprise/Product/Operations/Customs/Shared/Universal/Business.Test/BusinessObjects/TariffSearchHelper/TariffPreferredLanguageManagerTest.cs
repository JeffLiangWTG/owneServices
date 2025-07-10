using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.Universal.Testing
{
	public class TariffPreferredLanguageManagerTest : TestCaseWithFactory
	{
		public void TestLanguage()
		{
			var manager = new TariffPreferredLanguageManager();

			manager.Language = "DEF";
			AssertEquals("When 'DEF' language is set, Language", "DEF", manager.Language);

			manager.Language = "IT-IT";
			AssertEquals("When 'IT-IT' language is set, Language", "IT-IT", manager.Language);

			manager.Language = "";
			AssertEquals("When empty language is set, default Language", "DEF", manager.Language);

			Env.Security.SaveDataImportWizardSettings.IsAllowed = false;
			manager.Language = "DE-DE";
			AssertEquals("When setting 'DE-DE' language but don't have security rights, Language: saving of setting should not be restricted", "DE-DE", manager.Language);
		}

		public void TestIsDefaultLanguage()
		{
			var manager = new TariffPreferredLanguageManager();

			manager.Language = "";
			AssertEquals("When empty language is set, IsDefaultLanguage", true, manager.IsDefaultLanguage);

			manager.Language = "DEF";
			AssertEquals("When 'DEF' language is set, IsDefaultLanguage", true, manager.IsDefaultLanguage);

			manager.Language = "ABC";
			AssertEquals("When 'ABC' language is set, IsDefaultLanguage", false, manager.IsDefaultLanguage);
		}
	}
}
