using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRX.Transforms.FreightForceX12_214_2_UniEvent;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.TRX.Tests
{
	[TestClass]
    public class FreightForceX12_214_2_UniEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFreightForceX12_214_2_UniEvent()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_F01", "TRXELPELP", "Freight Force 214 - Receive Shipment Status", "Defaults", "Data Provider")).Return("FreightForce");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            string sourceFile = "FreightForceX12_214_2_UniEvent.TestFiles.FreightForceX12_214_2_UniEvent_Input.xml";
            string expectedFile = "FreightForceX12_214_2_UniEvent.TestFiles.FreightForceX12_214_2_UniEvent_Output.xml";
            mapTester.ExecuteCompiled<FreightForceX12_214_2_UniEvent>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFreightForceX12_214_2_UniEvent_WithLadingExceptionCode()
        {
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_F01", "TRXELPELP", "Freight Force 214 - Receive Shipment Status", "Defaults", "Data Provider")).Return("FreightForce");

            var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            string sourceFile = "FreightForceX12_214_2_UniEvent.TestFiles.FreightForceX12_214_2_UniEvent_InputWithLadingExceptionCode.xml";
            string expectedFile = "FreightForceX12_214_2_UniEvent.TestFiles.FreightForceX12_214_2_UniEvent_OutputWithLadingExceptionCode.xml";
            mapTester.ExecuteCompiled<FreightForceX12_214_2_UniEvent>(sourceFile, expectedFile);

            mockCodeMapper.VerifyAllExpectations();
        }
	}
}
