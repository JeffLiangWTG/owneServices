using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class LocalCartageJobTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJobType()
		{
			CommonCartageType cartageType = Factory.New<CommonCartageType>();
			cartageType.E3_JobType = "ESC";
			AssertHasError(cartageType.E3_JobTypeInfo, "Needs to be 4 characters");

			cartageType.E3_JobType = "ESC1";
			AssertNoErrors(cartageType.E3_JobTypeInfo);

			CommonCartageType cartageType2 = Factory.New<CommonCartageType>();
			cartageType2.E3_JobType = "ESC1";
			AssertHasError(cartageType2.E3_JobTypeInfo, "Another Port Transport Job Type is already using this Code");
		}
	}
}
