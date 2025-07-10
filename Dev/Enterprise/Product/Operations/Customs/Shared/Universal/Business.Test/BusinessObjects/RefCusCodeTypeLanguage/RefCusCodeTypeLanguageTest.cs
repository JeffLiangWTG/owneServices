using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeTypeLanguage))]
	class RefCusCodeTypeLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("CHS", "ChineseSimplified");
			var refCusCodeType = factory.New<RefCusCodeType>();
			refCusCodeType.ZZK_CodeType = "ABC";
			refCusCodeType.ZZK_Description = "Test";
			refCusCodeType.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.China;
			var refCusCodeTypeLanguage = factory.New<RefCusCodeTypeLanguage>();
			refCusCodeTypeLanguage.ZXI_ZZK_CodeType = refCusCodeType.PK;
			refCusCodeTypeLanguage.ZXI_ZX6_NKLanguage = "CHS";
			refCusCodeTypeLanguage.ZXI_Description = "测试";
			return refCusCodeTypeLanguage;
		}
	}
}
