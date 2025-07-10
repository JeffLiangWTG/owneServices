using System;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	partial class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllCharges()
		{
			AssertEquals(4, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, IncoTermAndCustomsChargeFactory.OverseasFreight);
			AssertGetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, IncoTermAndCustomsChargeFactory.OverseasInsurance);
			AssertGetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, IncoTermAndCustomsChargeFactory.Other);
			AssertGetCharge(InvoiceLineCharge.ChargeTypes.OptionalItemCharges, IncoTermAndCustomsChargeFactory.OptionalItem);
		}

		protected override Type GetCustomsChargeCodeProviderActualType() => typeof(IncoTermAndCustomsChargeFactory);
	}
}
