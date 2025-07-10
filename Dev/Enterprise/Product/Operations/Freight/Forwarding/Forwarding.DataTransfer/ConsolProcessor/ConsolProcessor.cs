using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(Enterprise.Freight.Forwarding.DataTransfer.ConsolProcessor))]

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ConsolProcessor
	{
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^" + ConsolSubject + "$")]
		public bool ProcessMailItem(MailItem item, ILogger logger)
		{
			return ProcessCore(item, logger.GetTaskNotificationSubscriber());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used in an attribute so has to be a constant")]
		public const string ConsolSubject = "ediEnterprise Consol XML File";

		#region Implementation

		bool ProcessCore(MailItem importMailItem, INotifications notifications)
		{
			try
			{
				foreach (MailAttachment attachment in importMailItem.MailAttachments)
				{
					ProcessAttachment(attachment);
				}

				return true;
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				notifications.AddError(Res.GetString("14781f4f-360c-498d-abc7-75f47f0f6791", "{0} Import - Fail to process email message:\r\n{1}", ConsolSubject, ex.Message));
			}

			return false;
		}

		void ProcessAttachment(MailAttachment attachment)
		{
			ZString directory = SystemDataRegistry.Instance.ConsolsDataImportDirectory.Value;
			if (!directory.IsEmpty)
			{
				try
				{
					attachment.SaveTo(directory);
				}
				catch (ArgumentException)
				{
					SendExceptionEmail(Res.GetString("626b20da-0015-426f-acea-13bcf0e79a6a", "{0} Import - File could not be written", ConsolSubject), attachment, Res.GetString("c4f5ef76-a63f-45c1-8942-364ec52c28b0", "The enclosed Consol XML file did not import as it could not be written to the directory \"{0}\".{1}{2}", directory, System.Environment.NewLine, System.Environment.NewLine));
					throw;
				}
				catch (IOException)
				{
					SendExceptionEmail(Res.GetString("f68025ce-68f9-4db7-8460-45ea73c0b3d0", "{0} Import - File could not be written", ConsolSubject), attachment, Res.GetString("3413236a-e53c-4f90-bb6a-847bf7d8a8e3", "The enclosed Consol XML file did not import as it could not be written to the directory \"{0}\".{1}{2}", directory, System.Environment.NewLine, System.Environment.NewLine));
					throw;
				}
			}
		}

		void SendExceptionEmail(string subject, MailAttachment attachment, string bodyTitle)
		{
			EmailDef email = new EmailDef();
			email.FromAddress = Env.Registry.MailboxEmailAddress;
			email.FromDisplayName = Env.Registry.MailboxDisplayName;
			email.Subject = subject;
			string fileDir = Env.TempPath;
			string fileName = Path.Combine(fileDir, attachment.MA_FileName);
			try
			{
				attachment.SaveTo(fileDir);
				AttachmentDef attachmentDef = new AttachmentDef(Path.GetFileName(fileName), fileName);
				email.Attachments.Add(attachmentDef);
			}
			finally
			{
				File.Delete(fileName);
			}
			email.Body = bodyTitle;

			try
			{
				Env.OutgoingMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
			}
			catch (EmailSendFailedException)
			{
				// if the user hasn't configured notifications correctly, emailing is not possible
			}
		}

		#endregion
	}
}

