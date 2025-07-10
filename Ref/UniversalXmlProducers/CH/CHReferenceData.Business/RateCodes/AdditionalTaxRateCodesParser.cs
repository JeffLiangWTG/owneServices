using CargoWise.RefDbRepo.CHReferenceData.Business.ExportTariffs.MasterData;
using CargoWise.RefDbRepo.CHReferenceData.Services;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes
{
	public class AdditionalTaxRateCodesParser : RateCodesParser
	{
		public AdditionalTaxRateCodesParser(DownloadResult masterdataDownload) : base(masterdataDownload)
		{
		}

		protected override string RateType => "ADT";

		protected override string RateTypeDescription => "Additional Taxes";

		protected override IMasterDataRateType[] GetRateTypes(tariffMasterData masterData) => masterData.additionalTaxes.type;
	}
}
