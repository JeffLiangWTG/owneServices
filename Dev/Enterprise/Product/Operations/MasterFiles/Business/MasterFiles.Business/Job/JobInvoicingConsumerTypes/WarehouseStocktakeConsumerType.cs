using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.Warehouse;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class WarehouseStocktakeConsumerType : JobInvoicingConsumerType
	{
		public WarehouseStocktakeConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsStocktake; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IWhsStocktake>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceWarehouse; }
		}
	}
}
