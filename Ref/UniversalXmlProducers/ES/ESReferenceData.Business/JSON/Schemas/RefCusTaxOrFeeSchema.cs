using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class RefCusTaxOrFeeSchema : JsonlSchema
{
	public IEnumerable<TaxOrFeeCodes> value { get; set; }

	public class TaxOrFeeCodes
	{
		public string ZZF_Code { get; set; } = string.Empty;
		public double ZZF_Value { get; set; } = -1;
	}
}
