
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class CargoReleaseMessageStatusCalculator : MessageStatusCalculator
	{
		public CargoReleaseMessageStatusCalculator(IMessageAttachee header)
			: base(header)
		{
		}

		protected override ZString GetClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.CargoReleaseDelete:
					status = ImportMessageStatusList.Codes.ClearCargoReleaseDelete;
					break;
				case EM_MessageSubTypeList.Codes.CargoReleaseReplace:
					status = ImportMessageStatusList.Codes.ClearCargoReleaseReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
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
				case EM_MessageSubTypeList.Codes.CargoReleaseDelete:
					status = ImportMessageStatusList.Codes.ErrorCargoReleaseDelete;
					break;
				case EM_MessageSubTypeList.Codes.CargoReleaseReplace:
					status = ImportMessageStatusList.Codes.ErrorCargoReleaseReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.ErrorCargoReleaseOriginal;
					break;
			}
			return status;
		}

		protected override ZString GetAwaitingStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.CargoReleaseDelete:
					status = ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete;
					break;
				case EM_MessageSubTypeList.Codes.CargoReleaseReplace:
					status = ImportMessageStatusList.Codes.AwaitingCargoReleaseReplace;
					break;
				default:
					status = ImportMessageStatusList.Codes.AwaitingCargoReleaseOriginal;
					break;
			}
			return status;
		}
	}
}
