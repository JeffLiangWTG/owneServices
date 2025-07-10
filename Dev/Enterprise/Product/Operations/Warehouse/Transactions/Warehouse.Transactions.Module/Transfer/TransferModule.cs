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
	public class TransferModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public TransferModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsTransfer;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsTransfer);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new TransferFilterControl(GridCollection, (TransferFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new TransferFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsTransferCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerCoreAnd4PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsTransfer;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.WhsTransferWorkflowDescriptorCode;

		protected override IZForm ShowNewForm()
		{
			var form = (TransferEntryForm)base.ShowNewForm();
			if (form != null) // form will be null if security denied
			{
				var docket = (WhsTransfer)form.BusinessEntity;
				var fBO = (TransferFilterBusinessObject)FilterBusinessObject;
				if (!fBO.WD_OH_Client.IsEmpty)
				{
					docket.WD_OH_Client = fBO.WD_OH_Client;
				}

				if (!fBO.WD_WW_Whs.IsEmpty)
				{
					docket.WD_WW_Whs = fBO.WD_WW_Whs;
				}
			}
			return form;
		}

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new TransferOperationalActionsSupporter();
	}
}
