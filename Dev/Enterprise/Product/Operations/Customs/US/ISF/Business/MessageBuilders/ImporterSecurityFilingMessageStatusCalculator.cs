using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ImporterSecurityFilingMessageStatusCalculator : MessageStatusCalculator
	{
		public ImporterSecurityFilingMessageStatusCalculator(IMessageAttachee header)
			: base(header)
		{
		}

		protected override ZString GetClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.ISFDelete:
					status = MessageStatusList.Codes.ClearISFDelete;
					break;
				case EM_MessageSubTypeList.Codes.ISFReplace:
					status = MessageStatusList.Codes.ClearISFReplace;
					break;
				default:
					status = MessageStatusList.Codes.ClearISFAdd;
					break;
			}
			return status;
		}

		protected override ZString GetPartialClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.ISFDelete:
					status = MessageStatusList.Codes.ClearWithWarningISFDelete;
					break;
				case EM_MessageSubTypeList.Codes.ISFReplace:
					status = MessageStatusList.Codes.ClearWithWarningISFReplace;
					break;
				default:
					status = MessageStatusList.Codes.ClearWithWarningISFAdd;
					break;
			}
			return status;
		}

		protected override ZString GetRejectedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.ISFDelete:
					status = MessageStatusList.Codes.ErrorISFDelete;
					break;
				case EM_MessageSubTypeList.Codes.ISFReplace:
					status = MessageStatusList.Codes.ErrorISFReplace;
					break;
				default:
					status = MessageStatusList.Codes.ErrorISFAdd;
					break;
			}
			return status;
		}

		protected override ZString GetAwaitingStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.ISFDelete:
					status = MessageStatusList.Codes.AwaitingISFDelete;
					break;
				case EM_MessageSubTypeList.Codes.ISFReplace:
					status = MessageStatusList.Codes.AwaitingISFReplace;
					break;
				default:
					status = MessageStatusList.Codes.AwaitingISFAdd;
					break;
			}
			return status;
		}
	}
}
