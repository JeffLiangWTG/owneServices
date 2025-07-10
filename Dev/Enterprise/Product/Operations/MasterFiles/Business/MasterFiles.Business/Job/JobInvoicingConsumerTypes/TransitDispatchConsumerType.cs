using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class TransitDispatchConsumerType : JobInvoicingConsumerType
	{
		public TransitDispatchConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsTransitDispatchConsignment; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<ITransitDispatchConsignment>(); }
		}

		public override bool ExcludeFromClientVisibleOption
		{
			get { return true; }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceTransitWarehouse; }
		}
	}
}
