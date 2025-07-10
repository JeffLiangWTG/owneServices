using System;
using CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.ExportTariffs.MasterData
{
	public partial class tariffMasterDataType : IMasterDataRateType
	{
		public string Value => value;
		public string MeaningDe => meaningDe;
		public string MeaningFr => meaningFr;
		public string MeaningIt => meaningIt;
		public string MeaningEn => meaningEn;
		public DateTime ValidFrom => validFrom;
		public DateTime ValidTo => validTo;
	}

	public partial class tariffMasterDataAdditionalTaxesType : IMasterDataRateType
	{
		public string Value => value;
		public string MeaningDe => meaningDe;
		public string MeaningFr => meaningFr;
		public string MeaningIt => meaningIt;
		public string MeaningEn => meaningEn;
		public DateTime ValidFrom => DateTime.MinValue;
		public DateTime ValidTo => DateTime.MaxValue;
	}
}
