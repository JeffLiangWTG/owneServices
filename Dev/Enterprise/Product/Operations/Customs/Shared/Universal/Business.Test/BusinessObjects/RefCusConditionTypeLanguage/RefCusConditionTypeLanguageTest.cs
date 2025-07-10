using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusConditionTypeLanguage))]
	class RefCusConditionTypeLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateOrGetLanguage("ENG", "English");
			factory.Save();
			var refCusConditionType = factory.New<RefCusConditionType>();
			refCusConditionType.ZX2_ConditionType = "ABC";
			refCusConditionType.ZX2_ConditionClass = "CLASS";
			refCusConditionType.ZX2_Description = "Language Data";
			refCusConditionType.ZX2_ZZZ_NKDataGrouping = "EUN";
			factory.Save();
			var refCusConditionTypeLanguage = Factory.New<RefCusConditionTypeLanguage>();
			refCusConditionTypeLanguage.ZXW_ZX2_ConditionType = refCusConditionType.PK;
			refCusConditionTypeLanguage.ZXW_Description = "Test Description With Language";
			refCusConditionTypeLanguage.ZXW_ZX6_NKLanguage = "ENG";
			return refCusConditionTypeLanguage;
		}
	}
}
