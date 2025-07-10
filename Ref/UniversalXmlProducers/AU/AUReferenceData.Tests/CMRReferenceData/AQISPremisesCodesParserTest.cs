using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISPremisesCodesParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISPremisesCodes";

		protected override string TextFileName => "AQSPREMS-P1-EDMAIN-2303240139.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_AQISPremisesCodes.xml";

		protected override DateTime PublishedDate => new DateTime(2023, 03, 24, 01, 39, 00);

		protected override ICMRDataParser Parser => new AQISPremisesCodesParser();
	}
}
