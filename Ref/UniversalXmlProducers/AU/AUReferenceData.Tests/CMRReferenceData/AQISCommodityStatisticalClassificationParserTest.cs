using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISCommodityStatisticalClassificationParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISCommodityStatisticalClassification";

		protected override string TextFileName => "AQSCMSTC-P1-EDMAIN-2212010212.txt";

		protected override string XMLFileName => "RefCusTariff_AU_AQISCommodityStatisticalClassification.xml";

		protected override DateTime PublishedDate => new DateTime(2022, 12, 01, 02, 12, 00);

		protected override ICMRDataParser Parser => new AQISCommodityStatisticalClassificationParser();
	}
}
