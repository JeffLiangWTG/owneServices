using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickingModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public PickingModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsPicking;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.WhsPickWorkflowDescriptorCode;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsPicking);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PickingFilterControl(GridCollection, (PickingFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PickingFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsPickCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsPicking;

		public override bool AllowDelete => false;

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			if (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value)
			{
				var menuItems = base.GetNewStandardMenuItems();
				if (NewMenuItem != null)
				{
					NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("a69cc7c9-e79a-4d0f-9ef0-5372604c1919", "New Pick For Available Inventory"),
						(s, e) => new PickingController().ShowNewForm()));

					NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("e882cfe5-8a6a-42d4-9d99-8de7a68c7e3c", "New Pick For Held Inventory"),
						(s, e) => new PickingController(PickType.Codes.HeldInventoryOrder).ShowNewForm()));
				}
				return menuItems;
			}
			else
			{
				return base.GetNewStandardMenuItems();
			}
		}

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new PickOperationalActionsSupporter();
	}
}
