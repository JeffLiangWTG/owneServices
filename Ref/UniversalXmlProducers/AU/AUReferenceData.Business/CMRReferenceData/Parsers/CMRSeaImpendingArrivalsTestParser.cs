using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CMRSeaImpendingArrivalsTestParser : CMRSeaImpendingArrivalsParser
	{
		protected override string FileNamePrefix => ApplicationConfig.CMRSeaImpendingArrivalsTestFilePrefix;
		protected override string IndexUri => ApplicationConfig.AUReferenceTestFilesDirectory;
		protected override string OutputXMLName => "RefVesselZZ_AU_CMRSeaImpendingArrivalsTest.xml";
		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate) => CMRXMLWriterConfigurationBuilder.BuildCMRSeaImpendingArrivalsConfiguration(Constants.DataGroupingTest);
	}
}
