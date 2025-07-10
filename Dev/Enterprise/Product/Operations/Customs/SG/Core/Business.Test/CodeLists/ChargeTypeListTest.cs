using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class ChargeTypeListTest : TestCase
	{
		public void TestChargeTypeList()
		{
			ChargeTypeList chargeTypeList = new ChargeTypeList();
			AssertEquals(3, chargeTypeList.Count);
			Assert(chargeTypeList.ContainsCode(CustomsChargeTypeList.Codes.OverseasFreight));
			Assert(chargeTypeList.ContainsCode(CustomsChargeTypeList.Codes.OverseasInsurance));
			Assert(chargeTypeList.ContainsCode(CustomsChargeTypeList.Codes.OtherCharges));
		}
	}
}
