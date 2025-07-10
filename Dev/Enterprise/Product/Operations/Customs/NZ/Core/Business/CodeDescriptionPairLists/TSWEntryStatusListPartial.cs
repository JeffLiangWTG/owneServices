using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business
{
	partial class TSWEntryStatusList
	{
		public static class EntryTypes
		{
			public const string WriteOff = "WOF";
		}

		public static bool IsCompletedStatus(string status, string entryType, string bioStatus)
		{
			return status == TSWEntryStatusList.Codes.CCC
				|| status == TSWEntryStatusList.Codes.NCC
				|| status == TSWEntryStatusList.Codes.CC
				|| status == TSWEntryStatusList.Codes.CLR
				|| (entryType == EntryTypes.WriteOff && (status == LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff || status == LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved))
				|| (status == TSWEntryStatusList.Codes.CIC && bioStatus == StatusList.Codes.MPIBiosecurityDirectionsGivenCleared);
		}

		public static bool IsImpedimentStatus(string status)
		{
			return status.Contains("1");
		}
	}
}
