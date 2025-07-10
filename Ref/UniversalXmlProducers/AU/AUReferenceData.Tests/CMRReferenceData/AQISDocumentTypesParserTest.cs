using CargoWise.RefDbRepo.AUReferenceData.Business;
using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISDocumentTypesParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISDocumentTypes";

		protected override string TextFileName => "AQSDCMNT-P1-EDMAIN-2209300150.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_AQISDocumentTypes.xml";

		protected override DateTime PublishedDate => new DateTime(2022, 09, 30, 01, 50, 00);

		protected override ICMRDataParser Parser => new AQISDocumentTypesParser();
	}
}
