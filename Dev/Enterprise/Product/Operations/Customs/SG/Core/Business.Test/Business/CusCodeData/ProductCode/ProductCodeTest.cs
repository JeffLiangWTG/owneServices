using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.SG;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(ProductCode))]
	public class ProductCodeTest : Customs.Business.Testing.CusCodeDataTest<ProductCode>
	{
		public void TestSetDefaultValues()
		{
			ProductCode productCode = Factory.New<ProductCode>();
			AssertEquals(CusCodeDataTypeList.Codes.ProductCode, productCode.CY_Type);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var classification = Factory.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			return classification.ProductCodes.AddNew();
		}

		protected override IEnumerable<ProductCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var classification = factory.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			yield return classification.ProductCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var classification = factory.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			return classification.ProductCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			Classification classification = Factory.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			return classification.ProductCodes.AddNew();
		}
	}
}
