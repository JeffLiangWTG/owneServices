using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class GlbAccreditationJobSkillPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestHAJ_HJG()
		{
			var group1 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			var group2 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();

			var pivot1 = Factory.NewWithValidTestData<GlbAccreditationJobSkillPivot>();
			AssertNoError("should not have duplicate error", pivot1.HAJ_HJGInfo, "Cannot add same Skill twice.");

			pivot1.HAJ_HJG = group1.PK;
			group1.SkillPivots.Add(pivot1);
			AssertNoError("should not have duplicate error", pivot1.HAJ_HJGInfo, "Cannot add same Skill twice.");

			var pivot2 = Factory.NewWithValidTestData<GlbAccreditationJobSkillPivot>();
			AssertNoError("should not have duplicate error", pivot2.HAJ_HJGInfo, "Cannot add same Skill twice.");

			AssertNoError("should not have duplicate error", pivot2.HAJ_HJGInfo, "Cannot add same Skill twice.");

			pivot2.HAJ_HJG = group1.PK;
			group1.SkillPivots.Remove(pivot2);
			AssertHasError("should have duplicate error", pivot2.HAJ_HJGInfo, "Cannot add same Skill twice.");

			pivot1.HAJ_HJG = group2.PK;

			group1.SkillPivots.Remove(pivot1);
			group2.SkillPivots.Add(pivot1);

			pivot2.Validation.ValidateHAJ_HJG();
			AssertNoError("should not have duplicate error", pivot2.HAJ_HJGInfo, "Cannot add same Skill twice.");
			AssertNoError("should not have duplicate error", pivot1.HAJ_HJGInfo, "Cannot add same Skill twice.");

			pivot2.HAJ_HJG = group1.PK;
			group1.SkillPivots.Remove(pivot2);
			group2.SkillPivots.Add(pivot2);
			AssertNoError("should not have duplicate error", pivot2.HAJ_HJGInfo, "Cannot add same Skill twice.");
		}
	}
}
