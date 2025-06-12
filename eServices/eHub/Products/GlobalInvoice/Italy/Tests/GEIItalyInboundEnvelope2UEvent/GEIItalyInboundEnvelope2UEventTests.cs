using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GlobalInvoice.Italy.Transforms.GEIItalyInboundEnvelope2UEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GlobalInvoice.Italy.Tests
{
	[TestClass]
	public class GEIItalyInboundEnvelope2UEventTests
	{
		const string filePath = "GEIItalyInboundEnvelope2UEvent.TestFiles.";

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGEIItalyInboundEnvelope2UEvent()
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output.xml";

			var mockDateMapper = MockRepository.GenerateMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10");
			mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
			mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AAABBBCCC");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "GEI_ITALY", "@ST_ID", "GEIMSG", "@value", "IDENTIFIER")).Return("AAABBBCCC").Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GEI_ITALY", "@recipientId", "AAABBBCCC", "@ST_ID", "GEIMSG", "@value", "IDENTIFIER")).Return("BATCH").Repeat.Once();
			


			var extensionObjects = new Dictionary<string, object>() 
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			try
			{
				mapTester.Execute<GEIItalyInboundEnvelope2UEvent>(input, expectedOutput);
			}
			catch (Exception ex)
			{
				Assert.AreEqual("RecipientID cannot be found.", ex.Message);
			}

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}

		
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGEIItalyInboundEnvelope2UEvent_ResolveRecipientIDByFilename()
		{
			var input = filePath + "Test_ResolveRecipientIDByFilename_input.xml";
			var expectedOutput = filePath + "Test_ResolveRecipientIDByFilename_output.xml";

			var mockDateMapper = MockRepository.GenerateMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10");
			mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
			mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AAABBBCCC");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "GEI_ITALY", "@ST_ID", "GEIMSG", "@value", "IDENTIFIER")).Return("").Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClientsWithCollation", "", "@senderId", "GEI_ITALY", "@ST_ID", "GEIMSG", "@value", "0DSg0")).Return("AAABBBCCC").Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GEI_ITALY", "@recipientId", "AAABBBCCC", "@ST_ID", "GEIMSG", "@value", "IDENTIFIER")).Return("BATCH").Repeat.Once();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			try
			{
				mapTester.Execute<GEIItalyInboundEnvelope2UEvent>(input, expectedOutput);
			}
			catch (Exception ex)
			{
				Assert.AreEqual("RecipientID cannot be found.", ex.Message);
			}

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGEIItalyInboundEnvelope2UEvent_RecipientIDNotFoundThrowsException()
		{
			var input = filePath + "Test_RecipientIDNotFoundThrowsException_input.xml";
			var expectedOutput = filePath + "Test_RecipientIDNotFoundThrowsException_output.xml";

			var mockDateMapper = MockRepository.GenerateMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10");
			mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
			mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AAABBBCCC");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "GEI_ITALY", "@ST_ID", "GEIMSG", "@value", "IDENTIFIER")).Return("").Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClientsWithCollation", "", "@senderId", "GEI_ITALY", "@ST_ID", "GEIMSG", "@value", "0DSg0")).Return("").Repeat.Once();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			try
			{
				mapTester.Execute<GEIItalyInboundEnvelope2UEvent>(input, expectedOutput);
			}
			catch (Exception ex)
			{
				Assert.AreEqual("RecipientID cannot be found.", ex.Message);
			}

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}

	}


}
