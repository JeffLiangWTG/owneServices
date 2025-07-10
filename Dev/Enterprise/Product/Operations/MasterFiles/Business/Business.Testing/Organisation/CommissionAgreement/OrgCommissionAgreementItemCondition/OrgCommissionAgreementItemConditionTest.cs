using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionAgreementItemCondition))]
	sealed class OrgCommissionAgreementItemConditionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateDraft()
		{
			var condition = Factory.New<OrgCommissionAgreementItemCondition>();
			condition.CIC_Mode = "SHP";
			condition.CIC_RL_NKDestination = "12345";
			condition.CIC_RL_NKOrigin = "67894";
			condition.CIC_CAI = ZGuid.NewZGuid();

			var draft = condition.CreateDraft();
			AssertEquals("SHP", draft.CIC_Mode);
			AssertEquals("12345", draft.CIC_RL_NKDestination);
			AssertEquals("67894", draft.CIC_RL_NKOrigin);
			AssertEquals(ZGuid.Empty, draft.CIC_CAI);
			AssertEquals(false, draft.HasErrors);
		}

		public void TestCanDelete()
		{
			var item = Factory.NewWithValidTestData<OrgCommissionAgreementItem>();
			item.CAI_Type = "PRD";
			item.CAI_Code = "SHP";

			var cond1 = item.ConditionCollection[0];
			var cond2 = item.ConditionCollection.AddNew();

			AssertEquals(2, item.ConditionCollection.Count);

			AssertEquals(true, cond1.CanDelete);
			AssertEquals(true, cond2.CanDelete);

			cond2.Delete();

			AssertEquals(false, cond1.CanDelete);
		}
	}
}
