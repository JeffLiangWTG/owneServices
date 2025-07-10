using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingOrderLookups : JobOrderHeaderLookups
	{
		public TrackingOrderLookups(TrackingOrder parent) : base(parent)
		{
		}

		#region Suppliers

		public override OrgHeaderCollection Suppliers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public override OrgHeaderCollection Buyers
		{
			get { return new OrgHeaderCollection(Factory); }
		}
		#endregion
	}
}
