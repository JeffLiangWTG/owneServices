using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	class CusGoodsCatalogProductionInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCGI_Reference() => ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyHasValue(GoodsCatalogInfo.CGI_ReferenceInfo, GoodsCatalogInfo.CGI_BFR_ForeignOperatorInfo, ZGuid.Empty);

		BaseCusGoodsCatalogProductionInfo GoodsCatalogInfo => goodsCatalogInfo ??= Factory.New<BaseCusGoodsCatalogProductionInfo>();
		BaseCusGoodsCatalogProductionInfo goodsCatalogInfo;
	}
}


