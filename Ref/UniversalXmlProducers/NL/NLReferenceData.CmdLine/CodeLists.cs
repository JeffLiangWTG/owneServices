using System;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Business;
using CargoWise.RefDbRepo.NLReferenceData.Services;

namespace CargoWise.RefDbRepo.NLReferenceData.CmdLine
{
	class CodeLists
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();

			var processManagerAdditionalInformation = new AdditionalInformationWebClientProcessManager(new AdditionalInformationBuilder(errorCollector));
			processManagerAdditionalInformation.RunProcess(ApplicationConfig.OutputPath, errorCollector);

			var processManagerAdditionalSupplements = new AdditionalSupplementsWebDriverHelperProcessManager(new AdditionalSupplementsBuilder(errorCollector));
			processManagerAdditionalSupplements.RunProcess(ApplicationConfig.OutputPath, errorCollector);

			var processManagerItineraryCountries = new ItineraryCountriesProcessManager(new ItineraryCountriesBuilder(errorCollector));
			processManagerItineraryCountries.RunProcess(ApplicationConfig.OutputPath, errorCollector);

			var processManagerSupportingDocument = new SupportingDocumentTypesWebClientProcessManager(new SupportingDocumentBuilder(errorCollector));
			processManagerSupportingDocument.RunProcess(ApplicationConfig.OutputPath, errorCollector);

			var processManagerTransportDocument = new TransportDocumentsWebClientProcessManager(new TransportDocumentBuilder(errorCollector));
			processManagerTransportDocument.RunProcess(ApplicationConfig.OutputPath, errorCollector);

			var processManagerPreviousDocument = new PreviousDocumentTypesWebClientProcessManager(new PreviousDocumentBuilder(errorCollector));
			processManagerPreviousDocument.RunProcess(ApplicationConfig.OutputPath, errorCollector);

			var processManagerAdditionalReference = new AdditionalReferenceWebClientProcessManager(new AdditionalReferenceBuilder(errorCollector));
			processManagerAdditionalReference.RunProcess(ApplicationConfig.OutputPath, errorCollector);

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred: \r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
