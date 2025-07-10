using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(ProfitShareRedistribution))]
	public class ProfitShareRedistributionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPSR_BatchNumberIsSavedCorrectly()
		{
			var newPSR = Factory.NewWithValidTestData<ProfitShareRedistribution>();
			Factory.Save();

			AssertEquals("PSR00000001", newPSR.PSR_BatchNumber);
		}

		public void TestHumanReadableName()
		{
			var profitShareRedistribution = Factory.New<ProfitShareRedistribution>();
			AssertEquals("Without Batch Number", "ProfitShareRedistribution", profitShareRedistribution.HumanReadableName);
			profitShareRedistribution.PSR_BatchNumber = "12000";
			AssertEquals("With Batch Number", "Profit Redistribution 12000", profitShareRedistribution.HumanReadableName);
		}

		public void TestDefaultValues_PSR_GC_Company()
		{
			var otherCompany = Factory.New<GlbCompany>();
			var otherBranch = otherCompany.Branches.AddNew();
			Factory.Save();

			var profitShareRedistribution = Factory.New<ProfitShareRedistribution>();
			AssertEquals(Env.CurrentCompanyPK, profitShareRedistribution.PSR_GC_Company);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, otherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				profitShareRedistribution = Factory.New<ProfitShareRedistribution>();
				AssertEquals(otherCompany.PK, profitShareRedistribution.PSR_GC_Company);
			}
		}
	}
}
