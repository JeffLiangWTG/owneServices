using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class CMRSeaImpendingArrivalsPartialParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "CMRSeaImpendingArrivals";

		protected override string TextFileName => "SEAIMPAR-P1-EDCHNG-2502120217.txt";

		protected override string XMLFileName => "RefVesselZZ_AU_CMRSeaImpendingArrivals_Partial.xml";

		protected override DateTime PublishedDate => new DateTime(2025, 02, 12, 02, 17, 00);

		protected override ICMRDataParser Parser => new CMRSeaImpendingArrivalsPartialParser();
	}
}
