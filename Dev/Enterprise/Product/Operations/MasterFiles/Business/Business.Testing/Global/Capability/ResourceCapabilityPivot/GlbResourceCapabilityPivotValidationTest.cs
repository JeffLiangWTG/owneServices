using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbResourceCapabilityPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSkillLevel_ShouldValidateAgainstValueInG5_SkillLevel()
		{
			var pivot = Factory.NewWithValidTestData<GlbResourceCapabilityPivot>();
			pivot.G5_DateExperienceGained = ZDateTime.UtcNow;
			var validation = new GlbResourceCapabilityPivotValidation(pivot);

			pivot.G5_SkillLevel = 0;
			validation.ValidateAll();
			AssertHasError(pivot.SkillLevelInfo, "Please select a value.");

			pivot.G5_SkillLevel = 1;
			validation.ValidateAll();
			AssertNoErrors(pivot);
		}
	}
}
