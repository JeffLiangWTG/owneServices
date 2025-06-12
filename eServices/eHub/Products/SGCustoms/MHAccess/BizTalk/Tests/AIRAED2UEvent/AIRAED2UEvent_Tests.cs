using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRAED2UEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class AIRAED2UEvent_Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAED2UEvent1_failure()
		{
			var input = "AIRAED2UEvent.TestFiles.Test1_input.xml";
			var expectedOutput = "AIRAED2UEvent.TestFiles.Test1_output_failure.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockDateMapper.Expect(x => x.CurrentDateTime("s")).Return("2017-08-23T11:30:01");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN000009200001E", "@referenceType", "CNRF")).Return("000011");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN000009200002E", "@referenceType", "CNRF")).Return("000012|00002413");

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGTVWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ErrorCode: 99999999;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: CargoWise.eHub.Core.Orchestrations.Helper.FatalMessageProcessingException: CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Common.Orchestrations.MHAcc");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<AIRAED2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAED2UEvent1_success()
		{
			var input = "AIRAED2UEvent.TestFiles.Test1_input.xml";
			var expectedOutput = "AIRAED2UEvent.TestFiles.Test1_output_success.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockDateMapper.Expect(x => x.CurrentDateTime("s")).Return("2017-08-23T11:30:01");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN000009200001E", "@referenceType", "CNRF")).Return("000011");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN000009200002E", "@referenceType", "CNRF")).Return("000012|00002413");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGTVWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ACKSuccess");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<AIRAED2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAED2UEvent_AIRERRConsignmentsShouldNotBeIncluded()
		{
			var input = "AIRAED2UEvent.TestFiles.Test1_input.xml";
			var expectedOutput = "AIRAED2UEvent.TestFiles.Test1_output_success_AIRERRNotIncluded.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockDateMapper.Expect(x => x.CurrentDateTime("s")).Return("2017-08-23T11:30:01");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN000009200001E", "@referenceType", "CNRF")).Return("AIRERR");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN000009200002E", "@referenceType", "CNRF")).Return("000012|00002413");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGTVWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ACKSuccess");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<AIRAED2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAED2UEvent2()
		{
			var input = "AIRAED2UEvent.TestFiles.Test2_input.xml";
			var expectedOutput = "AIRAED2UEvent.TestFiles.Test2_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockDateMapper.Expect(x => x.CurrentDateTime("s")).Return("2017-08-24T11:30:01");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "8716E", "@referenceType", "CNRF")).Return("0000166|0000267|0000368");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "827127E", "@referenceType", "CNRF")).Return("0000169");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "827146E", "@referenceType", "CNRF")).Return("0000170");

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("Source");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("Destination");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ErrorCode: 99999999;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: CargoWise.eHub.Core.Orchestrations.Helper.FatalMessageProcessingException: CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Common.Orchestrations.MHAcc");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<AIRAED2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
