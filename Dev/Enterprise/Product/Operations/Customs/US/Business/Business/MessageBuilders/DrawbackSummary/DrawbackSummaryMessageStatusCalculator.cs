using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class DrawbackSummaryMessageStatusCalculator
	{
		public ZString Calculate(MQEDIMessage message, ABIResponseStatus status)
		{
			ZString result = ZString.Empty;
			if (message.IsTransmitMessage)
			{
				result = GetAwaitingStatus(message);
			}
			else
			{
				switch (status)
				{
					case ABIResponseStatus.Rejected:
						result = GetRejectedStatus(message);
						break;
					case ABIResponseStatus.CensusWarning:
						result = GetCensusWarningStatus(message);
						break;
					default:
						result = GetClearedStatus(message);
						break;
				}
			}
			return result;
		}

		ZString GetAwaitingStatus(MQEDIMessage message)
		{
			var result = ZString.Empty;
			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryDelete)
			{
				result = DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryDelete;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal)
			{
				result = DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryOriginal;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryReplacement)
			{
				result = DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryReplacement;
			}
			return result;
		}

		ZString GetRejectedStatus(MQEDIMessage message)
		{
			var result = ZString.Empty;
			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryDelete)
			{
				result = DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryDelete;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal)
			{
				result = DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryOriginal;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryReplacement)
			{
				result = DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryReplacement;
			}
			return result;
		}

		ZString GetClearedStatus(MQEDIMessage message)
		{
			var result = ZString.Empty;
			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryDelete)
			{
				result = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryDelete;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal)
			{
				result = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryReplacement)
			{
				result = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryReplacement;
			}
			return result;
		}

		ZString GetCensusWarningStatus(MQEDIMessage message)
		{
			var result = ZString.Empty;

			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal)
			{
				result = DrawbackSummaryStatusList.Codes.DrawbackSummaryOriginalAcceptedWithCensusWarnings;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.DrawbackSummaryReplacement)
			{
				result = DrawbackSummaryStatusList.Codes.DrawbackSummaryReplacementAcceptedWithCensusWarnings;
			}

			return result;
		}
	}
}
