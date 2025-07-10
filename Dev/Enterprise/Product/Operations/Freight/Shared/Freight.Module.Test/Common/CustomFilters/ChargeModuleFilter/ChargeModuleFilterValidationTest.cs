using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Module.Testing
{
	sealed class ChargeModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestChargeGroup()
		{
			Filter.ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			AssertNoNotifications(Filter.ChargeGroupInfo);

			Filter.ChargeGroup = "XXX";
			AssertHasError(Filter.ChargeGroupInfo, "Enter a valid selection.");

			Filter.ChargeGroup = "";
			AssertNoNotifications(Filter.ChargeGroupInfo);
		}

		public void TestBounds()
		{
			const string upperError = "Upper bound cannot be less than lower bound.";
			const string lowerError = "Lower bound cannot be greater than upper bound.";

			Filter.UseLowerBound = true;
			Filter.UseUpperBound = true;
			Filter.LowerBound = 4;
			Filter.UpperBound = 5;

			AssertNoNotifications(Filter.LowerBoundInfo);
			AssertNoNotifications(Filter.UpperBoundInfo);

			Filter.UpperBound = 3;

			AssertHasError(Filter.LowerBoundInfo, lowerError);
			AssertHasError(Filter.UpperBoundInfo, upperError);

			Filter.LowerBound = 2.9m;
			AssertNoNotifications(Filter.LowerBoundInfo);
			AssertNoNotifications(Filter.UpperBoundInfo);

			Filter.LowerBound = 1;
			Filter.UseUpperBound = false;
			AssertNoNotifications(Filter.LowerBoundInfo);
			AssertNoNotifications(Filter.UpperBoundInfo);

			Filter.UseUpperBound = true;
			Filter.UseLowerBound = false;
			filter.UpperBound = -1;
			AssertNoNotifications(Filter.LowerBoundInfo);
			AssertNoNotifications(Filter.UpperBoundInfo);
		}

		#region Implementation

		ChargeModuleFilter Filter
		{
			get { return filter ?? (filter = new ChargeModuleFilter("description", (q, b) => q)); }
		}
		ChargeModuleFilter filter;

		#endregion
	}
}
