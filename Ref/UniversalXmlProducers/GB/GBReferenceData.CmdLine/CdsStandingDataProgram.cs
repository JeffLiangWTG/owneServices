using System.Threading.Tasks;
using CargoWise.RefDbRepo.GBReferenceData.Business.CDSStandingData;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.CmdLine
{
	class CdsStandingDataProgram
	{
		public static void Run()
		{
			var webWrapper = new WebClientWrapper();

			Parallel.Invoke(
				() => new SupportingDocumentCodeParser(
							webWrapper.GetContent(ConfigurationProvider.SupportingDocumentCodeNationalUrl),
							webWrapper.GetContent(ConfigurationProvider.SupportingDocumentCodeStatusUrl),
							webWrapper.GetContent(ConfigurationProvider.SupportingDocumentCodeUnionUrl),
							webWrapper
						).ExportXml(ConfigurationProvider.OutputDirectory),
				() => new AdditionalInformationParser(
							webWrapper.GetContent(ConfigurationProvider.AdditionalInformationUrl),
							webWrapper
						).ExportXml(ConfigurationProvider.OutputDirectory),
				() => new ErrorCodeParser(webWrapper,
							webWrapper.GetContent(ConfigurationProvider.CDSErrorCodeUrl)
						).ExportXml(ConfigurationProvider.OutputDirectory)
			);
		}
	}
}
