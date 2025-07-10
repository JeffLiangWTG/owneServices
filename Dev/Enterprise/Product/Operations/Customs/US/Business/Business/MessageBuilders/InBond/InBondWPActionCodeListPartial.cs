namespace Enterprise.Customs.US.Business
{
	partial class InBondWPActionCodeList
	{
		public static bool IsArrivalAction(string code)
		{
			return code == Codes.ArriveBillOfLadingAtDestination ||
				code == Codes.ArriveContainerAtDestination ||
				code == Codes.ArriveEntireInBondAtDestination;
		}

		public static bool IsExportationAction(string code)
		{
			return code == Codes.ExportBillOfLadingFromDestinationPort ||
				code == Codes.ExportContainerFromDestinationPort ||
				code == Codes.ExportEntireInBondFromDestinationPort;
		}

		public static bool IsTOLAction(string code)
		{
			return code == Codes.TransferOfInBondLiabilityForBillOfLading ||
				code == Codes.TransferOfInBondLiabilityForContainer ||
				code == Codes.TransferOfInBondLiabilityForEntireInBond;
		}

		public static bool IsFDATransmissionAction(string code)
		{
			return code == Codes.FDADataSubmissionForInBond ||
				code == Codes.FDADataSubmissionForOtherThanInBond;
		}

		public static bool IsDiversionRequest(string code)
		{
			return code == Codes.DiversionRequest;
		}

		public static bool IsBillLevel(string code)
		{
			return code == Codes.ArriveBillOfLadingAtDestination || code == Codes.ArriveContainerAtDestination
				|| code == Codes.ExportBillOfLadingFromDestinationPort || code == Codes.ExportContainerFromDestinationPort;
		}

		public static bool IsContainerLevel(string code)
		{
			return code == Codes.ArriveContainerAtDestination || code == Codes.ExportContainerFromDestinationPort;
		}
	}
}
