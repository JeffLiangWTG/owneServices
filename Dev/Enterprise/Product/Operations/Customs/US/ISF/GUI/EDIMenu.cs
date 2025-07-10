using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.GUI
{
	public class EDIMenu : ZMenuItem
	{
		internal static class MenuItemText
		{
			internal const string Send = "&Send";
			internal const string Delete = "&Delete";
			internal const string ResetToOriginal = "&Reset To Original";
		}

		public EDIMenu(CusISFHeader iSFHeader)
		{
			ISFHeader = iSFHeader;
			Text = ResString.GetMultilingualString("02714840-faa1-4863-ae0a-48e2be8669b6", "&Messaging");

			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("b06e871e-52a6-4e6d-babe-736186db486c", MenuItemText.Send), SendMessageClick));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("5c21df95-b69f-4202-932f-b5fdf7f83bf4", MenuItemText.Delete), SendDeleteMessageClick));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("fcb696ba-d14b-4b89-90d5-f250f0bd87d4", MenuItemText.ResetToOriginal), ResetToOriginalClick));
		}

		public readonly CusISFHeader ISFHeader;

		void ResetToOriginalClick(object sender, EventArgs e)
		{
			if (Env.Security.ImporterSecurityFilingResetToOriginal.IsAllowed)
			{
				if (ISFHeader != null)
				{
					string caption = "Reset Messaging?";
					string message = "Resetting the messaging should only be done as a last resort as it may lead to you getting out of sync with Customs. " +
		"Are you sure you wish to continue?";
					if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						ResetToOriginal();
					}
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.ImporterSecurityFilingResetToOriginal);
			}
		}

		void SendMessageClick(object sender, EventArgs e)
		{
			SendMessage(UpdateActionCode.Add);
		}

		void SendDeleteMessageClick(object sender, EventArgs e)
		{
			SendMessage(UpdateActionCode.Delete);
		}

		protected virtual void ResetToOriginal()
		{
			ISFHeader.ResetToOriginal();
		}

		protected virtual void SendMessage(UpdateActionCode actionCode)
		{
			if (ISFHeader != null && IsMessagingAllowed())
			{
				if (actionCode == UpdateActionCode.Delete)
				{
					if (!ISFHeader.CanSendDelete)
					{
						Globals.Message.ShowError("Cannot send 'Delete' message as message hasn't been cleared yet.", "Send 'Delete' Message");
						return;
					}
					if (ISFHeader.BF_CustomsStatus == MessageStatusList.Codes.ClearISFDelete)
					{
						Globals.Message.ShowError("Cannot send 'Delete' message as message has been already deleted.", "Send 'Delete' Message");
						return;
					}
					if (Globals.Message.Show("You are about to send 'Delete' message. Are you sure?", "Send 'Delete' Message", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
					{
						return;
					}
				}
				else
				{
					if (ISFHeader.BF_CustomsStatus == MessageStatusList.Codes.ClearISFDelete)
					{
						Globals.Message.ShowError(string.Format("Cannot send further messages as this Customs Reference '{0}' has been deleted from Customs system.\r\nYou can use 'Reset to Original' to reuse the job and get a new Customs Reference.", ISFHeader.BF_CustomsReference), "Send Message");
						return;
					}
					else if (!ISFHeader.ShouldSendAdd)
					{
						actionCode = UpdateActionCode.Replace;
					}
				}

				if (ISFHeader.HasChanges)
				{
					if (Globals.Message.Show("The data has not yet been saved. Do you want to save and proceed?", "Save", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						MainForm.FireSaveButton();
						if (ISFHeader.HasChanges || ISFHeader.HasErrors)
						{
							return;
						}
					}
					else
					{
						return;
					}
				}

				SendType sendType = SendType.Send;
				if (actionCode != UpdateActionCode.Delete)
				{
					sendType = ContinueWithNotifications();
				}

				if (sendType != SendType.DoNotSend)
				{
					try
					{
						MQEDIMessage message = null;
						var builder2 = new ImporterSecurityFilingMessageBuilder<ABIInputBlockControlGenerator, APLB, APLY>(ISFHeader, actionCode);
						message = builder2.PopulateMessage();

						if (message != null)
						{
							message.EM_SendWithMessageErrors = sendType == SendType.SendWithError;
							message.EM_ApplicationReference = ISFHeader.BF_CustomsReference;
							ISFHeader.Factory.Save();
							Globals.Message.Show("1 message was created.");
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(e);
					}
				}
			}
		}

		enum SendType { Send, SendWithError, DoNotSend }

		SendType ContinueWithNotifications()
		{
			SendType result = SendType.DoNotSend;
			ISFHeader.RunPreSaveValidation();
			var notifications = ISFMessageSendingValidation.New(ISFHeader, null).CheckBusinessObjectLevelValidation();

			if (notifications.ContainsError())
			{
				Globals.Message.ShowWarning(notifications.NotificationsAsString());
			}
			else if (notifications.ContainsWarning())
			{
				if (Globals.Message.Show(notifications.NotificationsAsString(), "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					if (Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed)
					{
						result = SendType.SendWithError;
					}
					else
					{
						Env.Security.ImporterSecurityFilingSendWithMessageErrors.ShowError();
					}
				}
			}
			else
			{
				result = SendType.Send;
			}

			return result;
		}
		ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		internal const string CannotSendMessageWaitingText = "You cannot send a message while waiting for a response. Exit the job, and then select the job again, to refresh the status information and see if a reply has been received.";
		bool IsMessagingAllowed()
		{
			bool result = true;
			if (Env.Security.ImporterSecurityFilingMessaging.IsAllowed)
			{
				ZStringBuilder messages = new ZStringBuilder();

				if (ISFHeader.IsWaitingForResponse)
				{
					messages.Append(CannotSendMessageWaitingText);
				}
				if (!messages.IsEmpty)
				{
					result = false;
					Globals.Message.ShowError(messages.ToStringWithNewLineBetweenAppends(), "Cannot Send Message");
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.ImporterSecurityFilingMessaging);
				result = false;
			}

			return result;
		}
	}
}
