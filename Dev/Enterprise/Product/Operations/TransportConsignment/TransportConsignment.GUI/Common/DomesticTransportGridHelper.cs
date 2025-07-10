using System;
using CargoWise.EntityFramework;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.GUI
{
	public static class DomesticTransportGridHelper
	{
		#region HookDoubleClickToOpenJob

		public static void HookDoubleClickToOpenRunSheet(ZGrid grid)
		{
			HookDoubleClickToOpenJob<DtbConsignmentRunSheet>(grid, ControllerIDs.DtbConsignmentRunSheet);
		}

		public static void HookDoubleClickToOpenRunSheetFromConfirmation(ZGrid grid)
		{
			HookDoubleClickToOpenJob(grid, ControllerIDs.DtbConsignmentRunSheet, GetBizOToOpenDelegate);
		}

		static Func<DtbConsignmentConfirmation, BusinessObject> GetBizOToOpenDelegate
		{
			get { return c => c.RunSheetInstruction != null ? c.RunSheetInstruction.RunSheet : null; }
		}

		public static void HookDoubleClickToOpenConsignmentFromConfirmation(ZGrid grid)
		{
			HookDoubleClickToOpenJob<DtbConsignmentConfirmation>(grid, ControllerIDs.DtbBookingConsignment, c => c.Instruction.Booking);
		}

		static void HookDoubleClickToOpenJob<T>(ZGrid grid, ControllerID controllerID, Func<T, BusinessObject> getBizOToOpen = null)
			where T : BusinessObject
		{
			grid.MouseDown += (sender, e) =>
			{
				if (grid.IsDoubleClickOnRow(e))
				{
					var bizO = grid.GetCurrent() as T;
					if (bizO != null)
					{
						var controller = ZControllerFactory.Create(controllerID);
						var bizOToOpen = (getBizOToOpen != null) ? getBizOToOpen(bizO) : bizO;
						if (bizOToOpen != null)
						{
							controller.ShowEditForm(bizOToOpen);
						}

						#region Test
#if DEBUG
						ControllerForTesting = controller;
#endif
						#endregion
					}
				}
			};
		}

		#region Test
#if DEBUG
		[ThreadStatic]
		internal static ZController ControllerForTesting;
#endif
		#endregion

		#endregion

		#region HookContextMenuShowSignature

		public static void HookContextMenuShowSignature(ZGrid grid)
		{
			var showSignatureMenuItem = new ZMenuItem(ShowSignatureMenuItemText, (s, e) => { ShowSignatureForm(grid); });

			var contextMenu = grid.ContextMenu;
			contextMenu.Popup += (s, e) =>
			{
				UpdateContextMenu(grid, showSignatureMenuItem);
			};

			contextMenu.MenuItems.Add("-");
			contextMenu.MenuItems.Add(showSignatureMenuItem);
		}

		#region UpdateContextMenu

		static void UpdateContextMenu(ZGrid grid, ZMenuItem showSignatureMenuItem)
		{
			var selectedItem = grid.GetCurrent() as ISignatureSupporter;
			var hasSignature = selectedItem != null && selectedItem.HasSignature;

			showSignatureMenuItem.Enabled = hasSignature;
			showSignatureMenuItem.Caption = hasSignature ? ShowSignatureMenuItemText : NoSignatureMenuItemText;
		}

		#endregion

		#region ShowSignatureForm

		static void ShowSignatureForm(ZGrid grid)
		{
			var selectedItem = grid.GetCurrent() as ISignatureSupporter;
			ZFormModaliser.ShowDialogAndDispose(new SignatureForm(selectedItem.ReceivedBySignature));
		}

		#endregion

		#region MenuItemTexts

		static MultilingualString ShowSignatureMenuItemText
		{
			get { return ResString.GetMultilingualString("ConfirmationsGridHelper|ShowSignature", "Show Signature"); }
		}

		static MultilingualString NoSignatureMenuItemText
		{
			get { return ResString.GetMultilingualString("ConfirmationsGridHelper|NoSignature", "No Signature"); }
		}

		#endregion

		#endregion
	}
}
