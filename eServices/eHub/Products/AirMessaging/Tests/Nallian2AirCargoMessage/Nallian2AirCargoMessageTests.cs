using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.AirMessaging.Transforms.Nallian;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.AirMessaging.Nallian.Tests
{
	[TestClass]
	public class Nallian2AirCargoMessageTests
	{
		const string filePath = "Nallian2AirCargoMessage.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNallian2FMA()
		{
			AssertMapping("Test_input_Nallian2AirCargoMessage_FMA.xml",
				"Test_output_Nallian2AirCargoMessage_FMA.xml",
				"08150823894");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNallian2FNA()
		{
			AssertMapping("Test_input_Nallian2AirCargoMessage_FNA.xml",
				"Test_output_Nallian2AirCargoMessage_FNA.xml",
				"08150823894");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ArgumentException))]
		public void TestNallian2AirCargoMessage_Error()
		{
			AssertMapping("Test_input_Nallian2AirCargoMessage_Error.xml",
				"Test_output_Nallian2AirCargoMessage_FMA.xml",
				"08150823894");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string clientAWB)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			var trackingID = "10001000-1000-1000-1000-100010001000";
			string aggregateMessageInformation;
			if (expectedOutputFile.Contains("FMA"))
			{
				aggregateMessageInformation = "FMA_081-50823894__QF_";
			}
			else
			{
				aggregateMessageInformation = "FNA_081-50823894__QF_";
			}

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("NALLIAN");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AIR_CARGO_MESSAGING");
			mockContextAccessor.Expect(x => x.GetContextProperty("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return(trackingID);
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", aggregateMessageInformation));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("AIRAWB", "NALLIAN", "AIR_CARGO_MESSAGING", $"{clientAWB}-{trackingID}-Inbound", trackingID, "TrackingID"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetAirlineCodeFromPrefix", "", "@Prefix", "081")).Return("QF");

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<Nallian2AirCargoMessage>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

	}
}
