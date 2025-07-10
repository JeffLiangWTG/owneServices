using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class RecalculatePatternsInitializer
	{
		readonly IDeduplicatable sourceBizo;
		protected List<ZMenuItem> menuItems = new List<ZMenuItem>();

		public RecalculatePatternsInitializer(IDeduplicatable sourceBizo)
		{
			Argument.NotNull(sourceBizo, "Source Bizo");
			this.sourceBizo = sourceBizo;
		}

		public void CreateRecalculateMenuItem(Menu parentMenu, EventHandler onClick)
		{
			Argument.NotNull(parentMenu, "Parent Menu");
			var recalculatePatternsMenu = new ZMenuItem(
				ResString.GetMultilingualString("Form|ActionMenu|RecalculatePatternTable", "Recalculate &Pattern Tables"),
				delegate(object sender, EventArgs e)
				{
					var bizO = sourceBizo as BusinessObject;
					if (bizO == null)
					{
						return;
					}

					if (bizO.HasChanges)
					{
						Globals.Message.Show(
							Res.GetString("9D8D49C9-D7E9-43B0-9D8F-408EA1D76403", "Please save the form before running this function."),
							Res.GetString("45CBC816-0578-4481-A64A-E33DF5CCB5B6", "Please save the form"),
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
					}
					else
					{
						onClick?.Invoke(sender, e);
					}
				});

			recalculatePatternsMenu.Shortcut = Shortcut.CtrlShiftP;

			int len = parentMenu.MenuItems.Count - 1;

			while (len > 0)
			{
				MenuItem item = parentMenu.MenuItems[len];
				if (item.Text.Equals("-"))
				{
					parentMenu.MenuItems.Remove(item);
					break;
				}
				len--;
			}

			recalculatePatternsMenu.Visible = ShouldShowRegenRunnerMenu;
			recalculatePatternsMenu.Enabled = ((BusinessObject)sourceBizo).IsInDatabase && ShouldShowRegenRunnerMenu;

			parentMenu.MenuItems.Add(recalculatePatternsMenu);
			if (parentMenu.MenuItems.IndexOf(recalculatePatternsMenu) <= parentMenu.MenuItems.Count - 1)
			{
				parentMenu.MenuItems.Add(new ZMenuItem("-", (EventHandler)null));
			}

			menuItems.Add(recalculatePatternsMenu);
		}

		public bool ShouldShowRegenRunnerMenu => (Env.CurrentUser != null && Env.CurrentUser.IsSupportUser) || ZArchitecture.Modules.ClientHookLoader.Instance?.Client == Clients.EDI;
	}
}
