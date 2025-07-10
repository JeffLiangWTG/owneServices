using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobRoleSkillPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckH1_SkillsWeighting()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();

			HRJobRoleSkillPivot pivotSkill1 = jobRole.JobRoleSkills.AddNew();
			pivotSkill1.H1_SkillsWeighting = 0;

			AssertHasErrors("Skill weighting does not add to 100, should have errors", pivotSkill1.H1_SkillsWeightingInfo);

			pivotSkill1.H1_SkillsWeighting = 100;

			AssertNoErrors("Skill weighting adds to 100, should NOT have errors", pivotSkill1.H1_SkillsWeightingInfo);

			HRJobRoleSkillPivot pivotSkill2 = jobRole.JobRoleSkills.AddNew();
			pivotSkill2.H1_SkillsWeighting = 55;

			AssertHasErrors("Skill weighting does not add to 100, should have errors", pivotSkill2.H1_SkillsWeightingInfo);

			pivotSkill1.H1_SkillsWeighting = 45;
			pivotSkill2.H1_SkillsWeighting = 55;

			AssertNoErrors("Skill weighting adds to 100, should NOT have errors", pivotSkill1.H1_SkillsWeightingInfo);
			AssertNoErrors("Skill weighting adds to 100, should NOT have errors", pivotSkill2.H1_SkillsWeightingInfo);

			pivotSkill1.H1_SkillsWeighting = 200;
			var minus100 = -100;
			unchecked
			{
				pivotSkill2.H1_SkillsWeighting = (ZByte)(minus100);
			}

			AssertHasErrors("Weighting total adds to 100, but is negative, should have errors", pivotSkill2.H1_SkillsWeightingInfo);
		}
	}
}
