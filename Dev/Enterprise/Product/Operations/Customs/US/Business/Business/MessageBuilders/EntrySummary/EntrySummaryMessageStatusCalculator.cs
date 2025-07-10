
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class EntrySummaryMessageStatusCalculator : MessageStatusCalculator
	{
		public EntrySummaryMessageStatusCalculator(IMessageAttachee header)
			: base(header)
		{
		}

		protected override ZString GetClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;

			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.EntrySummaryDelete:
					status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
					break;
				case EM_MessageSubTypeList.Codes.EntrySummaryReplace:
					status = ImportMessageStatusList.Codes.ClearEntrySummaryReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
					break;
			}

			return status;
		}

		protected override ZString GetPartialClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;

			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.EntrySummaryDelete:
					status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
					break;
				case EM_MessageSubTypeList.Codes.EntrySummaryReplace:
					status = ImportMessageStatusList.Codes.ClearEntrySummaryReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
					break;
			}

			return status;
		}

		protected override ZString GetRejectedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;

			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.EntrySummaryDelete:
					status = ImportMessageStatusList.Codes.ErrorEntrySummaryDelete;
					break;
				case EM_MessageSubTypeList.Codes.EntrySummaryReplace:
					status = ImportMessageStatusList.Codes.ErrorEntrySummaryReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
					break;
			}

			return status;
		}

		protected override ZString GetWarningStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;

			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.EntrySummaryReplace:
					status = ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings;
					break;
				default:
					status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings;
					break;
			}

			return status;
		}

		protected override ZString GetCensusWarningStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;

			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.EntrySummaryReplace:
					status = ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings;
					break;
				default:
					status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
					break;
			}

			return status;
		}

		protected override ZString GetAwaitingStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;

			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.EntrySummaryDelete:
					status = ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete;
					break;
				case EM_MessageSubTypeList.Codes.EntrySummaryReplace:
					status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
					break;
			}

			return status;
		}
	}
}
