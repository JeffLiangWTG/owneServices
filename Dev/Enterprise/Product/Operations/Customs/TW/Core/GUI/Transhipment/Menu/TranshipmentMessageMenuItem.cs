using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public class TranshipmentMessageMenuItem : ZMenuItem
	{
		public TranshipmentMessageMenuItem(CusInBondHeader header)
			: base(ResString.GetMultilingualString("Transhipment|MessageMenuItem", "M&essaging"))
		{
			Header = header;
			AddMenuItems();
		}

		#region Constants
		public static class Constants
		{
			public static class Message
			{
				public static string DataNotSavedNotification => ResString.GetMultilingualString("TranshipmentMenu|7F290EE9-9726-49B8-87EC-EC363F61D6C3", "The Transhipment data has not yet been saved. Do you want to save and proceed?");
			}

			public static class Caption
			{
				public static string SaveDataCaption => ResString.GetMultilingualString("TranshipmentMenu|6C0FBCB9-E19C-426C-A2AB-1CB2296EC17B", "Save Data");

				public static ResourceString SendTranshipmentApplication => ResString.GetMultilingualString("Transhipment|MessageMenuItem|SendTranshipmentApplication", "Send Transhipment Application");

				public static MultilingualString CannotSendMessage => ResString.GetMultilingualString("Transhipment|MessageMenuItem|CannotSendMessageCaption", "Cannot Send Message");

				public static MultilingualString SendMessage => ResString.GetMultilingualString("Transhipment|MessageMenuItem|SendMessageCaption", "Send Message");
			}
		}
		#endregion

		public CusInBondHeader Header
		{
			get { return fHeader; }
			internal set
			{
				if (fHeader != value)
				{
					fHeader = value;
					if (!Header.HasMessageInitiator)
					{
						Header.MessageInitiator = new SendsMessagesToCustomsGUI();
					}
				}
			}
		}

		CusInBondHeader fHeader;

		void AddMenuItems()
		{
			MenuItems.Add(new ZMenuItem(Constants.Caption.SendTranshipmentApplication, SendTranshipmentApplicationClick));
		}

		protected bool PreSendInBondHeader(CusInBondHeader parent)
		{
			bool result = true;

			if (parent.HasChanges)
			{
				if (Globals.Message.Show(Constants.Message.DataNotSavedNotification, Constants.Caption.SaveDataCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					result = MainForm.FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		ZForm MainForm => (ZForm)GetMainMenu()?.GetForm();

		void SendTranshipmentApplicationClick(object sender, EventArgs e)
		{
			var header = Header;
			if (PreSendInBondHeader(header))
			{
				bool continueWithSend = true;

				var credential = header.GetCredential();
				if (credential == null)
				{
					continueWithSend = false;
					Globals.Message.Show(ResString.GetMultilingualString("Transhipment|MessageMenuItem|CannotSendMessage_BadCredential", "Please create a valid Credential for {0} on {1} - Brokerage.", header.BH_CustomsProfile, header.BH_GS_NKCusAgent),
					Constants.Caption.CannotSendMessage, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else if (credential.GP_PasswordStatus != PasswordStatusList.Codes.Valid)
				{
					continueWithSend = Globals.Message.Show(ResString.GetMultilingualString("Transhipment|MessageMenuItem|CannotSendMessage_Continue", "The credential (for {0} on {1}) you have selected is not valid. Sending this message will most likely result in an error. Do you still want to proceed with this action?", header.BH_CustomsProfile, header.BH_GS_NKCusAgent), Constants.Caption.SendMessage, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
				}

				if (continueWithSend)
				{
					var transhipmentWrapper = new TranshipmentMessageSendingObjectParent(header, MessageTypeList.Codes.TRA);
					using (var form = new TranshipmentMessageSendingForm(transhipmentWrapper))
					{
						continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
					}

					if (continueWithSend)
					{
						var messageManager = new TranshipmentMultiMessageManager(transhipmentWrapper, MessageTypeList.Codes.TRA, new MessageNotificationCollector());
						messageManager.SendMessages(header.MessageInitiator);
					}
				}
			}
		}
	}
}
