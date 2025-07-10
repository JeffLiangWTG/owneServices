using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IDailyTariffUpdatesFileProvider
	{
		ICollection<IRawNomenclatureDailyRecord> NomenclatureDailyRawRecordCollection { get; }

		ICollection<IRawRateDailyRecord> RateDailyRawRecordCollection { get; }

		DateTime? LastDailyPublishTime { get; }

		void CleanAll();

		void DownloadAndExtractDailyUpdates(DateTime referenceDate, DateTime monthlyPublishDate);
	}
}
