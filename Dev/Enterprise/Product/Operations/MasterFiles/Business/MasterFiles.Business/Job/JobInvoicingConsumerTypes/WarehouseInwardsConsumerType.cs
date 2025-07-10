using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class WarehouseInwardsConsumerType : JobInvoicingConsumerType
	{
		public WarehouseInwardsConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsReceive; }
		}

		public override bool ShouldExcludeFromPeriodicBillingByDefault => RatingDataRegistry.Instance.ExcludeInwardsFromPeriodicAutoRating.Value;

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IWhsReceive>(); }
		}

		public override bool ExcludeFromClientVisibleOption
		{
			get { return true; }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceWarehouse; }
		}
	}
}
