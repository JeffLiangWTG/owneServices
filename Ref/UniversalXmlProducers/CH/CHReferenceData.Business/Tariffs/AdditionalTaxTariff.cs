using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	class AdditionalTaxTariff
	{
		internal int assessmentCode;
		internal int factor;
		internal decimal rate;
		internal decimal rateMin;
		internal decimal rateMax;
		internal Dictionary<(string, int), AdditionalTaxRelationship> relationships = new Dictionary<(string, int), AdditionalTaxRelationship>();
		internal bool hasExclusions;
	}
}
