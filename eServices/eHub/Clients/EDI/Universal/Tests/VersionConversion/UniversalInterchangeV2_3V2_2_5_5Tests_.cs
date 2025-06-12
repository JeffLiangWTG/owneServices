using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
    [TestClass]
    public class UniversalInterchangeV2_3V2_2_5_5Tests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_3V1_SLT()
        {
            MapTester mapTester = GetMapTester();

            var sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT.xml";
            var expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_2_5ToV2_2>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV1_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_3V2_2_5_SLT()
        {
            MapTester mapTester = GetMapTester();

            var sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT.xml";
            var expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);
        }

        //"ID" was not generated as a event parameter but as event reference - very likely a bug CW1 ALP. An investigation task has been created and assigned to ANG.
        //We don't have the luxury to wait for CW1 to fix it so we made the map tolerant for both ID as a parameter and as a reference.
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_3V2_2_5_SLT_IDInReference()
        {
            MapTester mapTester = GetMapTester();

            var sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT_IDInReference.xml";
            var expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT_IDInReference.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT_IDInReference1.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT_IDInReference1.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT_IDInReference2.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT_IDInReference2.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT_IDInReference3.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT_IDInReference3.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT_IDInReference4.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT_IDInReference4.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT_IDInReference5.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT_IDInReference5.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT_IDInReference6.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT_IDInReference6.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_3ToV2_2_5_SLT_NoParameters()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_3_SLT_NoParameters.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_2_5_SLT_NoParameters.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_3ToV2_2_5_UnknownType_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_UnknownType.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_UnknownType.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_3ToV2_2_5_UnknownType_NoParameters_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_UnknownType_NoParameters.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV2_UnknownType_NoParameters.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeEnvelopeV2_3ToV2_2_5()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeEnvelopeV2_3.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeEnvelopeV2_2_5.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeIncludeV2_3ToV2_2_5()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeIncludeV2_3.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeIncludeV2_2_5.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeShipment.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeShipment.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV1_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV1.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2_5V2_3.UniversalInterchangeV1.xml";
            mapTester.Execute<UniversalInterchangeV2_3ToV2_2_5>(sourceFile, expectedFile);
        }

        MapTester GetMapTester()
        {
            return new MapTester(Assembly.GetExecutingAssembly());
        }
    }
}
