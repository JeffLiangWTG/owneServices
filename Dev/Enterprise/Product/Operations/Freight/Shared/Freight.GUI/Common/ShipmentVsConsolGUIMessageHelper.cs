using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class ShipmentVsConsolGUIMessageHelper : IShipmentVsConsolGUIMessageHelper
	{
		#region Instance

		ShipmentVsConsolGUIMessageHelper()
		{
		}

		public static IShipmentVsConsolGUIMessageHelper Instance
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && helperOverrideForTest != null)
				{
					return helperOverrideForTest;
				}
#endif

				if (instance == null)
				{
					instance = new ShipmentVsConsolGUIMessageHelper();
				}
				return instance;
			}
		}

		[ThreadStatic]
		static IShipmentVsConsolGUIMessageHelper instance;

#if DEBUG

		[ThreadStatic]
		static IShipmentVsConsolGUIMessageHelper helperOverrideForTest;

		public static IDisposable OverrideHelperInstance(IShipmentVsConsolGUIMessageHelper instance)
		{
			helperOverrideForTest = instance;
			return new DisposableAction(() => helperOverrideForTest = null);
		}
#endif

		#endregion

		#region OnShipmentMasterChanged

		public void OnShipmentMasterChanged(IShipmentVsConsolMessageHelper messageHelper, CommonShipment shipment, CommonConsol parentConsol, MasterChangedEventArgs e)
		{
			if (messageHelper != null && shipment != null && e != null)
			{
				var oldMaster = (CommonShipment)shipment.Factory.Load(shipment.GetType(), e.OldMasterPK);
				var newMaster = (CommonShipment)shipment.Factory.Load(shipment.GetType(), e.NewMasterPK);

				string message;
				CommonConsol[] consolsToDetach = messageHelper.GetConsolsToDetachFromSubShipments(out message, parentConsol, oldMaster, newMaster, new[] { shipment });
				if (consolsToDetach != null && consolsToDetach.Any())
				{
					string caption = Res.GetString("b6ae8f02-de3d-4828-97e5-18ed888293f2", "Detaching...");
					if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes)
					{
						var detachRequest = messageHelper.IsAllowedToDetachConsols(shipment, consolsToDetach);
						if (!detachRequest.RestrictedMessage.IsEmpty)
						{
							Globals.Message.ShowInformation(detachRequest.RestrictedMessage, caption);
						}
						else
						{
							messageHelper.DetachShipmentsFromConsols(new[] { shipment }, consolsToDetach);
						}
					}
				}
			}
		}

		#endregion

		#region IsAllowedToDetach

		public bool IsAllowedToDetachShipments(IShipmentVsConsolMessageHelper messageHelper, CommonConsol parentConsol, IEnumerable<CommonShipment> shipments)
		{
			if (messageHelper != null && parentConsol != null && shipments != null)
			{
				string caption = Res.GetString("fcd81489-f1c5-4407-90b4-a4c750784042", "Detaching...");

				IShipmentConsolDetachRequest detachRequest = messageHelper.IsAllowedToDetachShipments(parentConsol, shipments);
				if (!detachRequest.RestrictedMessage.IsEmpty)
				{
					Globals.Message.ShowError(detachRequest.RestrictedMessage);
					return false;
				}

				if (!detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage.IsEmpty)
				{
					Globals.Message.Show(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				if (!detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage.IsEmpty
					&& Globals.Message.Show(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					return false;
				}

				if (!detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage.IsEmpty
					&& Globals.Message.Show(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					return false;
				}

				if (!detachRequest.AdditionalMessage_CutOffDatePassed.IsEmpty
					&& Globals.Message.Show(detachRequest.AdditionalMessage_CutOffDatePassed, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.No)
				{
					return false;
				}

				if (!detachRequest.AdditionalMessage_DetachSubShipments.IsEmpty)
				{
					DialogResult dialogResult = Globals.Message.Show(detachRequest.AdditionalMessage_DetachSubShipments, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, DialogResult.Cancel);
					switch (dialogResult)
					{
						case DialogResult.Yes:
							messageHelper.DetachShipmentsFromConsols(shipments, new[] { parentConsol });
							return false;
						case DialogResult.No:
							return true;
						default:
							return false;
					}
				}

				return true;
			}

			return false;
		}

		public bool IsAllowedToDetachConsols(IShipmentVsConsolMessageHelper messageHelper, CommonShipment parentShipment, IEnumerable<CommonConsol> consols)
		{
			if (messageHelper != null && parentShipment != null && consols != null)
			{
				string caption = Res.GetString("fcd81489-f1c5-4407-90b4-a4c750784042", "Detaching...");

				IShipmentConsolDetachRequest detachRequest = messageHelper.IsAllowedToDetachConsols(parentShipment, consols);
				if (!detachRequest.RestrictedMessage.IsEmpty)
				{
					Globals.Message.ShowError(detachRequest.RestrictedMessage);
					return false;
				}

				if (!detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage.IsEmpty)
				{
					Globals.Message.Show(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				if (!detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage.IsEmpty
					&& Globals.Message.Show(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					return false;
				}

				if (!detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage.IsEmpty
					&& Globals.Message.Show(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					return false;
				}

				if (!detachRequest.AdditionalMessage_CutOffDatePassed.IsEmpty
					&& Globals.Message.Show(detachRequest.AdditionalMessage_CutOffDatePassed, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.No)
				{
					return false;
				}

				if (!detachRequest.AdditionalMessage_DetachSubShipments.IsEmpty)
				{
					DialogResult dialogResult = Globals.Message.Show(detachRequest.AdditionalMessage_DetachSubShipments, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, DialogResult.Cancel);
					switch (dialogResult)
					{
						case DialogResult.Yes:
							messageHelper.DetachShipmentsFromConsols(new[] { parentShipment }, consols);
							return false;
						case DialogResult.No:
							return true;
						default:
							return false;
					}
				}

				return true;
			}

			return false;
		}

		#endregion

		// The implementation of the delegate RequestPermissionByImpersonation to be used in the Business
		public bool RequestPermissionByImpersonation(string message, SecurityCheckpoint checkpoint)
		{
			do
			{
				using (var logForm = new LoginForm())
				{
					logForm.Message = message;
					var dlgResult = ZFormModaliser.ShowDialogWithoutDispose(logForm);

					if (dlgResult != DialogResult.OK)
					{
						return false;
					}

					if (logForm.Credentials?.UserSecurity?.FindCheckPoint(checkpoint.LookupKey).IsAllowed ?? false)
					{
						return true;
					}

					Globals.Message.Show(
						Res.GetString("1645EA1D-43AF-4388-B4E5-A560803383CC", "User does not exist or password is invalid!"),
						Res.GetString("6BAFDD31-360D-4DDF-A696-B640CA491CF4", "Invalid Credentials"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
				}
			} while (true);
		}
	}
}
