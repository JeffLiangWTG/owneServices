using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
    [TestClass]
    public class UniversalInterchangeV2_2ToV2_2_5Tests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV1ToV2_2_5_SSO()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV1_SSO.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_2_SSO.xml";
            mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_2_SSO.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_2_SSO.xml";
            mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_2_SSO.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_2_SSO.xml";
            mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_2_SSO.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_2_SSO.xml";
            mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);

            sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_2_SSO.xml";
            expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_2_5_SSO.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_2ToV2_2_5_SSO_NoParameters_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_SSO_NoParameters.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_SSO_NoParameters.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_2ToV2_2_5_UnknownType_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_UnknownType.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_UnknownType.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2_2ToV2_2_5_UnknownType_NoParameters_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_UnknownType_NoParameters.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV2_UnknownType_NoParameters.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeEnvelopeV2_2ToV2_2_5()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeEnvelopeV2_2.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeEnvelopeV2_2_5.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeIncludeV2_2ToV2_2_5()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeIncludeV2_2.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeIncludeV2_2_5.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeShipment.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeShipment.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_2_5>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV1_ShouldCopyAsIs()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV1.xml";
            string expectedFile = "VersionConversion.TestFiles.V2_2V2_2_5.UniversalInterchangeV1.xml";
            mapTester.Execute<UniversalInterchangeV2_2ToV2_2_5>(sourceFile, expectedFile);
        }

        MapTester GetMapTester()
        {
            return new MapTester(Assembly.GetExecutingAssembly());
        }
    }
}
