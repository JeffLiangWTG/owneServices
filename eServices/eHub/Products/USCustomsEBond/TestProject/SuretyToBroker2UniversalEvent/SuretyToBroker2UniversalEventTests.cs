using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Products.USCustoms.eBond.Transforms.SuretyToBroker2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.USCustoms.Tests
{
    [TestClass]
	public class SuretyToBroker2UniversalEventTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSuretyToBroker2UniversalEvent()
        {
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-07-04T05:43:00");
            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"))
				.Return("USCustomsEBond");
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "USCustomsEBond", "@value", "TransactionIDTransac", "@ST_ID", "USCEB")).Return("DSTCLIENT").Repeat.Once();
            mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "DSTCLIENT"));
            var extensionObjects = new Dictionary<string, object>() { 
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
            };
			
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			string sourceFile = "SuretyToBroker2UniversalEvent.Input.xml";
			string expectedFile = "SuretyToBroker2UniversalEvent.Output.xml";
            mapTester.Execute<SuretyToBroker_to_UniversalEvent>(sourceFile, expectedFile);

			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
            mockCodeMapper.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestSuretyToBroker2UniversalEvent_Legacy()
        {
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-07-04T05:43:00");
            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"))
                .Return("USCustomsEBond");
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "USCustomsEBond", "@value", "TransactionIDTransac", "@ST_ID", "USCEB")).Return("").Repeat.Once();
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "USCustomsEBond", "@value", "BrokerReferenceNumberBrok", "@ST_ID", "USCEB")).Return("DSTCLIENT").Repeat.Once();
            mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "DSTCLIENT"));
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "USCustomsEBond", "@recipientId", "DSTCLIENT", "@ST_ID", "USCEB", "@value", "BrokerReferenceNumberBrok")).Return("B00001072").Repeat.Once();
            var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
            };

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            string sourceFile = "SuretyToBroker2UniversalEvent.Input_Legacy.xml";
            string expectedFile = "SuretyToBroker2UniversalEvent.Output_Legacy.xml";
            mapTester.Execute<SuretyToBroker_to_UniversalEvent>(sourceFile, expectedFile);

            mockDateMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
            mockCodeMapper.VerifyAllExpectations();
        }
    }
}
