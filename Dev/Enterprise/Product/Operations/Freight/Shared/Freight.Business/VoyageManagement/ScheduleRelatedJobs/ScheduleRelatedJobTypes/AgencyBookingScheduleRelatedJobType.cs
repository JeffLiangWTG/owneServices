using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	public class AgencyBookingScheduleRelatedJobType : ScheduleRelatedJobType
	{
		public AgencyBookingScheduleRelatedJobType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AgencyBooking; }
		}

		public override Type BizOType
		{
			get { return ObjectFactory.GetType<Integration.Agency.IAgencyBooking>(); }
		}
	}
}
