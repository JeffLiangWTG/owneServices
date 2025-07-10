using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Procedure
{
	internal static class ProcedureCodeProcessor
	{
		internal static List<ProcedureCodeData> PopulateProcedureCodeData(IReadOnlyCollection<ProcedureCodeRaw> procedureCodes, IReadOnlyCollection<AdditionalProcedureCodeRaw> additionalProcedureCodes, IReadOnlyCollection<AdditionalProcedureMapping> additionalMappings)
		{
			var result = new List<ProcedureCodeData>();

			foreach (var pc in procedureCodes)
			{
				foreach (var acMap in additionalMappings.First(x => x.ProcedureCode == pc.Code).AdditionalProcedureCodes)
				{
					var ac = additionalProcedureCodes.First(x => x.Code == acMap);

					result.Add(new ProcedureCodeData
					{
						ProcedureCode = pc.Code,
						Description = $"{pc.Description} - {ac.Description}",
						AdditionalProcedureCode = ac.Code,
						ShipmentType = pc.ShipmentType,
					});
				}
			}

			return result;
		}

	}
}
