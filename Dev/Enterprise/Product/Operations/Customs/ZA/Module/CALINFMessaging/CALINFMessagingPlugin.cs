using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders.CALINF;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.Customs.ZA.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ZA.Module
{
	public class CALINFMessagingPlugin : ZPlugIn
	{
		public CALINFMessagingPlugin(JobVoyage voyage) : base(voyage)
		{
			this.voyage = voyage;
		}

		#region Overrides
		public override string Name => "ZA CALINF Messaging";
		protected override ZBool HasUserControl => true;
		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.ShippingManager;

		protected override Control GetNewUserControl()
		{
			return new CALINFMessageDisplayControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return voyage;
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			MenuItem topLevelMenu = null;

			if (IsAllowedInCurrentCountry && Universal.ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.ZACALINFMessaging, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now))
			{
				topLevelMenu = new ZMenuItem(ResString.GetMultilingualString("ZJobVoyageForm|Menu|ZAMessages", "Messages"));

				if (CanSendOriginal)
				{
					MenuItem sendOriginal = new ZMenuItem(ResString.GetMultilingualString("ZJobVoyageForm|Menu|ZASendOriginal", "Send ZA CALINF Message"), OnSendOriginalClick);
					topLevelMenu.MenuItems.Add(sendOriginal);
				}

				if (CanSendAmendment)
				{
					MenuItem sendAmendment = new ZMenuItem(ResString.GetMultilingualString("ZJobVoyageForm|Menu|ZASendAmendment", "Send ZA CALINF Amendment"), OnSendAmendmentClick);
					topLevelMenu.MenuItems.Add(sendAmendment);
				}

				if (CanSendCancellation)
				{
					MenuItem sendCancellation = new ZMenuItem(ResString.GetMultilingualString("ZJobVoyageForm|Menu|ZASendCancellation", "Send ZA CALINF Cancellation"), OnSendCancellationClick);
					topLevelMenu.MenuItems.Add(sendCancellation);
				}
			}

			return topLevelMenu;
		}
		#endregion

		#region Implementation
		bool IsAllowedInCurrentCountry => GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica;

		bool CanSendOriginal => voyage != null && !voyage.Messages.Any(); // Rules to be defined on message sender
		bool CanSendAmendment => voyage != null && voyage.Messages.Any(); // Rules to be defined on message sender
		bool CanSendCancellation => voyage != null && voyage.Messages.Any(); // Rules to be defined on message sender
		#endregion

		#region Menu Events

		void OnSendOriginalClick(object sender, EventArgs e)
		{
			if (CanSendOriginal)
			{
				SendCalinfEdiMessage(MessageSubTypes.Create);
			}
		}

		void OnSendAmendmentClick(object sender, EventArgs e)
		{
			if (CanSendAmendment)
			{
				SendCalinfEdiMessage(MessageSubTypes.Change);
			}
		}

		void OnSendCancellationClick(object sender, EventArgs e)
		{
			if (CanSendCancellation)
			{
				SendCalinfEdiMessage(MessageSubTypes.Withdraw);
			}
		}

		void SendCalinfEdiMessage(MessageSubTypes messageSubType)
		{
			if (SaveDataFirst.Confirm(voyage, Form) && AskToProceedDespiteErrors())
			{
				var notificationCollector = new MessageNotificationCollector();
				var messageManager = new CALINFMessageManager(new CALINFMessageData(voyage), notificationCollector);
				if (!messageManager.SendMessage(messageSubType))
				{
					ShowMessageSendingResult(notificationCollector);
				}
			}
		}

		bool AskToProceedDespiteErrors()
		{
			var messageSendingNotification = voyage.GetJobVoyageMessageSendingNotification();
			if (!messageSendingNotification.IsEmpty)
			{
				return Globals.Message.Show(messageSendingNotification, "Proceed with errors?", MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes;
			}
			return true;
		}

		void ShowMessageSendingResult(IMessageNotificationCollector notification)
		{
			var notifications = notification?.Notifications;
			if (notifications != null)
			{
				if (notifications.ContainsError())
				{
					notification.ShowError(notifications.ErrorNotificationsAsString(), Res.GetString("498288B5-8CDA-4A0F-B34E-DD8AC42253D4", "Error in Message Sending"));
				}
				if (notifications.ContainsInformation())
				{
					notification.ShowInformation(notifications.InformationNotificationsAsString(), Res.GetString("C5E4D627-F6D9-422E-AE27-4F9EBF46E6ED", "Message Sending Result"));
				}
			}
		}

		#endregion

		readonly JobVoyage voyage;
	}
}
