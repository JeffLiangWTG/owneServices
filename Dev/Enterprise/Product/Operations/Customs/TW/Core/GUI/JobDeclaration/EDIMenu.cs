using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.Customs.TW.DataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public EDIMenu()
		{
			Popup += Menu_Popup;
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		bool IsImport => Declaration?.IsImport ?? false;

		bool IsExport => Declaration?.IsExport ?? false;

		bool IsSubmitTypeITF => (Declaration?.JE_ApplicationCode ?? ZString.Empty) == Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;

		protected override bool DisplayGenerateEntriesMenuOption => true;

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			SendCustomsDeclarationMenuItem = new ZMenuItem(Constants.SendCustomsDeclaration, new EventHandler((sender, e) => SendCustomsDeclaration(false)));
			MenuItems.Add(SendCustomsDeclarationMenuItem);

			SendImportCustomsDeclarationForAircraftPartsMenuItem = new ZMenuItem(Constants.SendImportCustomsDeclarationForAircraftParts, new EventHandler((sender, e) => SendImportCustomsDeclarationForAircraftParts()));
			MenuItems.Add(SendImportCustomsDeclarationForAircraftPartsMenuItem);

			SendControllingImportCustomsDeclarationMenuItem = new ZMenuItem(Constants.SendControllingImportCustomsDeclaration, new EventHandler((sender, e) => SendCustomsDeclaration(true)));
			MenuItems.Add(SendControllingImportCustomsDeclarationMenuItem);

			SendGoodsExaminationApplicationMenuItem = new ZMenuItem(Constants.SendGoodsExaminationApplication, new EventHandler(SendGoodsExaminationApplicationMenuItem_Click));
			MenuItems.Add(SendGoodsExaminationApplicationMenuItem);

			SendAdditionalDocumentMessageMenuItem = new ZMenuItem(Constants.SendAdditionalDocumentMessage, new EventHandler(SendAdditionalDocumentMessageMenuItem_Click));
			MenuItems.Add(SendAdditionalDocumentMessageMenuItem);

			SendMessagesToNCATKMenuItem = new ZMenuItem(Constants.SendMessagesToNCATK);
			SendApplicationMessageForCertificateOfOriginMenuItem = new ZMenuItem(Constants.SendApplicationMessageForCertificateOfOrigin, new EventHandler(SendApplicationMessageForCertificateOfOriginMenuItem_Click));
			SendMessagesToNCATKMenuItem.MenuItems.Add(SendApplicationMessageForCertificateOfOriginMenuItem);
			MenuItems.Add(SendMessagesToNCATKMenuItem);
			SendMessageForQuarantineApplicationMenuItem = new ZMenuItem(Constants.SendMessageForQuarantineApplication, new EventHandler(SendMessageForQuarantineApplicationMenuItem_Click));
			SendMessagesToNCATKMenuItem.MenuItems.Add(SendMessageForQuarantineApplicationMenuItem);

			SendFoodAndDrugImportApplicationMenuItem = new ZMenuItem(Constants.SendFoodAndDrugImportApplication, new EventHandler(SendFoodAndDrugImportApplicationMenuItem_Click));
			SendMessagesToNCATKMenuItem.MenuItems.Add(SendFoodAndDrugImportApplicationMenuItem);

			SendNX101NCATKMessageMenuItem = new ZMenuItem(Constants.SendNX101NCATKMessage, new EventHandler(SendNX101NCATKMessageMenuItem_Click));
			SendMessagesToNCATKMenuItem.MenuItems.Add(SendNX101NCATKMessageMenuItem);

			SendLicensingMessagesMenuItem = new ZMenuItem(Constants.SendLicensingMessages);
			MenuItems.Add(SendLicensingMessagesMenuItem);

			SendNX101MessageMenuItem = new ZMenuItem(Constants.SendNX101Messages, new EventHandler(SendNX101MessageMenuItem_Click));
			SendLicensingMessagesMenuItem.MenuItems.Add(SendNX101MessageMenuItem);

			SendNX201_01MessageMenuItem = new ZMenuItem(Constants.SendNX201_01Messages, new EventHandler(SendNX201_01MessageMenuItem_Click));
			SendLicensingMessagesMenuItem.MenuItems.Add(SendNX201_01MessageMenuItem);

			SendNX201_07MessageMenuItem = new ZMenuItem(Constants.SendNX201_07Messages, new EventHandler(SendNX201_07MessageMenuItem_Click));
			SendLicensingMessagesMenuItem.MenuItems.Add(SendNX201_07MessageMenuItem);

			SendNX301MessageMenuItem = new ZMenuItem(Constants.SendNX301Messages, new EventHandler(SendNX301MessageMenuItem_Click));
			SendLicensingMessagesMenuItem.MenuItems.Add(SendNX301MessageMenuItem);

			SendNX301_AXMessageMenuItem = new ZMenuItem(Constants.SendNX301_AXMessages, new EventHandler(SendNX301_AXMessageMenuItem_Click));
			SendLicensingMessagesMenuItem.MenuItems.Add(SendNX301_AXMessageMenuItem);

			SendNX301_DNMessageMenuItem = new ZMenuItem(Constants.SendNX301_DNMessages, new EventHandler(SendNX301_DNMessageMenuItem_Click));
			SendLicensingMessagesMenuItem.MenuItems.Add(SendNX301_DNMessageMenuItem);

			SendNX401MessageMenuItem = new ZMenuItem(Constants.SendNX401Messages, new EventHandler(SendNX401MessageMenuItem_Click));
			SendLicensingMessagesMenuItem.MenuItems.Add(SendNX401MessageMenuItem);

			SendNX601MessageMenuItem = new ZMenuItem(Constants.SendNX601Messages, new EventHandler(SendNX601MessageMenuItem_Click));
			SendLicensingMessagesMenuItem.MenuItems.Add(SendNX601MessageMenuItem);

			SendNX603MessageMenuItem = new ZMenuItem(Constants.SendNX603Messages, new EventHandler(SendNX603MessageMenuItem_Click));
			SendLicensingMessagesMenuItem.MenuItems.Add(SendNX603MessageMenuItem);
		}

		internal ZMenuItem SendCustomsDeclarationMenuItem;
		internal ZMenuItem SendImportCustomsDeclarationForAircraftPartsMenuItem;
		internal ZMenuItem SendControllingImportCustomsDeclarationMenuItem;
		internal ZMenuItem SendGoodsExaminationApplicationMenuItem;
		internal ZMenuItem SendAdditionalDocumentMessageMenuItem;
		internal ZMenuItem SendMessagesToNCATKMenuItem;
		internal ZMenuItem SendApplicationMessageForCertificateOfOriginMenuItem;
		internal ZMenuItem SendMessageForQuarantineApplicationMenuItem;
		internal ZMenuItem SendFoodAndDrugImportApplicationMenuItem;
		internal ZMenuItem SendLicensingMessagesMenuItem;
		internal ZMenuItem SendNX101MessageMenuItem;
		internal ZMenuItem SendNX101NCATKMessageMenuItem;
		internal ZMenuItem SendNX201_01MessageMenuItem;
		internal ZMenuItem SendNX201_07MessageMenuItem;
		internal ZMenuItem SendNX301MessageMenuItem;
		internal ZMenuItem SendNX301_AXMessageMenuItem;
		internal ZMenuItem SendNX301_DNMessageMenuItem;
		internal ZMenuItem SendNX401MessageMenuItem;
		internal ZMenuItem SendNX601MessageMenuItem;
		internal ZMenuItem SendNX603MessageMenuItem;

		public static class Constants
		{
			public static MultilingualString SendCustomsDeclaration => ResString.GetMultilingualString("JobDeclarationForm|Menu|SendCustomsDeclaration", "Send Customs Declaration");

			public static MultilingualString SendImportCustomsDeclaration => ResString.GetMultilingualString("JobDeclarationForm|Menu|SendImportCustomsDeclaration", "Send Import Customs Declaration");

			public static MultilingualString SendImportCustomsDeclarationForAircraftParts => ResString.GetMultilingualString("JobDeclarationForm|Menu|SendImportCustomsDeclarationForAircraftParts", "Send Import Customs Declaration (Aircraft Parts)");

			public static MultilingualString SendControllingImportCustomsDeclaration => ResString.GetMultilingualString("JobDeclarationForm|Menu|SendControllingImportCustomsDeclaration", "Send Controlling Import Customs Declaration");

			public static MultilingualString SendExportCustomsDeclaration => ResString.GetMultilingualString("JobDeclarationForm|Menu|SendExportCustomsDeclaration", "Send Export Customs Declaration");

			public static MultilingualString SendGoodsExaminationApplication => ResString.GetMultilingualString("JobDeclarationForm|Menu|SendGoodsExaminationApplication", "Send Goods Examination Application");

			public static MultilingualString SendAdditionalDocumentMessage => ResString.GetMultilingualString("JobDeclarationForm|Menu|SendAdditionalDocumentMessage", "Send Additional Document Message");

			public static MultilingualString SendMessagesToNCATK => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendMessagesToNCATK", "Send messages to NCATK");

			public static MultilingualString SendApplicationMessageForCertificateOfOrigin => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendApplicationMessageForCertificateOfOrigin", "Send Application Message for Certificate of Origin (X101)");

			public static MultilingualString SendMessageForQuarantineApplication => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendMessageforQuarantineApplication", "Send Message for Quarantine Application (NX401)");

			public static MultilingualString MissingControllingMessageHeader(string messageType) => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|CannotSendNCATKMessage", "You have not created a {0} message. Please visit the ‘Licensing’ tab to create a {0} message by selecting ‘{0}’ in the ‘Message Type’ column before proceeding.", messageType);

			public static MultilingualString SendFoodAndDrugImportApplication => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendFoodAndDrugImportApplication", "Send Food and Drug Import Application (NX601)");

			public static MultilingualString MissingEntryNumber(string messageType) => ResString.GetMultilingualString("AD3CE8CF-6446-45EC-98A5-4C72B420AD7F", "You have not created a {0} message. Please Generate entry number first.", messageType);

			public static MultilingualString SendNX101NCATKMessage => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX101NCATKMessage", "Send Application Message for Certificate of Origin (NX101)");

			public static MultilingualString SendLicensingMessages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendLicensingMessages", "Send Licensing Messages");

			public static MultilingualString SendNX101Messages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX101Messages", "Send Application Message for Certificate of Origin (NX101)");

			public static MultilingualString SendNX201_01Messages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX201_01Messages", "Send Application Message for I/E Permit (NX201_01)");

			public static MultilingualString SendNX201_07Messages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX201_07Messages", "Send Application Message for Exhibition (NX201_07)");

			public static MultilingualString SendNX301Messages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX301Messages", "Send Message for Inspection Application (NX301)");

			public static MultilingualString SendNX301_DNMessages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX301_DNMessages", "Send Inspection Application Message for Alcoholic Beverages (NX301_DN)");

			public static MultilingualString SendNX301_AXMessages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX301_AXMessages", "Send Inspection Application Message for Fodder (NX301_AX)");

			public static MultilingualString SendNX401Messages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX401Messages", "Send Message for Quarantine Application (NX401)");

			public static MultilingualString SendNX601Messages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX601Messages", "Send Inspection Application Message for Food and Chinese Medicinal Materials (NX601)");

			public static MultilingualString SendNX603Messages => ResString.GetMultilingualString("TWJobDeclarationForm|Menu|SendNX603Messages", "Send Inspection Application Message for Medical Equipment and Drug (NX603)");
		}

		void Menu_Popup(object sender, EventArgs e)
		{
			var isImport = IsImport;
			var isExport = IsExport;
			var showSendCustomsDeclarationMenuItem = !IsSubmitTypeITF;
			if (showSendCustomsDeclarationMenuItem)
			{
				if (isImport)
				{
					SendCustomsDeclarationMenuItem.Caption = Constants.SendImportCustomsDeclaration;
				}
				else if (isExport)
				{
					SendCustomsDeclarationMenuItem.Caption = Constants.SendExportCustomsDeclaration;
				}
				else
				{
					SendCustomsDeclarationMenuItem.Caption = Constants.SendCustomsDeclaration;
				}
			}
			SendCustomsDeclarationMenuItem.Visible = showSendCustomsDeclarationMenuItem;

			var isImportAndSubmitTypeNotITF = isImport && !IsSubmitTypeITF;
			SendAdditionalDocumentMessageMenuItem.Visible = !IsSubmitTypeITF;
			SendControllingImportCustomsDeclarationMenuItem.Visible = isImportAndSubmitTypeNotITF;
			SendGoodsExaminationApplicationMenuItem.Visible = isImportAndSubmitTypeNotITF;
			SendFoodAndDrugImportApplicationMenuItem.Visible = isImport;
			SendImportCustomsDeclarationForAircraftPartsMenuItem.Visible = isImportAndSubmitTypeNotITF;

			var showSendNX101MessageMenuItem = isExport;
			var showSendNX201_01MessageMenuItem = RegistryHelper.EnableNX201_01;
			var showSendNX201_07MessageMenuItem = isImport && RegistryHelper.EnableNX201_07;
			var showSendNX301MessageMenuItem = isImport && RegistryHelper.EnableNX301;
			var showSendNX301_DNMessageMenuItem = isImport && RegistryHelper.EnableNX301_DN;
			var showSendNX301_AXMessageMenuItem = isImport && RegistryHelper.EnableNX301_AX;
			var showSendNX401MessageMenuItem = RegistryHelper.EnableNX401;
			var showSendNX601MessageMenuItem = isImport && RegistryHelper.EnableNX601;
			var showSendNX603MessageMenuItem = isImport && RegistryHelper.EnableNX603;

			SendNX101MessageMenuItem.Visible = showSendNX101MessageMenuItem;
			SendNX201_01MessageMenuItem.Visible = showSendNX201_01MessageMenuItem;
			SendNX201_07MessageMenuItem.Visible = showSendNX201_07MessageMenuItem;
			SendNX301MessageMenuItem.Visible = showSendNX301MessageMenuItem;
			SendNX301_DNMessageMenuItem.Visible = showSendNX301_DNMessageMenuItem;
			SendNX301_AXMessageMenuItem.Visible = showSendNX301_AXMessageMenuItem;
			SendNX401MessageMenuItem.Visible = showSendNX401MessageMenuItem;
			SendNX601MessageMenuItem.Visible = showSendNX601MessageMenuItem;
			SendNX603MessageMenuItem.Visible = showSendNX603MessageMenuItem;
			SendLicensingMessagesMenuItem.Visible = showSendNX101MessageMenuItem ||
				showSendNX201_01MessageMenuItem ||
				showSendNX201_07MessageMenuItem ||
				showSendNX301MessageMenuItem ||
				showSendNX301_DNMessageMenuItem ||
				showSendNX301_AXMessageMenuItem ||
				showSendNX401MessageMenuItem ||
				showSendNX601MessageMenuItem ||
				showSendNX603MessageMenuItem;
		}

		#region Send licensing messages
		void SendNX101MessageMenuItem_Click(object sender, EventArgs e)
		{
			SendNXMMessage(ControllingMessageTypeList.Codes.NX101, SendNX101MessageMenuItem.Caption);
		}

		void SendNX201_01MessageMenuItem_Click(object sender, EventArgs e)
		{
			SendNXMMessage(ControllingMessageTypeList.Codes.NX201_01, SendNX201_01MessageMenuItem.Caption);
		}

		void SendNX201_07MessageMenuItem_Click(object sender, EventArgs e)
		{
			SendNXMMessage(ControllingMessageTypeList.Codes.NX201_07);
		}

		void SendNX301MessageMenuItem_Click(object sender, EventArgs e)
		{
			SendNXMMessage(ControllingMessageTypeList.Codes.NX301);
		}

		void SendNX301_AXMessageMenuItem_Click(object sender, EventArgs e)
		{
			SendNXMMessage(ControllingMessageTypeList.Codes.NX301_AX);
		}

		void SendNX301_DNMessageMenuItem_Click(object sender, EventArgs e)
		{
			SendNXMMessage(ControllingMessageTypeList.Codes.NX301_DN);
		}

		void SendNX401MessageMenuItem_Click(object sender, EventArgs e)
		{
			SendNXMMessage(ControllingMessageTypeList.Codes.NX401);
		}

		void SendNX601MessageMenuItem_Click(object sender, EventArgs e)
		{
			SendNXMMessage(ControllingMessageTypeList.Codes.NX601);
		}

		void SendNX603MessageMenuItem_Click(object sender, EventArgs e)
		{
			SendNXMMessage(ControllingMessageTypeList.Codes.NX603);
		}

		#endregion

		#region Send messages to NCATK
		void SendApplicationMessageForCertificateOfOriginMenuItem_Click(object sender, EventArgs e)
		{
			SendApplicationMessageForCertificateOfOrigin(new NCATKMessageSendingObjectParent(Declaration, ControllingMessageTypeList.Codes.X101, SendApplicationMessageForCertificateOfOriginMenuItem.Caption));
		}

		void SendMessageForQuarantineApplicationMenuItem_Click(object sender, EventArgs e)
		{
			SendApplicationMessageForCertificateOfOrigin(new NCATKMessageSendingObjectParent(Declaration, ControllingMessageTypeList.Codes.NX401, SendMessageForQuarantineApplicationMenuItem.Caption));
		}

		void SendFoodAndDrugImportApplicationMenuItem_Click(object sender, EventArgs e)
		{
			SendApplicationMessageForCertificateOfOrigin(new NCATKMessageSendingObjectParent(Declaration, ControllingMessageTypeList.Codes.NX601, SendFoodAndDrugImportApplicationMenuItem.Caption));
		}

		void SendNX101NCATKMessageMenuItem_Click(object sender, EventArgs e)
		{
			SendApplicationMessageForCertificateOfOrigin(new NCATKMessageSendingObjectParent(Declaration, ControllingMessageTypeList.Codes.NX101, SendNX101NCATKMessageMenuItem.Caption));
		}

		void SendApplicationMessageForCertificateOfOrigin(NCATKMessageSendingObjectParent wrapper)
		{
			var declaration = Declaration;
			if (PreSaveDeclaration(declaration) && MessageManager.CheckDeclarationNumber(declaration, new MessageNotificationCollector()))
			{
				if (wrapper.SendingObjectsCollection.Count <= 0)
				{
					Globals.Message.Show(Constants.MissingControllingMessageHeader(wrapper.MessageType), MessageManager.Constants.CannotSendMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					var continueWithSend = true;
					using (var form = new ControllingMessageSendingForm(wrapper))
					{
						continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
					}
					if (continueWithSend)
					{
						SendNCATKMessages(wrapper);
					}
				}
			}
		}

		void SendNCATKMessages(ControllingMessageSendingObjectParent wrapper)
		{
			var continueWithSend = true;
			var declaration = Declaration;
			var entryHeader = declaration.EntryHeader;
			if (entryHeader.EntryNumber.IsEmpty)
			{
				using (entryHeader.GetGenerateEntryNumberExceptionSupporter())
				{
					try
					{
						declaration.AllocateEntryNumberToEntry();
						declaration.Factory.Save();
					}
					catch (GenerateEntryNumberException ex)
					{
						continueWithSend = false;
						declaration.ReloadSafe();
						Globals.Message.Show(ex.Message, MessageManager.Constants.CannotSendMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
			if (continueWithSend)
			{
				var messagingHelper = new NCATKUniversalMessagingHelper(wrapper);
				var messagesCount = messagingHelper.SendUniversalMessage();
				if (messagesCount > 0)
				{
					wrapper.SendingObjectsCollection.Cast<NCATKMessageSendingObject>().Where(obj => obj.ShouldSend).Select(obj => obj.Header).ForEach(x => x.Messages.Load());
					Globals.Message.ShowInformation(Res.GetString("6A08090A-410B-4B4B-91C3-C1AA867E7BA0", "{0} message(s) queued for sending", messagesCount));
				}
			}
		}
		#endregion

		void SendNXMMessage(string messageType, string caption = "")
		{
			var declaration = Declaration;
			if (PreSaveDeclaration(declaration))
			{
				var wrapper = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(Declaration, messageType, caption);
				if (wrapper.SendingObjectsCollection.Count > 0)
				{
					var notificationCollector = new MessageNotificationCollector();
					var ediMessageType = wrapper.SendingObjectsCollection.Cast<LicensingMessageSendingObject>().FirstOrDefault().EM_MessageType;
					var continueWithSend = TWMessage.ShouldLicensingMessageAllocateEntryNumber(ediMessageType) ? MessageManager.CheckEntries(declaration, notificationCollector) && MessageManager.CheckDeclarationNumber(declaration, notificationCollector) : MessageManager.CheckDeclarationNumber(declaration, notificationCollector);
					if (continueWithSend)
					{
						using (var form = new NXMDocumentForm(wrapper))
						{
							continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
						}
						if (continueWithSend)
						{
							var messageManager = new LicensingMultiMessageManager(wrapper, wrapper.MessageType);
							messageManager.SendMessages(new SendsMessagesToCustomsGUI());
						}
					}
				}
				else
				{
					Globals.Message.Show(Constants.MissingControllingMessageHeader(wrapper.MessageType), MessageManager.Constants.CannotSendMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		void SendCustomsDeclaration(ZBool includeControllingMessageInformation)
		{
			if (MessageType == MessageTypeList.Codes.ECD || MessageType == MessageTypeList.Codes.ICD)
			{
				SendCustomsMessageAdditionalDocument(MessageType, includeControllingMessageInformation, SendCustomsDeclarationMenuItem.Caption);
			}
			else
			{
				SendCustomsMessage(MessageType, SendCustomsDeclarationMenuItem.Caption);
			}
		}

		void SendImportCustomsDeclarationForAircraftParts()
		{
			SendCustomsMessageAdditionalDocument(MessageTypeList.Codes.CAA, false, SendImportCustomsDeclarationForAircraftPartsMenuItem.Caption);
		}

		void SendCustomsMessage(ZString messageType, ZString menuCaption)
		{
			var sendingObjectParent = new JobDeclarationMessageSendingObjectParent(Declaration, messageType);
			sendingObjectParent.MenuCaption = menuCaption;
			SendCustomsMessage(messageType, sendingObjectParent, (wrapper) => new MessageSendingForm(wrapper));
		}

		void SendCustomsMessageAdditionalDocument(ZString messageType, ZBool includeControllingMessageInformation, ZString menuCaption)
		{
			var sendingObjectParent = new AdditionalDocumentMessageSendingObjectParent(Declaration, messageType, includeControllingMessageInformation);
			sendingObjectParent.MenuCaption = menuCaption;
			SendCustomsMessage(messageType, sendingObjectParent, (wrapper) => new AdditionalDocumentForm(wrapper as AdditionalDocumentMessageSendingObjectParent));
		}

		void SendCustomsMessage(ZString messageType, JobDeclarationMessageSendingObjectParent wrapper, Func<JobDeclarationMessageSendingObjectParent, MessageSendingForm> getFormFunc)
		{
			var declaration = Declaration;
			if (PreSaveDeclaration(declaration))
			{
				var messageManagerWrapper = new DeclarationMessageManagerWrapper(declaration, wrapper, new MessageNotificationCollector(), declaration.MessageInitiator, messageType);
				messageManagerWrapper.PerformFunctionOperationalAction(true, () =>
				{
					var result = false;
					using (var form = getFormFunc.Invoke(wrapper))
					{
						result = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
					}
					return result;
				});
			}
		}

		void SendGoodsExaminationApplicationMenuItem_Click(object sender, EventArgs e)
		{
			if (ShouldSendMessage())
			{
				SendCustomsMessage(MessageTypeList.Codes.IEA, SendGoodsExaminationApplicationMenuItem.Caption);
			}
		}

		bool ShouldSendMessage()
		{
			var result = true;
			var entry = Declaration.EntryHeader;
			if (entry == null || entry.EntryNumber.IsEmpty)
			{
				result = false;
				Globals.Message.Show(Res.GetString("426CFB28-F3D1-4254-B027-163B44418D50", "The entry does not have entry number."), MessageManager.Constants.CannotSendMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return result;
		}

		void SendAdditionalDocumentMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (ShouldSendMessage())
			{
				SendCustomsMessageAdditionalDocument(MessageTypeList.Codes.ADM, false, SendAdditionalDocumentMessageMenuItem.Caption);
			}
		}

		ZString MessageType
		{
			get
			{
				var result = ZString.Empty;
				if (IsExport)
				{
					result = MessageTypeList.Codes.ECD;
				}
				else if (IsImport)
				{
					result = MessageTypeList.Codes.ICD;
				}
				return result;
			}
		}
	}
}
