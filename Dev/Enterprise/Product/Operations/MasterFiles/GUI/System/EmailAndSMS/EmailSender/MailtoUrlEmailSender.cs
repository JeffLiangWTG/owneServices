using System;
using System.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class MailtoUrlEmailSender : EmailSender
	{
		protected MailtoUrlEmailSender(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource)
			: base(uIThreadSyncInvoke, contactSource)
		{
		}

		protected override void DisplayEmailAndSave(EmailSenderConfiguration senderConfiguration)
		{
			string recipients = "";
			foreach (string email in senderConfiguration.HtmlEmail.AllRecipients)
			{
				if (!string.IsNullOrEmpty(recipients))
				{
					recipients += ";";
				}

				recipients += email;
			}
			string url = (NoResString)"mailto:" + recipients + (NoResString)"?subject=" + senderConfiguration.HtmlEmail.Subject;
			try
			{
				var parentBO = ContactSource as CargoWise.EntityFramework.BusinessObject;
				if (WebUrlLauncher.IsRemote && !senderConfiguration.SaveToEDocsDocumentType.IsEmpty && parentBO != null)
				{
					WebUrlLauncher.Launch(url, senderConfiguration.SaveToEDocsDocumentType, parentBO.PK.ToString(), parentBO.TablePrefix);
				}
				else
				{
					OpenUrl(url);
				}
			}
			catch (Win32Exception e)
			{
				if (e.ErrorCode == 2)
				{
					Globals.Message.ShowError(Res.GetString("cfd39729-1b04-4ccc-9edd-40f2326a0632", "Your mail client isn't set up correctly."));
				}
			}
		}

		protected virtual void ShowMailClientNotSetupError()
		{
			Globals.Message.ShowError(Res.GetString("cfd39729-1b04-4ccc-9edd-40f2326a0632", "Your mail client isn't set up correctly."));
		}

		protected virtual void OpenUrl(string url)
		{
			WebUrlLauncher.Launch(url);
		}

		#region New

		public static MailtoUrlEmailSender New(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource)
		{
			MailtoUrlEmailSender result;

			if (NewMailtoUrlEmailSender != null)
			{
				result = NewMailtoUrlEmailSender(uIThreadSyncInvoke, contactSource);
			}
			else
			{
				result = new MailtoUrlEmailSender(uIThreadSyncInvoke, contactSource);
			}

			return result;
		}

		protected delegate MailtoUrlEmailSender GetNewMailtoUrlEmailSender(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource);
		[ThreadStatic]
		protected static GetNewMailtoUrlEmailSender NewMailtoUrlEmailSender;

		#endregion
	}
}
