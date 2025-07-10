using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusRateTypeLanguage))]
	class RefCusRateTypeLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateOrGetLanguage("ENG", "English");
			factory.Save();
			var refCusRateType = factory.New<RefCusRateType>();
			refCusRateType.ZZR_RateType = "ABC";
			refCusRateType.ZZR_Description = "Language Data";
			refCusRateType.ZZR_ZZZ_NKDataGrouping = "EUN";
			factory.Save();
			var refCusRateTypeLanguage = Factory.New<RefCusRateTypeLanguage>();
			refCusRateTypeLanguage.ZXT_ZZR_RateType = refCusRateType.PK;
			refCusRateTypeLanguage.ZXT_Description = "Test Description With Language";
			refCusRateTypeLanguage.ZXT_ZX6_NKLanguage = "ENG";
			return refCusRateTypeLanguage;
		}
	}
}
