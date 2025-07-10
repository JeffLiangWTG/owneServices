using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class TrackingShipmentProcessTask : Freight.Forwarding.Business.ForwardingShipmentProcessTask
	{
		public TrackingShipmentProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(TrackingShipment); }
		}

		public new TrackingShipment Parent
		{
			get { return (TrackingShipment)base.Parent; }
		}

		#region Implementation

		protected override Type TransportParentType
		{
			get { return typeof(TrackingShipment); }
		}

		#endregion
	}
}
