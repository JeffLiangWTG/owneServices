using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	public static class ExtractFTZResponseResult
	{
		public static bool IsFailure(ZString narrativeMessageTypeCode)
		{
			return narrativeMessageTypeCode == DataRejection || !narrativeMessageTypeCode.IsEmpty && narrativeMessageTypeCode != DataAcceptance && narrativeMessageTypeCode != DataAcceptanceWithWarning;
		}

		public static bool IsCancelPTTMessageCode(ZString code)
		{
			return code == CancelPTTDataAccepted || code == CancelPTTNotAuthorized;
		}

		public static bool IsUnauthorized(ZString notificationCode)
		{
			return notificationCode.Contains(UnauthorizedError) || notificationCode.Contains(CancelPTTNotAuthorized);
		}
		const string DataRejection = "01";
		const string DataAcceptance = "02";
		const string DataAcceptanceWithWarning = "03";
		const string UnauthorizedError = "073";
		internal const string CancelPTTDataAccepted = "224";
		internal const string CancelPTTNotAuthorized = "215";
	}
}
