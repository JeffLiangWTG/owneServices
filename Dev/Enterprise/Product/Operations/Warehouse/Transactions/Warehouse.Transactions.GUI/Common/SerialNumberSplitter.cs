using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	class SerialNumberSplitter
	{
		#region AddSplitSerialProductLinesMenuItemAndHookEvents

		public static void AddSplitSerialProductLinesMenuItemAndHookEvents(ZGrid grid, INotifications notifications)
		{
			Argument.NotNull(grid, nameof(grid));
			Argument.NotNull(notifications, nameof(notifications));

			if (!grid.ContextMenu.MenuItems.ContainsKey((NoResString)"Split Lines with Serial Numbers")) // Its an identifier
			{
				var splitSerialProductLinesMenuItem = new ZMenuItem(
					ResString.GetMultilingualString("{4148612F-84F0-46EC-A425-C3AB8A9E7FD9}", "S&plit Lines with Serial Numbers"),
					(sender, e) => SplitSerialProductLines(grid, notifications));

				grid.ContextMenu.MenuItems.Add(splitSerialProductLinesMenuItem);
				grid.ContextMenu.Popup += (sender, e) => UpdateContextMenuItems(grid, splitSerialProductLinesMenuItem);
			}
		}

		// old code I did *not* write this crap.
		static void SplitSerialProductLines(ZGrid grid, INotifications notifications)
		{
			var selectedRows = grid.SelectedElements.Length;
			var firstSelectedElement = selectedRows > 0 ? (ISerialSplittableLine)grid.SelectedElements[0] : null;
			if (firstSelectedElement != null)
			{
				if (firstSelectedElement.IsFinalised)
				{
					notifications.AddError(Res.GetString("fef2e6b7-a849-4406-9900-8ff680e43811", "Splitting is not allowed on Finalized Jobs."));
				}
				else
				{
					var selectedElements = Array.ConvertAll(grid.SelectedElements, s => (ISerialSplittableLine)s);
					if (selectedElements.Any(s => s.HasErrors))
					{
						notifications.AddError(Res.GetString("08483b92-0117-4478-b3bf-7515ffb46308", "No lines were split as some lines have Errors."));
					}
					else
					{
						int numValid = 0;
						foreach (var line in selectedElements)
						{
							if ((line.Units > 1) && line.IsSplittableProduct)
							{
								numValid++;
								line.SplitWhenSerialNumberExists();
							}
						}

						if (numValid == 0)
						{
							if (selectedRows == 1)
							{
								var line = selectedElements[0];
								if (line.Units <= 1)
								{
									notifications.AddError(Res.GetString("6db483c6-614a-4207-9593-1aa2a7bd4b76", "Line must have a quantity greater than 1 to split."));
								}
								else
								{
									notifications.AddError(Res.GetString("75e29272-4543-4c67-b98e-6ff379284e72", "Line has no Serial Number attributes, unable to split."));
								}
							}
							else
							{
								notifications.AddError(Res.GetString("a399d375-f3a4-49d0-bed8-fcf168908593", "No lines were split because they are not valid Serial Numbered Products."));
							}
						}
						else if (numValid < selectedRows)
						{
							notifications.AddWarning(Res.GetString("ae187677-b140-4707-9b1c-759115417b05", "Some lines were not split because they are not valid Serial Numbered Products."));
						}
					}
				}
			}
		}

		static void UpdateContextMenuItems(ZGrid grid, ZMenuItem splitSerialProductLinesMenuItem)
		{
			splitSerialProductLinesMenuItem.Enabled = (grid.SelectedElements.Length > 0);
		}

		#endregion
	}
}
