using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class IPGADataChangeTrackerExtensions
	{
		public static void LoadAllPGARelatedDataIfNeeded(this IPGADataChangeTrackerSupporter supporter)
		{
			if (!supporter.IsUnCommittedRow)
			{
				supporter.Tracker?.LoadAllPGARelatedDataIfNeeded();
			}
		}

		public static ZString LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(this IPGADataChangeTrackerSupporter supporter, ZString oldValue, ZString newValue,
			IEnumerable<IPGADataCorrection> pgas)
		{
			var result = newValue;
			if (!supporter.IsUnCommittedRow)
			{
				result = supporter.Tracker?.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, newValue, pgas) ?? result;
			}
			return result;
		}
	}
}
