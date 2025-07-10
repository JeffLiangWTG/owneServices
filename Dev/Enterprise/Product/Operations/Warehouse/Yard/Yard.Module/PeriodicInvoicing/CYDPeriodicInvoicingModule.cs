using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.Warehouse.Invoicing.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDPeriodicInvoicingModule : PeriodicInvoicingModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CYDPeriodicInvoicing;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CYDPeriodicInvoicing;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ContainerYard;

		protected override PeriodicInvoicingCollection CreatePeriodicInvoicingCollection()
		{
			return new PeriodicInvoicingCollection(Factory, PeriodicInvoicingStorageTypes.Codes.ContainerYard);
		}

		protected override PeriodicInvoicingMultiClientInvoice CreatePeriodicInvoicingMultiClientInvoice()
		{
			return new PeriodicInvoicingMultiClientInvoice(PeriodicInvoicingStorageTypes.Codes.ContainerYard);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CYDPeriodicInvoicing);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CYDPeriodicInvoicingFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new PeriodicInvoicingCollection(Factory, PeriodicInvoicingStorageTypes.Codes.ContainerYard);
		}
	}
}
