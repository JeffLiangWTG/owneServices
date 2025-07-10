using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.LandTransport;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class TransportLTConsignmentConsumerType : JobInvoicingConsumerType
	{
		public TransportLTConsignmentConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.DtbConsignment; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IDtbConsignment>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceLandTransport; }
		}

		public override bool SupportsWiseRates
		{
			get { return true; }
		}
	}
}
