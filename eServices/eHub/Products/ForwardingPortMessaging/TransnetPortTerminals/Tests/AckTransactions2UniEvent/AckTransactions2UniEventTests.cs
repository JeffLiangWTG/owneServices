using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.TPT.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
	[TestClass]
	public class AckTransactions2UniEventTests
	{
		const string filePath = "AckTransactions2UniEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAckTransactions2UniEvent()
		{
			TestMapping("Test1_input.xml", "Test1_output_good_subscription_reference.xml", "121", "C123456_M12345_Landing_ZACPTK"); 
			TestMapping("Test1_input.xml", "Test1_output_bad_subscription_missing_documentName.xml", "121", "C123456_M12345__ZACPTK"); // Invalid case
			TestMapping("Test1_input.xml", "Test1_output_bad_subscription_missing_portCode.xml", "121", "C123456_M12345_Landing_"); // Invalid case
			TestMapping("Test2_multiple_error_no_eventTime_input.xml", "Test2_multiple_error_no_eventTime_output.xml", "121", "C123456_M12345_Shipping_");
			TestMapping("Test3_multiple_ediAcknowledgement_input.xml", "Test3_multiple_ediAcknowledgement_output.xml", "121", "C123456_M12345_Transhipment_ZACPTK");
		}

		public void TestMapping(string sourceFile, string expectedFile, string interchangeNumber, string subscriptionReference)
		{
			var input = filePath + sourceFile;
			var expectedOutput = filePath + expectedFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TPT", "@recipientId", "$recipientID", "@ST_ID", "TPTMSG", "@value", interchangeNumber)).Return(subscriptionReference).Repeat.Once();
			mockCodeMapper.Stub(x => x.GetRecipientCode("TPT", "TPT", "Service Instruction Acknowledgment from TPT", "Error", "Error Description", "SIE_CUSRES_VOC_MISSING_1")).Return("");

			mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2015-07-09T09:30:10");
			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<AckTransactions2UniEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
