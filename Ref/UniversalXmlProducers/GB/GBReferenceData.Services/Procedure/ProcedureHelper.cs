using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Procedure
{
	public static class ProcedureHelper
	{
		public static (IReadOnlyCollection<ProcedureCodeData> Codes, IReadOnlyCollection<CategoryProcedureMapping> Mappings) GetProcedureCodesAndMappings(IWebClientWrapper webClient, ICDSProcedureSource source)
		{
			var codeScraper = new ProcedureCodeScraper(webClient.GetContent(source.ProcedureUrl));
			var categoryProcedureMappings = codeScraper.ExtractCategoryProcedureMapping();
			var rawProcedureCodes = codeScraper.ExtractProcedureDescriptions(source.ShipmentType);

			var additionalScrapper = new AdditionalProcedureCodeScraper(webClient);
			var rawAddtionalProcedureCodes = additionalScrapper.ExtractCodesWithDescription(source.AdditionalProcedureUrl);
			var content = additionalScrapper.GetContent(source.AdditionalProcedureMatrixUrl);
			using (var data = AdditionalProcedureCodeScraper.GetDocumentData(content))
			{
				var mappings = AdditionalProcedureCodeScraper.GetMappings(data);
				var procedureCodes = ProcedureCodeProcessor.PopulateProcedureCodeData(rawProcedureCodes, rawAddtionalProcedureCodes, mappings.ToArray());
				return (procedureCodes, categoryProcedureMappings);
			}
		}
	}
}

