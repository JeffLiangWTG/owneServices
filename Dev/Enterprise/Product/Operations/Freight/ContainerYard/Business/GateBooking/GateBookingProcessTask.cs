using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateBookingProcessTask : ProcessTask, IGateBookingProcessTask
	{
		public GateBookingProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(GateBooking);

		public new GateBooking Parent => (GateBooking)base.Parent;
	}
}
