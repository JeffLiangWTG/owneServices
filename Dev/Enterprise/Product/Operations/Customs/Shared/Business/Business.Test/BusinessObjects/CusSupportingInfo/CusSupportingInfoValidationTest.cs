using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSupportingInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_LineNo()
		{
			cusSupportingInfo.CSI_LineNo = -1;
			AssertHasErrorContaining(cusSupportingInfo.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);

			cusSupportingInfo.CSI_LineNo = 0;
			AssertNoErrorContaining(cusSupportingInfo.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckCSI_Quantity()
		{
			cusSupportingInfo.CSI_Quantity = -1;
			AssertHasErrorContaining(cusSupportingInfo.CSI_QuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			cusSupportingInfo.CSI_Quantity = 0;
			AssertNoErrorContaining(cusSupportingInfo.CSI_QuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckCSI_Quantity2()
		{
			cusSupportingInfo.CSI_Quantity2 = -1;
			AssertHasErrorContaining(cusSupportingInfo.CSI_Quantity2Info, MandatoryValidation.ValueCannotBeNegative);

			cusSupportingInfo.CSI_Quantity2 = 0;
			AssertNoErrorContaining(cusSupportingInfo.CSI_Quantity2Info, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckCSI_Quantity3()
		{
			cusSupportingInfo.CSI_Quantity3 = -1;
			AssertHasErrorContaining(cusSupportingInfo.CSI_Quantity3Info, MandatoryValidation.ValueCannotBeNegative);

			cusSupportingInfo.CSI_Quantity3 = 0;
			AssertNoErrorContaining(cusSupportingInfo.CSI_Quantity3Info, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckCSI_PackQty()
		{
			cusSupportingInfo.CSI_PackQty = -1;
			AssertHasErrorContaining(cusSupportingInfo.CSI_PackQtyInfo, MandatoryValidation.ValueCannotBeNegative);

			cusSupportingInfo.CSI_PackQty = 0;
			AssertNoErrorContaining(cusSupportingInfo.CSI_PackQtyInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckCSI_ItemNumber()
		{
			cusSupportingInfo.CSI_ItemNumber = -1;
			AssertHasNotifications("Item number cannot be negative", cusSupportingInfo.CSI_ItemNumberInfo);
			cusSupportingInfo.CSI_ItemNumber = -32453;
			AssertHasNotifications("Item number cannot be negative", cusSupportingInfo.CSI_ItemNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusSupportingInfo = Factory.New<CusSupportingInfo>();
		}

		CusSupportingInfo cusSupportingInfo;
	}
}
