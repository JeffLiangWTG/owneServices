using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(VPBAmountCodeData))]
	sealed class VPBAmountCodeDataTest : Customs.Business.Testing.CusCodeDataTest<VPBAmountCodeData>
	{
		public void TestVPBAmountCodeDataDefaultValues()
		{
			var vpbAmount = Factory.New<VPBAmountCodeData>();
			Assert(vpbAmount.CY_Type == CusCodeDataTypeList.Codes.VPBAmount);
		}

		public void TestCheckCY_Code()
		{
			var tester = Factory.New<VPBAmountCodeData>();
			tester.CY_Code = "Line1";
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = "";
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = "测试";
			AssertHasErrorContaining(tester.CY_CodeInfo, "Type only accepts Western European languages characters.");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var message = factory.New<CUSDECEDIMessage>();
			return message.VPBAmounts.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<VPBAmountCodeData>();
	}
}
