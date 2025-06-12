using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.Products.GlobalInvoice.Common.Transforms.DeliveryNotification2UniEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;


namespace CargoWise.eHub.Products.GlobalInvoice.Common.Tests
{
    [TestClass]
    public class DeliveryNotification2UniEventTests
    {
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DeliveryNotification2UniEventITTest()
        {
            var sourceFile = "DeliveryNotification2UniEvent.TestFiles.Input_1.xml";
            var expectedFile = "DeliveryNotification2UniEvent.TestFiles.Output_1.xml";

            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("XXXYYYZZZ");
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Messaging System", "Message Type", "Italy electronic invoicing system")).Return("IT").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Messaging System", "Message SubType", "Italy electronic invoicing system")).Return("").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Notification Type", "Event Type", "NACK", "Italy electronic invoicing system")).Return("IRJ").Repeat.Any();
            mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2018-11-09T22:37:52");

            var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<DeliveryNotification2UniEvent>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DeliveryNotification2UniEventTWACKTest()
        {
            var sourceFile = "DeliveryNotification2UniEvent.TestFiles.Input_2.xml";
            var expectedFile = "DeliveryNotification2UniEvent.TestFiles.Output_2.xml";

            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("XXXYYYZZZ");
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Messaging System", "Message Type", "Taiwan electronic invoicing system")).Return("TW").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Messaging System", "Message SubType", "Taiwan electronic invoicing system")).Return("").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Notification Type", "Event Type", "ACK", "Taiwan electronic invoicing system")).Return("ISN").Repeat.Any();
            mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2018-11-09T22:37:52");

            var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<DeliveryNotification2UniEvent>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DeliveryNotification2UniEventTWNACKTest()
        {
            var sourceFile = "DeliveryNotification2UniEvent.TestFiles.Input_3.xml";
            var expectedFile = "DeliveryNotification2UniEvent.TestFiles.Output_3.xml";

            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("XXXYYYZZZ");
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Messaging System", "Message Type", "Taiwan electronic invoicing system")).Return("TW").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Messaging System", "Message SubType", "Taiwan electronic invoicing system")).Return("").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Notification Type", "Event Type", "NACK", "Taiwan electronic invoicing system")).Return("IRJ").Repeat.Any();
            mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2018-11-09T22:37:52");

            var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<DeliveryNotification2UniEvent>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DeliveryNotification2UniEventIN_ACKTest()
        {
            var sourceFile = "DeliveryNotification2UniEvent.TestFiles.Input_4.xml";
            var expectedFile = "DeliveryNotification2UniEvent.TestFiles.Output_4.xml";

            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("XXXYYYZZZ");
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Messaging System", "Message Type", "India electronic invoicing system")).Return("IN").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Messaging System", "Message SubType", "India electronic invoicing system")).Return("GEN").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "GEI DeliveryNotification to UniEvent", "Notification Type", "Event Type", "ACK", "India electronic invoicing system")).Return("IAK").Repeat.Any();
            mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2018-11-09T22:37:52");

            var extensionObjects = new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<DeliveryNotification2UniEvent>(sourceFile, expectedFile);
        }
    }
}
