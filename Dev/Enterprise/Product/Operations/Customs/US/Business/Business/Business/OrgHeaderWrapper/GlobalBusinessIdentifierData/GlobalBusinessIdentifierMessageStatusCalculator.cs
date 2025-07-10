using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business
{
	public class GlobalBusinessIdentifierMessageStatusCalculator : MessageStatusCalculator
	{
		public GlobalBusinessIdentifierMessageStatusCalculator(IMessageAttachee attachee) : base(attachee)
		{
		}

		public const string Cleared = "Cleared";
		public const string Rejected = "Rejected";
		public const string Awaiting = "Awaiting";
		public const string InvalidMessageSubTypeExceptionMessage = @"Invalid message subtype for {0} status calculation.
Message Sub Type: {1}
Organisation PK: {2}";

		protected override ZString GetAwaitingStatus(ZString messageSubType)
		{
			var result = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierAdd:
					result = GBISubmissionStatusList.Codes.AwaitingGBIAdd;
					break;
				case EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierUpdate:
					result = GBISubmissionStatusList.Codes.AwaitingGBIUpdate;
					break;
				case EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierDelete:
					result = GBISubmissionStatusList.Codes.AwaitingGBIDelete;
					break;
				default:
					throw new DeveloperNotificationException(GetExceptionDetails(messageSubType, Awaiting));
			}
			return result;
		}

		protected override ZString GetClearedStatus(ZString messageSubType)
		{
			var result = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierAdd:
					result = GBISubmissionStatusList.Codes.ClearGBIAdd;
					break;
				case EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierUpdate:
					result = GBISubmissionStatusList.Codes.ClearGBIUpdate;
					break;
				case EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierDelete:
					result = GBISubmissionStatusList.Codes.ClearGBIDelete;
					break;
				default:
					throw new DeveloperNotificationException(GetExceptionDetails(messageSubType, Cleared));
			}
			return result;
		}

		protected override ZString GetPartialClearedStatus(ZString messageSubType)
		{
			return ZString.Empty;
		}

		protected override ZString GetRejectedStatus(ZString messageSubType)
		{
			var result = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierAdd:
					result = GBISubmissionStatusList.Codes.ErrorGBIAdd;
					break;
				case EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierUpdate:
					result = GBISubmissionStatusList.Codes.ErrorGBIUpdate;
					break;
				case EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierDelete:
					result = GBISubmissionStatusList.Codes.ErrorGBIDelete;
					break;
				default:
					throw new DeveloperNotificationException(GetExceptionDetails(messageSubType, Rejected));
			}
			return result;
		}

		ZString GetExceptionDetails(ZString messageSubType, string calculationType)
		{
			return string.Format(InvalidMessageSubTypeExceptionMessage, calculationType, messageSubType, attachee.BusinessObjectPK);
		}
	}
}
