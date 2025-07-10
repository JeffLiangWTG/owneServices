using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "menuItem is disposed by parent control")]
	public abstract class WiseRatesMenuItem
	{
		protected WiseRatesMenuItem(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
		{
			this.assignCommand = assignCommand;
			grid = gridBoundToWiseEntryViewList
				?? throw new ArgumentNullException(nameof(gridBoundToWiseEntryViewList));
		}

		protected readonly ZGrid grid;
		protected ZMenuItem menuItem;

		protected readonly IWiseRatesCommand assignCommand;

		protected WiseEntryView SelectedRateEntry => (WiseEntryView)grid.GetCurrent();

		protected virtual void InitializeMenuItem()
		{
			menuItem = new ZMenuItem(GetMenuCaption(), OnMenuItemClick);

			var contextMenu = grid.ContextMenu;
			contextMenu.MenuItems.Add(menuItem);
			contextMenu.Popup += ContextMenu_Popup;
		}

		protected void ContextMenu_Popup(object sender, EventArgs e)
		{
			menuItem.Enabled = assignCommand.IsEnabled(SelectedRateEntry);
			ContextMenu_PopupCore();
		}

		protected virtual void ContextMenu_PopupCore()
		{
			menuItem.Caption = GetMenuCaption(SelectedRateEntry);
		}

		protected abstract MultilingualString GetMenuCaption(WiseEntryView entry = default);

		protected void OnMenuItemClick(object sender, EventArgs e)
		{
			var entry = SelectedRateEntry;
			if (entry == null)
			{
				return;
			}

			using (new ZWaitCursorChanger(grid.FindForm()))
			{
				if (assignCommand.Assign(SelectedRateEntry, sender))
				{
					if (grid.DataSource is WiseRatingHeaderView header)
					{
						header.RecalculateWiseEntryViewsFromLastResponse();
					}
				}
			}
		}
	}
}
