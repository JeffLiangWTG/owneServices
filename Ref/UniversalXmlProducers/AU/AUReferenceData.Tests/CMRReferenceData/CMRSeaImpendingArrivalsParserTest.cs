using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class CMRSeaImpendingArrivalsParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "CMRSeaImpendingArrivals";

		protected override string TextFileName => "SEAIMPAR-P1-EDMAIN-2502120217.txt";

		protected override string XMLFileName => "RefVesselZZ_AU_CMRSeaImpendingArrivals.xml";

		protected override DateTime PublishedDate => new DateTime(2025, 02, 12, 02, 17, 00);

		protected override ICMRDataParser Parser => new CMRSeaImpendingArrivalsParser();
	}
}
