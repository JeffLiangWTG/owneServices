using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusConditionLanguage))]
	class RefCusConditionLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("CHS", "ChineseSimplified");
			factory.Save();
			return new RefCusConditionCreatorForTest(Factory).CreateRefCusConditionLanguage("CHS", "Test", "测试");
		}
	}
}
