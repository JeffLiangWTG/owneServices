using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AgentBookingConsumerType : JobInvoicingConsumerType
	{
		public AgentBookingConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID => ControllerIDs.DtbBooking;

		public override Type BizoType => ObjectFactory.GetType<IDtbAgentBooking>();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceLocalTransport;

		public override bool SupportsWiseRates => true;
	}
}
