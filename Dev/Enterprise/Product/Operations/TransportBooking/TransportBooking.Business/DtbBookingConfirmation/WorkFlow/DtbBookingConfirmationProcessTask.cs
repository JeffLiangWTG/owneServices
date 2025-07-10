namespace Enterprise.TransportBookings.Business
{
	using System;
	using System.Data;
	using CargoWise.EntityFramework;
	using Enterprise.Integration.TransportBooking;
	using Enterprise.MasterFiles.Business;
	public class DtbBookingConfirmationProcessTask : ProcessTask, IDtbBookingConfirmationProcessTask
	{
		public DtbBookingConfirmationProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(DtbBookingConfirmation); }
		}

		public new DtbBookingConfirmation Parent
		{
			get { return (DtbBookingConfirmation)base.Parent; }
		}
	}
}
