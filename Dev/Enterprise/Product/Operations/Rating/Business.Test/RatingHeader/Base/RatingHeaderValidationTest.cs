using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingHeaderValidationTest : BusinessObjectValidationTestCase
	{
		// Helper used in tests for classes which inherit RatingHeaderValidation
		// Do not add tests here
		protected TestHelper Helper => fHelper ?? (fHelper = new TestHelper(Factory));
		TestHelper fHelper;
	}
}
