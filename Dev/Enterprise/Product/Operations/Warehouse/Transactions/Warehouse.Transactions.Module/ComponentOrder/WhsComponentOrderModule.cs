using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class WhsComponentOrderModule : ZFilterGridModule
	{
		public WhsComponentOrderModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		protected override IZForm ShowNewForm()
		{
			var form = base.ShowNewForm() as DynamicWorkOrderEntryForm;
			if (form != null) // form will be null if security denied
			{
				var docket = (WhsComponentOrder)form.BusinessEntity;
				var fBO = (WhsComponentOrderFilterBusinessObject)FilterBusinessObject;

				if (fBO.WD_OH_Client.IsValid)
				{
					docket.WD_OH_Client = fBO.WD_OH_Client;
				}

				if (fBO.WD_WW_Whs.IsValid)
				{
					docket.WD_WW_Whs = fBO.WD_WW_Whs;
				}
			}
			return form;
		}
	}
}
