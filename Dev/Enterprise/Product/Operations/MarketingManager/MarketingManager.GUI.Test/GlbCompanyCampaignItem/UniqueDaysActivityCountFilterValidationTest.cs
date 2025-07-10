using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class UniqueDaysActivityCountFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckProperty()
		{
			UniqueDaysActivityCountFilter filter = new UniqueDaysActivityCountFilter("Test", DummyBizoSchema.Z0_Decimal);
			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			filter.Property = 0;
			filter.Validation.ValidateAll();
			AssertHasErrors(filter.PropertyInfo);

			filter.Property = 1;
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.PropertyInfo);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 0;
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.PropertyInfo);
		}
	}
}
