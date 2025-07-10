using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISCommodityCodesParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISCommodityCodes";

		protected override string TextFileName => "AQSCMDTY-P1-EDMAIN-2203020303.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_AQISCommodityCodes.xml";

		protected override DateTime PublishedDate => new DateTime(2022, 03, 02, 03, 03, 00);

		protected override ICMRDataParser Parser => new AQISCommodityCodesParser();
	}
}
