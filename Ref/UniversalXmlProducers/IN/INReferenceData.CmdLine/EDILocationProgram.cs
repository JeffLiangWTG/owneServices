using System;
using System.IO;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.INReferenceData.CmdLine
{
	public static class EDILocationProgram
	{
		public static void Run()
		{
			var sourceUrl = AppConfig.EDILocation.Url;
			var outputPath = AppConfig.Shared.OutputDirectory;

			var filename = "RefEDILocationZZ_IN.xml";
			var outputFile = Path.Combine(outputPath, filename);

			var errors = new EDILocationXMLProducer(new HttpClientHelper()).GenerateEDILocationXMLFile(sourceUrl, outputFile);

			if (!string.IsNullOrEmpty(errors))
			{
				Console.Error.WriteLine(errors);
			}
		}
	}
}
