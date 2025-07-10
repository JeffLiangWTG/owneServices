using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.T4Runner
{
	public static class TTFileRunner
	{
		public static void Run(ITTFileConfig[] ttFileConfigs)
		{
			Argument.NotNull(ttFileConfigs, nameof(ttFileConfigs));
			foreach (var ttObj in ttFileConfigs)
			{
				Console.WriteLine($"Running file {ttObj.FileName}");
				ttObj.CallProcessor(ttObj.NamespaceName, ttObj.FileName);
			}
		}
	}
}
