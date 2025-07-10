using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	abstract class ShippingManagerMessageRetriever : IProcessor
	{
		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			while (ProcessCore(notifications, 10, token))
			{
				token.ThrowIfCancellationRequested();
			}
		}

		bool ProcessCore(INotifications notifications, int count, CancellationToken token)
		{
			var factory = new BusinessObjectFactory();
			var items = MailFilter.Load(factory, count);
			if (items.Length == 0 || token.IsCancellationRequested)
			{
				return false;
			}
			else
			{
				foreach (MailItem item in items)
				{
					token.ThrowIfCancellationRequested();
					ProcessMailItem(notifications, item);
				}

				factory.Save();

				return items.Length == count;
			}
		}

		void ProcessMailItem(INotifications notifications, MailItem item)
		{
			string applicationCode = GetApplicationCode(item);
			ZString mailFailureMessage = MailFailureMessage(item);

			if (applicationCode == null)
			{
				notifications.Notify(new InfoNotification(Res.GetString("c213db7c-ab5b-402b-9aab-b26a57445969", "Cant identify the correct application code for mail message, ignoring. (from: {0}, date: {1}, subject: {2})", item.MI_From, item.MI_ReceivedDateTime, item.MI_Subject)));
				item.MI_Status = MailStatus.Processed;
			}
			else if (mailFailureMessage != "")
			{
				notifications.Notify(new InfoNotification(mailFailureMessage));
				item.MI_Status = MailStatus.Failed;
			}
			else
			{
				try
				{
					string[] interchangeTexts = GetInterchangeText(item);
					bool foundInterchangeText = false;

					if (interchangeTexts != null)
					{
						foreach (string interchangeText in interchangeTexts)
						{
							if (!string.IsNullOrEmpty(interchangeText))
							{
								foundInterchangeText = true;
								ProcessInterchangeText(notifications, item, interchangeText, applicationCode);
							}
						}
					}

					if (!foundInterchangeText)
					{
						notifications.Notify(new InfoNotification(Res.GetString("b77cb16a-fbf8-4ee1-b99e-d4ef73636b7c", "Mail message has no content, ignoring. (from: {0}, date: {1}, subject: {2})", item.MI_From, item.MI_ReceivedDateTime, item.MI_Subject)));
						item.MI_Status = MailStatus.Failed;
					}
					else
					{
						item.MI_Status = MailStatus.Processed;
					}
				}
				catch (MessageProcessingException ex)
				{
					if (ex.ShouldSendDeveloperInformation)
					{
						ErrorReporter.ReportOnce("ShippingManagerMessageRetriever.ProcessCore", "", ex);
					}

					notifications.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
					item.MI_Status = MailStatus.Failed;
				}
			}
		}

		static void ProcessInterchangeText(INotifications notifications, MailItem item, string interchangeText, string applicationCode)
		{
			EDIInterchange interchange;

			try
			{
				interchange = GetInterchangeFromText(item.Factory, StripNewLines(interchangeText), applicationCode, item.MI_ReceivedDateTime.ToOffset());
			}
			catch (MessageProcessingException ex)
			{
				LogMessageWithInvalidContent(notifications, item, ex.Message);
				interchange = null;
			}
			catch (InvalidFormatException ex)
			{
				ReportBrokenInterchangeEmail(item, interchangeText, ex.Message);
				LogMessageWithInvalidContent(notifications, item, ex.Message);
				interchange = null;
			}

			if (interchange != null)
			{
				if (interchange.IsInDatabase)
				{
					notifications.Notify(new InfoNotification(Res.GetString("6c292a59-4c67-4686-9955-d01943334b50", "Received a duplicate interchange (#{0})", interchange.EI_InterchangeNum)));
				}
				else
				{
					notifications.Notify(new InfoNotification(Res.GetString("ffba8288-6a9f-44fc-949a-4b4024f14249", "Received interchange (#{0})", interchange.EI_InterchangeNum)));
				}
			}
		}

		static void LogMessageWithInvalidContent(INotifications notifications, MailItem item, string detail)
		{
			string message = Res.GetString("9b580921-48c7-4362-9728-1aba7fc21517",
				"Mail message appears to contain invalid content. (from: {0}, date: {1}, subject: {2})\r\n{3}",
				item.MI_From, item.MI_ReceivedDateTime, item.MI_Subject, detail
			);

			notifications.Notify(new InfoNotification(message));
		}

		static void ReportBrokenInterchangeEmail(MailItem interchangeEmail, string interchangeText, string detail)
		{
			StringBuilder builder = new StringBuilder();

			builder.AppendLine(Res.GetString("caf56cf6-02a9-42c7-8827-eee059aaab01", "Interchange text appears malformed."));
			builder.AppendLine();
			builder.AppendLine(Res.GetString("a9e36e86-4812-413a-9056-fca7c2a733db", "Subject: {0}", interchangeEmail.MI_Subject));
			builder.AppendLine(Res.GetString("41c83e0a-acbd-4a2d-8a5a-1b7682b92cf6", "From: {0}", interchangeEmail.MI_From));
			builder.AppendLine(Res.GetString("1fb899e4-b377-40f2-b372-9b20767ac606", "Date: {0}", interchangeEmail.MI_ReceivedDateTime.ToLongTimeString()));
			builder.AppendLine();
			builder.AppendLine(Res.GetString("5b17adf0-a32b-45f7-b7f6-d0a51673435e", "Detail: {0}", detail));

			GuidRegistryItem notification = AgencyRegistry.Instance.CMMErrorEmailGroup;

			EmailDef email = new EmailDef();
			email.Subject = Res.GetString("a76ab4a1-6107-42fb-907a-d58368c0bc24", "Error extracting interchange from email.");
			email.ContentType = EmailContentTypes.PlainText;
			email.Body = builder.ToString();
			email.Attachments.Add(new AttachmentDef("interchange.edi", Encoding.ASCII.GetBytes(interchangeText)));
			email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(notification.Value, notification);

			Env.OutgoingMailManager.Create(interchangeEmail.Factory, email);
		}

		static EDIInterchange GetInterchangeFromText(BusinessObjectFactory factory, string interchangeText, string applicationCode, ZDateTimeOffset receivedDateTime)
		{
			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(factory, interchangeText, applicationCode);

			if (interchange != null)
			{
				try
				{
					EDIInterchange matchingInterchange = interchange.ExistingInterchangeMatchingToFromAndInterchangeNum;
					if (matchingInterchange != null)
					{
						interchange.Delete();
						interchange = matchingInterchange;
					}

					if (string.IsNullOrEmpty(interchange.EI_From) || string.IsNullOrEmpty(interchange.EI_InterchangeNum))
					{
						string errorMessage = Res.GetString("c35698bc-5afb-499a-b649-526d1ea67019", "no sender or interchange number");
						throw new InvalidFormatException(errorMessage);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (!interchange.IsDeleted)
					{
						interchange.Delete();
					}
					throw;
				}

				if (!receivedDateTime.IsEmpty)
				{
					interchange.Logs.AddNew(Events.Delivered, receivedDateTime, true);
				}
			}

			return interchange;
		}

		static string[] GetInterchangeText(MailItem item)
		{
			if (!item.HasContent)
			{
				return null;
			}
			else if (item.MailAttachments.Count == 0)
			{
				return new string[] { item.MI_Body.Trim() };
			}
			else
			{
				List<string> result = new List<string>(item.MailAttachments.Count);

				foreach (MailAttachment attachment in item.MailAttachments)
				{
					result.Add(Encoding.UTF8.GetString(attachment.MA_Data).Trim());
				}

				return result.ToArray();
			}
		}

		static string StripNewLines(string inString)
		{
			StringBuilder builder = new StringBuilder(inString);

			int write = 0;
			for (int read = 0; read < builder.Length; read++)
			{
				char c = builder[read];

				if (c != '\r' && c != '\n')
				{
					if (read != write)
					{
						builder[write] = c;
					}

					write++;
				}
			}

			if (write == inString.Length)
			{
				return inString;
			}
			else
			{
				builder.Length = write;
				return builder.ToString();
			}
		}

		protected abstract string GetApplicationCode(MailItem item);

		protected virtual ZString MailFailureMessage(MailItem item)
		{
			return "";
		}

		protected abstract IMailFilter MailFilter { get; }
	}
}


