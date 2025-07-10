using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AdjustmentModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public AdjustmentModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new AdjustmentOperationalActionSupporter();

		#endregion

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsAdjustment; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsAdjustment);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AdjustmentFilterControl(GridCollection, (AdjustmentFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AdjustmentFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsAdjustmentCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.WarehouseManagerCoreAnd4PL; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.WhsAdjustment; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode; }
		}

		protected override IZForm ShowNewForm()
		{
			var form = (AdjustmentEntryForm)base.ShowNewForm();
			if (form != null) // form will be null if security denied
			{
				var docket = (WhsAdjustment)form.BusinessEntity;
				var fbo = (AdjustmentFilterBusinessObject)FilterBusinessObject;
				if (!fbo.WD_OH_Client.IsEmpty)
				{
					docket.WD_OH_Client = fbo.WD_OH_Client;
				}

				if (!fbo.WD_WW_Whs.IsEmpty)
				{
					docket.WD_WW_Whs = fbo.WD_WW_Whs;
				}
			}
			return form;
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = base.GetNewStandardMenuItems();
			if (NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("f0c51bdf-4079-45fd-ac2b-47111286aaca", "New Adjustment"),
					(s, e) => new AdjustmentController().ShowNewForm()));

				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("64f50c65-a65d-4a61-9a3b-33ccecbb8fda", "New Ownership Adjustment"),
					(s, e) => new AdjustmentController(AdjustmentType.Codes.OwnershipAdjustment).ShowNewForm()));

				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("e25d5104-42fc-4bc9-ac6a-c579ceb9767f", "New Internal Warehouse Adjustment"),
					(s, e) => new AdjustmentController(AdjustmentType.Codes.InternalWarehouseAdjustment).ShowNewForm()));

				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("c8d83b87-a1ae-4b0a-a8e3-9ecc28281bd7", "New Customs Amendment Adjustment"),
					(s, e) => new AdjustmentController(AdjustmentType.Codes.Customs).ShowNewForm()));
			}
			return menuItems;
		}
	}
}
