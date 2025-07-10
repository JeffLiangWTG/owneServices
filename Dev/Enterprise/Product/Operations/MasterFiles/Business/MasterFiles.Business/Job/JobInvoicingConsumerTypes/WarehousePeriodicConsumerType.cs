using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.Warehouse;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class WarehousePeriodicConsumerType : JobInvoicingConsumerType
	{
		public WarehousePeriodicConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsInvoicing; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IWhsInvoice>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceWarehouse; }
		}
	}
}
