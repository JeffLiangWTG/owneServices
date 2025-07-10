using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	[TestedType(typeof(CusSCAHouseDataEventParentFinder))]
	sealed class CusSCAHouseDataEventParentFinderBaseOnlyTest : CusSCAHouseDataEventParentFinderAbstractTest<CusSCAHouseDataEventParentFinder, CusSCAHouseForTest>
	{
		protected override CusSCAHouseDataEventParentFinder GetNewParentFinder(BusinessObjectFactory factory, CusSCAHouseDataContextManager manager, IXmlImportLogger logger)
			=> new CusSCAHouseDataEventParentFinder(factory, manager, logger);

		protected override string HouseBillEventXML => TestFileHelper.GetFileContents("SCAHouseEvent");
	}
}
