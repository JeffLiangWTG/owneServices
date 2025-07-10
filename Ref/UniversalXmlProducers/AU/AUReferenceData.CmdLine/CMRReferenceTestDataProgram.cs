using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.CmdLine
{
	class CMRReferenceTestDataProgram
	{
		public static void Run(string[] args)
		{
			var clientHelper = new HttpClientHelper();

			foreach (var parser in CMRParserProvider.TestParsers)
			{
				var (content, _) = CMRReferenceFileDownloader.Download(clientHelper, parser.FileNamePrefix, ApplicationConfig.AUReferenceTestFilesDirectory);
				parser.Parse(content, args, ApplicationConfig.OutputDirectory);
			}

			Console.WriteLine($"End of {nameof(CMRReferenceDataProgram)}.");
		}
	}
}
