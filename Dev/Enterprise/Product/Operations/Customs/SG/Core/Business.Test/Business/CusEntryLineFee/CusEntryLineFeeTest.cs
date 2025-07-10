using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	public class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryLineFee to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLineFee)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
