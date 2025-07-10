using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeListAttributeNameLanguage))]
	class RefCusCodeListAttributeNameLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("CHS", "ChineseSimplified");
			var codeType = helper.CreateNewOrGetExistingCusCodeType("ABC", "abc", Core.Constants.CountryCodes.Australia);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia, "Australia");
			factory.Save();
			var refCusCodeListAttributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("XYZ", "Test", codeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType);
			var refCusCodeListAttributeNameLanguage = factory.New<RefCusCodeListAttributeNameLanguage>();
			refCusCodeListAttributeNameLanguage.ZXH_ZXE_CodeListAttributeName = refCusCodeListAttributeName.PK;
			refCusCodeListAttributeNameLanguage.ZXH_ZX6_NKLanguage = "CHS";
			refCusCodeListAttributeNameLanguage.ZXH_Description = "测试";
			return refCusCodeListAttributeNameLanguage;
		}
	}
}
