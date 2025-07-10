using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTradeGroupLanguage))]
	class RefCusTradeGroupLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var refCusTradeGroupLanguage = factory.New<RefCusTradeGroupLanguage>();
			refCusTradeGroupLanguage.ZXD_Description = "Language Data";
			return refCusTradeGroupLanguage;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
