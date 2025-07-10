using System;
using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.DIS.Business
{
	/// <summary>
	/// Collect QUEed inbound EDIMessage and process them
	/// </summary>
	public class InboundMessageProcessor : BaseMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new MessageProcessorFactory(Logger));
			return result;
		}

		class MessageProcessorFactory : ApplicationTypeMessageProcessor
		{
			public MessageProcessorFactory(LoggingInformation logger)
				: base(logger)
			{
			}

			protected override string ApplicationCodeCore
			{
				get { return EDIMessage.ApplicationCodes.USCustomsDIS; }
			}

			protected override string MessageFriendlyNameCore
			{
				get { return "DIS Response Messages"; }
			}

			protected override void ProcessMessageCore(Enterprise.Messaging.Business.EDIMessage message)
			{
				if (!IsProcessibleMessageType(message.EM_MessageType))
				{
					var errorMessage = "Cannot process: Unidentified message type:" + message.EM_MessageType;
					var branch = ProcesserHelper.GetFallbackBranch(null, (message as EDIMessage)?.RelatedMessage, message) ?? GlbBranch.CurrentBranch;
					ProcesserHelper.SendNotification(message.Factory, errorMessage, message.EM_MessageText, branch.GB_GC, branch.PK);
					throw new InvalidOperationException(errorMessage);
				}

				if (message.EM_MessageType == MessageTypeList.Codes.DocumentReviewResponse)
				{
					new DocumentReviewResponseProcessor(Logger).Process((EDIMessage)message);
				}
				else if (message.EM_MessageType == MessageTypeList.Codes.DocumentValidationResponse)
				{
					new DocumentValidationResponseProcessor(Logger).Process((EDIMessage)message);
				}
				message.EM_Status = EDIMessage.Status.Received;
			}

			static bool IsProcessibleMessageType(string messageType)
			{
				return messageType == MessageTypeList.Codes.DocumentReviewResponse
					|| messageType == MessageTypeList.Codes.DocumentValidationResponse;
			}
		}
	}
}
