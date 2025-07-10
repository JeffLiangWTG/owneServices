using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCarrierCodeLanguage))]
	class RefCarrierCodeLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("KO", "Korean");
			var refCarrierCode = helper.CreateCarrierCode("ABC", "Language Data", "KR");
			return helper.CreateCarrierCodeLanguage(refCarrierCode, "KO", "대한항공");
		}
	}
}
