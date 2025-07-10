using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class WarehouseAdHocServiceJobConsumerType : JobInvoicingConsumerType
	{
		public WarehouseAdHocServiceJobConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsAdHocServiceJob; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IWhsAdHocServiceJob>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceWarehouse; }
		}
	}
}
