using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Interop.OutlookIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class OutlookEmailSender : EmailSender
	{
		protected OutlookEmailSender(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource)
			: base(uIThreadSyncInvoke, contactSource)
		{
		}

		#region Implementation

		protected override void DisplayEmailAndSave(EmailSenderConfiguration senderConfiguration)
		{
			DisplayEmailAndSave(new OutlookApplication(), senderConfiguration);
		}

		protected void DisplayEmailAndSave(OutlookApplication outlook, EmailSenderConfiguration senderConfiguration)
		{
			if (outlook == null)
			{
				ErrorReporter.ReportOnce("OutlookEmailSender_OutlookNull", "outlook null");
			}

			try
			{
				string tempMsgFile = Env.GetTempFileName(Env.TempPath, "msg");
				OutlookMailInProgressData mailData = new OutlookMailInProgressData(senderConfiguration);

				object cookie = SetOutlookMailInProgressData(mailData);
				OutlookMailItem mailItem = outlook.CreateMailItem(cookie);
				if (mailItem == null)
				{
					ErrorReporter.ReportOnce("OutlookEmailSender_MailItemNull", "mailItem null");
				}

				mailItem.FileNameToSaveOnSend = tempMsgFile;
				mailItem.Subject = senderConfiguration.HtmlEmail.Subject;

				mailData.MailItem = mailItem;

				foreach (ZString email in senderConfiguration.HtmlEmail.AllRecipients)
				{
					if (!email.IsEmpty)
					{
						mailItem.AddRecipient(email);
					}
				}
				mailItem.MailItemSend += new OutlookMailItemSendEventHandler(OnMailItemSend_Save);
				mailItem.Display(false);
			}
			catch (SocketException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (COMException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (OutlookOperationAbortedException)
			{
			}
			catch (OutlookException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowDeveloperException(ex);

#if DEBUG
				if (Globals.IsTest)
				{
					Globals.Message.ShowError(ex.ToString());
				}
				else
#endif
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void OnMailItemSend_Save(object customData)
		{
			if (UIThreadSyncInvoke.InvokeRequired)
			{
				UIThreadSyncInvoke.Invoke(new OutlookMailItemSendEventHandler(OnMailItemSend_Save), new object[] { customData });
			}
			else
			{
				OutlookMailInProgressData data = GetOutlookMailInProgressData(customData);
				if (data != null)
				{
					try
					{
						if (!data.SenderConfiguration.SaveToEDocsDocumentType.IsEmpty)
						{
							AddEDoc(File.ReadAllBytes(data.MailItem.FileNameToSaveOnSend), data.MailItem.Subject + Path.GetExtension(data.MailItem.FileNameToSaveOnSend), data.SenderConfiguration.SaveToEDocsDocumentType, data.MailItem.Subject);
						}
					}
					finally
					{
						try
						{
							File.Delete(data.MailItem.FileNameToSaveOnSend);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							Globals.Message.ShowError((NoResString)"Could not delete temporary email file (reason: " + ex.Message + (NoResString)")");
						}
					}
				}
			}
		}

		#region OutlookMailInProgressData

		readonly Hashtable fCookiesToOutlookMailInProgressData = new Hashtable();

		OutlookMailInProgressData GetOutlookMailInProgressData(object cookie)
		{
			OutlookMailInProgressData data = (OutlookMailInProgressData)fCookiesToOutlookMailInProgressData[cookie];
			return data;
		}

		object SetOutlookMailInProgressData(OutlookMailInProgressData data)
		{
			Guid cookie = Guid.NewGuid();
			fCookiesToOutlookMailInProgressData[cookie] = data;
			return cookie;
		}

		class OutlookMailInProgressData
		{
			public OutlookMailInProgressData(EmailSenderConfiguration senderConfiguration)
			{
				this.SenderConfiguration = senderConfiguration;
			}

			public EmailSenderConfiguration SenderConfiguration;
			public OutlookMailItem MailItem;
		}

		#endregion

		#endregion

		#region New

		public static OutlookEmailSender New(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource)
		{
			OutlookEmailSender result;

			if (NewOutlookEmailSender != null)
			{
				result = NewOutlookEmailSender(uIThreadSyncInvoke, contactSource);
			}
			else
			{
				result = new OutlookEmailSender(uIThreadSyncInvoke, contactSource);
			}

			return result;
		}

		protected delegate OutlookEmailSender GetNewOutlookEmailSender(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource);
		[ThreadStatic]
		protected static GetNewOutlookEmailSender NewOutlookEmailSender;

		#endregion
	}
}
