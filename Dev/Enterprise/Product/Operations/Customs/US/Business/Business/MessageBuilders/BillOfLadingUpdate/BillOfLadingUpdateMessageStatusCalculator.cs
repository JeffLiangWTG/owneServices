
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class BillOfLadingUpdateMessageStatusCalculator : MessageStatusCalculator
	{
		public BillOfLadingUpdateMessageStatusCalculator(IMessageAttachee header)
			: base(header)
		{
		}

		protected override ZString GetClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.BillOfLadingUpdate:
					status = ImportMessageStatusList.Codes.ClearBillOfLadingUpdate;
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
				case EM_MessageSubTypeList.Codes.BillOfLadingUpdate:
					status = ImportMessageStatusList.Codes.ErrorBillOfLadingUpdate;
					break;
			}

			return status;
		}

		protected override ZString GetAwaitingStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.BillOfLadingUpdate:
					status = ImportMessageStatusList.Codes.AwaitingBillOfLadingUpdate;
					break;
			}

			return status;
		}
	}
}
