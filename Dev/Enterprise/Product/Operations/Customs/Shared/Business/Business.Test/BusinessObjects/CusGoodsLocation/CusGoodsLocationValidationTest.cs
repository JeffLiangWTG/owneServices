using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCGL_Type()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(cusGoodsLocation.CGL_TypeInfo, "?", CusGoodsLocationTypeList.Codes.ApprovedPlace);
		}

		public void TestCheckCGL_Qualifier()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(cusGoodsLocation.CGL_QualifierInfo, "?", CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGoodsLocation = Factory.New<CusGoodsLocation>();
		}
		CusGoodsLocation cusGoodsLocation;
	}
}
