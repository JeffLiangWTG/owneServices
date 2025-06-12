using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.YAS.Transforms.ImportChargesAndCost2EDIShipmentInternal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.YAS.Tests
{
	[TestClass]
	public class ImportChargesAndCost2EDIShipmentInternalTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImportChargesAndCost2EDIShipmentInternal()
		{
			string sourceFile = "ImportChargesAndCost2EDIShipmentInternal.TestFiles.SampleFileImportChargesAndCosts.xml";
			string expectedFile = "ImportChargesAndCost2EDIShipmentInternal.TestFiles.ShipmentInternal.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
            mapTester.ExecuteCompiled<ImportChargesAndCost2EDIShipmentInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImportChargesAndCost2EDIShipmentInternal_CurrencyCode()
		{
			string sourceFile = "ImportChargesAndCost2EDIShipmentInternal.TestFiles.SampleFileImportChargesAndCosts_WithCurrencyCode.xml";
			string expectedFile = "ImportChargesAndCost2EDIShipmentInternal.TestFiles.ShipmentInternal_WithCurrencyCode.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<ImportChargesAndCost2EDIShipmentInternal>(sourceFile, expectedFile);
		}
	}		
}
