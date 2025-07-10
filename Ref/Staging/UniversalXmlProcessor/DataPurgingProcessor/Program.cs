using System;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	class Program
	{
		static int Main(string[] args)
		{
			Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);
			using (var stagingRepo = new StagingRepository(Application.RefDbRepoStaging) { CommandTimeout = 600 })
			{
				return (int)Run(stagingRepo);
			}
		}

		static ProducerStatus Run(IStagingRepository stagingRepo)
		{
			if (stagingRepo != null)
			{
				new DataPurgerForPRSStatus(stagingRepo).Purge();
				new DataPurgerForAllStatuses(stagingRepo).Purge();
				if (Application.ShouldPurgeOrphanData)
				{
					new DataPurgerForOrphanData(stagingRepo).Purge();
				}
			}
			return ProducerStatus.Success;
		}
	}
}
