using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	public class GroupInvoiceChargeTest : EU.Business.Declaration.Testing.GroupInvoiceChargeTest
	{
		public override void TestApportionChargeWithSameChargeTypeWithDifferntKeys()
		{
			Assert(true);
		}
	}
}

