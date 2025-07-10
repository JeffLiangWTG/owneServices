using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISProcessingTypeParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISProcessingType";

		protected override string TextFileName => "AQSPROCS-P1-EDMAIN-2205140152.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_AQISProcessingType.xml";

		protected override DateTime PublishedDate => new DateTime(2022, 05, 14, 01, 52, 00);

		protected override ICMRDataParser Parser => new AQISProcessingTypeParser();
	}
}
