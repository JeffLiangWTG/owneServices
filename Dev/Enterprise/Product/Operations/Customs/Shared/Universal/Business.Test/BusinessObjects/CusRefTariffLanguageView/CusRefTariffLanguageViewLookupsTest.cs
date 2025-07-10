using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class CusRefTariffLanguageViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLanguageTypeList()
		{
			var languageType = Factory.New<RefLanguageType>();
			languageType.ZX6_Language = "ZHS";
			var languageType2 = Factory.New<RefLanguageType>();
			languageType2.ZX6_Language = "DE";
			var languageType3 = Factory.New<RefLanguageType>();
			languageType3.ZX6_Language = "IT";
			CombineAssertions(() =>
			{
				var language = Factory.New<CusRefTariffLanguageView>();
				var list = language.Lookups.LanguageTypeList;
				AssertType<RefLanguageTypeCollection>("Type", list);
				AssertSequencesEqual("Sorted codes", new ZString[] { "DE", "IT", "ZHS" }, list.Select(x => x.ZX6_Language));
				AssertSame("Cached", list, language.Lookups.LanguageTypeList);
			}

			);
		}
	}
}
