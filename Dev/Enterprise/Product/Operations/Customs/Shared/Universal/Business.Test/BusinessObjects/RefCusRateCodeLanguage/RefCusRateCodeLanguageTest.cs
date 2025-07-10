using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusRateCodeLanguage))]
	class RefCusRateCodeLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var refCusRateType = factory.New<RefCusRateType>();
			refCusRateType.ZZR_RateType = "ABC";
			refCusRateType.ZZR_Description = "Language Data";
			refCusRateType.ZZR_ZZZ_NKDataGrouping = "EUN";
			var refCusRateCode = factory.New<CusRefRateCodeView>();
			refCusRateCode.ZY1_RateCode = "ABC";
			refCusRateCode.ZY1_Description = "This is a test";
			refCusRateCode.ZY1_RateType = "ABC";
			refCusRateCode.ZY1_ZZZ_NKDataGrouping = "EUN";
			return refCusRateCode;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
