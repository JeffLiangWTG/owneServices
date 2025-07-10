using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusPreferenceLanguage))]
	public class RefCusPreferenceLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateNewCusPreferenceLanguage(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateNewCusPreferenceLanguage(Factory);
		}

		RefCusPreferenceLanguage CreateNewCusPreferenceLanguage(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateOrGetLanguage("ENG", "English");
			factory.Save();
			var preference = Factory.New<RefCusPreference>();
			preference.ZZS_ZZZ_NKDataGrouping = "EUN";
			preference.ZZS_Preference = "100";
			preference.ZZS_Description = "Test Description";
			var preferenceLanguage = Factory.New<RefCusPreferenceLanguage>();
			preferenceLanguage.ZX9_ZZS_Preference = preference.PK;
			preferenceLanguage.ZX9_Description = "Test Description With Language";
			preferenceLanguage.ZX9_ZX6_NKLanguage = "ENG";
			return preferenceLanguage;
		}
	}
}
