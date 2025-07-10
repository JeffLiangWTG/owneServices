using System;
using System.Data;

using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingProcessTask : AgencyShipmentProcessTask, Integration.Agency.IAgencyBookingProcessTask
	{
		public AgencyBookingProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID
		{
			get { return ZArchitecture.Modules.ControllerIDs.AgencyBooking; }
		}

		protected override Type ParentType
		{
			get { return typeof(AgencyBooking); }
		}

		public new AgencyBooking Parent
		{
			get { return (AgencyBooking)base.Parent; }
		}
	}
}
