using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class CartageConsumerType : JobInvoicingConsumerType
	{
		public CartageConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Cartage; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<ICommonCartage>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceLocalTransport; }
		}
	}
}
