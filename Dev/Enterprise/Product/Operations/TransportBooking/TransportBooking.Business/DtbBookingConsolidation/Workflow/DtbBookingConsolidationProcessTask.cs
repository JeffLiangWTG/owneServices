using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationProcessTask : ProcessTask, IDtbBookingConsolidationProcessTask
	{
		public DtbBookingConsolidationProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID => ControllerIDs.DtbBookingConsolidation;

		protected override Type ParentType
		{
			get { return typeof(DtbBookingConsolidation); }
		}

		public new DtbBookingConsolidation Parent
		{
			get { return (DtbBookingConsolidation)base.Parent; }
		}
	}
}
