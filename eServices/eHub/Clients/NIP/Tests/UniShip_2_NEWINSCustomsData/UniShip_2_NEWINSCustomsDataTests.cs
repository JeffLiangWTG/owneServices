using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Clients.NIP.Transforms.UniShip_2_NEWINSCustomsData;

namespace CargoWise.eHub.Clients.NIP.Tests
{
    [TestClass]
    public class UniShip_2_NEWINSCustomsDataTest
    {
        const string filePath = "UniShip_2_NEWINSCustomsData.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniShip_2_NEWINS_tester()
        {
            AssertMapping("CoLoad_Input.xml", "CoLoad_Output.xml");
            AssertMapping("Sample_Shipment.xml", "Sample_Shipment_Output.xml");
            AssertMapping("Sample_Consol.xml", "Sample_Consol_Output.xml");
            AssertMapping("Sample_JobDec.xml", "Sample_JobDec_Output.xml");
            AssertMapping("SeaCargo-S700051199.xml", "SeaCargo-S700051199_Output.xml");
            AssertMapping("Shipment_Export_ECC.xml", "Shipment_Export_ECC_Output.xml");
            AssertMapping("Shipment_Export_ECM.xml", "Shipment_Export_ECM_Output.xml");
            AssertMapping("Shipment_Import_STC.xml", "Shipment_Import_STC_Output.xml");
            AssertMapping("Shipment_Import_CCC.xml", "Shipment_Import_CCC_Output.xml");
            AssertMapping("Shipment_Import_CLR.xml", "Shipment_Import_CLR_Output.xml");
        }

        void AssertMapping(string inputFile, string expectedOutputFile)
        {
            var input = filePath + inputFile;
            var expectedOutput = filePath + expectedOutputFile;

            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockCodeMapper.Expect(x => x.GetRecipientCode("NIPSYDSYD", "NIPSYDSYD_NEW", "NEWINS txt - Send Customs Declaration Data", "Transport Mode", "SEA")).Return("S").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("NIPSYDSYD", "NIPSYDSYD_NEW", "NEWINS txt - Send Customs Declaration Data", "Transport Mode", "AIR")).Return("A").Repeat.Any();

            var extensionObjects = new Dictionary<string, object>() { 
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", new DateMapper() },
            };

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<UniShip_2_NEWINSCustomsData>(input, expectedOutput);
        }
    }
}