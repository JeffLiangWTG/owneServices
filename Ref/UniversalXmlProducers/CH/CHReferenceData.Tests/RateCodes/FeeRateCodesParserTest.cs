using CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.RateCodes
{
	[TestFixture]
	internal class FeeRateCodesParserTest : RateCodesParserBaseTest
	{
		protected override string RateType => "FEE";

		protected override RateCodesParser GetRateCodesParser(DownloadResult masterdataDownload) => new FeeRateCodesParser(masterdataDownload);

		protected override string ExpectedConfiguration => "FeeConfiguration.xml";

		protected override string SampleInput => "FeesSample.xml";
	}
}
