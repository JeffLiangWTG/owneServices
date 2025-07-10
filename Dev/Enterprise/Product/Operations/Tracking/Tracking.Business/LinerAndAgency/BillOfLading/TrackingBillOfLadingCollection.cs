using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingBillOfLadingCollection : BillOfLadingCollection
	{
		public TrackingBillOfLadingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new TrackingBillOfLading this[int index]
		{
			get { return (TrackingBillOfLading)base[index]; }
		}

		public new TrackingBillOfLading AddNew()
		{
			return (TrackingBillOfLading)base.AddNew();
		}
	}
}
