using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class StmFeatureTestValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateFeatureName()
		{
			var feature = Factory.New<StmFeatureTest>();

			feature.RunPreSaveValidation();

			AssertHasError(
				"Feature name should not be blank",
				feature.SFT_FeatureNameInfo,
				"Please enter a Feature Name."
			);
		}
	}
}
