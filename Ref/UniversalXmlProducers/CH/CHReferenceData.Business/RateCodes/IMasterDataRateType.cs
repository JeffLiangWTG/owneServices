using System;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes
{
	public interface IMasterDataRateType
	{
		string Value { get; }
		string MeaningDe { get; }
		string MeaningFr { get; }
		string MeaningIt { get; }
		string MeaningEn { get; }
		DateTime ValidFrom { get; }
		DateTime ValidTo { get; }
	}
}
