using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class InventoryModuleWithoutOperationalActions : ZFilterGridModule
	{
		protected InventoryModuleWithoutOperationalActions(bool addItem = true)
		{
			if (addItem)
			{
				AddExportMenuItems();
			}
		}

		#region Related Business Objects

		WhsInventoryViewCollection SelectedInventory
		{
			get
			{
				var result = new WhsInventoryViewCollection(Factory);
				result.AddRange(Grid.SelectedElements);
				return result;
			}
		}

		#endregion

		#region Export Menu Items

		void AddExportMenuItems()
		{
			AddExportDataMenuItem(CommonDataTransferCaptions.ToXmlMenuText, delegate
			{
				var exporter = new XmlDataTransferExporter(new DataTransfer.WhsInventoryViewValueObjectDataAdapter(), true);
				var businessObjects = LoadInventoryUsingFilter().ToArray();
				exporter.PromptUserAndExport(businessObjects);
				Factory.Save();  //Save export events
			});
		}

		protected WhsInventoryViewCollection LoadInventoryUsingFilter()
		{
			var result = new WhsInventoryViewCollection(Factory, FilterBusinessObject.Filter);
			result.Load();
			return result;
		}

		#endregion

		#region Module Overrides

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsInventory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new InventoryFilterControl(GridCollection, (InventoryFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new InventoryFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsModuleInventoryCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerCoreAnd4PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsInventory;

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			result.Add(new ZMenuItem(ResString.GetMultilingualString("efa1063b-f781-4199-9fda-ff6e9d9eee82", "Committed..."), CommittedStockMenuItem_Click));
			return result.ToArray();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());
			result.Insert(1, new ZMenuItem(ResString.GetMultilingualString("d4e4c8bb-227a-4ecd-b23a-d5debbdbc879", "View Receipt"), ViewReceiptMenuItem_Click));
			return result.ToArray();
		}

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion

		#region View Receive

		void ViewReceiptMenuItem_Click(object sender, System.EventArgs e)
		{
			if (Grid.SelectedRowCount < 1)
			{
				Globals.Message.ShowError(NoItemSelectedErrorMsg);
			}
			else
			{
				InventoryHelper.ViewReceipt(((WhsInventoryView)Grid.SelectedElements[0]).InDocketLine);
			}
		}

		#endregion

		#region Show Committed Stock

		void CommittedStockMenuItem_Click(object sender, System.EventArgs e)
		{
			if (Grid.SelectedRowCount < 1)
			{
				Globals.Message.ShowError(NoCommittedItemSelectedErrorMsg);
			}
			else
			{
				InventoryHelper.ShowWhyInventoryIsCommitted(SelectedInventory.Cast<WhsInventoryView>().Select(i => i.InDocketLine).ToArray());
			}
		}

		#endregion

		#region Static Message Strings

		public static string NoReceiptErrorMsg => Res.GetString("7e9eed92-7668-407e-9c4e-cf1deda81479", "This inventory item is not linked to a receipt. It has been created by a Transfer or Adjustment transaction.");

		public static string NoItemSelectedErrorMsg => Res.GetString("7cdeab10-a5f1-4c1d-9afd-37d89c0b6770", "Select an inventory item before clicking this button.");

		public static string NoCommittedItemSelectedErrorMsg => Res.GetString("074d8e44-888f-4c92-81b0-be39b110671a", "Select an inventory item with committed units before clicking this button.");

		#endregion
	}
}
