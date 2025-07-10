using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUtcOffsetUpdater
{
	public class RefUNLOCOUtcOffsetRecord
	{
		public short UtcOffset { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public RefUNLOCOUtcOffset GetRefUNLOCOUtcOffsetObject(string unloco)
		{
			return new RefUNLOCOUtcOffset()
			{
				RLO_RL_NKCode = unloco,
				RLO_OffsetMinutesFromUtc = UtcOffset,
				RLO_StartTimeUtc = StartDate,
				RLO_EndTimeUtc = EndDate
			};
		}
	}
}
