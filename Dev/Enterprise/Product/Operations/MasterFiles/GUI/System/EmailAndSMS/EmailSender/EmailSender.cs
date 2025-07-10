using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.MailClientIntegration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
#if !WINZOR
using Enterprise.RemoteDesktopServices;
#endif
#if DEBUG
using CargoWise.Common;
using Enterprise.MasterFiles.GUI.Testing;
#endif

namespace Enterprise.MasterFiles.GUI
{
	public abstract class EmailSender
	{
		#region Construction

		protected EmailSender(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource)
		{
			this.UIThreadSyncInvoke = uIThreadSyncInvoke;
			this.ContactSource = contactSource;
		}

		static EmailSender GetInstance(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource)
		{
#if !WINZOR
			if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession)
			{
				return MailtoUrlEmailSender.New(uIThreadSyncInvoke, contactSource);
			}
			else
#endif
#if DEBUG
			if (Globals.IsTest && IsTestEmailSender)
			{
				return new TestEmailSender(uIThreadSyncInvoke, contactSource);
			}
			else
#endif
#if !WINZOR
			if (DefaultMailClientFinder.DefaultMailClient == MailClientType.Outlook)
			{
				return OutlookEmailSender.New(uIThreadSyncInvoke, contactSource);
			}
			else
#endif
			{
				return MailtoUrlEmailSender.New(uIThreadSyncInvoke, contactSource);
			}
		}

		public static ZMenuItem GetSendEmailMenuItem()
		{
			var menuItem = new ZMenuItem(ResString.GetMultilingualString("762EAFF0-AFE3-4ED5-A1C0-90B54A196C39", "&Send E-mail"));
			menuItem.Shortcut = Shortcut.CtrlShiftE;
			menuItem.ShowShortcut = true;
			return menuItem;
		}

#if DEBUG
		public static string GetSendEmailMenuItemTextForTest()
		{
			return GetSendEmailMenuItem().Text;
		}

		public static bool IsTestEmailSender
		{
			get
			{
				return isTestEmailSender.Value;
			}
			set
			{
				isTestEmailSender.Value = value;
			}
		}

		public static bool IsTestSendFileToEdoc
		{
			get => isTestSendFileToEdoc.Value;
			set => isTestSendFileToEdoc.Value = value;
		}

		readonly static Overridable<bool> isTestSendFileToEdoc = new Overridable<bool>(true);

		readonly static Overridable<bool> isTestEmailSender = new Overridable<bool>(false);
#endif

		public static void HandleEventHandler(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource, IMultilingualString customNoSelectionErrorMessage)
		{
			var businessObject = contactSource as BusinessObject;
			if (businessObject != null)
			{
				if ((businessObject.IsInDatabase && !businessObject.HasChanges) || (businessObject is NonPersistentBusinessObject && !businessObject.HasChanges))
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(() => GetInstance(uIThreadSyncInvoke, contactSource).PerformContactEmailSending((uIThreadSyncInvoke as Control)?.FindForm()), () => { });
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("393a9e72-0e86-4045-b3c4-005670e8370f", "Changes have been made to this record. Please save before an E-mail can be sent."));
				}
			}
			else
			{
				if (customNoSelectionErrorMessage == null)
				{
					Globals.Message.ShowError(Res.GetString("347ab0f2-7c9b-4b25-88c7-40cc3bf2b72b", "Please select a record before an E-mail can be sent."));
				}
				else
				{
					Globals.Message.ShowError(customNoSelectionErrorMessage.ToString());
				}
			}
		}

		#endregion

		#region Perform Send

		public void PerformContactEmailSending(Form parent)
		{
			EmailSenderConfiguration senderConfiguration = GetEmailSenderConfiguration(ContactSource);

			//On a remote session with attachments accessed through a shared folder, attachments get deleted from the remote server when the form is closed.
			//=> the form needs to be disposed AFTER the email has been sent.
			using (var emailSenderForm = new EmailSenderForm(senderConfiguration))
			{
				emailSenderForm.Owner = parent;
				if (ZFormModaliser.ShowDialogWithoutDispose(emailSenderForm, parent) == DialogResult.OK)
				{
					if (senderConfiguration.DeliveryMethod == MessageDeliveryMethod.EMail)
					{
						SendEmail(senderConfiguration);
					}
					else
					{
						DisplayEmailAndSave(senderConfiguration);
					}
				}
			}
		}

		protected delegate EmailSenderConfiguration GetEmailSenderConfigurationDelegate(ISendEmailSource source);
		protected virtual GetEmailSenderConfigurationDelegate GetEmailSenderConfiguration
		{
			get
			{
				return source =>
				{
					var senderConfiguration = new EmailSenderConfiguration(source);
					senderConfiguration.HtmlEmail.Subject = source.EmailSubject;
					return senderConfiguration;
				};
			}
		}

		protected void PerformContactEmailSendingEventHandler(object sender, EventArgs e)
		{
			PerformContactEmailSending((sender as Control)?.FindForm());
		}

		#endregion

		#region SendEmail

		protected void SendEmail(EmailSenderConfiguration senderConfiguration)
		{
			if (senderConfiguration.CheckIsReadyToSendEmail())
			{
				EmailDef email = senderConfiguration.GetEmail();
				Env.OutgoingMailManager.CreateAndSave(email);

				AddEDoc(email, senderConfiguration);
				AddEmailSentEvent(senderConfiguration.HtmlEmail.AllRecipientsCommaDelimited);
				ContactSource.Logs?.Factory.Save();
			}
		}

		#endregion

		#region Implementation

		public readonly ISynchronizeInvoke UIThreadSyncInvoke;
		public readonly ISendEmailSource ContactSource;

		protected abstract void DisplayEmailAndSave(EmailSenderConfiguration senderConfiguration);

#if DEBUG
		protected virtual
#endif
		void AddEDoc(EmailDef email, EmailSenderConfiguration senderConfiguration)
		{
			if (!senderConfiguration.SaveToEDocsDocumentType.IsEmpty)
			{
				string fileName = HtmlEmailTransformation.ConvertFileWithExtensionPath(email.Subject);
				string docType = senderConfiguration.SaveToEDocsDocumentType;
				string format = (NoResString)"<p><strong>{0}</strong>: {1}</p>";

				StringBuilder emailAsString = new StringBuilder(string.Format(format, Res.GetString("AAC265CE-3ADA-4509-B53E-63E891CA3759", "From"), email.FromDisplayName));
				emailAsString.AppendFormat(format, Res.GetString("DEA1F8ED-4FB7-468F-A897-604E852455F9", "Subject"), email.Subject);
				emailAsString.AppendFormat(format, Res.GetString("5241CFE2-ED23-4AEC-A109-C3A5E0D2231A", "To"), email.Recipients.RecipientsAsDelimitedString());
				emailAsString.AppendFormat(format, Res.GetString("0436D1A8-793C-4193-AD46-BFDAB390D45A", "Cc"), email.CCRecipients.RecipientsAsDelimitedString());
				emailAsString.AppendFormat(format, Res.GetString("9A0C9537-EC16-4B78-84B5-723235C8163D", "Bcc"), email.BCCRecipients.RecipientsAsDelimitedString());
				emailAsString.AppendFormat(format, Res.GetString("0B0520B1-CA90-4502-97F8-918196C3A211", "Body"), string.Empty);
				emailAsString.Append(email.Body);

				using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(emailAsString.ToString())))
				{
					DocManagerInfo docSupportInfo = ContactSource.DocManagerInfo;
					IeDoc doc = docSupportInfo.AddFileOrDocument(stream, fileName, docType);
					if (string.IsNullOrEmpty(doc.Description))
					{
						var description = new ZString(email.Subject).SubstringSafe(0, StorageDocsSchema.SC_Desc.MaxLength);
						doc.Description = description;
					}

					var attachmentFiles = email.Attachments.OfType<AttachmentDef>()
						.Where(f => senderConfiguration.HtmlEmail.AttachmentList.OfType<CodeDescriptionPair>().Any(p => f.DisplayName == p.Code));
					foreach (var file in attachmentFiles)
					{
						docSupportInfo.AddFileOrDocument(file.Data, file.DisplayName, docType, overwriteExistingFileIfNotImageFile: false);
					}
					docSupportInfo.Save();
					AddDocumentAllocatedEvent(docType, doc.UniqueKey);
				}
			}
		}

		protected void AddEDoc(byte[] file, string fileName, string docType, string description)
		{
			using (var stream = (SubStreamableStream)new MemoryStream(file))
			{
				DocManagerInfo docSupportInfo = ContactSource.DocManagerInfo;
				IeDoc doc = docSupportInfo.AddFileOrDocument(stream, fileName, docType);
				if (string.IsNullOrEmpty(doc.Description))
				{
					doc.Description = description;
				}
				docSupportInfo.Save();

				AddDocumentAllocatedEvent(docType, doc.UniqueKey);
				ContactSource.Logs?.Factory.Save();
			}
		}

		void AddEmailSentEvent(ZString emailRecipients)
		{
			var logs = ContactSource.Logs;
			if (logs != null)
			{
				logs.AddNew(Events.EmailSent, emailRecipients.SubstringSafe(0, StmALogSchema.SL_Reference.MaxLength));
			}
		}

		void AddDocumentAllocatedEvent(string documentType, ZGuid eDocsUniqueKey)
		{
			var logs = ContactSource.Logs;
			if (logs != null)
			{
				logs.AddNew(Events.DocumentAllocated, StmALogEventSourceExtensions.GenerateEventReference(documentType, eDocsUniqueKey));
			}
		}
		#endregion
	}
}

#if DEBUG
namespace Enterprise.MasterFiles.GUI.Testing
{
	class TestEmailSender : EmailSender
	{
		internal TestEmailSender(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource)
		: base(uIThreadSyncInvoke, contactSource)
		{
		}

		protected override void DisplayEmailAndSave(EmailSenderConfiguration senderConfiguration)
		{
			throw new NotImplementedException();
		}

		protected override GetEmailSenderConfigurationDelegate GetEmailSenderConfiguration
		{
			get
			{
				return source =>
				{
					GlbStaff.CurrentUser.GS_EmailAddress = "jenny.nguyen@cargowise.com";
					var senderConfig = new EmailSenderConfiguration(source);
					senderConfig.HtmlEmail.Subject = source.EmailSubject;
					senderConfig.HtmlEmail.FromEmailAddress = "jenny.nguyen@cargowise.com";
					senderConfig.HtmlEmail.ToEmailAddress = "test@cargowise.com";
					senderConfig.HtmlEmail.Cc = "test+cc@cargowise.com";
					senderConfig.HtmlEmail.Bcc = "test+bcc@cargowise.com";
					senderConfig.HtmlEmail.Body = "This is the email body.";
					senderConfig.SaveToEDocsDocumentType = IsTestSendFileToEdoc ? "MSC" : string.Empty;
					return senderConfig;
				};
			}
		}
	}
}

#endif
