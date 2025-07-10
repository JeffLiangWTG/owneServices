using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[Immutable]
	public class TransportBookingWithAgentConsumerType : JobInvoicingConsumerType
	{
		public TransportBookingWithAgentConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID => ControllerIDs.DtbBooking;

		public override Type BizoType => ObjectFactory.GetType<IDtbBooking>();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceLocalTransport;

		public override bool SupportsWiseRates => true;
	}
}
