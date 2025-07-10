using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingProcessTask : ProcessTask, IDtbBookingProcessTask
	{
		public DtbBookingProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(DtbBooking); }
		}

		public new DtbBooking Parent
		{
			get { return (DtbBooking)base.Parent; }
		}
	}
}
