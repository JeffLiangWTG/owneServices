using CargoWise.RefDbRepo.AUReferenceData.Business;
using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class CharacteristicCodesParserTest : CMRReferenceDataParserAbstractTest
	{

		protected override string TestFileFolderName => "CharacteristicCodes";

		protected override string TextFileName => "CHRCTRST-P1-EDMAIN-1906040111.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_CharacteristicCodes.xml";

		protected override DateTime PublishedDate => new DateTime(2019, 06, 04, 01, 11, 00);

		protected override ICMRDataParser Parser => new CharacteristicCodesParser();
	}
}
