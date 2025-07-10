using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FTZMessageStatusCalculator
	{
		public ZString Calculate(MQEDIMessage message, bool isFailure, bool isCensusWarning)
		{
			var result = ZString.Empty;
			var messageSubType = message.EM_MessageSubType;
			if (message.IsTransmitMessage)
			{
				result = GetAwaitingStatus(messageSubType);
			}
			else
			{
				if (isFailure)
				{
					result = GetRejectedStatus(messageSubType);
				}
				else
				{
					result = GetClearedStatus(messageSubType, isCensusWarning);
				}
			}
			return result;
		}

		ZString GetAwaitingStatus(ZString messageSubType)
		{
			var result = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.FTZAdmissionDelete:
					result = FTZMessageStatusList.Codes.AwaitingFTZAdmissionDelete;
					break;
				case EM_MessageSubTypeList.Codes.FTZAdmissionAdd:
					result = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd;
					break;
				case EM_MessageSubTypeList.Codes.FTZAdmissionReplace:
					result = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAmend;
					break;
				case EM_MessageSubTypeList.Codes.FTZPermitToTransfer:
					result = FTZMessageStatusList.Codes.AwaitingPermitToTransfer;
					break;
				case EM_MessageSubTypeList.Codes.FTZPermitToTransferArrival:
					result = FTZMessageStatusList.Codes.AwaitingPermitToTransferArrival;
					break;
				case EM_MessageSubTypeList.Codes.FZArrival:
					result = FTZMessageStatusList.Codes.AwaitingGoodsArrival;
					break;
				case EM_MessageSubTypeList.Codes.FZConcurrence:
					result = FTZMessageStatusList.Codes.AwaitingConcurrence;
					break;
				case EM_MessageSubTypeList.Codes.FZDelivery:
					result = FTZMessageStatusList.Codes.AwaitingDeliveryOfGoods;
					break;
				case EM_MessageSubTypeList.Codes.FZUnconcurrence:
					result = FTZMessageStatusList.Codes.AwaitingUnconcurrence;
					break;
				case EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer:
					result = FTZMessageStatusList.Codes.AwaitingCancelPermitToTransfer;
					break;
				case EM_MessageSubTypeList.Codes.FTZSendPermitToTransferUnArrival:
					result = FTZMessageStatusList.Codes.AwaitingPermitToTransferUnArrival;
					break;
				default:
					break;
			}
			return result;
		}

		ZString GetRejectedStatus(ZString messageSubType)
		{
			var result = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.FTZAdmissionDelete:
					result = FTZMessageStatusList.Codes.ErrorFTZAdmissionDelete;
					break;
				case EM_MessageSubTypeList.Codes.FTZAdmissionAdd:
					result = FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd;
					break;
				case EM_MessageSubTypeList.Codes.FTZAdmissionReplace:
					result = FTZMessageStatusList.Codes.ErrorFTZAdmissionAmend;
					break;
				case EM_MessageSubTypeList.Codes.FTZPermitToTransfer:
					result = FTZMessageStatusList.Codes.ErrorPermitToTransfer;
					break;
				case EM_MessageSubTypeList.Codes.FTZPermitToTransferArrival:
					result = FTZMessageStatusList.Codes.ErrorPermitToTransferArrival;
					break;
				case EM_MessageSubTypeList.Codes.FZArrival:
					result = FTZMessageStatusList.Codes.ErrorGoodsArrival;
					break;
				case EM_MessageSubTypeList.Codes.FZConcurrence:
					result = FTZMessageStatusList.Codes.ErrorConcurrence;
					break;
				case EM_MessageSubTypeList.Codes.FZDelivery:
					result = FTZMessageStatusList.Codes.ErrorDeliveryOfGoods;
					break;
				case EM_MessageSubTypeList.Codes.FZUnconcurrence:
					result = FTZMessageStatusList.Codes.ErrorUnconcurrence;
					break;
				case EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer:
					result = FTZMessageStatusList.Codes.PermitToTransferCancelUnauthorized;
					break;
				case EM_MessageSubTypeList.Codes.FTZSendPermitToTransferUnArrival:
					result = FTZMessageStatusList.Codes.PermitToTransferArrivalCancelUnauthorized;
					break;
				default:
					break;
			}
			return result;
		}

		ZString GetClearedStatus(ZString messageSubType, bool isCensusWarning)
		{
			var result = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.FTZAdmissionDelete:
					result = FTZMessageStatusList.Codes.ClearFTZAdmissionDelete;
					break;
				case EM_MessageSubTypeList.Codes.FTZAdmissionAdd:
					if (isCensusWarning)
					{
						result = FTZMessageStatusList.Codes.ClearFTZAdmissionAddWithWarnings;
					}
					else
					{
						result = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
					}
					break;
				case EM_MessageSubTypeList.Codes.FTZAdmissionReplace:
					result = FTZMessageStatusList.Codes.ClearFTZAdmissionAmend;
					break;
				case EM_MessageSubTypeList.Codes.FTZPermitToTransfer:
					result = FTZMessageStatusList.Codes.ClearPermitToTransfer;
					break;
				case EM_MessageSubTypeList.Codes.FTZPermitToTransferArrival:
					result = FTZMessageStatusList.Codes.PermitToTransferArrived;
					break;
				case EM_MessageSubTypeList.Codes.FZArrival:
					result = FTZMessageStatusList.Codes.ClearGoodsArrival;
					break;
				case EM_MessageSubTypeList.Codes.FZConcurrence:
					result = FTZMessageStatusList.Codes.ClearConcurrence;
					break;
				case EM_MessageSubTypeList.Codes.FZDelivery:
					result = FTZMessageStatusList.Codes.ClearDeliveryOfGoods;
					break;
				case EM_MessageSubTypeList.Codes.FZUnconcurrence:
					result = FTZMessageStatusList.Codes.ClearUnconcurrence;
					break;
				case EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer:
					result = FTZMessageStatusList.Codes.PermitToTransferCancelAccepted;
					break;
				case EM_MessageSubTypeList.Codes.FTZSendPermitToTransferUnArrival:
					result = FTZMessageStatusList.Codes.PermitToTransferUnArrived;
					break;
				default:
					break;
			}
			return result;
		}
	}
}
