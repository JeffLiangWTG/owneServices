using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class BerthCodesParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "BerthCodes";

		protected override string TextFileName => "BERTH-P1-EEMAIN-2209070120.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_BerthCodes.xml";

		protected override DateTime PublishedDate => new DateTime(2022, 09, 07, 01, 20, 00);

		protected override ICMRDataParser Parser => new BerthCodesParser();
	}
}
