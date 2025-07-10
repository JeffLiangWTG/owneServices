using System;
using CargoWise.Application;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AgencyBookingConsumerType : AgencyConsumerType
	{
		public AgencyBookingConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AgencyBooking; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Agency.IAgencyBooking>(); }
		}
	}
}
