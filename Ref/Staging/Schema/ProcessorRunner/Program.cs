using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner
{
	class Program
	{
		static void Main(string[] args)
		{
			Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);
			if (args.Length < 4)
			{
				throw new ArgumentException("Usage: ProcessorRunner <Processor Name> <Processor CountryCode> <Processor Exe> <Job Name> [<Parameters passed to Processor>]");
			}
			var schedName = "RefDbRepoQuartzServer";
			var programName = args[0];
			var groupName = args[1];
			var programExe = args[2];
			var jobName = args[3];
			var programArgs = string.Join(" ", args.Skip(4));
			using (var logWrapper = new LogWrapper(programName))
			{
				var logHelper = new LogHelper(logWrapper, programName);
				ProcessorRunnerHelper.RunAppAsProcessor(new ProcessorInfo(programName, groupName, schedName), logHelper, jobName,
					programExe, programArgs, new [] {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UniversalXMLProducers") });
			}
		}
	}
}
