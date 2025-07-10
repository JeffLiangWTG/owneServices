using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal sealed class EIDOInterchangeSender : BaseInterchangeSender
	{
		public EIDOInterchangeSender()
		{
			failures = new FailedMessageList();
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Calling getter")]
		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			if (messages.Count > 0)
			{
				switch (messages[0].EM_ApplicationCode)
				{
					case EDIMessage.ApplicationCodes.EIDO:
						var eidoInterchanges = new EIDOInterchangeProvider(messages, failures).Interchanges;
						break;

					default:
						ErrorReporter.ReportOnce("EIDOInterchangeSender.PackageMessagesIntoInterchanges", "Unsupported Application Code : " + messages[0].EM_ApplicationCode);
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

			try
			{
				return SendEIDOInterchange(Logger, interchange);
			}
			catch (MessageProcessingException ex)
			{
				if (ex.ShouldSendDeveloperInformation || !ex.ShouldSendEmailToUsers)
				{
					throw;
				}
				else
				{
					interchange.EI_Status = EDIInterchange.Status.Failed;
					failures.AddRange(ex.Message, interchange.ContainedMessages.ToArray<EDIMessage>());
				}
			}

			return false;
		}

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			SendOutboundInterchanges(EDIInterchange.ApplicationCodes.EIDO, token);

			if (failures.Count > 0)
			{
				ReportFailures();
				failures.Clear();
			}
		}

		protected override ZQuery ValidBranchesForMessageFilter(string[] applicationCode)
		{
			return new ZQuery();
		}

		#region Implementation

		static bool SendEIDOInterchange(LoggingInformation logger, EDIInterchange interchange)
		{
			bool success = false;

			EIDOMessagingHeader messagingDetail = MessagingHelper.GetEIDOMessagingDetail();

			List<string> ccEmailAddresses = new List<string>();

			foreach (string line in AgencyRegistry.Instance.EIDOCCEmailAddresses.Value.Split(new char[] { '\n' }))
			{
				string trimmedLine = line.Trim();

				if (!string.IsNullOrEmpty(trimmedLine))
				{
					ccEmailAddresses.Add(trimmedLine);
				}
			}

			if (SendInterchangeEmail(logger, interchange, messagingDetail.Email, "IFCSUM", ccEmailAddresses.ToArray()))
			{
				success = true;
				interchange.LogInterchangeInProgressForAllMessages();
				interchange.EI_Status = EDIInterchange.Status.Sent;
			}

			return success;
		}

		static bool SendInterchangeEmail(LoggingInformation logger, EDIInterchange interchange, ZString emailAddress, ZString subject, string[] ccEmailAddresses)
		{
			bool success = false;

			try
			{
				SendInterchangeEmailCore(interchange, emailAddress, subject, ccEmailAddresses);
				success = true;
			}
			catch (Exception ex)
			{
				if (logger == null || ex.IsCriticalException())
				{
					throw;
				}

				logger.Log("Failed to send: " + ex.Message); // Error log message
			}

			return success;
		}

		static void SendInterchangeEmailCore(EDIInterchange interchange, ZString emailAddress, ZString subject, string[] ccEmailAddresses)
		{
			byte[] content = System.Text.Encoding.ASCII.GetBytes(interchange.EI_InterchangeText);

			EmailDef email = new EmailDef();
			email.AddRecipientForSystemCommunication(emailAddress);
			email.AddRecipientForSystemCommunication(ccEmailAddresses, RecipientDef.RecipientTypes.CC);
			email.Attachments.Add(new AttachmentDef(GetFileName(interchange), content));
			email.Priority = EmailDef.PriorityFlag.High;
			email.Subject = subject;

			Env.OutgoingMailManager.CreateAndSave(email);
		}

		static string GetFileName(EDIInterchange interchange)
		{
			return interchange.EI_InterchangeNum.PadLeft(8, '0') + ".edi";
		}

		void ReportFailures()
		{
			GuidRegistryItem reg = AgencyRegistry.Instance.EIDOErrorEmailGroup;
			EmailGroupUtility util = new EmailGroupUtility();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbGroup group = factory.Load<GlbGroup>(reg.Value);

			StringCollection recipients;

			if (group != null && (recipients = util.GetGroupEmailCollection(reg.Value, false)).Count > 0)
			{
				string subject = Res.GetString("0fff697c-5b66-46f3-81e0-4659f626fcaa", "Error Sending E-IDO Message");

				FailedMessageHtmlBuilder builder = new FailedMessageHtmlBuilder();
				builder.WriteEmailHeader(subject);
				builder.WriteText(Res.GetString("2728cc78-ac18-4185-8a20-238d41b904ad", "There were errors when attempting to send the following E-IDO messages. Please fix and try again."));

				foreach (KeyValuePair<string, List<FailedMessage>> pair in failures)
				{
					builder.WriteTable(pair.Key, pair.Value);
				}

				builder.WriteText(Res.GetString("ed0e479d-7a2c-4044-868e-36dc0a3b8c13", "You received this email because you are in the '{0}' group and this group is configured to receive these emails. If you do not want to receive these emails then ask your administrator to either remove you from this group or edit the Liner & Agency -> E-IDO Messaging -> Error Email Group registry option.", group.GG_Code, ((IRegistryItemInternals)reg).Location));
				builder.WriteEmailFooter();

				EmailDef email = new EmailDef();
				email.Subject = subject;
				email.Body = builder.ToString();
				email.ContentType = EmailContentTypes.HTML;

				foreach (string recipient in recipients)
				{
					email.AddRecipientForUserCommunication(recipient, RecipientDef.RecipientTypes.TO);
				}

				Env.OutgoingMailManager.CreateAndSave(email);
			}
		}

		readonly FailedMessageList failures;

		#endregion
	}
}



