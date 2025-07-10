using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.Freight.Forwarding.PortMessaging.DataTransfer;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	class DakosyPortMessagingMenusProvider : IPortMessagingMenusProvider
	{
		public DakosyPortMessagingMenusProvider(IBusiness hostEntity)
		{
			this.hostEntity = hostEntity;
		}

		readonly IBusiness hostEntity;

		#region PortMessagingManager

		public PortMessagingManager PortMessagingManager
		{
			get
			{
				if (portMessagingManager == null)
				{
					portMessagingManager = GetShipmentPortMessagingManager() ?? GetConsolPortMessagingManager();
				}

				return portMessagingManager;
			}
		}
		PortMessagingManager portMessagingManager;

		PortMessagingManager GetShipmentPortMessagingManager()
		{
			var host = hostEntity as ForwardingShipment;

			return host != null
				? new ShipmentPortMessagingManager(host)
				: null;
		}

		PortMessagingManager GetConsolPortMessagingManager()
		{
			var host = hostEntity as ForwardingConsol;

			return host != null
				? new ConsolPortMessagingManager(host)
				: null;
		}

		public bool Enabled => PortMessagingManager != null && PortMessagingManager.ShouldShowPortMessagingForDakosy;

		#endregion

		public void AddMenuItems(ZMenuItem parentMenu)
		{
			if (!Env.Security.PortMessaging.IsAllowed)
			{
				parentMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("82b2f880-d56d-4791-b989-c2dc0c9e4227", "DASOKY Access denied, click this menu for details."), AccessDeniedMenuItemClick));
			}
			else if (PortMessagingManager != null)
			{
				foreach (var group in PortMessageMenuItemInfo.GetAllMenuItemInfos(PortMessagingManager).GroupBy(x => x.Category))
				{
					var subMenu = new ZMenuItem(group.First().Category);
					parentMenu.MenuItems.Add(subMenu);

					foreach (var menuItemInfo in group)
					{
						subMenu.MenuItems.Add(new PortMessageMenuItem(menuItemInfo.MenuItemName, GetSendMessageDelegate(menuItemInfo), menuItemInfo.MessageAvailablePredicate));
					}

					parentMenu.Popup += (s, e) =>
					{
						var menuitems = subMenu.MenuItems.OfType<PortMessageMenuItem>();
						foreach (var menuItem in menuitems)
						{
							menuItem.Visible = menuItem.VisibilityPredicate();
						}

						subMenu.Visible = menuitems.Any(x => x.Visible);
					};
				}
			}
		}

		void AccessDeniedMenuItemClick(object sender, EventArgs args)
		{
			Env.Security.PortMessaging.ShowError();
		}

		#region Send Message

		EventHandler GetSendMessageDelegate(PortMessageMenuItemInfo menuItemInfo)
		{
			return (sender, args) => SendMessage(menuItemInfo.MessageType, menuItemInfo.IsCancellation);
		}

		void SendMessage(PortMessagingManager.MessageType messageType, bool isCancellation)
		{
			if (PortMessagingManager != null)
			{
				var confirmCancellation = true;
				if (isCancellation && !PortMessagingManager.StatusRetriever.HasMessageToCancel(messageType))
				{
					var result = Globals.Message.Show(Res.GetString("121372A8-0C01-4FFF-AC69-061E3071DD44", "Are you sure you want to (re)send this Cancellation/Withdrawal request?"), Res.GetString("6dac82bc-df8f-40e0-b743-9c251ec0c9ac", "Cancellation"), ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning, ZDialogResult.No);
					confirmCancellation = result == ZDialogResult.Yes;
				}

				ZString validationErrors = PortMessagingManager.RunPreSendDataValidation(messageType, isCancellation, confirmCancellation);
				if (!validationErrors.IsEmpty)
				{
					ShowError(validationErrors);
				}
				else
				{
					using (new ZWaitCursorChanger())
					{
						var validationWarnings = PortMessagingManager.CheckPreSendWarningsRequiringConfirmation(messageType);
						if (!validationWarnings.IsEmpty)
						{
							var caption = Res.GetString("a26f049c-dece-4540-9673-d338ce910257", "Confirmation");
							DialogResult result = Globals.Message.Show(validationWarnings, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
							if (result == DialogResult.No)
							{
								return;
							}
						}
						SendMessageCore(messageType, isCancellation);
					}
				}
			}
		}

		void SendMessageCore(PortMessagingManager.MessageType messageType, bool isCancellation)
		{
			var purposeCode = PortMessagingManager.GetPurposeCode(messageType, isCancellation, PortMessagingManager.HasDG);

			var importer = new PortMessagingImporter();
			var importFactory = new BusinessObjectFactory { NameForDebugging = "Port Import Factory" };
			using (importFactory.AddDisposableService())
			{
				var bizo = importFactory.Load(PortMessagingManager.MessageOriginator.GetType(), PortMessagingManager.MessageOriginator.PK);
				var importLog = importer.Import(bizo, messageType, purposeCode);

				var importLogHasErrors = importLog.HasErrors();
				Exception saveFailedEx = null;

				if (!importLogHasErrors)
				{
					try
					{
						importFactory.Save();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						saveFailedEx = ex;
					}
				}

				if (importLogHasErrors)
				{
					ShowError(Res.GetString("F5E4E5AA-DBB1-48E5-AD16-547811963A43"
						, "Error sending declaration to Dakosy:-\r\n\r\n{0}"
						, importLog.ToString()).Trim());
				}
				else if (saveFailedEx != null)
				{
					ShowError(Res.GetString("F5E4E5AA-DBB1-48E5-AD16-547811963A47"
						, "Error sending declaration to Dakosy. Save failed because:-\r\n\r\n{0}"
						, saveFailedEx.Message).Trim());
				}
				else
				{
					Event eventLog;
					string message;

					if (isCancellation)
					{
						eventLog = Events.MessageWithdrawCancelRequest;
						message = Res.GetString("d41e9fe3-29be-47a1-9f07-2027f55d7527", "Message Cancellation queued for sending to DAKOSY.");
					}
					else
					{
						eventLog = Events.MessageSent;
						message = Res.GetString("a0990cc0-9746-4a0c-b69b-a71bcd1d1dc8", "Message queued for sending to DAKOSY.");
					}

					string logReference = GetLogReferenceFromMessageType(messageType, isCancellation);

					var newFactory = new BusinessObjectFactory() { NameForDebugging = "Factory for saving Port Messaging parent for event logging" };
					var messageOriginator = newFactory.Load(PortMessagingManager.MessageOriginator.GetType(), PortMessagingManager.MessageOriginator.PK);

					AddMessageSentEventLog(messageOriginator, eventLog, logReference);

					newFactory.Save();

					ShowInformation(message);

					PortMessagingManager.StatusRetriever.RefreshBinding();
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		static string GetLogReferenceFromMessageType(PortMessagingManager.MessageType messageType, bool isCancellation)
		{
			var result = "";

			switch (messageType)
			{
				case PortMessagingManager.MessageType.PortOrderWithHDS:
					result = "Port Order with HDS";
					break;

				case PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors:
					result = "Port Order with HDS Cancellation Because of Errors";
					break;

				case PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit:
					result = "Port Order with HDS Export on Exit Cancellation";
					break;

				case PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation:
					result = "Port Order with HDS Cancellation as Forwarding to Another Port";
					break;

				case PortMessagingManager.MessageType.PortOrderInbound:
					result = "Port Order for Inbound Delivery";
					break;

				case PortMessagingManager.MessageType.PortOrderOutbound:
					result = "Port Order for Outbound Delivery";
					break;

				case PortMessagingManager.MessageType.GatePass:
					result = "Gate Pass";
					break;

				case PortMessagingManager.MessageType.RequestForPortServices:
					result = "Request for Port Services";
					break;
				case PortMessagingManager.MessageType.CertificateOfObligation:
					result = "Certificate of Obligation";
					break;
				case PortMessagingManager.MessageType.RequestForRailDischarge:
					result = "Request for Rail Discharge";
					break;

				case PortMessagingManager.MessageType.StopRequest:
					if (!isCancellation)
					{
						result = "Stop Request";
					}
					break;
			}

			if (string.IsNullOrEmpty(result))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Message type {0} not supported.", messageType));
			}

			return (isCancellation && !PortMessagingManager.IsHDS(messageType)) ? result + " Cancellation" : result;
		}

		static void AddMessageSentEventLog(BusinessObject parent, Event eventType, string messageTypeFromPurpose)
		{
			var parameters = new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, messageTypeFromPurpose),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Dakosy")
			};

			((IStmALogParent)parent).Logs.AddNew(eventType, parameters);
		}

		#endregion

		#region Show Information Dialogs

		void ShowError(string message)
		{
			ShowMessage(message, true);
		}

		void ShowInformation(string message)
		{
			ShowMessage(message, false);
		}

		void ShowMessage(string message, bool isError)
		{
			string caption = Res.GetString("E50107C7-04FF-4BAE-9820-339CBBB32476", "Send Port Order to DAKOSY");

			if (isError)
			{
				Globals.Message.ShowError(message, caption);
			}
			else
			{
				Globals.Message.ShowInformation(message, caption);
			}
		}

		#endregion

		#endregion

		class PortMessageMenuItem : ZMenuItem
		{
			public PortMessageMenuItem(MultilingualString caption, EventHandler onClick, Func<bool> visibilityPredicate)
				: base(caption, onClick)
			{
				this.visibilityPredicate = visibilityPredicate;
			}

			readonly Func<bool> visibilityPredicate;

			public Func<bool> VisibilityPredicate
			{
				get { return visibilityPredicate; }
			}
		}
	}
}
