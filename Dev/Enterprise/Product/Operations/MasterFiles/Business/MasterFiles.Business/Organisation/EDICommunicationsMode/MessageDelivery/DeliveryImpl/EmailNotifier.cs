using System;
using System.Globalization;
using System.IO;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class EmailNotifier : IErrorNotifier<IEDICommunicationsMode>
	{
		public void Notify(BusinessObjectFactory factory, string message, IEDICommunicationsMode mode, Stream sentMessageData, Stream responseMessageData)
		{
			ZStringBuilder bodyBuilder = new ZStringBuilder();
			bodyBuilder.Append(message);
			bodyBuilder.Append(BuildModeMessage(mode));
			SendEmail(factory, bodyBuilder.ToString(), sentMessageData, responseMessageData);
		}

		public void Notify(BusinessObjectFactory factory, Exception exception, IEDICommunicationsMode mode, Stream sentMessageData, Stream responseMessageData)
		{
			Notify(factory, BuildHTMLExceptionMessage(exception), mode, sentMessageData, responseMessageData);
		}

		public void Notify(BusinessObjectFactory factory, Exception exception, string environmentStackTrace, IEDICommunicationsMode mode, Stream sentMessageData, Stream responseMessageData)
		{
			var messageBuilder = new ZStringBuilder();
			var exceptionMessage = BuildHTMLExceptionMessage(exception);
			var headerAndEnvironmentStackTrace = WebUtility.HtmlEncode(Res.GetString("b89a9e12-8d24-4f76-ba36-d4d31c175f16", "Environment Stack Trace: {0}", environmentStackTrace));
			messageBuilder.AppendLine(exceptionMessage).AppendLine(HTMLParagraphMarker).AppendLine(headerAndEnvironmentStackTrace);
			var message = messageBuilder.ToString();

			Notify(factory, message, mode, sentMessageData, responseMessageData);
		}

		public string Subject
		{
			get { return Res.GetString("54ee4f23-837e-452d-be90-0e0128322118", "Workflow process delivery failed"); }
		}

		void SendEmail(BusinessObjectFactory factory, string message, Stream sentMessageData, Stream responseMessageData)
		{
			var email = new HtmlEmailDef();
			email.Subject = Subject;
			email.Body = message;
			var locator = GroupSourceLocator.GetFromRegistryItem(NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup);
			email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.Value, locator.Location);

			AttachStreamAsZippedFileSafe(email, sentMessageData, "RawMessageContent");
			AttachStreamAsZippedFileSafe(email, responseMessageData, "RawResponseContent");

			try
			{
				if (factory != null)
				{
					Env.OutgoingMailManager.Create(factory, email);
				}
				else
				{
					Env.OutgoingMailManager.CreateAndSave(email);
				}
			}
			catch (EmailHasNoRecipientsException)
			{
				// Email must have at least one recipient, CC or BCC
				// "No members of the group have set up an email address." 
				// "No staff members were found in the staff group."
			}
		}

		static void AttachStreamAsZippedFileSafe(HtmlEmailDef email, Stream attachmentContent, string fileName)
		{
			if (attachmentContent != null && attachmentContent.CanRead && attachmentContent.Length > 0)
			{
				try
				{
					email.Attachments.Add(AttachmentDef.CreateZippedAttachment(fileName + ".zip", fileName + ".txt", new UnclosableStream(attachmentContent)));
				}
				catch (Exception exception)
				{
					if (exception.IsCriticalException())
					{ throw; }
				}
			}
		}

		protected virtual string BuildHTMLExceptionMessage(Exception exception)
		{
			return BuildHTMLExceptionMessage(exception, false);
		}

		protected string BuildHTMLExceptionMessage(Exception exception, bool shouldSuppressCallStack)
		{
			var bodyBuilder = new ZStringBuilder();
			bodyBuilder.AppendLine(WebUtility.HtmlEncode(Res.GetString("0c4b7d05-d468-4321-b308-503ab4bbbd6a", "Delivery failed with error :\"{0}\"", shouldSuppressCallStack ? exception.Message : exception.ToString())));
			for (var exceptionWithInnerException = exception; exceptionWithInnerException.InnerException != null; exceptionWithInnerException = exceptionWithInnerException.InnerException)
			{
				bodyBuilder.AppendLine(HTMLParagraphMarker);
				bodyBuilder.AppendLine(Res.GetString("472c9e54-5a55-4a16-99f1-ce66e221d3eb", "Inner Exception: {0}", WebUtility.HtmlEncode(shouldSuppressCallStack ? exceptionWithInnerException.InnerException.Message : exceptionWithInnerException.InnerException.ToString())));
			}
			return bodyBuilder.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is an HTML Tag, not something human readable")]
		const string HTMLParagraphMarker = "<p><p>";

		string BuildModeMessage(IEDICommunicationsMode mode)
		{
			var bodyBuilder = new ZStringBuilder();
			if (mode.Organisation is OrgHeader organisation)
			{
				string workItemUrl = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, organisation.PK.ToGuid());
				bodyBuilder.AppendLine(HTMLParagraphMarker);
				bodyBuilder.AppendLine(Res.GetString("cc56dcb1-798b-4e8c-ac36-f383201b9814", "Organization		:  {0}",
					string.Format(CultureInfo.InvariantCulture, "<a href=\"{0}\">{1}</a>", workItemUrl, WebUtility.HtmlEncode(organisation.OH_Code + " - " + organisation.OH_FullNameTruncated))));
			}
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			bodyBuilder.AppendLine(HTMLParagraphMarker);
			bodyBuilder.AppendLine(Res.GetString("a939954a-adde-42cd-8c34-da81ae5fe47b", "Enterprise Code	: {0}", WebUtility.HtmlEncode(registrationKey.EnterpriseCode)));
			bodyBuilder.AppendLine(HTMLParagraphMarker);
			bodyBuilder.AppendLine(Res.GetString("23311ded-b124-4a9a-a56a-a188160e8dc5", "Machine			: {0}", WebUtility.HtmlEncode(registrationKey.ServerCode)));
			bodyBuilder.AppendLine(HTMLParagraphMarker);
			bodyBuilder.AppendLine(Res.GetString("4dcfcb48-c6e0-4849-8c19-39e3b5daa310", "Port				: {0}", WebUtility.HtmlEncode(mode.EK_PortNumber.ToString())));
			bodyBuilder.AppendLine(HTMLParagraphMarker);
			bodyBuilder.AppendLine(Res.GetString("83c40b10-6755-4afd-8e4d-d45fd5f87226", "Destination		: {0}", WebUtility.HtmlEncode(mode.EK_Destination)));
			bodyBuilder.AppendLine(HTMLParagraphMarker);
			bodyBuilder.AppendLine(Res.GetString("332a5e3c-53ee-48e7-9adb-2e6b88b34604", "File name			: {0}", WebUtility.HtmlEncode(mode.EK_Filename)));
			bodyBuilder.AppendLine(HTMLParagraphMarker);
			return bodyBuilder.ToString();
		}
	}
}
