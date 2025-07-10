using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.TransportConsignment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class TransportBookingConsignmentConsumerType : JobInvoicingConsumerType
	{
		public TransportBookingConsignmentConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.DtbBookingConsignment; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IDtbBookingConsignment>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceLandTransport; } // change to transport consignmnets WI00084977
		}

		public override bool SupportsWiseRates
		{
			get { return true; }
		}
	}
}
