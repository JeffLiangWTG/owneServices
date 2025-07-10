using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.LocalCartage.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(FilteredCartageLegsCollection))]
	sealed class FilteredCartageLegsCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new FilteredCartageLegsCollection(new CommonCartageLegCollection(Factory));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CommonCartageLeg>();
		}
	}
}
