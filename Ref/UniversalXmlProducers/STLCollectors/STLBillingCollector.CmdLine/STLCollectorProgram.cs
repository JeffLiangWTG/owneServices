using System;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.STLBillingCollector.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.STLBillingCollector.CmdLine
{
	public static class STLCollectorProgram
	{
		public static void Run() => RunWithOutputPath(AppConfig.Collectors.OutputDirectory);

		static void RunWithOutputPath(string outputPath)
		{
			var outputDirectory = outputPath;
			var error = new STLBillingCollectorHelper(new HttpClientHelper(), new StringBuilder()).ProcessStlBillingCollectors(Constants.ProgramFunctions.SourceNamespace, outputDirectory);
			Console.Error.WriteLine(error);
		}
	}
}
