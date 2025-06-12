using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.AUCustomsNEXDOC.Tests
{
	[TestClass]
	public class NEXDOC2UEvent_Tests
	{
		const string filePath = "Maps.NEXDOC2UEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_01_FaultAmend1()
		{
			var input = filePath + "sample 01 - failed amend - Input.xml";
			var expectedOutput = filePath + "sample 01 - failed amend - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_AMEND");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_02_FaultLodge1()
		{
			var input = filePath + "sample 02 - failed lodge - Input.xml";
			var expectedOutput = filePath + "sample 02 - failed lodge - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_LODGE");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_03_FaultLodge2()
		{
			var input = filePath + "sample 03 - failed lodge - Input.xml";
			var expectedOutput = filePath + "sample 03 - failed lodge - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_LODGE");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_04_LodgeResponse()
		{
			var input = filePath + "sample 04 - successful lodge - Input.xml";
			var expectedOutput = filePath + "sample 04 - successful lodge - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_LODGE");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_05_NotificationResponse()
		{
			var input = filePath + "sample 05 - notification response - Input.xml";
			var expectedOutput = filePath + "sample 05 - notification response - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_06_FaultOrder()
		{
			var input = filePath + "sample 06 - failed order - Input.xml";
			var expectedOutput = filePath + "sample 06 - failed order - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_07_OrderResponse()
		{
			var input = filePath + "sample 07 - successful order - Input.xml";
			var expectedOutput = filePath + "sample 07 - successful order - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_08_FaultAmend2()
		{
			var input = filePath + "sample 08 - failed amend - Input.xml";
			var expectedOutput = filePath + "sample 08 - failed amend - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_AMEND");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_09_AmendResponse()
		{
			var input = filePath + "sample 09 - successful amend - Input.xml";
			var expectedOutput = filePath + "sample 09 - successful amend - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_AMEND");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_10_FaultWithdrawal()
		{
			var input = filePath + "sample 10 - failed withdrawal - Input.xml";
			var expectedOutput = filePath + "sample 10 - failed withdrawal - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_WITHDRAW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_11_WithdrawalResponse()
		{
			var input = filePath + "sample 11 - successful withdrawal - Input.xml";
			var expectedOutput = filePath + "sample 11 - successful withdrawal - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_WITHDRAW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_12_FaultWithMessages()
		{
			var input = filePath + "sample 12 - failed with messages - Input.xml";
			var expectedOutput = filePath + "sample 12 - failed with messages - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_WITHDRAW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_13_CancelEDNResponse()
		{
			var input = filePath + "sample 13 - Cancel_EDN_Response - Input.xml";
			var expectedOutput = filePath + "sample 13 - Cancel_EDN_Response - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_WITHDRAW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_14_TransferEDNResponse()
		{
			var input = filePath + "sample 14 - Transfer_EDN_Response - Input.xml";
			var expectedOutput = filePath + "sample 14 - Transfer_EDN_Response - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_WITHDRAW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_15_CancelREXResponse()
		{
			var input = filePath + "sample 15 - Cancel_REX_Response - Input.xml";
			var expectedOutput = filePath + "sample 15 - Cancel_REX_Response - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_WITHDRAW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_16_ReIssueResponse()
		{
			var input = filePath + "sample 16 - ReIssueResponse - Input.xml";
			var expectedOutput = filePath + "sample 16 - ReIssueResponse - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_ReIssueResponse");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_17_InvalidToken()
		{
			var input = filePath + "sample 17 - invalid token - Input.xml";
			var expectedOutput = filePath + "sample 17 - invalid token  - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_InvalidToken");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_18_ReadCertificateResponse()
		{
			var input = filePath + "sample 18 - ReadCertificateResponse - Input.xml";
			var expectedOutput = filePath + "sample 18 - ReadCertificateResponse - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_ReadCertificateResponse");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_19_ReplaceCertificateResponse()
		{
			var input = filePath + "sample 19 - ReplaceCertificateResponse - Input.xml";
			var expectedOutput = filePath + "sample 19 - ReplaceCertificateResponse - Output.xml";
			AssertMapping(input, expectedOutput, "S000112234_ReplaceCertificateResponse");
		}

		void AssertMapping(string input, string expectedOutput, string OverrideFileName)
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockDateMapper.Stub(_ => _.CurrentDateTimeUTC("s")).Return("2018-02-07T21:20:29");
			mockContextAccessor.Stub(_ => _.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("NEXDOCSTest");
			mockContextAccessor.Stub(_ => _.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("NEXDOCSTest");
			mockContextAccessor.Stub(_ => _.GetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06")).Return(OverrideFileName);
			mockDataModelAccessor.Stub(_ => _.InsertSubscriptionValue(
				Arg<string>.Is.Equal("NEXDOC"),
				Arg<string>.Is.Equal("NEXDOCSTest"),
				Arg<string>.Is.Equal("NEXDOCSTest"),
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<NEXDOC2UEvent>(input, expectedOutput);
		}
	}
}
