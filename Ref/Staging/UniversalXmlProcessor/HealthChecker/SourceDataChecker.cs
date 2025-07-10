using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.HealthChecker
{
	public class SourceDataChecker
	{
		readonly IStagingRepository stagingRepo;

		public SourceDataChecker(IStagingRepository stagingRepository)
		{
			Argument.NotNull(stagingRepository, nameof(stagingRepository));
			stagingRepo = stagingRepository;
		}

		public void CheckLongRunningSourceData()
		{
			var lastCreatedTime = DateTime.UtcNow.AddDays(-1);
			var sourceData = stagingRepo.Get<SourceData>().Where(x => x.SDA_Source == "INT" && x.SDA_Filetype == "XML" && x.SDA_Status == "PRS" && x.SDA_CreatedTime < lastCreatedTime);
			if (sourceData.Any())
			{
				Console.Error.WriteLine($"The following xml files did not finish processing after 24 hours. SDA_PK: {string.Join(", ", sourceData.Select(x => x.SDA_PK))}");
			}
			else
			{
				Console.WriteLine("All xml files finished processing within 24 hours.");
			}
		}

		public void CheckSourceDataStatus(int checkPeriodInHours, double failedRatio)
		{
			var lastCreatedTime = DateTime.UtcNow.AddHours(-checkPeriodInHours);
			var sourceData = stagingRepo.Get<SourceData>().Where(x => x.SDA_CreatedTime > lastCreatedTime);
			var mergedCount = sourceData.Where(x => x.SDA_Status == "MER" || x.SDA_Status == "FIN").Count();
			var failedCount = sourceData.Where(x => x.SDA_Status == "ERR" || x.SDA_Status == "FIE").Count();

			var shouldAlert = false;
			if (mergedCount > 0 && failedCount > 0)
			{
				shouldAlert = (double)failedCount / mergedCount >= failedRatio;
			}
			else if (mergedCount == 0 && failedCount > 0)
			{
				shouldAlert = true;
			}
			if (shouldAlert)
			{
				Console.Error.WriteLine($"SourceData failure ratio reached {failedRatio} in last {checkPeriodInHours} hours. {mergedCount} succeeded, {failedCount} failed.");
			}
			else
			{
				Console.WriteLine($"SourceData process results in last {checkPeriodInHours} hours: {mergedCount} succeeded, {failedCount} failed.");
			}
		}
	}
}
