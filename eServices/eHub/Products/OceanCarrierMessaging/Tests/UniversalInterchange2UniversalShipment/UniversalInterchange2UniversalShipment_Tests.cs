using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UInterchange2UShipment.V2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
    public class UniversalInterchange2UniversalShipment_Tests
	{
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchange2UniversalShipment()
        {
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniversalInterchange2UniversalShipment.TestFiles.Test1_UniversalInterchange_VM2.xml";
            string expectedFile = "UniversalInterchange2UniversalShipment.TestFiles.Test1_UniversalShipment_VM2.xml";
            mapTester.ExecuteCompiled<UniversalInterchange2UniversalShipment_V2>(sourceFile, expectedFile);
        }
	}
}
