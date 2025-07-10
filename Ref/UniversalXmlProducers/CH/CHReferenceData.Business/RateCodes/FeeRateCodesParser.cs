using CargoWise.RefDbRepo.CHReferenceData.Business.ExportTariffs.MasterData;
using CargoWise.RefDbRepo.CHReferenceData.Services;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes
{
	public class FeeRateCodesParser : RateCodesParser
	{
		public FeeRateCodesParser(DownloadResult masterdataDownload) : base(masterdataDownload)
		{
		}

		protected override string RateType => "FEE";

		protected override string RateTypeDescription => "Fee";

		protected override IMasterDataRateType[] GetRateTypes(tariffMasterData masterData) => masterData.fees;
	}
}
