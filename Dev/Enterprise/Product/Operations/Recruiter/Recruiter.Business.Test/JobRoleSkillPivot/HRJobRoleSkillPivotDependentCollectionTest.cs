using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobRoleSkillPivotDependentCollection))]
	sealed class HRJobRoleSkillPivotDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			return new HRJobRoleSkillPivotDependentCollection(jobRole);
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			var pivotCollectionRoleParent = new HRJobRoleSkillPivotDependentCollection(jobRole);
		}

		public void TestSetDefaultsForNewChild()
		{
			var jobRole = Factory.New<HRJobRole>();
			var pivotCollection = new HRJobRoleSkillPivotDependentCollection(jobRole);

			var pivot1 = pivotCollection.AddNew();
			AssertEquals("1 Pivot exists, default weighting should be", (ZByte)100, pivot1.H1_SkillsWeighting);
			AssertNoErrors("Validation suspended while setting default, should not have errors", pivot1.H1_SkillsWeightingInfo);
			pivot1.H1_SkillsWeighting = 60;

			var pivot2 = pivotCollection.AddNew();
			AssertEquals("Default weighting should be", (ZByte)40, pivot2.H1_SkillsWeighting);

			var pivot3 = pivotCollection.AddNew();
			AssertEquals("Default weighting should be", (ZByte)0, pivot3.H1_SkillsWeighting);
		}

		public void TestSkillWeightingsAddTo()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			var pivotCollection = new HRJobRoleSkillPivotDependentCollection(jobRole);

			HRJobRoleSkillPivot pivot1 = pivotCollection.AddNew();
			pivot1.H1_SkillsWeighting = 60;
			AssertEquals("Skills weighting should add to", (ZByte)60, pivotCollection.SkillWeightingsAddTo);

			HRJobRoleSkillPivot pivot2 = pivotCollection.AddNew();
			pivot2.H1_SkillsWeighting = 80;
			AssertEquals("Skills weighting should add to", (ZByte)140, pivotCollection.SkillWeightingsAddTo);

			HRJobRoleSkillPivot pivot3 = pivotCollection.AddNew();
			pivot3.H1_SkillsWeighting = 10;
			AssertEquals("Skills weighting should add to", (ZByte)150, pivotCollection.SkillWeightingsAddTo);

			pivot1.H1_SkillsWeighting = 10;
			AssertEquals("Skills weighting should add to", (ZByte)100, pivotCollection.SkillWeightingsAddTo);
		}
	}
}
