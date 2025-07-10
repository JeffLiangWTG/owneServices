
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class SimplifiedEntryMessageStatusCalculator : MessageStatusCalculator
	{
		public SimplifiedEntryMessageStatusCalculator(IMessageAttachee attachee)
			: base(attachee)
		{
		}

		protected override ZString GetClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.ACECargoReleaseDelete:
					status = ImportMessageStatusList.Codes.ClearACECargoReleaseDelete;
					break;
				case EM_MessageSubTypeList.Codes.ACECargoReleaseReplace:
					status = ImportMessageStatusList.Codes.ClearACECargoReleaseReplace;
					break;
				case EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate:
					status = ImportMessageStatusList.Codes.ClearACECargoReleaseUpdate;
					break;
				default:
					status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
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
				case EM_MessageSubTypeList.Codes.ACECargoReleaseDelete:
					status = ImportMessageStatusList.Codes.ErrorACECargoReleaseDelete;
					break;
				case EM_MessageSubTypeList.Codes.ACECargoReleaseReplace:
					status = ImportMessageStatusList.Codes.ErrorACECargoReleaseReplace;
					break;
				case EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate:
					status = ImportMessageStatusList.Codes.ErrorACECargoReleaseUpdate;
					break;
				default:
					status = ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd;
					break;
			}
			return status;
		}

		protected override ZString GetAwaitingStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.ACECargoReleaseDelete:
					status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseDelete;
					break;
				case EM_MessageSubTypeList.Codes.ACECargoReleaseReplace:
				case EM_MessageSubTypeList.Codes.EntrySummaryReplace:
					status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseReplace;
					break;
				case EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate:
					status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseUpdate;
					break;
				default:
					status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
					break;
			}
			return status;
		}

		protected override ZString GetPendingReviewStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.ACECargoReleaseReplace:
					status = ImportMessageStatusList.Codes.ReplaceRequestPending;
					break;
				default:
					status = ImportMessageStatusList.Codes.CancellationRequestPending;
					break;
			}
			return status;
		}
	}
}
