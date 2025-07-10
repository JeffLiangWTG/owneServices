using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class AHECCFullParser : BaseAHECCParser
	{
		public AHECCFullParser(IDateTimeProvider dateProvider) : base(dateProvider)
		{
		}

		protected override UpdateType UpdateType => UpdateType.Full;

		protected override string DataSource => "AU Customs AHECC";

		protected override string OutputFileName => "AU Export Tariff (Full).xml";

		protected override string DownloadDirectory => ApplicationConfig.AUReferenceFilesDirectory;

		protected override string DownloadFileNamePrefix => ApplicationConfig.AHECCFilePrefix;

		protected override string DataGrouping => Constants.DataGrouping;
	}
}
