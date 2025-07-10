using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class MessageManagementMenu : ZMenuItem
	{
		public MessageManagementMenu(MultiMessageManager manager)
		{
			this.Text = manager.MessagingApplicationName;
			this.manager = manager;
			MenuItems.Add(new ZMenuItem((NoResString)"Placeholder; menu items added OnPopup"));
		}

		#region Click Handlers

		protected void SendMessages_Click(object sender, EventArgs e)
		{
			using (GetCellSuspender())
			{
				if (CheckErrorAndFireSaveButtonIfNeeded())
				{
					SendMessagesClickCore(sender);
				}
			}
		}

		protected virtual bool SendMessagesClickCore(object sender)
		{
			if (IsMessagingAllowed && manager.SendOriginalMessages(Sender).Any())
			{
				SaveFactory();
				return true;
			}
			return false;
		}

		void AmendMessages_Click(object sender, EventArgs e)
		{
			if (IsMessagingAllowed && CheckErrorAndFireSaveButtonIfNeeded() && manager.AmendMessages(Sender))
			{
				SaveFactory();
			}
		}

		void WithdrawMessages_Click(object sender, EventArgs e)
		{
			if (IsMessagingAllowed && CheckErrorAndFireSaveButtonIfNeeded() && manager.WithdrawMessages(Sender))
			{
				SaveFactory();
			}
		}

		protected bool CheckErrorAndFireSaveButtonIfNeeded()
		{
			var result = true;
			var mainMenu = this.GetMainMenu();
			var form = mainMenu != null ? mainMenu.GetForm() as ZForm : null;
			var topLevelBizObj = form != null ? form.BusinessEntity : null;
			if (topLevelBizObj != null)
			{
				if (topLevelBizObj.HasChanges)
				{
					var dialogResult = Globals.Message.Show(Res.GetString("1A4354C3-1E9A-4876-ABBB-C9DDFC13E8B8", "There are changes on this form. Do you want to save changes first?"), Res.GetString("07A926E0-0E48-4CCD-8DE9-9853F411E98F", "Save"), MessageBoxButtons.YesNo, DialogResult.Yes);
					result = dialogResult == DialogResult.Yes && form.FireSaveButton() == ContinueWithSave.Yes;
				}
				else if (topLevelBizObj.HasErrors())
				{
					var msgBox = new ZErrorMessageBox(topLevelBizObj, Res.GetString("931B106C-91C6-4F21-A2CA-E51B2DB54BDB", "{0} messages", topLevelBizObj.HumanReadableName), Res.GetString("6020C37E-A9F5-4038-9FFA-B1E49C070D6D", "send"), Res.GetString("765FB830-34EB-4122-95F5-7B57CD2B898F", "sent"));
					ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
					msgBox.Dispose();
					result = false;
				}
			}
			return result;
		}

		void ResetToOriginal_Click(object sender, EventArgs e)
		{
			if (IsMessagingAllowed && CheckErrorAndFireSaveButtonIfNeeded())
			{
				manager.ResetToOriginal(Sender);
			}
		}

		void MessagingHelp_Click(object sender, EventArgs e)
		{
			WebUrlLauncher.Launch(MessagingHelpURL);
		}

		#endregion

		#region Implementation

		protected override void OnPopup(EventArgs e)
		{
			if (!isInitialized)
			{
				MenuItems.Clear();
				InitializeMenu();
				HookClickEvents();
				isInitialized = true;
			}
			base.OnPopup(e);
		}
		bool isInitialized;

		protected virtual ZString MessagingHelpText
		{
			get { return ZString.Empty; }
		}

		protected virtual ZString MessagingHelpURL
		{
			get { return ZString.Empty; }
		}

		protected virtual bool IsMessagingSuppressed
		{
			get { return false; }
		}

		protected virtual ZString ReasonMessagingIsSuppressed
		{
			get { return ZString.Empty; }
		}

		protected bool IsMessagingAllowed
		{
			get
			{
				if (IsMessagingSuppressed)
				{
					Argument.NotNullOrEmpty(ReasonMessagingIsSuppressed, "ReasonMessagingIsSuppressed", "ReasonMessagingIsSuppressed should be overridden and a value supplied when messaging is suppressed.");
					Globals.Message.ShowWarning(ReasonMessagingIsSuppressed, Res.GetString("fad57584-31cd-4ceb-b379-ecacad60bdb8", "Messaging not allowed"));
					return false;
				}
				return true;
			}
		}

		protected virtual void InitializeMenu()
		{
			sendMessages = new ZMenuItem(ResString.GetMultilingualString("Customs.Shared.MessageManagement.SendMessage", "&Send Message(s)"));
			amendMessages = new ZMenuItem(ResString.GetMultilingualString("Customs.Shared.MessageManagement.AmendMessage", "&Amend Message(s)"));
			amendMessages.Visible = manager.AllowManualAmendments;
			withdrawMessages = new ZMenuItem(ResString.GetMultilingualString("Customs.Shared.MessageManagement.WithdrawMessage", "&Withdraw Message(s)"));
			resetToOriginal = new ZMenuItem(ResString.GetMultilingualString("Customs.Shared.MessageManagement.ResetToOriginal", "&Reset to Original"));
			MenuItems.Add(sendMessages);
			MenuItems.Add(amendMessages);
			MenuItems.Add(withdrawMessages);
			MenuItems.Add(resetToOriginal);
			if (!MessagingHelpText.IsEmpty)
			{
				messagingHelpMenuItem = new ZMenuItem(MessagingHelpText);
				MenuItems.Add(messagingHelpMenuItem);
			}
		}

		protected virtual void HookClickEvents()
		{
			sendMessages.Click += new EventHandler(SendMessages_Click);
			amendMessages.Click += new EventHandler(AmendMessages_Click);
			withdrawMessages.Click += new EventHandler(WithdrawMessages_Click);
			resetToOriginal.Click += new EventHandler(ResetToOriginal_Click);
			if (!MessagingHelpText.IsEmpty)
			{
				messagingHelpMenuItem.Click += new EventHandler(MessagingHelp_Click);
			}
		}

		protected internal ISendsMessagesToCustoms SenderInternal => Sender;
		protected virtual ISendsMessagesToCustoms Sender
		{
			get { return new SendsMessagesToCustomsGUI(); }
		}

		protected internal void SaveFactoryInternal() => SaveFactory();
		protected virtual void SaveFactory()
		{
			try
			{
				manager.Factory.Save();
				if (manager.ShowNotificationsAfterSave && manager.AfterSaveNotifications != null && manager.AfterSaveNotifications.Length > 0)
				{
					Globals.Message.Show(manager.AfterSaveNotifications.ToStringWithNewLineBetweenAppends(), Res.GetString("7b4360e5-35bb-454e-9981-122073439998", "Messages sending confirmation"), MessageBoxButtons.OK, MessageBoxIcon.Information);
					manager.AfterSaveNotifications = new ZStringBuilder();
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}
		}

		IDisposable GetCellSuspender()
		{
			var form = GetMainMenu()?.GetForm();
			if (form != null)
			{
				return new CellNotificationSuspender(form);
			}

			return new DisposableObject();
		}

		protected internal MultiMessageManager manager;

		protected internal MenuItem sendMessages;
		protected internal MenuItem amendMessages;
		protected internal MenuItem withdrawMessages;
		protected internal MenuItem resetToOriginal;
		protected internal MenuItem messagingHelpMenuItem;
	}

	#endregion
}
