using TinyCsvParser.Mapping;

namespace CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel
{
	class CsvFlagCodeMapping : CsvMapping<FlagCodeMap>
	{
		public CsvFlagCodeMapping()
		{
			MapProperty(0, x => x.FlagCode);
			MapProperty(2, x => x.IsoTwoLetterCode);
		}
	}
}
