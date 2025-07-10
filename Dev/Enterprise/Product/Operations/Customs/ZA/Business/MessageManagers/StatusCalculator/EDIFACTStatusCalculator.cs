using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.ZA;

namespace Enterprise.Customs.ZA.Business.MessageManagers
{
	public class EDIFACTStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public EDIFACTStatusCalculator(ZString messageType)
		{
			this.messageType = messageType;
		}

		public override ZString MessageTypeDescription => messageType;

		public override bool IsClear(ZString currentJobStatus)
		{
			ErrorReporter.ReportOnce("Enterprise.Customs.ZA.Business.MessageManagers.EDIFACTStatusCalculator.IsClear(ZString currentJobStatus) is not supported yet.");
			return false;
		}

		public override bool IsLodged(ZString currentJobStatus)
		{
			ErrorReporter.ReportOnce("Enterprise.Customs.ZA.Business.MessageManagers.EDIFACTStatusCalculator.IsLodged(ZString currentJobStatus) is not supported yet.");
			return false;
		}

		public override bool IsWithdrawn(ZString currentJobStatus)
		{
			ErrorReporter.ReportOnce("Enterprise.Customs.ZA.Business.MessageManagers.EDIFACTStatusCalculator.IsWithdrawn(ZString currentJobStatus) is not supported yet.");
			return false;
		}

		public override bool IsAwaitingReply(ZString currentMessageStatus)
		{
			return ZAMessageStatusList.IsAwaiting(currentMessageStatus);
		}

		public override ZString GetMessageAwaitingStatus(ZString messageSubType)
		{
			return ZAMessageStatusList.Codes.AwaitingResponse;
		}

		public override ZString GetMessageAcknowledgedStatus(ZString messageSubType)
		{
			return ZAMessageStatusList.Codes.Acknowledged;
		}

		public override ZString GetMessageClearedStatus(ZString messageSubType)
		{
			return ZAMessageStatusList.Codes.Acknowledged;
		}

		public override ZString GetMessageRejectedStatus(ZString messageSubType)
		{
			return ZAMessageStatusList.Codes.Error;
		}

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			// TODO: ToBeImplemented, Status Transition logic to be implemented later #Victor 20160422
			return ZString.Empty;
		}

		readonly ZString messageType;
	}
}
