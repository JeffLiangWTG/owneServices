using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public static class EstimateValueExpiryMenuItemBuilder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void SetupContextMenu(ZGrid grid, Func<IEnumerable<OrgTradeDetail>> getSelectedTradeDetails)
		{
			var contextMenu = grid.ContextMenu;

			contextMenu.MenuItems.Add(new ZMenuItem("-"));

			var setExpiryMenuItem = new ZMenuItem(
				ResString.GetMultilingualString("D031FBD0-2AF8-4CE1-B0F6-C23870C138D7", "Expire Estimate Commitment"),
				(sender, e) =>
				{
					var selectedDetails = getSelectedTradeDetails();
					if (selectedDetails.Any(x => x.IsCommitted && !x.IsSuperceded))
					{
						var action = new EstimateValueExpiryAction(selectedDetails, EstimateValueExpiryAction.ActionType.SetExpiry);
						var form = new EstimateValueExpiryForm(action);
						ZFormModaliser.ShowDialogAndDispose(form);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("8088772F-0EE3-4204-9DA2-00CE6550882C", "Please select at least one committed value."));
					}
				});
			contextMenu.MenuItems.Add(setExpiryMenuItem);

			var undoExpiryMenuItem = new ZMenuItem(
				ResString.GetMultilingualString("45407189-311E-456C-9DF2-54C739047C70", "Undo Estimate Commitment Expiry"),
				(sender, e) =>
				{
					var selectedDetails = getSelectedTradeDetails();
					if (selectedDetails.Any(x => x.IsExpired && !x.IsSuperceded))
					{
						string message = Res.GetString("92283181-26FB-4CF9-8520-D3087A610E9C", "You are about to undo expiry on selected estimate values. Do you want to proceed?");
						string caption = Res.GetString("BEACF0B7-D5EF-439F-85C1-091705088EB0", "Undo Expiry Confirmation");
						if (DialogResult.Yes == Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No))
						{
							var action = new EstimateValueExpiryAction(selectedDetails, EstimateValueExpiryAction.ActionType.UndoExpiry);
							action.Apply();
						}
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("BCD042DE-7B09-403E-AF5D-3B04AAEE4B48", "Please select at least one expired value."));
					}
				});
			contextMenu.MenuItems.Add(undoExpiryMenuItem);
		}
	}
}
