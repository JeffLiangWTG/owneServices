using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	sealed class GroupInvoiceChargeTest : Customs.Business.Testing.BaseGroupInvoiceChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseGroupInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestAllowNonWesternEuropeanCharacterForChargeDescription()
		{
			var charge = Factory.New<GroupInvoiceCharge>();
			Assert("AllowNonWesternEuropeanCharacterForChargeDescription should be true", charge.AllowNonWesternEuropeanCharacterForChargeDescription);
		}
	}
}
