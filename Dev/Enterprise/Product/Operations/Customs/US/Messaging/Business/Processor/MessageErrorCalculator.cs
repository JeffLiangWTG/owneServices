using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class MessageErrorCalculator
	{
		public MessageErrorCalculator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public void ReportAbnormalityInResponseMessageIfNeeded(IMessageAttachee messageAttachee, CBPEDIMessage message, bool isFailure)
		{
			if (message != null && !IsAbnormalityMessageReportingDisable)
			{
				var topLevelBusinessObject = messageAttachee.TopLevelBusinessObject;
				var originalMessage = message.OriginalMessage;
				if (topLevelBusinessObject != null && originalMessage != null)
				{
					AbnormalityReporter.Report(topLevelBusinessObject, originalMessage, message, new ResponseMessageAbnormalityReporter.IsMessageClear(() => !isFailure));
				}
			}
		}

		public ZString GetLongDescription(ZString errorCode, ZString initialDescription)
		{
			return GetLongDescriptionCore(errorCode, initialDescription);
		}

		#region Implementation
		protected readonly BusinessObjectFactory factory;

		protected ResponseMessageAbnormalityReporter AbnormalityReporter
		{
			get
			{
				if (abnormalityReporter == null)
				{
					abnormalityReporter = new ResponseMessageAbnormalityReporter();
					abnormalityReporter.GetMessageOverride = GetMessage;
					abnormalityReporter.GetReportKeyOverride = GetReportKey;
				}
				return abnormalityReporter;
			}
		}
		ResponseMessageAbnormalityReporter abnormalityReporter;

		protected virtual bool IsAbnormalityMessageReportingDisable
		{
			get { return false; }
		}

		protected virtual ZString GetLongDescriptionCore(ZString errorCode, ZString initialDescription)
		{
			return initialDescription;
		}

		ZString GetMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			ZStringBuilder builder = new ZStringBuilder();
			builder.Append("Formatted Message:");
			builder.Append(message.EM_FormattedMessageText);
			builder.Append("");
			builder.Append("Message Interpretation:");
			builder.Append(message.EM_MessageInterpretation);

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetReportKey(ZString originalKey, Enterprise.Messaging.Business.EDIMessage message)
		{
			return originalKey + string.Format(" for Message Type '{0}' and Application Code '{1}'", message.EM_MessageType, message.EM_ApplicationCode);
		}
		#endregion
	}
}
