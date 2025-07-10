using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class TransitReceiveConsumerType : JobInvoicingConsumerType
	{
		public TransitReceiveConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsTransitReceiveConsignment; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<ITransitReceiveConsignment>(); }
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
