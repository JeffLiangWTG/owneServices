using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal sealed class PortAuthorityInterchangeSender : ShippingManagerInterchangeSender
	{
		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Calling getter")]
		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			if (messages.Count > 0)
			{
				switch (messages[0].EM_ApplicationCode)
				{
					case EDIMessage.ApplicationCodes.PortAuthority:
						var portAuthorityInterchanges = new PortAuthorityInterchangeProvider(messages).Interchanges;
						break;

					default:
						ErrorReporter.ReportOnce("ShippingManagerMessageSender.PackageMessagesIntoInterchanges", "Unsupported Application Code : " + messages[0].EM_ApplicationCode);
						break;
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		protected override bool SendInt(EDIInterchange interchange)
		{
			if (interchange == null)
			{
				throw new ArgumentNullException(nameof(interchange));
			}

			if (interchange.ContainedMessages.Count == 0)
			{
				throw new ArgumentException("interchanges.ContainedMessages cannot be empty", "interchanges");
			}

			switch (interchange.EI_ApplicationCode)
			{
				case EDIInterchange.ApplicationCodes.PortAuthority:
					return SendPortAuthorityInterchange(Logger, interchange);

				default:
					throw new InvalidOperationException("Unknown application code");
			}
		}

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			SendOutboundInterchanges(EDIInterchange.ApplicationCodes.PortAuthority, token);
		}

		protected override ZQuery ValidBranchesForMessageFilter(string[] applicationCode)
		{
			return new ZQuery();
		}

		static bool SendPortAuthorityInterchange(LoggingInformation logger, EDIInterchange interchange)
		{
			bool success = false;

			ISailingEndPoint endPoint = MessagingHelper.GetEndPointFromMessage(interchange.ContainedMessages[0]);
			PortAuthoritySetting setting = MessagingHelper.GetPortAuthoritySettingFromEndPoint(endPoint, interchange.ContainedMessages[0].EM_ApplicationReference);

			if (setting != null)
			{
				ZString email = setting.Email;
				ZString subject = MessagingHelper.GetPortAuthoritySubject(endPoint, setting.SenderID);

				if (SendInterchangeEmail(logger, interchange, email, subject, Array.Empty<string>()))
				{
					success = true;
					interchange.LogInterchangeInProgressForAllMessages();
					interchange.EI_Status = EDIInterchange.Status.Sent;
				}
			}

			return success;
		}
	}
}
