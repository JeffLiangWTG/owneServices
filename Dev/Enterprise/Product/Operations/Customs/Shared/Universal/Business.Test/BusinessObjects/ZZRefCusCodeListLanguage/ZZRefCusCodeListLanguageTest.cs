using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListLanguage))]
	class ZZRefCusCodeListLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var codeList = Factory.NewWithValidTestData<ZZRefCusCodeList>();
			Factory.Save();
			var codeListLanguage = factory.New<ZZRefCusCodeListLanguage>();
			codeListLanguage.ZXA_ZX6_NKLanguage = "ZHT";
			codeListLanguage.ZXA_ZZD_CodeList = codeList.PK;
			codeListLanguage.ZXA_Description = "Test";
			return codeListLanguage;
		}
	}
}
