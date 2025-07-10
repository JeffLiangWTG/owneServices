using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public class ValidateCommunicationModesMenuItemProvider : IValidateCommunicationModesMenuItemProvider
	{
		public void AddItemToGrid(ZGrid grid)
		{
			var menuItem = new ZMenuItem(ResString.GetMultilingualString("42EAEF48-AB44-4384-869D-239E654F62CF", "Validate Communication Modes"),
				delegate
				{
					if (grid.ListManager.GetCurrent() is ProcessTask task)
					{
						var processTaskCollectionSource = grid.DataSource as BusinessObjectCollection<ProcessTask>;
						if (processTaskCollectionSource != null)
						{
							EDICommunicationRulesValidation.ValidateEDICommunicationRules(processTaskCollectionSource);
						}
					}
				});
			var contextMenu = grid.ContextMenu;
			contextMenu.MenuItems.Add(grid.ContextMenu.MenuItems.Count - 1, menuItem);
		}
	}
}
