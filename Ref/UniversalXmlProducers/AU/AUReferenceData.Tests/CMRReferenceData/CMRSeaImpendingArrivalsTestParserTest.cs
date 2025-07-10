using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class CMRSeaImpendingArrivalsTestParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "CMRSeaImpendingArrivals";

		protected override string TextFileName => "SEAIMPAR-Q1-EDMAIN-2502050027.txt";

		protected override string XMLFileName => "RefVesselZZ_AU_CMRSeaImpendingArrivalsTest.xml";

		protected override DateTime PublishedDate => new DateTime(2025, 02, 05, 00, 27, 00);

		protected override ICMRDataParser Parser => new CMRSeaImpendingArrivalsTestParser();
	}
}
