using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusAttributeFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBG_AttributeValue1_Mandatory()
		{
			var cusAttributeFilter = Factory.New<CusAttributeFilter>();
			ValidationTestHelper.AssertErrorIfNotEntered(cusAttributeFilter.BG_AttributeValue1Info, "Attribute Value");
		}

		public void TestAttribute1()
		{
			AssertEquals("Attribute 1", CusAttributeFilterValidation.AttributeName.Attribute1);
		}

		public void TestAttribute2()
		{
			AssertEquals("Attribute 2", CusAttributeFilterValidation.AttributeName.Attribute2);
		}

		public void TestAttribute3()
		{
			AssertEquals("Attribute 3", CusAttributeFilterValidation.AttributeName.Attribute3);
		}
	}
}
