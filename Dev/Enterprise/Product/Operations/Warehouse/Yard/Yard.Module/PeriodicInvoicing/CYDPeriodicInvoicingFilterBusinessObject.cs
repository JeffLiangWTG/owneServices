using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Invoicing.Module;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDPeriodicInvoicingFilterBusinessObject : PeriodicInvoicingFilterBusinessObject
	{
		public CYDPeriodicInvoicingFilterBusinessObject() : base(WarehouseCollectionType.CYDWarehouse)
		{
		}

		protected override SecurityCheckpoint CheckPointForJobManagement => Env.Security.CYDPeriodicInvoicingJobInvoicing;

		public override ResourceString WarehouseFilterDescription => ResString.GetMultilingualString("9fa3e8ac-ba7a-48f1-bf1b-90fac5169b79", "Yard");

		protected override string WarehouseFilterValidationError => Res.GetString("06e903ce-f73a-4016-9c82-4fcb80789b22", "Please select a yard to filter by");
	}
}
