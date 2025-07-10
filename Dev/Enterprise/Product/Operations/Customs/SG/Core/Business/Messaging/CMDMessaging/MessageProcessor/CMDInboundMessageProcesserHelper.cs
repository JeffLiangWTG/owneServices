using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDInboundMessageProcesserHelper
	{
		public CMDInboundMessageProcesserHelper(LoggingInformation logger)
		{
			this.logger = logger;
		}

		readonly LoggingInformation logger;

		public bool CreateMessageThrouthInterchangeAndSendEmail(EDIInterchange interchange, CMDInbound inboundMessage)
		{
			var result = false;
			var message = CreateMessage(interchange, inboundMessage.IsValid ? inboundMessage.StandardMessageIdentifier : (ZString)CMDInbound.Constants.UNK);
			var factory = interchange.Factory;
			var originatingMessage = GetOriginatingMessage(factory, inboundMessage);
			if (originatingMessage == null)
			{
				message.EM_Status = EDIMessage.Status.Failed;
				interchange.EI_Status = EDIInterchange.Status.Failed;
				logger.LogWarning(string.Format(CultureInfo.InvariantCulture, "Could not find originating message for inbound '{0}' message for MAWB '{1}' and HAWB '{2}'. Ignoring.", inboundMessage.StandardMessageIdentifier, inboundMessage.MasterBillNumber, inboundMessage.HouseBillNumber));
			}
			else
			{
				message.EM_LinkedObject = originatingMessage;
				message.EM_Status = EDIMessage.Status.Received;

				ConstructAndSendInboundMessageEmailNotification(originatingMessage, message, inboundMessage, inboundMessage.IsErrorMessage);

				if (!inboundMessage.IsErrorMessage)
				{
					//check if this is the first accepted response.
					var findPreviousAcceptedMessagesQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, originatingMessage.PK);
					findPreviousAcceptedMessagesQuery.AddToFilter(EDIMessageSchema.EM_MessageType, CMDInbound.Constants.CMA);
					findPreviousAcceptedMessagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
					findPreviousAcceptedMessagesQuery.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, message.PK);

					var previouslyAcceptedMessages = factory.Load<EDIMessage>(findPreviousAcceptedMessagesQuery);

					if (previouslyAcceptedMessages.Length == 0)
					{
						var logger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();
						logger.CreateLog(Env.Licence.CMDReporting, true);
					}
				}

				logger.Log(string.Format(CultureInfo.InvariantCulture, "Successfully processed inbound {0} message for MAWB '{1}' and HAWB '{2}'", inboundMessage.StandardMessageIdentifier, inboundMessage.MasterBillNumber, inboundMessage.HouseBillNumber));
				result = true;
			}
			return result;
		}

		CMDEDIMessage CreateMessage(EDIInterchange interchange, ZString messageIdentifier)
		{
			var message = interchange.Factory.New<CMDEDIMessage>();
			message.EM_EI = interchange.PK;
			message.EM_MessageText = interchange.EI_BodyText;
			message.EM_MessageType = messageIdentifier;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			return message;
		}

		internal CMDEDIMessage GetOriginatingMessage(BusinessObjectFactory factory, CMDInbound inbound)
		{
			CMDEDIMessage message = null;

			var consolFilter = new ZQuery(JobConsolSchema.JK_MasterBillNum, inbound.MasterBillNumber.ExcludeChars("-"));
			consolFilter.AddToFilter(JoinCondition.Or, JobConsolSchema.JK_MasterBillNum, SQLComparisonOperator.Equal, inbound.MasterBillNumber);
			var dateFilter = new ZQuery(JobConsolSchema.JK_MasterBillIssueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDate.Today.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
			dateFilter.AddToFilter(JoinCondition.Or, JobConsolSchema.JK_MasterBillIssueDate, System.DBNull.Value);
			var overallQuery = new ZQuery();
			overallQuery.AddToFilter(consolFilter);
			overallQuery.AddToFilter(dateFilter);
			ForwardingConsol[] consols = factory.Load<ForwardingConsol>(overallQuery);
			if (consols.Length == 1)
			{
				var filter = GetOriginatingMessageFilter(consols[0]);
				var messages = factory.Load<CMDEDIMessage>(filter);
				if (messages.Length == 1)
				{
					message = messages[0];
				}
				else
				{
					// If consol type is direct but is attached to more than one shipments, then there is a data corruption error				
					if (!inbound.HouseBillNumber.IsEmpty)
					{
						foreach (CMDEDIMessage msg in messages)
						{
							var shipment = factory.Load<ForwardingShipment>(msg.EM_LinkUniqueID);
							if (shipment.JS_HouseBill.KeepAlphanumericCharacters() == inbound.HouseBillNumber)
							{
								message = msg;
								break;
							}
						}
					}
				}
			}
			else
			{
				logger.LogWarning(string.Format(CultureInfo.InvariantCulture, "Did not find exactly one consol dated within the MAWB recycle period. Found {0}. MAWB# was {1}.", consols.Length, inbound.MasterBillNumber));
			}

			return message;
		}

		ZQuery GetOriginatingMessageFilter(ForwardingConsol consol)
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ApplicationReference, consol.JK_UniqueConsignRef);
			filter.AddToFilter(EDIMessageSchema.EM_LinkTable, JobShipmentSchema.Constants.TableName);
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.SingaporeCMD);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessage.ApplicationCodes.SingaporeCMD);
			filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.NotEqual, CMDGenerator.Constants.TDB);
			filter.AddToFilter(EDIMessageSchema.EM_IsActive, ZBool.True);
			return filter;
		}

		internal CMDInbound CreateInboundFromXDCMessage(EDIMessage xdcMessage)
		{
			var messageText = xdcMessage.EM_MessageText.Trim();
			if (messageText.StartsWith("<CMD>"))
			{
				var cmdMessageContent = messageText.Replace("<CMD>", "").Replace("</CMD>", "");
				string messageType = cmdMessageContent.Left(3);
				var sender = xdcMessage.Interchange?.EI_From ?? ZString.Empty;
				messageText = ZString.Format("CMD{0}\r\n{1}\r\n{2}", messageType, sender, cmdMessageContent);
			}

			return new CMDInbound(messageText);
		}

		#region Email Notifications

		internal void ConstructAndSendInboundMessageEmailNotification(CMDEDIMessage orgMessage, EDIMessage newMessage, CMDInbound inboundMessage, bool isError)
		{
			var notificationEmail = new EmailDef();
			var recipient = orgMessage.Staff?.GS_EmailAddress ?? ZString.Empty;

			if (!recipient.IsEmpty)
			{
				notificationEmail.AddRecipientForSystemCommunication(recipient);
			}
			else
			{
				var collection = new EmailGroupUtility().GetCompanyNotificationGroupEmails();
				var array = new string[collection.Count];
				collection.CopyTo(array, 0);
				notificationEmail.AddRecipientForSystemCommunication(array);
			}

			var errorOrSuccess = isError ? " (ERROR) " : " (SUCCESS) ";
			notificationEmail.Subject = inboundMessage.StandardMessageIdentifier + errorOrSuccess + "Message Received for MasterBill: '" + inboundMessage.MasterBillNumber + "'";
			notificationEmail.Body = "Message Sent: " + orgMessage.EM_StatusDateTime.ToString() + "\n" + newMessage.EM_MessageText;
			try
			{
				Env.OutgoingCustomsMailManager.CreateAndSave(notificationEmail);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.LogError("Could not create email. " + ex.Message);
			}
		}

		#endregion
	}
}
