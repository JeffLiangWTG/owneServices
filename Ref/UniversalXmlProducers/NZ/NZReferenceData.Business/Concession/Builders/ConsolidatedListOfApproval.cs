using System.Collections.Generic;
using System.Text.Json;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	internal class ConsolidatedListOfApproval
	{
		public string concessionCode { get; set; }
		public string tariffItem { get; set; }
		public string description { get; set; }
		public string normalTariff { get; set; }
		public string preferentialTariff { get; set; }
		public string part2Ref { get; set; }
		public string effectiveFrom { get; set; }
		public string effectiveTo { get; set; }
		public string scheduleNo { get; set; }
		public string docTitle { get; set; }
		public string docUrl { get; set; }

		public static ConsolidatedListOfApproval GetConsolidatedListOfApprovalFromJson(string json)
		{
			ConsolidatedListOfApproval result = null;
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				result = JsonSerializer.Deserialize<ConsolidatedListOfApproval>(json);
			}
			catch { }
#pragma warning restore CA1031 // Do not catch general exception types
			return result;
		}

		public static Dictionary<string, ConsolidatedListOfApproval> GetConsolidatedListOfApprovalsFromJson(IEnumerable<string> jsonLines, ILogger logger)
		{
			var result = new Dictionary<string, ConsolidatedListOfApproval>();

			foreach (var line in jsonLines)
			{
				var consolidatedListOfApproval = GetConsolidatedListOfApprovalFromJson(line);
				if (consolidatedListOfApproval != null)
				{
					result[consolidatedListOfApproval.concessionCode] = consolidatedListOfApproval;
				}
				else
				{
					logger.LogError($"Cannot deserialize ConsolidatedListOfApproval from line: {line}");
				}
			}

			return result;
		}
	}
}
