using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ReconMessageStatusCalculator
	{
		public ZString Calculate(MQEDIMessage message, ABIResponseStatus status, bool isWarning)
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
					case ABIResponseStatus.Cleared:
						result = GetClearedStatus(message, isWarning);
						break;
				}
			}

			return result;
		}

		ZString GetAwaitingStatus(MQEDIMessage message)
		{
			ZString result = ZString.Empty;
			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ReconDelete)
			{
				result = ReconMessageStatusList.Codes.AwaitingReconDelete;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ReconOriginal)
			{
				result = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ReconReplace)
			{
				result = ReconMessageStatusList.Codes.AwaitingReconReplace;
			}
			return result;
		}

		ZString GetRejectedStatus(MQEDIMessage message)
		{
			ZString result = ZString.Empty;
			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ReconDelete)
			{
				result = ReconMessageStatusList.Codes.ErrorReconDelete;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ReconOriginal)
			{
				result = ReconMessageStatusList.Codes.ErrorReconOriginal;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ReconReplace)
			{
				result = ReconMessageStatusList.Codes.ErrorReconReplace;
			}
			return result;
		}

		ZString GetClearedStatus(MQEDIMessage message, bool isWarning)
		{
			ZString result = ZString.Empty;
			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ReconDelete)
			{
				result = ReconMessageStatusList.Codes.ClearReconDelete;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ReconOriginal)
			{
				result = isWarning ? ReconMessageStatusList.Codes.ReconOriginalAcceptedWarnings : ReconMessageStatusList.Codes.ClearReconOriginal;
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ReconReplace)
			{
				result = isWarning ? ReconMessageStatusList.Codes.ReconReplaceAcceptedWarnings : ReconMessageStatusList.Codes.ClearReconReplace;
			}
			return result;
		}
	}
}
