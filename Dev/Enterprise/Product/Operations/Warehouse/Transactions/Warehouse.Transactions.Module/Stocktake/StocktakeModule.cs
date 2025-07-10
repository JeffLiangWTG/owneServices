using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class StocktakeModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public StocktakeModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsStocktake;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsStocktake);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new StocktakeFilterControl(GridCollection, (StocktakeFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new StocktakeFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsStocktakeCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsStocktake;

		protected override IZForm ShowNewForm()
		{
			var form = (StocktakeEntryForm)base.ShowNewForm();
			if (form != null) // form will be null if security denied
			{
				var stocktake = (WhsStocktake)form.BusinessEntity;
				var fBO = (StocktakeFilterBusinessObject)FilterBusinessObject;
				if (!fBO.WS_WW_Whs.IsEmpty)
				{
					stocktake.WS_WW_Whs = fBO.WS_WW_Whs;
				}

				if (!fBO.WS_OH_Client.IsEmpty)
				{
					stocktake.WS_OH_Client = fBO.WS_OH_Client;
				}
			}
			return form;
		}

		public override bool AllowDelete => false;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.WhsStocktakeWorkflowDescriptorCode;

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new StocktakeOperationalActionsSupporter();
	}
}
