using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class EstablishmentCodesParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "EstablishmentCodes";

		protected override string TextFileName => "ESTABMNT-P1-EEMAIN-2304150135.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_EstablishmentCodes.xml";

		protected override DateTime PublishedDate => new DateTime(2023, 04, 15, 01, 35, 00);

		protected override ICMRDataParser Parser => new EstablishmentCodesParser();
	}
}
