using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Business
{
	public static class ACEResponseDataReferenceBlocksExtensionMethods
	{
		public static int GetEntryLineNumber(this Dictionary<string, AENSE0> dataReferenceBlocks)
		{
			int result = 0;

			AENSE0 e0;
			if (dataReferenceBlocks.TryGetValue(EntrySummaryReferenceDataList.Codes.LINITM, out e0))
			{
				result = e0.OccurrencePosition;
			}

			return result;
		}

		public static string GetTariffNumber(this Dictionary<string, AENSE0> dataReferenceBlocks)
		{
			string result = "";

			AENSE0 e0;
			if (dataReferenceBlocks.TryGetValue(EntrySummaryReferenceDataList.Codes.TARIFF, out e0))
			{
				result = e0.ReferenceDataText;
			}
			return result;
		}

		public static string GetPGAAgencyCode(this Dictionary<string, AENSE0> dataReferenceBlocks)
		{
			string result = "";
			AENSE0 e0;
			if (dataReferenceBlocks.TryGetValue(EntrySummaryReferenceDataList.Codes.PG01, out e0))
			{
				result = e0.ReferenceDataText.SubstringSafe(3, 3);
			}
			return result;
		}

		public static string GetPGALineNo(this Dictionary<string, AENSE0> dataReferenceBlocks)
		{
			string result = "";
			AENSE0 e0;
			if (dataReferenceBlocks.TryGetValue(EntrySummaryReferenceDataList.Codes.PG01, out e0))
			{
				result = e0.ReferenceDataText.SubstringSafe(0, 3);
			}
			return result;
		}
	}
}
