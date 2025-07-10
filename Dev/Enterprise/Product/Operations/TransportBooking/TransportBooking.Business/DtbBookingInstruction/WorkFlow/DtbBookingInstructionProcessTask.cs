using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingInstructionProcessTask : ProcessTask, IDtbBookingInstructionProcessTask
	{
		public DtbBookingInstructionProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(DtbBookingInstruction); }
		}

		public new DtbBookingInstruction Parent
		{
			get { return (DtbBookingInstruction)base.Parent; }
		}
	}
}
