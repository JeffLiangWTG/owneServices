
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class BorderCargoReleaseMessageStatusCalculator : MessageStatusCalculator
	{
		public BorderCargoReleaseMessageStatusCalculator(IMessageAttachee header)
			: base(header)
		{
		}

		protected override ZString GetClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.BorderCargoReleaseDelete:
					status = ImportMessageStatusList.Codes.ClearBorderCargoReleaseDelete;
					break;
				case EM_MessageSubTypeList.Codes.BorderCargoReleaseReplace:
					status = ImportMessageStatusList.Codes.ClearBorderCargoReleaseReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal;
					break;
			}
			return status;
		}

		protected override ZString GetPartialClearedStatus(ZString messageSubType)
		{
			return ZString.Empty;
		}

		protected override ZString GetRejectedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.BorderCargoReleaseDelete:
					status = ImportMessageStatusList.Codes.ErrorBorderCargoReleaseDelete;
					break;
				case EM_MessageSubTypeList.Codes.BorderCargoReleaseReplace:
					status = ImportMessageStatusList.Codes.ErrorBorderCargoReleaseReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.ErrorBorderCargoReleaseOriginal;
					break;
			}
			return status;
		}

		protected override ZString GetAwaitingStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.BorderCargoReleaseDelete:
					status = ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseDelete;
					break;
				case EM_MessageSubTypeList.Codes.BorderCargoReleaseReplace:
					status = ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseOriginal;
					break;
			}
			return status;
		}
	}
}
