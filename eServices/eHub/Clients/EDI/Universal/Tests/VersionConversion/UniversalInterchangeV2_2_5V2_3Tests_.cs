using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
    [TestClass]
    public class UniversalInterchangeV2_2_5V2_3Tests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV1V2_3_SLT()
        {
            MapTester mapTester = GetMapTester();

            var sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV1_SLT.xml";
            var expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_2_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_2_5V2_3_SLT()
        {
            MapTester mapTester = GetMapTester();

            var sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            var expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_2_5ToV2_3_SLT_NoParameters()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT_NoParameters.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT_NoParameters.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_2_5V2_3_ESC()
        {
            MapTester mapTester = GetMapTester();

            var sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_ESC.xml";
            var expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_2_5ToV2_3_UnknownType_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_UnknownType.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_UnknownType.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_2_5ToV2_3_UnknownType_NoParameters_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_UnknownType_NoParameters.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_UnknownType_NoParameters.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeEnvelopeV2_2_5ToV2_3()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeEnvelopeV2_2_5.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeEnvelopeV2_3.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeIncludeV2_2_5ToV2_3()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeIncludeV2_2_5.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeIncludeV2_3.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeShipment.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeShipment.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV1_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV1.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV1.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_3>(sourceFile, expectedFile);
        }

        MapTester GetMapTester()
        {
            return new MapTester(Assembly.GetExecutingAssembly());
        }
    }
}
