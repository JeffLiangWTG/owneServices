using System;
using System.Windows.Forms;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public class OutturnMenu : ZMenuItem
	{
		public OutturnMenu(AsycudaManifestHeader header) : base(Captions.Outturn)
		{
			this.header = header;
			this.header.GateInOutMessageTypeInfo.ValueChanged += GateInOutMessageTypeInfo_ValueChanged;

			BuildMenus();
			GateInOutMessageTypeInfo_ValueChanged(this.header, EventArgs.Empty);
		}

		void GateInOutMessageTypeInfo_ValueChanged(object sender, EventArgs e) => Enabled = header.GateInOutMessageType.IsEmpty;

		void BuildMenus()
		{
			sendOutturnCostcoMenuItem = new ZMenuItem(Captions.SendOutturnCostco, (sender, args) => SendCOSTCO(MainForm, header, MessageSubTypes.Create));
			sendAmendmentOutturnMenuItem = new ZMenuItem(Captions.SendAmendmentOutturn, (sender, args) => SendCOSTCO(MainForm, header, MessageSubTypes.Change));
			sendCancellationOutturnMenuItem = new ZMenuItem(Captions.SendCancellationOutturn, (sender, args) => SendCOSTCO(MainForm, header, MessageSubTypes.Withdraw));
			MenuItems.AddRange(new MenuItem[]
			{
				sendOutturnCostcoMenuItem,
				sendAmendmentOutturnMenuItem,
				sendCancellationOutturnMenuItem
			});
		}

		static void SendCOSTCO(ZForm mainforn, AsycudaManifestHeader outturn, MessageSubTypes messageSubType)
		{
			if (SaveDataFirst.Confirm(outturn, mainforn))
			{
				new COSTCOMessageManager(new COSTCOHeader(outturn), new MessageNotificationCollector()).SendMessage(messageSubType);
			}
		}

		protected override void OnPopup(EventArgs e)
		{
			var hasAcceptedMessage = header.GetLastAcceptedCOSTCOEDIMessage() != null;
			sendOutturnCostcoMenuItem.Visible = !hasAcceptedMessage;
			sendAmendmentOutturnMenuItem.Visible = hasAcceptedMessage;
			sendCancellationOutturnMenuItem.Visible = hasAcceptedMessage;
			base.OnPopup(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && header != null)
			{
				header.GateInOutMessageTypeInfo.ValueChanged -= GateInOutMessageTypeInfo_ValueChanged;
			}

			base.Dispose(disposing);
		}

		static class Captions
		{
			public static MultilingualString Outturn => ResString.GetMultilingualString("OutturnAndGateInOutMenu|Outturn", "&Outturn");
			public static MultilingualString SendOutturnCostco => ResString.GetMultilingualString("OutturnAndGateInOutMenu|SendOutturnCostco", "Send &Original");
			public static MultilingualString SendAmendmentOutturn => ResString.GetMultilingualString("OutturnAndGateInOutMenu|SendAmendmentOutturn", "Send &Amendment");
			public static MultilingualString SendCancellationOutturn => ResString.GetMultilingualString("OutturnAndGateInOutMenu|SendCancellationOutturn", "Send &Cancellation");
		}

		ZForm MainForm => (ZForm)GetMainMenu()?.GetForm();

		protected ZMenuItem sendOutturnCostcoMenuItem;
		protected ZMenuItem sendAmendmentOutturnMenuItem;
		protected ZMenuItem sendCancellationOutturnMenuItem;
		readonly AsycudaManifestHeader header;
	}
}
