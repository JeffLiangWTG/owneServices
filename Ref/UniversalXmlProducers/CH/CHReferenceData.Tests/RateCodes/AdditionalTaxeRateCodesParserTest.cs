using CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.RateCodes
{
	[TestFixture]
	internal class AdditionalTaxRateCodesParserTest : RateCodesParserBaseTest
	{
		protected override string RateType => "ADT";

		protected override RateCodesParser GetRateCodesParser(DownloadResult masterdataDownload) => new AdditionalTaxRateCodesParser(masterdataDownload);

		protected override string ExpectedConfiguration => "AdtConfiguration.xml";

		protected override string SampleInput => "AdditionalTaxesSample.xml";
	}
}
