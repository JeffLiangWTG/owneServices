using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI;

public abstract class AmendmentSnapshotManagementMenuItemsCreator
{
	protected AmendmentSnapshotManagementMenuItemsCreator(EDIMenu menu)
	{
		this.menu = Argument.NotNull(menu, nameof(menu));
	}

	readonly EDIMenu menu;
	ZMenuItem revertToLastClearedDataMenuItem;

	public ZMenuItem CreateMenuItem()
	{
		revertToLastClearedDataMenuItem = new ZMenuItem(ResString.GetMultilingualString("B9D4A42B-DB11-4AD6-AF72-99FAD0AC6569", "Revert to last cleared data"));
		revertToLastClearedDataMenuItem.Visible = false;

		return revertToLastClearedDataMenuItem;
	}

	public void RefreshMenuItem(BaseJobDeclaration declaration)
	{
		revertToLastClearedDataMenuItem.MenuItems.Clear();
		if (revertToLastClearedDataMenuItem is not null && declaration is not null)
		{
			if (Env.Security.EnableRevertToLastClearedData.IsAllowed)
			{
				declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ForEach(entry =>
				{
					if (ShouldRevertToLastClearedMenuItemForTypicalEntryHeader(entry))
					{
						var revertByOverriding = new ZMenuItem(ResString.GetMultilingualString("9DF21FB7-EEA7-4D02-B7C8-BAE1A4E77E45", "Revert to last cleared data (override if conflict)"));
						revertByOverriding.Click += (s, e) => RevertToLastClearedData(entry, SnapshotRevertingStrategy.Override);
						var revertBySkipping = new ZMenuItem(ResString.GetMultilingualString("BF9DB15B-9038-4D2D-9EED-FA2C39BA629F", "Revert to last cleared data (skip if conflict)"));
						revertBySkipping.Click += (s, e) => RevertToLastClearedData(entry, SnapshotRevertingStrategy.Skip);

						var entryMenuItem = new ZMenuItem((NoResString)entry.EntryHeaderDescriptiveMenuItemText, new MenuItem[] { revertByOverriding, revertBySkipping });

						revertToLastClearedDataMenuItem.MenuItems.Add(entryMenuItem);
					}
				});
				revertToLastClearedDataMenuItem.Visible = revertToLastClearedDataMenuItem.MenuItems.Count > 0;
			}
			else
			{
				revertToLastClearedDataMenuItem.Visible = false;
			}
		}

		void RevertToLastClearedData(CusEntryHeader entry, SnapshotRevertingStrategy strategy)
		{
			if (menu.CheckHasChanges() && entry.GetNewAmendmentSnapshotManager() is { } amendmentSnapshotManager)
			{
				if (amendmentSnapshotManager.HasLodgedSnapshot)
				{
					amendmentSnapshotManager.RevertToLastLodged(strategy);
					if (menu.FireSaveButton())
					{
						Globals.Message.ShowInformation(Res.GetString("F7FD2C36-8B07-4F85-A8C6-0D467ED3216C", "Entry {0} has been restored to previous cleared data. Please Reload this Form before continuing.Logs can be found under Notes -> Entry Restore Log for more information.", new string[] { entry.CH_BGMReference }));
					}
				}
				else
				{
					Globals.Message.ShowWarning(Res.GetString("F5B8044A-070A-4FD3-9430-01E3A6A51210", "There is no previous cleared data available for reverting."));
				}
			}
		}
	}

	bool ShouldRevertToLastClearedMenuItemForTypicalEntryHeader(CusEntryHeader entryHeader) => ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore(entryHeader);
	protected abstract bool ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore(CusEntryHeader entryHeader);
}
