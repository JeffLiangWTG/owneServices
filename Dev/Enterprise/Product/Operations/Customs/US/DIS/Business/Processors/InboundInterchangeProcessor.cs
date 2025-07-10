using System.Linq;
using System.Xml.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.DIS.Business
{
	/// <summary>
	/// Collect QUEed interchanges and create EDIMessage
	/// </summary>
	public class InboundInterchangeProcessor : Enterprise.Messaging.Business.InboundInterchangeProcessor
	{
		public InboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes
		{
			get { return new string[] { EDIMessage.ApplicationCodes.USCustomsDIS }; }
		}

		protected override IInboundMessageCreator GetMessageCreator(Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new InboundMessageCreator());
		}
		IInboundMessageCreator messageCreator;

		#region InboundMessageCreator Class

		class InboundMessageCreator : IInboundMessageCreator
		{
			void IInboundMessageCreator.CreateMessagesForInterchange(Enterprise.Messaging.Business.EDIInterchange interchange)
			{
				var message = interchange.Factory.New<EDIMessage>();
				interchange.ContainedMessages.Add(message);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageText = interchange.EI_BodyText;

				message.EM_Status = EDIMessage.Status.Queued;

				var messageContent = message.MessageContent;
				message.EM_MessageText = messageContent.ToString();//to fix indentation

				var messageHeader = messageContent.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.MessageHeader));
				var messageBody = messageContent.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.MessageBody));

				if (messageHeader != null)
				{
					message.EM_MessageNum = GetMessageID(messageContent);

					var messageType = messageHeader.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.MessageType));
					var docReviewResponse = messageBody.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewResponse));
					message.EM_MessageType = docReviewResponse != null ? MessageTypeList.Codes.DocumentReviewResponse : messageType != null ? MessageTypeList.GetCodeFrom(messageType.Value) : string.Empty;

					interchange.EI_InterchangeType = message.EM_MessageType;
				}

				message.NullifyMessageContent();
			}

			string GetMessageID(XElement messageContent)
			{
				var messageBody = messageContent.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.MessageBody));

				var messageID = messageBody.Descendants().FirstOrDefault(x => x.Matches(EDIMessage.Constants.MessageID));

				return messageID != null ? messageID.Value : string.Empty;
			}
		}

		#endregion
	}
}
