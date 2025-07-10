
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public class InBondMessageStatusCalculator : MessageStatusCalculator
	{
		public InBondMessageStatusCalculator(IMessageAttachee header)
			: base(header)
		{
		}

		protected override ZString GetPartialClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.InBondDepartureOriginal:
					status = ImportMessageStatusList.Codes.ClearDeparturePartialOriginal;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureReplacement:
					status = ImportMessageStatusList.Codes.ClearDeparturePartialAmendment;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureDelete:
					status = ImportMessageStatusList.Codes.ClearDeparturePartialWithdraw;
					break;
				case EM_MessageSubTypeList.Codes.InBondArrival:
					status = ImportMessageStatusList.Codes.ClearArrival;
					break;
				case EM_MessageSubTypeList.Codes.InBondExportation:
					status = ImportMessageStatusList.Codes.ClearExportation;
					break;
				case EM_MessageSubTypeList.Codes.InBondFDATransmission:
					status = ImportMessageStatusList.Codes.ClearFDATransmission;
					break;
				case EM_MessageSubTypeList.Codes.InBondTransferOfLiability:
					status = ImportMessageStatusList.Codes.ClearTransferOfLiability;
					break;
				case EM_MessageSubTypeList.Codes.FDACorrection:
					status = ImportMessageStatusList.Codes.ClearFDACorrection;
					break;
			}
			return status;
		}

		protected override ZString GetRejectedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.InBondDepartureOriginal:
					status = ImportMessageStatusList.Codes.ErrorDepartureOriginal;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureReplacement:
					status = ImportMessageStatusList.Codes.ErrorDepartureAmendment;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureDelete:
					status = ImportMessageStatusList.Codes.ErrorDepartureWithdraw;
					break;
				case EM_MessageSubTypeList.Codes.InBondArrival:
					status = ImportMessageStatusList.Codes.ErrorArrival;
					break;
				case EM_MessageSubTypeList.Codes.InBondExportation:
					status = ImportMessageStatusList.Codes.ErrorExportation;
					break;
				case EM_MessageSubTypeList.Codes.InBondFDATransmission:
					status = ImportMessageStatusList.Codes.ErrorFDATransmission;
					break;
				case EM_MessageSubTypeList.Codes.InBondTransferOfLiability:
					status = ImportMessageStatusList.Codes.ErrorTransferOfLiability;
					break;
				case EM_MessageSubTypeList.Codes.FDACorrection:
					status = ImportMessageStatusList.Codes.ErrorFDACorrection;
					break;
			}
			return status;
		}

		protected override ZString GetAwaitingStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.InBondDepartureOriginal:
					status = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureReplacement:
					status = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureDelete:
					status = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
					break;
				case EM_MessageSubTypeList.Codes.InBondArrival:
					status = ImportMessageStatusList.Codes.AwaitingArrival;
					break;
				case EM_MessageSubTypeList.Codes.InBondExportation:
					status = ImportMessageStatusList.Codes.AwaitingExportation;
					break;
				case EM_MessageSubTypeList.Codes.InBondFDATransmission:
					status = ImportMessageStatusList.Codes.AwaitingFDATransmission;
					break;
				case EM_MessageSubTypeList.Codes.InBondTransferOfLiability:
					status = ImportMessageStatusList.Codes.AwaitingTransferOfLiability;
					break;
				case EM_MessageSubTypeList.Codes.FDACorrection:
					status = ImportMessageStatusList.Codes.AwaitingFDACorrection;
					break;
			}
			return status;
		}

		protected override ZString GetClearedStatus(ZString messageSubType)
		{
			ZString status = ZString.Empty;
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.InBondDepartureOriginal:
					status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureReplacement:
					status = ImportMessageStatusList.Codes.ClearDepartureAmendment;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureDelete:
					status = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
					break;
				case EM_MessageSubTypeList.Codes.InBondArrival:
					status = ImportMessageStatusList.Codes.ClearArrival;
					break;
				case EM_MessageSubTypeList.Codes.InBondExportation:
					status = ImportMessageStatusList.Codes.ClearExportation;
					break;
				case EM_MessageSubTypeList.Codes.InBondFDATransmission:
					status = ImportMessageStatusList.Codes.ClearFDATransmission;
					break;
				case EM_MessageSubTypeList.Codes.InBondTransferOfLiability:
					status = ImportMessageStatusList.Codes.ClearTransferOfLiability;
					break;
				case EM_MessageSubTypeList.Codes.FDACorrection:
					status = ImportMessageStatusList.Codes.ClearFDACorrection;
					break;
			}
			return status;
		}

		protected override void SetMessageStatus(MQEDIMessage lastMessage, string statusCalculated)
		{
			if (attachee is CusInBondMoveHeader moveHeader && moveHeader.Header.BH_ApplicationCode == CusInBondApplicationCodeList.Codes.InBond)
			{
				if (lastMessage.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability || lastMessage.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse)
				{
					moveHeader.BM_MessageStatus = statusCalculated;
				}
				else
				{
					moveHeader.BM_CustomsStatus = statusCalculated;
				}
			}
			else
			{
				base.SetMessageStatus(lastMessage, statusCalculated);
			}
		}
	}
}
