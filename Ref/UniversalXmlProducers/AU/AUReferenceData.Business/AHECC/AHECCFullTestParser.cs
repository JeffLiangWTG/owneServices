using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class AHECCFullTestParser : BaseAHECCParser
	{
		public AHECCFullTestParser(IDateTimeProvider dateProvider) : base(dateProvider)
		{
		}

		protected override UpdateType UpdateType => UpdateType.Full;

		protected override string DataSource => "AU Customs AHECC Test";

		protected override string OutputFileName => "AU Export Tariff Test (Full).xml";

		protected override string DownloadDirectory => ApplicationConfig.AUReferenceTestFilesDirectory;

		protected override string DownloadFileNamePrefix => ApplicationConfig.AHECCTestFilePrefix;

		protected override string DataGrouping => Constants.DataGroupingTest;
	}
}
