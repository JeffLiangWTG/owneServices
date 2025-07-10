namespace CargoWise.RefDbRepo.NewService.Test
{
	sealed class LoggingReferenceDataRepository : ReferenceDataRepository
	{
		public LoggingReferenceDataRepository(string nameOrConnectionString) : base(nameOrConnectionString)
		{
			EnableStatistics();
		}
	}
}
