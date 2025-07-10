using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.CmdLine
{
	public class LuxuryCarTaxProgram
	{
		public void Run(string outputPath)
		{
			var url = Services.ApplicationConfig.LCTThresholdsUrl;
			Console.WriteLine($"Retrieving LCT details from {url}");

			var htmlContent = HttpClient.GetWebPageAsync(url).Result;
			if (htmlContent != null)
			{
				var entities = LCTParser.Parse(htmlContent, out var lastModified);
				if (entities.Any())
				{
					if (!DateTime.TryParse(lastModified, out var publicationDate))
					{
						publicationDate = DateTime.Today;
						Console.WriteLine($"Could not convert Last Modified '{lastModified}' to Date.  Using Today.");
					}

					var outputFileFullPath = Path.Combine(outputPath, OutputFileName);
					WriteToDestination(entities, outputFileFullPath, publicationDate);

					Console.WriteLine($"Retrieving LCT details completed");
				}
				else
				{
					Console.WriteLine($"The Page at {url} did not contain the expected data.");
				}
			}
			else
			{
				Console.WriteLine($"Loading from {url} Failed.");
			}
		}

		static void WriteToDestination(IEnumerable<RefCusTaxOrFee> entities, string outputFileFullPath, DateTime publicationDate)
		{
			Helper.ExportToXMLFile(XMLWriterDataSource, outputFileFullPath, Helper.GetRefCusTaxOrFeeWriterConfiguration(), publicationDate, Common.UniversalXmlWriter.UpdateType.Partial, entities);
		}

		protected virtual IHttpClientHelper HttpClient => new HttpClientHelper();

		const string OutputFileName = "AU Customs LCT Thresholds.xml";
		const string XMLWriterDataSource = "AU Customs LCT Rates";
	}
}
