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
	public class WarehouseOrdersConsumerType : JobInvoicingConsumerType
	{
		public WarehouseOrdersConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsOrder; }
		}

		public override bool ShouldExcludeFromPeriodicBillingByDefault => RatingDataRegistry.Instance.ExcludeOrdersFromPeriodicAutoRating.Value;

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IWhsOrder>(); }
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
