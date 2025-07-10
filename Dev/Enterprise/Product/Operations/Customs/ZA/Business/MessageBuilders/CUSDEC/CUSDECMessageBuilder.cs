using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	class CUSDECMessageBuilder : EDIFACTMessageBuilder<CusEntryHeader, CUSDECMessage, CUSDECEDIMessage>, ICustomsMessageGenerator
	{
		public CUSDECMessageBuilder(MessageSendingObject source, MessageSubTypes messageSubType)
			: base(source.Header, messageSubType, new ZACharacterSet())
		{
			cUSDECMessageDataProvider = source;
		}

		protected override void PopulateEdifactMessage()
		{
			CUSDECMessageTextBuilder.PopulateCUSDECMessage(edifactMessage, cUSDECMessageDataProvider);
		}

		protected override ZString GetMessageSubType()
		{
			var result = ZString.Empty;
			if (messageSubType == MessageSubTypes.Replace)
			{
				result = MessageSubTypeCodes.Codes.Replace;
			}
			else
			{
				result = base.GetMessageSubType();
			}
			return result;
		}

		protected override CUSDECEDIMessage PopulateMessagesReturningResult()
		{
			var result = base.PopulateMessagesReturningResult();

			var vocReason = cUSDECMessageDataProvider.VOCReason;
			if (result != null && !vocReason.IsEmpty)
			{
				result.SetVOCReason(vocReason);
			}
			result.CopyVOCValues(cUSDECMessageDataProvider, cUSDECMessageDataProvider);

			var messageSubType = result.EM_MessageSubType;
			if (messageSubType == MessageSubTypeCodes.Codes.Original || messageSubType == MessageSubTypeCodes.Codes.Replace)
			{
				result.CopyVPBValues();
			}

			return result;
		}

		EDIMessage ICustomsMessageGenerator.GenerateMessage()
		{
			var msg = PopulateMessagesReturningResult();

			var submissionDate = cUSDECMessageDataProvider.SubmissionDate;
			if (submissionDate > ZDate.Today)
			{
				msg.EM_HeldUntilDate = submissionDate;
			}

			return msg;
		}

		readonly MessageSendingObject cUSDECMessageDataProvider;
	}
}
