using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	[TestedType(typeof(CusSCAOceanBillDataEventParentFinder))]
	sealed class CusSCAOceanBillDataEventParentFinderBaseOnlyTest : CusSCAOceanBillDataEventParentFinderAbstractTest<CusSCAOceanBillDataEventParentFinder, TestCusSCAOceanBill>
	{
		protected override CusSCAOceanBillDataEventParentFinder GetNewParentFinder(BusinessObjectFactory factory, CusSCAOceanBillDataContextManager manager, IXmlImportLogger logger)
			=> new CusSCAOceanBillDataEventParentFinder(factory, manager, logger);

		protected override string OceanBillEventXML => TestFileHelper.GetFileContents("OceanBillEvent");

		protected override ZString CorrectApplicationCode => ZString.Empty;
	}
}
