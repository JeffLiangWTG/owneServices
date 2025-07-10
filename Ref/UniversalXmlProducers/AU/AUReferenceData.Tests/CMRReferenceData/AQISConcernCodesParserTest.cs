using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISConcernCodesParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISConcernCodes";

		protected override string TextFileName => "AQSCNCRN-P1-EDMAIN-2207260135.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_AQISConcernCodes.xml";

		protected override DateTime PublishedDate => new DateTime(2022, 07, 26, 01, 35, 00);

		protected override ICMRDataParser Parser => new AQISConcernCodesParser();
	}
}
