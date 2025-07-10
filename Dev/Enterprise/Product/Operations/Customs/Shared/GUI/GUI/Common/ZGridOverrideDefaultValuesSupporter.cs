using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class ZGridOverrideDefaultValuesSupporter : IDisposable
	{
		public ZGridOverrideDefaultValuesSupporter(ZGrid grid)
		{
			this.grid = grid;
			grid.AfterBind += Grid_AfterBind;
		}

		readonly ZGrid grid;
		IOverrideDefaultValuesCollection list;
		CurrencyManager listManager;
		ZMenuItem overrideDefaultValuesMenuItem;

		void Grid_AfterBind(object sender, EventArgs e)
		{
			grid.ContextMenu.Popup += ContextMenu_Popup;
			listManager = grid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged -= ListManager_CurrentChanged;
				listManager.CurrentChanged += ListManager_CurrentChanged;
				HookList();
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			UnHookList();
			HookList();
		}

		void HookList()
		{
			list = listManager?.List as IOverrideDefaultValuesCollection;
			if (list != null)
			{
				if (overrideDefaultValuesMenuItem != null)
				{
					var isEnabled = list.IsOverrideDefaultValuesEnabled;
					overrideDefaultValuesMenuItem.Visible = isEnabled;
					overrideDefaultValuesMenuItem.Enabled = isEnabled;
				}

				var info = list.OverrideDefaultValuesInfo;
				info.ValueChanged -= OverrideDefaultValuesInfo_ValueChanged;
				info.ValueChanged += OverrideDefaultValuesInfo_ValueChanged;
			}
		}

		void UnHookList()
		{
			if (list != null)
			{
				list.OverrideDefaultValuesInfo.ValueChanged -= OverrideDefaultValuesInfo_ValueChanged;
				list = null;
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			if (list != null && list.IsOverrideDefaultValuesEnabled)
			{
				if (overrideDefaultValuesMenuItem == null)
				{
					overrideDefaultValuesMenuItem = new ZMenuItem(ResString.GetMultilingualString("0511F3D7-77C4-4B43-9AE7-115DAB76109A", "Override Default Values"), OverrideDefaultValues_Clicked);
					grid.ContextMenu.MenuItems.Add("-");
					grid.ContextMenu.MenuItems.Add(overrideDefaultValuesMenuItem);
				}

				overrideDefaultValuesMenuItem.Checked = list.OverrideDefaultValuesInfo.Value;
			}
		}

		void OverrideDefaultValuesInfo_ValueChanged(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs ve)
			{
				if (overrideDefaultValuesMenuItem != null)
				{
					overrideDefaultValuesMenuItem.Checked = (ZBool)ve.NewValue;
				}
			}
		}

		void OverrideDefaultValues_Clicked(object sender, EventArgs e)
		{
			if (list != null)
			{
				list.OverrideDefaultValuesInfo.Value = !overrideDefaultValuesMenuItem.Checked;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (listManager != null)
			{
				listManager.CurrentChanged -= ListManager_CurrentChanged;
				listManager = null;
			}
			if (grid != null)
			{
				grid.AfterBind -= Grid_AfterBind;
			}

			UnHookList();
			if (overrideDefaultValuesMenuItem != null)
			{
				overrideDefaultValuesMenuItem.Click -= OverrideDefaultValues_Clicked;
			}
		}

		#endregion
	}
}
