using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingOrderProcessTasks : OrderProcessTasks
	{
		public TrackingOrderProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		protected override Type ParentType
		{
			get { return typeof(TrackingOrder); }
		}

		public new TrackingOrder Parent
		{
			get { return (TrackingOrder)base.Parent; }
		}

		#endregion
	}
}
