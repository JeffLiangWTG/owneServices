using System;
using System.IO;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public static class SchedulerConstants
	{
		public static string[] UXMLProducerConfigPaths => new[]
		{
			Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UniversalXMLProducers\net8.0"))
		};
	}
}
