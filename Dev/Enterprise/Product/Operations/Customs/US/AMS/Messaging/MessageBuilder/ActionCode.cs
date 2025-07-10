namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public enum ActionCode
	{
		Creating = 0,
		AmendingAdd = 1,
		AmendingUpdate = 2,
		AmendingDelete = 3,
		Equipment = 4,
		GeneralOrderStatus = 5,
		PermitToTransfer = 6,
		SubsequentInBondOriginal = 7,
		SubsequentInBondAmendment = 8,
		InBondArrival = 9,
		InBondExportation = 10,
		InBondTransferOfLiability = 11,
		InBondDiversion = 12,
		CancelPermitToTransfer = 13,
		VesselArrival = 14,
		VesselDeparture = 15,
		ChangeEstDateOfArrival = 16,
		SubsequentInBondDelete = 17,
	}

	public static class ActionCodeTool
	{
		public static bool IsAmendingType(ActionCode actionCode)
		{
			return actionCode == ActionCode.AmendingAdd || actionCode == ActionCode.AmendingUpdate || actionCode == ActionCode.AmendingDelete || actionCode == ActionCode.SubsequentInBondAmendment || actionCode == ActionCode.SubsequentInBondDelete;
		}

		public static bool IsInBondArrivalExportationTOL(ActionCode actionCode)
		{
			return actionCode == ActionCode.InBondArrival ||
				actionCode == ActionCode.InBondExportation ||
				actionCode == ActionCode.InBondTransferOfLiability;
		}

		public static bool IsInBondVesselArrivalDeparture(ActionCode actionCode)
		{
			return IsInBondArrivalExportationTOL(actionCode) ||
				actionCode == ActionCode.InBondDiversion ||
				actionCode == ActionCode.CancelPermitToTransfer ||
				IsVesselEvent(actionCode);
		}

		public static bool IsInBondType(ActionCode actionCode)
		{
			return actionCode == ActionCode.InBondArrival || actionCode == ActionCode.SubsequentInBondOriginal || actionCode == ActionCode.SubsequentInBondAmendment || actionCode == ActionCode.SubsequentInBondDelete || actionCode == ActionCode.InBondExportation || actionCode == ActionCode.InBondTransferOfLiability || actionCode == ActionCode.InBondDiversion;
		}

		public static bool IsPermitToTransferAction(ActionCode actionCode)
		{
			return actionCode == ActionCode.CancelPermitToTransfer || actionCode == ActionCode.PermitToTransfer;
		}

		public static bool IsVesselEvent(ActionCode actionCode)
		{
			return actionCode == ActionCode.ChangeEstDateOfArrival ||
				actionCode == ActionCode.VesselArrival ||
				actionCode == ActionCode.VesselDeparture;
		}

		public static bool IsVesselArrivalEventRelevent(ActionCode actionCode)
		{
			return actionCode == ActionCode.ChangeEstDateOfArrival ||
				actionCode == ActionCode.VesselArrival;
		}
	}
}
