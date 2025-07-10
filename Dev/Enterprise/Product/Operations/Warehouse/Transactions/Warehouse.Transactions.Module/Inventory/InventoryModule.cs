using Enterprise.DataTransfer.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class InventoryModule : InventoryModuleWithoutOperationalActions, IOperationalActionSupportable
	{
		public InventoryModule()
			: base(false)
		{
			AddExportMenuItems();
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsInventory;

		void AddExportMenuItems()
		{
			AddInterfaceConnectorExportMenuItem(CommonDataTransferCaptions.ToXmlMenuText, delegate
			{
				var exporter = new XmlDataTransferExporter(new DataTransfer.WhsInventoryViewValueObjectDataAdapter(), true);
				var businessObjects = LoadInventoryUsingFilter().ToArray();
				exporter.PromptUserAndExport(businessObjects);
				Factory.Save();  //Save export events
			});
		}

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new InventoryOperationalActionSupporter();

		#endregion
	}
}
