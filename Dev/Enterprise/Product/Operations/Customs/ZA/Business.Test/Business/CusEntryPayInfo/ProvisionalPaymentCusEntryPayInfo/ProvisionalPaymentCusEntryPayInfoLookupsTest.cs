using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class TestProvisionalPaymentCusEntryPayInfoLookups : TestCaseWithFactory
	{
		public void TestProvisionalPaymentStatuses()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var tester = Factory.New<ProvisionalPaymentCusEntryPayInfo>();
			AssertType<ProvisionalPaymentCusEntryPayInfoLookups>(tester.Lookups);
			AssertContainsExactElementsInAnyOrder(testDeclaration.Lookups.EntryStatusList, tester.Lookups.ProvisionalPaymentStatuses);
		}
	}
}
