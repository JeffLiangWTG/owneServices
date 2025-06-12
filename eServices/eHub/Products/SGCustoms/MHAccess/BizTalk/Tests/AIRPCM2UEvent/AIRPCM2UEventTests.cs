using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRPCM2UEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class AIRPCM2UEventTests
	{
		const string filePath = "AIRPCM2UEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2_failure()
		{
			var input = filePath + "Test2_input.xml";
			var expectedOutput = filePath + "Test2_output_failure.xml";
			var subscriptionInput = ReadResource("AIRPCM2UEvent.TestFiles.Test2_subscription_Input.xml");
			var subscriptionOutput = ReadResource("AIRPCM2UEvent.TestFiles.Test2_subscription_Output_failure.xml");

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-06T12:00:00").Repeat.AtLeastOnce();

			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "MAN0000030I")).Return(subscriptionInput);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "MAN0000030I", subscriptionOutput));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRPCM2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2_success()
		{
			var input = filePath + "Test2_input.xml";
			var expectedOutput = filePath + "Test2_output_success.xml";
			var subscriptionInput = ReadResource("AIRPCM2UEvent.TestFiles.Test2_subscription_Input.xml");
			var subscriptionOutput = ReadResource("AIRPCM2UEvent.TestFiles.Test2_subscription_Output_success.xml");

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-06T12:00:00").Repeat.AtLeastOnce();

			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("ACKSuccess");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "MAN0000030I")).Return(subscriptionInput);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "MAN0000030I", subscriptionOutput));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRPCM2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		string ReadResource(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
