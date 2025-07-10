using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionAgreementConflictTest : TestCaseWithFactory
	{
		public void TestEquals()
		{
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			var agreement2 = Factory.New<OrgCommissionAgreement>();
			var agreement3 = Factory.New<OrgCommissionAgreement>();

			agreement1.ProductItems.DeleteAll();
			var item1AAA = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1, "AAA", "AAA", "AAA");
			var item1AAA_Duplicate = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "AAA", "AAA", "AAA");
			var item1BBB = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "BBB", "BBB", "BBB");

			var item2AAA = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement2, "AAA", "AAA", "AAA");
			var item3AAA = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement3, "AAA", "AAA", "AAA");

			var agreement2Draft = agreement2.CreateDraft();
			var item2AAADraft = agreement2Draft.ProductItems[0].ChildServiceItems[0].ChildSubModuleItems[0];

			AssertEquals(true, new CommissionAgreementItemConflict(item1AAA, item2AAA).Equals(new CommissionAgreementItemConflict(item1AAA, item2AAA)));
			AssertEquals(false, new CommissionAgreementItemConflict(item1AAA, item2AAA).Equals(new CommissionAgreementItemConflict(item2AAA, item1AAA)));
			AssertEquals(false, new CommissionAgreementItemConflict(item1AAA, item2AAA).Equals(new CommissionAgreementItemConflict(item2AAA, item1AAA)));
			AssertEquals(false, new CommissionAgreementItemConflict(item1AAA, item2AAA).Equals(new CommissionAgreementItemConflict(item1BBB, item2AAA)));

			AssertEquals(true, new CommissionAgreementItemConflict(item2AAA, item1AAA).Equals(new CommissionAgreementItemConflict(item2AAA, item1BBB)));
			AssertEquals(true, new CommissionAgreementItemConflict(item2AAA, item3AAA).Equals(new CommissionAgreementItemConflict(item2AAADraft, item3AAA)));
			AssertEquals(true, new CommissionAgreementItemConflict(item1AAA, item2AAA).Equals(new CommissionAgreementItemConflict(item1AAA_Duplicate, item2AAA)));
		}
	}
}
