using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CopyCommercialInvoiceModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CopyCommercialInvoice;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsCommercialInvoice;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			throw new ModuleGuiNotSupportedException("Findbox only");
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CommercialInvoiceFilterControl(GridCollection, (CommercialInvoiceFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CommercialInvoiceCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CommercialInvoiceFilterBusinessObject(true);
		}

		public override ZBool HasActions => false;
	}
}
