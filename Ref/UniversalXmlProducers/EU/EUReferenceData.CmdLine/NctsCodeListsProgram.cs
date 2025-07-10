using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.CmdLine
{
	class NctsCodeListsProgram
	{
		public static void Run(string outputFilePath) => Parallel.Invoke(GetFunctionsToRun(new HttpClientHelper(), outputFilePath).ToArray());

		static IEnumerable<Action> GetFunctionsToRun(IHttpClientHelper httpClientHelper, string outputPath)
		{
			return new Action[]
			{
				() => ParseXML(new NctsAdditionalInformationProducer()),
				() => ParseXML(new NctsAdditionalReferenceProducer()),
				() => ParseXML(new NctsDeclarationTypeProducer()),
				() => ParseXML(new NctsNationalityProducer()),
				() => ParseXML(new NctsPreviousDocumentProducer()),
				() => ParseXML(new ManifestPreviousDocumentTypeProducer()),
				() => ParseXML(new NctsSupportingDocumentProducer()),
				() => ParseXML(new NctsTransportDocumentProducer()),
				() => ParseXML(new NctsCountryCodesCommunityProducer()),
				() => ParseXML(new NctsGuaranteeTypeCTCProducer()),
				() => ParseXML(new NctsCountryCodesCTCProducer()),
				() => ParseXML(new NctsGuaranteeTypeEUNonTIRProducer()),
				() => ParseXML(new NctsPreviousDocumentUnionGoodsProducer()),
				() => ParseXML(new NctsReleaseTypeProducer()),
				() => ParseXML(new NCTSReleaseNotificationProducer()),
				() => ParseXML(new NCTSInvalidGuaranteeReasonProducer()),
				() => ParseXML(new NctsGuaranteeTypeProducer()),
				() => ParseXML(new NctsBusinessRejectionTypeDepExpProducer()),
				() => ParseXML(new NctsBusinessRejectionTypeDesExtProducer()),
				() => ParseXML(new NctsFunctionalErrorCodesIeCAProducer()),
				() => ParseXML(new NctsRejectionCodeDestinationExitProducer()),
				() => ParseXML(new NctsNoReleaseMotivationProducer()),
				() => ParseXML(new NctsRejectionCodeDepartureExportProducer()),
				() => ParseXML(new NctsXmlErrorCodesProducer()),
				() => ParseXML(new NctsIncidentCodeProducer()),
				() => ParseXML(new NctsDocumentTypeExciseProducer()),
				() => ParseXML(new NctsCountryOutsideCustomsSecurityAgreementAreaProducer()),
				() => ParseXML(new NctsCountryCodesCommonTransitProducer()),
				() => ParseXML(new NctsCountryCodesFullListProducer()),
				() => ParseXML(new NctsQueryIdentifierProducer()),
				() => ParseXML(new NctsRoleOfRequesterProducer()),
			};

			void ParseXML(NctsManyToOneCodeListXMLProducer producer) => Program.PrintErrorMessage(producer.DownloadAndConvertAllRefCusCodeListXMLs(httpClientHelper, outputPath).Result);
		}
	}
}
