namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	partial class AMSMessageSubTypeList
	{
		public static string GetSubTypeFromActionCode(ActionCode action)
		{
			var result = "";
			switch (action)
			{
				case ActionCode.AmendingAdd:
					result = AMSMessageSubTypeList.Codes.AmendingAdd;
					break;
				case ActionCode.AmendingUpdate:
					result = AMSMessageSubTypeList.Codes.AmendingUpdate;
					break;
				case ActionCode.AmendingDelete:
					result = AMSMessageSubTypeList.Codes.AmendingDelete;
					break;
				case ActionCode.Equipment:
					result = AMSMessageSubTypeList.Codes.Equipment;
					break;
				case ActionCode.GeneralOrderStatus:
					result = AMSMessageSubTypeList.Codes.GeneralOrderStatus;
					break;
				case ActionCode.InBondArrival:
					result = AMSMessageSubTypeList.Codes.InBondArrival;
					break;
				case ActionCode.InBondDiversion:
					result = AMSMessageSubTypeList.Codes.InBondDiversion;
					break;
				case ActionCode.InBondExportation:
					result = AMSMessageSubTypeList.Codes.InBondExportation;
					break;
				case ActionCode.InBondTransferOfLiability:
					result = AMSMessageSubTypeList.Codes.InBondTransferOfLiability;
					break;
				case ActionCode.CancelPermitToTransfer:
					result = AMSMessageSubTypeList.Codes.CancelPermitToTransfer;
					break;
				case ActionCode.VesselArrival:
					result = AMSMessageSubTypeList.Codes.VesselArrival;
					break;
				case ActionCode.VesselDeparture:
					result = AMSMessageSubTypeList.Codes.VesselDeparture;
					break;
				case ActionCode.ChangeEstDateOfArrival:
					result = AMSMessageSubTypeList.Codes.ChangeEstimatedDateOfArrival;
					break;
				case ActionCode.PermitToTransfer:
					result = AMSMessageSubTypeList.Codes.PermitToTransfer;
					break;
				case ActionCode.SubsequentInBondOriginal:
					result = AMSMessageSubTypeList.Codes.SubsequentInBondOriginal;
					break;
				case ActionCode.SubsequentInBondAmendment:
					result = AMSMessageSubTypeList.Codes.SubsequentInBondAmendment;
					break;
				case ActionCode.SubsequentInBondDelete:
					result = AMSMessageSubTypeList.Codes.SubsequentInBondDelete;
					break;
				default:
					result = AMSMessageSubTypeList.Codes.Creating;
					break;
			}
			return result;
		}

		public static ActionCode GetActionCodeFromSubType(string subType)
		{
			var result = ActionCode.Creating;
			switch (subType)
			{
				case AMSMessageSubTypeList.Codes.AmendingAdd:
					result = ActionCode.AmendingAdd;
					break;
				case AMSMessageSubTypeList.Codes.AmendingUpdate:
					result = ActionCode.AmendingUpdate;
					break;
				case AMSMessageSubTypeList.Codes.AmendingDelete:
					result = ActionCode.AmendingDelete;
					break;
				case AMSMessageSubTypeList.Codes.Equipment:
					result = ActionCode.Equipment;
					break;
				case AMSMessageSubTypeList.Codes.GeneralOrderStatus:
					result = ActionCode.GeneralOrderStatus;
					break;
				case AMSMessageSubTypeList.Codes.InBondArrival:
				case AMSMessageSubTypeList.Codes.PaperlessInBondOrVesselArrival:
					result = ActionCode.InBondArrival;
					break;
				case AMSMessageSubTypeList.Codes.InBondDiversion:
					result = ActionCode.InBondDiversion;
					break;
				case AMSMessageSubTypeList.Codes.InBondExportation:
					result = ActionCode.InBondExportation;
					break;
				case AMSMessageSubTypeList.Codes.InBondTransferOfLiability:
					result = ActionCode.InBondTransferOfLiability;
					break;
				case AMSMessageSubTypeList.Codes.CancelPermitToTransfer:
					result = ActionCode.CancelPermitToTransfer;
					break;
				case AMSMessageSubTypeList.Codes.VesselArrival:
					result = ActionCode.VesselArrival;
					break;
				case AMSMessageSubTypeList.Codes.VesselDeparture:
					result = ActionCode.VesselDeparture;
					break;
				case AMSMessageSubTypeList.Codes.PermitToTransfer:
					result = ActionCode.PermitToTransfer;
					break;
				case AMSMessageSubTypeList.Codes.SubsequentInBondOriginal:
					result = ActionCode.SubsequentInBondOriginal;
					break;
				case AMSMessageSubTypeList.Codes.SubsequentInBondAmendment:
					result = ActionCode.SubsequentInBondAmendment;
					break;
				case AMSMessageSubTypeList.Codes.SubsequentInBondDelete:
					result = ActionCode.SubsequentInBondDelete;
					break;
			}
			return result;
		}

		public static bool IsVesselArrivalEventRelevent(string subType)
		{
			return subType == AMSMessageSubTypeList.Codes.ChangeEstimatedDateOfArrival
				|| subType == AMSMessageSubTypeList.Codes.VesselArrival;
		}
	}
}
