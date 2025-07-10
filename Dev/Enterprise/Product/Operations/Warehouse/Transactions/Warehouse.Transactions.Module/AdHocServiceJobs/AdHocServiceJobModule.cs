using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AdHocServiceJobModule : ZFilterGridModule
	{
		public AdHocServiceJobModule()
		{
		}

		#region Module Overrides

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsAdHocServiceJob);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AdHocServiceJobFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AdHocServiceJobFilterControl((WhsAdHocServiceJobCollection)GridCollection, (AdHocServiceJobFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsAdHocServiceJobCollection(Factory);
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsAdHocServiceJob;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsAdHocServiceJob;

		protected override IZForm ShowNewForm()
		{
			var form = base.ShowNewForm() as AdHocServiceJobEntryForm;
			if (form != null) // form will be null if security denied
			{
				var adHocServiceJob = (WhsAdHocServiceJob)form.BusinessEntity;
				var fBO = (AdHocServiceJobFilterBusinessObject)FilterBusinessObject;

				if (fBO.ClientPK.IsValid)
				{
					adHocServiceJob.WSJ_OH_Client = fBO.ClientPK;
				}

				if (fBO.WarehousePK.IsValid)
				{
					adHocServiceJob.WSJ_WW_Whs = fBO.WarehousePK;
				}
			}

			return form;
		}

		#endregion
	}
}