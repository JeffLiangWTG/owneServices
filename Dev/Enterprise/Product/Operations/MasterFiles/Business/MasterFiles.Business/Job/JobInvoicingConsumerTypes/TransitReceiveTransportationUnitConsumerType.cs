using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class TransitReceiveTransportationUnitConsumerType : JobInvoicingConsumerType
	{
		public TransitReceiveTransportationUnitConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsItemReceiveTransportationUnit; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<ITransitReceiveTransportationUnit>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceTransitWarehouse; }
		}
	}
}
