using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DutyTaxFeeCharge))]
	sealed class DutyTaxFeeChargeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DutyTaxFeeCharge(Factory.NewWithValidTestData<CusEntryHeader>());
		}

		public void TestChargeTypeDesc()
		{
			var charge = (DutyTaxFeeCharge)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				charge.ChargeType = "A10";
				AssertEquals("進口稅", charge.ChargeTypeDesc);
				charge.ChargeType = "A19";
				AssertEquals("進口稅", charge.ChargeTypeDesc);
				charge.ChargeType = "B40";
				AssertEquals("營業稅", charge.ChargeTypeDesc);
				charge.ChargeType = "B49";
				AssertEquals("營業稅", charge.ChargeTypeDesc);
				charge.ChargeType = "B51";
				AssertEquals("進口推貿費", charge.ChargeTypeDesc);
				charge.ChargeType = "B52";
				AssertEquals("出口推貿費", charge.ChargeTypeDesc);
				charge.ChargeType = "B59";
				AssertEquals("進口推貿費", charge.ChargeTypeDesc);
				charge.ChargeType = "A20";
				AssertEquals("平衡稅", charge.ChargeTypeDesc);
				charge.ChargeType = "A30";
				AssertEquals("反傾銷稅", charge.ChargeTypeDesc);
				charge.ChargeType = "A40";
				AssertEquals("報復關稅", charge.ChargeTypeDesc);
				charge.ChargeType = "A50";
				AssertEquals("額外關稅", charge.ChargeTypeDesc);
				charge.ChargeType = "B10";
				AssertEquals("貨物稅", charge.ChargeTypeDesc);
				charge.ChargeType = "B31";
				AssertEquals("菸酒稅", charge.ChargeTypeDesc);
				charge.ChargeType = "B32";
				AssertEquals("健康福利捐", charge.ChargeTypeDesc);
				charge.ChargeType = "B60";
				AssertEquals("特種貨物及勞務稅", charge.ChargeTypeDesc);
				charge.ChargeType = "B19";
				AssertEquals("貨物稅", charge.ChargeTypeDesc);
				charge.ChargeType = "C10";
				AssertEquals("滯報費", charge.ChargeTypeDesc);
				charge.ChargeType = "C20";
				AssertEquals("滯納金", charge.ChargeTypeDesc);
				charge.ChargeType = "B69";
				AssertEquals("菸酒稅", charge.ChargeTypeDesc);
				charge.ChargeType = "B79";
				AssertEquals("健康福利捐", charge.ChargeTypeDesc);
				charge.ChargeType = "B89";
				AssertEquals("特種貨物及勞務稅", charge.ChargeTypeDesc);
			}

			);
		}

		public void TestMethodOfPaymentDesc()
		{
			var charge = (DutyTaxFeeCharge)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				charge.MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
				AssertEquals("Cash", charge.MethodOfPaymentDesc);
				charge.MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
				AssertEquals("Deferred", charge.MethodOfPaymentDesc);
			}

			);
		}

		public void TestChargeType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var charge = header.DutyTaxFeeCharges.AddNew();
			charge.ChargeType = "A10";
			charge.MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			AssertEquals("TypeCode should be", "A10", charge.ChargeType);
			charge.ChargeType = "A19";
			charge.MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			AssertEquals("TypeCode should be", "A19", charge.ChargeType);
		}
	}
}
