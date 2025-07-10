using System;
using System.Windows.Forms;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public class OutturnGateInOutMenu : ZMenuItem
	{
		public OutturnGateInOutMenu(AsycudaManifestHeader outturn) : base(ResString.GetMultilingualString("OutturnAndGateInOut|OutturnAndGateInOutSendMenuItem", "&Gate In/Out"))
		{
			this.outturn = outturn;
			this.outturn.AMA_ManifestTypeInfo.ValueChanged += AMA_ManifestTypeInfo_ValueChanged;

			SendOriginalMenuItem = new ZMenuItem(ResString.GetMultilingualString("OutturnAndGateInOut|OutturnAndGateInOutSendMenuItem|SendOriginal", "Send &Original"), (s, e) => SendGOVGIO(MessageSubTypes.Create));
			SendAmendemntMenuItem = new ZMenuItem(ResString.GetMultilingualString("OutturnAndGateInOut|OutturnAndGateInOutSendMenuItem|SendAmendment", "Send &Amendment"), (s, e) => SendGOVGIO(MessageSubTypes.Change));
			SendCancellationMenuItem = new ZMenuItem(ResString.GetMultilingualString("OutturnAndGateInOut|OutturnAndGateInOutSendMenuItem|SendCancellation", "Send &Cancellation"), (s, e) => SendGOVGIO(MessageSubTypes.Withdraw));
			MenuItems.AddRange(new MenuItem[]
			{
				SendOriginalMenuItem,
				SendAmendemntMenuItem,
				SendCancellationMenuItem
			});

			AMA_ManifestTypeInfo_ValueChanged(this.outturn, EventArgs.Empty);
		}

		void AMA_ManifestTypeInfo_ValueChanged(object sender, EventArgs e) => Enabled = outturn.AMA_ManifestType.IsEmpty;

		void SendGOVGIO(MessageSubTypes messageSubType)
		{
			if (SaveDataFirst.Confirm(outturn, MainForm) && AskToProceedDespiteErrors())
			{
				new GOVGIOMessageManager(new GOVGIOMessageHeader(outturn), new MessageNotificationCollector()).SendMessage(messageSubType);
			}
		}

		bool AskToProceedDespiteErrors()
		{
			var messageSendingNotification = outturn.GetGateInOutMessageSendingNotification();
			if (!messageSendingNotification.IsEmpty)
			{
				return Globals.Message.Show(messageSendingNotification, "Proceed with errors?", MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes;
			}

			return true;
		}

		protected override void OnPopup(EventArgs e)
		{
			var hasAcceptedMessage = outturn.GetLastAcceptedGOVGIOEDIMessage() != null;
			SendOriginalMenuItem.Visible = !hasAcceptedMessage;
			SendAmendemntMenuItem.Visible = hasAcceptedMessage;
			SendCancellationMenuItem.Visible = hasAcceptedMessage;
			base.OnPopup(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && outturn != null)
			{
				outturn.AMA_ManifestTypeInfo.ValueChanged -= AMA_ManifestTypeInfo_ValueChanged;
			}

			base.Dispose(disposing);
		}

		ZForm MainForm => (ZForm)GetMainMenu()?.GetForm();

		protected readonly ZMenuItem SendOriginalMenuItem;
		protected readonly ZMenuItem SendAmendemntMenuItem;
		protected readonly ZMenuItem SendCancellationMenuItem;
		readonly AsycudaManifestHeader outturn;
	}
}
