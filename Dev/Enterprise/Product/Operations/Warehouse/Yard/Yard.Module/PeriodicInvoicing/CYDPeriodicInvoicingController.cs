using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.Warehouse.Invoicing.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDPeriodicInvoicingController : PeriodicInvoicingController
	{
		public override ControllerID ID => ControllerIDs.CYDPeriodicInvoicing;

		public override ModuleIdentifier ModuleID => ModuleIDs.CYDPeriodicInvoicing;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CYDPeriodicInvoicingNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CYDPeriodicInvoicingEdit;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CYDPeriodicInvoicingView;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CYDPeriodicInvoicingDelete;

		protected override ZString StorageType => PeriodicInvoicingStorageTypes.Codes.ContainerYard;
	}
}
