using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRAEU2UEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class AIRAEU2UEvent_Tests
	{
		const string filePath = "AIRAEU2UEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAEU2UEvent_AmendBasic_failure()
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output_failure.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-20T14:21:01");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN000001700038E", "@referenceType", "CNRF")).Return("000011|000022");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB2MAN000001700056E", "@referenceType", "CNRF")).Return("000013");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB3MAN000001700074E", "@referenceType", "CNRF")).Return("000014|000025");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEU2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAEU2UEvent_AmendBasic_success()
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output_success.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ACKSuccess");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-20T14:21:01");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN000001700038E", "@referenceType", "CNRF")).Return("000011|000022");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB2MAN000001700056E", "@referenceType", "CNRF")).Return("000013");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB3MAN000001700074E", "@referenceType", "CNRF")).Return("000014|000025");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEU2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAEU2UEvent_AmendBasic_success_AIRERRConsignmentsShouldNotBeIncluded()
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output_success_AIRERRNotIncluded.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ACKSuccess");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-20T14:21:01");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN000001700038E", "@referenceType", "CNRF")).Return("000011|000022");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB2MAN000001700056E", "@referenceType", "CNRF")).Return("000013");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB3MAN000001700074E", "@referenceType", "CNRF")).Return("AIRERR");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEU2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAEU2UEvent_Amend1IDT100HAWB_MAWB()
		{
			var input = filePath + "Test2_input.xml";
			var expectedOutput = filePath + "Test2_output.xml";

			int SubscribedCSTCounter = 1;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-20T14:21:01");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper(Arg<string>.Is.Equal("SelectSubscribedReference"),
				Arg<string>.Is.Equal("@reference"),
				Arg<string>.Is.Equal("@senderId"),
				Arg<string>.Is.Equal("VWGT.VWGT001"),
				Arg<string>.Is.Equal("@recipientId"),
				Arg<string>.Is.Equal("PRET1.PRET001"),
				Arg<string>.Is.Equal("@ST_ID"),
				Arg<string>.Is.Equal("SGCMSG"),
				Arg<string>.Is.Equal("@value"),
				Arg<string>.Is.Anything,
				Arg<string>.Is.Equal("@referenceType"),
				Arg<string>.Is.Equal("CNRF"))).Return("000011|000022").WhenCalled(x =>
				{
					if (SubscribedCSTCounter % 10 == 0)
					{
						x.ReturnValue = "000011";
					}
					SubscribedCSTCounter++;
				});

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEU2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAEU2UEvent_Amend1IDT1HAWBOver2CST()
		{
			var input = filePath + "Test3_input.xml";
			var expectedOutput = filePath + "Test3_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-20T14:21:01");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN000009200038E", "@referenceType", "CNRF")).Return("000011|000022|000033|000044|000055|000066|000077|000088|000099|0001010|0001111|0001212|0001313|0001414|0001515|0001616|0001717|0001818|0001919|0002020|0002121|0002222|0002323|0002424|0002525|0002626|0002727|0002828|0002929|0003030|0003131|0003232|0003333|0003434|0003535|0003636|0003737|0003838|0003939|0004040|0004141|0004242|0004343|0004444|0004545|0004646|0004747|0004848|0004949|0005050");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN000009200039E", "@referenceType", "CNRF")).Return("0005151|0005252|0005353|0005454|0005555|0005656|0005757|0005858|0005959|0006060|0006161|0006262|0006363|0006464|0006565|0006666");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN000009200056E", "@referenceType", "CNRF")).Return("000011|000022|000033|000044|000055");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEU2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAEU2UEvent_Cancel()
		{
			var input = filePath + "Test4_input.xml";
			var expectedOutput = filePath + "Test4_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-20T14:21:01");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "199702247W201708230001E", "@referenceType", "MAWB")).Return("012-98765432");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "199702247W201708230001E", "@referenceType", "HAWB")).Return("HAWB1|HAWB2");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "199702247W|20170823|0001E", "@referenceType", "OriginalIDT")).Return("201612334A|20170823|0001").Repeat.Twice();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN0000017201612334A|20170823|0001E", "@referenceType", "CST")).Return("00001|00002");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN000001700001E", "@referenceType", "CNRF")).Return("000011|000022|000033|000044|000055|000066|000077|000088|000099|0001010|0001111|0001212|0001313|0001414|0001515|0001616|0001717|0001818|0001919|0002020|0002121|0002222|0002323|0002424|0002525|0002626|0002727|0002828|0002929|0003030|0003131|0003232|0003333|0003434|0003535|0003636|0003737|0003838|0003939|0004040|0004141|0004242|0004343|0004444|0004545|0004646|0004747|0004848|0004949|0005050");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN000001700002E", "@referenceType", "CNRF")).Return("0005151|0005252|0005353|0005454|0005555|0005656|0005757|0005858|0005959|0006060|0006161|0006262|0006363|0006464|0006565|0006666");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN0000017201612334A|20170823|0001E", "@referenceType", "CST")).Return("00003");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN000001700003E", "@referenceType", "CNRF")).Return("000011|000022|000033|000044|000055||000066");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEU2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRAEU2UEvent_PartialDelete()
		{
			var input = filePath + "Test5_input.xml";
			var expectedOutput = filePath + "Test5_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-20T14:21:01");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "199702247W201708230001E", "@referenceType", "MAWB")).Return("012-98765432");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432EXPHB1MAN000001700038E", "@referenceType", "CNRF")).Return("000011|000022|000033");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432EXPHB2MAN000001700057E", "@referenceType", "CNRF")).Return("000011|000022|000033|000044|000055");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEU2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
