using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusGoodsCatalogValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCGC_DescriptionInfo()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(GoodsCatalog.CGC_DescriptionInfo);
		}

		public void TestCheckCGC_TypeInfo()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(GoodsCatalog.CGC_TypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(GoodsCatalog.CGC_TypeInfo, "XXX", GoodsCatalogTypeList.Codes.Export);
		}

		BaseCusGoodsCatalog GoodsCatalog => goodsCatalog ??= Factory.New<BaseCusGoodsCatalog>();
		BaseCusGoodsCatalog goodsCatalog;
	}
}

