using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISEntityCodesParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISEntityCodes";

		protected override string TextFileName => "AQSENTIT-P1-EDMAIN-2303180142.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_AQISEntityCodes.xml";

		protected override DateTime PublishedDate =>  new DateTime(2023, 03, 18, 01, 42, 00);

		protected override ICMRDataParser Parser => new AQISEntityCodesParser();
	}
}
