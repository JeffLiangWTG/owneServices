using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingCFSShipmentsCollection : CFSShipmentList
	{
		public TrackingCFSShipmentsCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new TrackingCFSShipment this[int index]
		{
			get { return (TrackingCFSShipment)Elements[index]; }
		}

		public new TrackingCFSShipment AddNew()
		{
			return (TrackingCFSShipment)base.AddNew();
		}
	}
}
