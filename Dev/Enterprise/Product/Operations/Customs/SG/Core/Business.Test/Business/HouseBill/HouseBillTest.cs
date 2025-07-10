using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(Bill))]
	class HouseBillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
	{
		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseHouseBill to include a decider for this class", Factory.New(typeof(Customs.Business.Bill)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
