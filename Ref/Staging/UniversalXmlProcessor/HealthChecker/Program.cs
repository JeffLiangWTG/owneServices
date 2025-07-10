using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.Staging.HealthChecker
{
	class Program
	{
		static int Main(string[] args)
		{
			Argument.NotNull(args, nameof(args));
			Argument.GreaterThanZero(args.Length, nameof(args));

			switch (args[0].ToUpperInvariant())
			{
				case ApplicationConfig.SOURCEDATA:
					return CheckSourceData();
				default:
					throw new NotImplementedException($"Invalid argument: {args[0]}");
			}
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to catch all exceptions and write logs")]
		static int CheckSourceData()
		{
			Console.WriteLine("Start to check SourceData status.");
			try
			{
				using (var stagingRepository = new StagingRepository(ApplicationConfig.StagingConnectionString))
				{
					var sourceDataChecker = new SourceDataChecker(stagingRepository);
					sourceDataChecker.CheckLongRunningSourceData();
					sourceDataChecker.CheckSourceDataStatus(ApplicationConfig.CheckPeriodInHours, ApplicationConfig.SourceDataFailedRatio);
				}
				return (int)ProducerStatus.Success;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				return (int)ProducerStatus.Failure;
			}
		}
	}
}
