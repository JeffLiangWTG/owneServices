using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class EmailDelivery : Delivery
	{
		public override IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<Messaging.Integration.IEDIMessage> getMessageFunc = null)
		{
			stream.PopulateStream(context.Factory);

			var emailAddresses = new List<string>();
			foreach (var emailAddress in mode.EK_Destination.Split(','))
			{
				var trimmedEmail = emailAddress.Trim();
				if (!string.IsNullOrEmpty(trimmedEmail))
				{
					emailAddresses.Add(trimmedEmail);
				}
			}
			if (emailAddresses.Count == 0)
			{
				var warningMessage = Res.GetString("{FF714B0F-580B-48A2-A18E-453153FE5E1B}", "The evaluated email destination of {0} in {1} is empty, please check the Email Address macro you used.", context.ActionDescription, context.ParentHumanReadableName);
				context.Notifications?.AddWarning(warningMessage);
				new EmailNotifier().Notify(context.Factory, warningMessage, mode, stream.Content, null);
				return DeliveryResult.ReportInvalidConfiguration(warningMessage);
			}

			var factory = context.Factory;
			var email = new EmailDef();
			var subject = mode.EK_ServerAddressSubject;
			var body = Encoding.UTF8.GetString(stream.Content.ToByteArray());

			if (mode.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.NotificationEmail)
			{
				email = new HtmlNotificationEmailSender().CreateEmail(subject, body);
			}
			else if (mode.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.NotificationBodyEmail)
			{
				email = new HtmlNotificationEmailSender().CreateEmail(subject, body, false);
			}
			else
			{
				email.Body = body;
				email.Subject = subject;
			}

			foreach (var emailAddress in emailAddresses)
			{
				email.AddRecipientForUserCommunication(emailAddress);
			}

			if (context.ParentInfo != null)
			{
				var parentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(context.ParentInfo.TableName);
				if (!string.IsNullOrEmpty(parentTableCode))
				{
					email.SetupBusinessEntityInfo(context.ParentInfo.InternalPK, parentTableCode, context.ParentHumanReadableName);
				}
			}

			Env.OutgoingMailManager.Create(factory, email);

			return DeliveryResult.Success;
		}
	}
}
