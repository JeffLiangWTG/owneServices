using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.CmdLine
{
	class CCICodeListsProgram
	{
		public static void Run(string outputFilePath) => Parallel.Invoke(GetFunctionsToRun(new HttpClientHelper(), outputFilePath).ToArray());

		static IEnumerable<Action> GetFunctionsToRun(IHttpClientHelper httpClientHelper, string outputPath)
		{
			return new Action[]
			{
				() => ParseXML(new CCIPreviousDocumentTypeProducer(), ApplicationConfig.Instance.CCIPreviousDocumentTypeUrl),
				() => ParseXML(new CCIMethodOfPaymentProducer(), ApplicationConfig.Instance.CCIMethodOfPaymentUrl),
			};

			void ParseXML(CommonXMLProducer producer, string downloadUrl) => Program.PrintErrorMessage(producer.DownloadAndConvertToXML(httpClientHelper, outputPath, downloadUrl));
		}
	}
}
