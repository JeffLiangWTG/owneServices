using CargoWise.EntityFramework.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingOrderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSuppliers()
		{
			AssertNotNull(ParentOrder.Lookups.Suppliers);
		}

		public void TestBuyers()
		{
			AssertNotNull(ParentOrder.Lookups.Buyers);
		}

		#region Implementation

		TrackingOrder ParentOrder
		{
			get
			{
				if (fParentOrder == null)
				{
					fParentOrder = Factory.New<TrackingOrder>();
				}
				return fParentOrder;
			}
		}
		TrackingOrder fParentOrder;

		#endregion Implementation
	}
}
