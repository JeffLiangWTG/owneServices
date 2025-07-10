using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	public class HVLVISFMetaHeaderMessagingMenu : EDIMenu
	{
		public HVLVISFMetaHeaderMessagingMenu(HVLVISFMetaHeader metaHeader)
			: base(metaHeader.FirstImporterSecurityFilingJob)
		{
			Caption = ResString.GetMultilingualString("4335deef-cfe5-4a6d-b5a6-989944f579ed", "Messaging");
			MetaHeader = metaHeader;
		}

		HVLVISFMetaHeader MetaHeader { get; }

		protected override void ResetToOriginal()
		{
			if (Env.Security.ImporterSecurityFilingResetToOriginal.IsAllowed)
			{
				if (Globals.Message.Show(ResetMessagingMessage, ResetMessagingCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					foreach (CusISFHeader isf in MetaHeader.RelatedJobs)
					{
						isf.ResetToOriginal();
					}
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.ImporterSecurityFilingResetToOriginal);
			}
		}

		protected override void SendMessage(UpdateActionCode actionCode)
		{
			using (var form = new HVLVISFMessagesSendForm(MetaHeader))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					SendMessageCore(form.SelectedCusISFHeaders, actionCode);
				}
			}
		}

		void SendMessageCore(IEnumerable<CusISFHeader> cusISFHeaders, UpdateActionCode actionCode)
		{
			if (cusISFHeaders.Any() && IsMessagingAllowed(cusISFHeaders))
			{
				var jobsWithDeletedCustomsReference = cusISFHeaders.Where(header => header.BF_CustomsStatus == MessageStatusList.Codes.ClearISFDelete).ToList();
				if (jobsWithDeletedCustomsReference.Count > 0)
				{
					var messages = new ZStringBuilder();
					messages.Append(CustomsReferenceDeletedMessage);
					jobsWithDeletedCustomsReference.ForEach(r => messages.Append(r.BF_HouseBill));

					Globals.Message.ShowError(messages.ToStringWithNewLineBetweenAppends(), SendMessageCaption);
					return;
				}

				if (cusISFHeaders.Any(header => header.HasChanges))
				{
					if (Globals.Message.Show(SaveChangesCaptionMessage, SaveCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						MainForm.FireSaveButton();
						if (cusISFHeaders.Any(header => header.HasChanges || header.HasErrors))
						{
							return;
						}
					}
					else
					{
						return;
					}
				}

				var sendType = SendType.Send;
				if (actionCode != UpdateActionCode.Delete)
				{
					sendType = ContinueWithNotifications(cusISFHeaders);
				}

				if (sendType != SendType.DoNotSend)
				{
					try
					{
						foreach (var header in cusISFHeaders)
						{
							MQEDIMessage message;

							if (actionCode != UpdateActionCode.Delete && !header.ShouldSendAdd)
							{
								actionCode = UpdateActionCode.Replace;
							}

							var builder2 = new ImporterSecurityFilingMessageBuilder<ABIInputBlockControlGenerator, APLB, APLY>(header, actionCode);
							message = builder2.PopulateMessage();

							if (message != null)
							{
								message.EM_SendWithMessageErrors = sendType == SendType.SendWithError;
								message.EM_ApplicationReference = header.BF_CustomsReference;
							}
						}

						MetaHeader.Factory.Save();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(e);
					}
					finally
					{
						Globals.Message.ShowInformation(MessagesSentMessage, MessagesSentCaption);
					}
				}
			}
		}

		bool IsMessagingAllowed(IEnumerable<CusISFHeader> cusISFHeaders)
		{
			var result = true;
			if (Env.Security.ImporterSecurityFilingMessaging.IsAllowed)
			{
				var messages = new ZStringBuilder();
				if (cusISFHeaders.Any(header => header.IsWaitingForResponse))
				{
					messages.Append(MessagePendingResponseErrorMessage);
				}

				if (!messages.IsEmpty)
				{
					result = false;
					Globals.Message.ShowError(messages.ToStringWithNewLineBetweenAppends(), CannotSendMessagesCaption);
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.ImporterSecurityFilingMessaging);
				result = false;
			}

			return result;
		}

		SendType ContinueWithNotifications(IEnumerable<CusISFHeader> cusISFHeaders)
		{
			var result = SendType.DoNotSend;
			var hasErrors = false;
			var hasWarnings = false;

			MetaHeader.FirstImporterSecurityFilingJob.RunPreSaveValidation();

			foreach (var header in cusISFHeaders)
			{
				header.Validation.ValidateBF_HouseBill();
				foreach (var child in GetBizosForMessageValidation(header)) // only validate required data because the rest is same as MetaHeader.FirstImporterSecurityFilingJob
				{
					child.RunPreSaveValidation();
					var childNotifications = MessageSendingValidation.New(child, null, false).CheckBusinessObjectLevelValidation();
					if (childNotifications.ContainsError())
					{
						hasErrors = true;
					}
					else if (childNotifications.ContainsWarning())
					{
						hasWarnings = true;
					}
				}
			}

			if (hasErrors)
			{
				Globals.Message.ShowError(ErrorMessage);
			}
			else if (hasWarnings)
			{
				if (Globals.Message.Show(WarningMessage, ContinueCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

		IEnumerable<BusinessObject> GetBizosForMessageValidation(CusISFHeader header)
		{
			foreach (var line in header.Lines)
			{
				yield return line;
			}

			foreach (var reference in header.ReferenceDatas)
			{
				yield return reference;
			}

			foreach (var address in header.DocAddresses)
			{
				yield return address;
			}
		}

		ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		enum SendType { Send, SendWithError, DoNotSend }

		static string ResetMessagingCaption => Res.GetString("f2ab1562-95f5-4011-8de6-26a4f94afa8e", "Reset Messaging?");
		static string SendMessageCaption => Res.GetString("f9f41253-1ee3-44c7-b26f-f7313c71a926", "Send Message");
		static string SaveCaption => Res.GetString("db76d43f-d9e6-4ea6-9db4-2272ccc22fe4", "Save");
		static string ContinueCaption => Res.GetString("3a06f6a8-3d4d-4518-920d-e2b0f43be5d8", "Continue?");
		static string CannotSendMessagesCaption => Res.GetString("764c1275-2b4d-4651-b537-569ede5f9907", "Cannot Send Messages");
		static string MessagesSentCaption => Res.GetString("50e44ed5-1306-4627-a7d9-895c4935f9bc", "Messages Sent");
		static string ResetMessagingMessage => Res.GetString("8892efd6-a2d0-4cbf-833f-56c46e9d8d9e", "Resetting the messaging should only be done as a last resort as it may lead to you getting out of sync with Customs. Are you sure you wish to continue?");
		static string SaveChangesCaptionMessage => Res.GetString("8f0a425d-6d99-41c9-a230-5a9b31572f1d", "The data has not yet been saved. Do you want to save and proceed?");
		static string MessagePendingResponseErrorMessage => Res.GetString("94e7ae32-e886-48f8-824d-c2f00c655e50", "You cannot send messages while waiting for a response. Exit the job, and then select the job again, to refresh the status information and see if a reply has been received.");
		static string CustomsReferenceDeletedMessage => Res.GetString("7368cacc-f324-45c4-ad3d-a64af066d6dd", "Cannot send further messages as at least one Customs Reference has been deleted from Customs system.\r\nYou can use 'Reset to Original' to reuse the job and get a new Customs Reference.");
		static string MessagesSentMessage => Res.GetString("5c6c8d79-93a2-40c9-89a1-70cff88dc4fa", "All selected messages in ISF meta-header created successfully.");
		static string WarningMessage => Res.GetString("ce87e301-f878-4f82-a96b-656522d891f9", "Your message(s) have warnings.\r\nDo you want to send the message(s) despite these warnings?");
		static string ErrorMessage => Res.GetString("1d1d7dc1-6e34-40b4-908a-09071a8e42be", "Your message(s) have errors.");
	}
}
