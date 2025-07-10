using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(ChargeModuleFilter))]
	sealed class ChargeModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUseUpperBound()
		{
			Filter.UseUpperBound = true;
			Filter.UpperBound = 5m;
			AssertEquals(false, Filter.UpperBoundInfo.ReadOnly);
			AssertEquals(5m, Filter.UpperBound);

			Filter.UseUpperBound = false;
			AssertEquals(true, Filter.UpperBoundInfo.ReadOnly);
			AssertEquals(0m, Filter.UpperBound);
		}

		public void TestUseLowerBound()
		{
			Filter.UseLowerBound = true;
			Filter.LowerBound = 4m;
			AssertEquals(false, Filter.LowerBoundInfo.ReadOnly);
			AssertEquals(4m, Filter.LowerBound);

			Filter.UseLowerBound = false;
			AssertEquals(true, Filter.LowerBoundInfo.ReadOnly);
			AssertEquals(0m, Filter.LowerBound);
		}

		#region Implementation

		ChargeModuleFilter Filter
		{
			get { return filter ?? (filter = new ChargeModuleFilter("Description", (q, b) => q)); }
		}
		ChargeModuleFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChargeModuleFilter("description", (q, b) => q);
		}

		#endregion
	}
}
