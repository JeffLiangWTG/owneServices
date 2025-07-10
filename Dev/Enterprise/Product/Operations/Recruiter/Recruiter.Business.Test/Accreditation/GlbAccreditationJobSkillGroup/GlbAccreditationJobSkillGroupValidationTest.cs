using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class GlbAccreditationJobSkillGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHJG_Threshold()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group.HJG_Threshold = 1;
			group.SkillPivots.Reload(true);
			group.Groups.AddNew();

			group.Validation.ValidateHJG_Threshold();
			AssertNoWarnings(group);
		}
	}
}
