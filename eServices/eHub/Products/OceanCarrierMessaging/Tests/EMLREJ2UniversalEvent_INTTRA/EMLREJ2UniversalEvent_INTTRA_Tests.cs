using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.EMLREJ_INTTRA2UEvent;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class EMLREJ2UniversalEvent_INTTRA_Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEMLREJ2UEvent()
		{
			AssertMapping("Test1_Input.xml", "Test1_Output.xml", "14496", "CGWHYEDAUAYA"); //Found Interchange Control Number from "Interchange Control Number: "
			AssertMapping("Test2_Input.xml", "Test2_Output.xml", "13545", "865928"); //Found Interchange Control Number from "Tx 1 : "
			AssertMapping("Test3_Input.xml", "Test3_Output.xml", "13545", "865928"); //VERMAS
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEMLREJ2UEvent_Fail()
		{
			AssertMapping_Fail("TestFail1_Input.xml", "TestFail1_Output.xml"); //Found no Interchange Control Number
		}

		#region Implementation

		void AssertMapping(string inputFile, string expectedOutputFile, string interchangeControlNumber, string iNTTRACodeForEHubClient)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			var recipientID = "HYEDAUAYA";
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID)).Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "INTTRA", "@ST_PK", "E994EF0E-BADF-4CA5-8013-598184A6E065", "@value", interchangeControlNumber)).Return(recipientID).Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "INTTRA", "@recipientId", "", "@ST_PK", "E994EF0E-BADF-4CA5-8013-598184A6E065", "@value", interchangeControlNumber)).Return("C02411207").Repeat.Once();
			mockDateMapper.Stub(x => x.CurrentDateTime("s")).Return("2017-04-12T00:30:00");


			var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockDateMapper },
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<EMLREJ_INTTRA2UEvent>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}

		void AssertMapping_Fail(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEDAUAYA")).Repeat.Never();
			mockDateMapper.Stub(x => x.CurrentDateTime("s")).Return("2017-04-12T00:30:00");

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockDateMapper },
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<EMLREJ_INTTRA2UEvent>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
		}

		const string filePath = "EMLREJ2UniversalEvent_INTTRA.TestFiles.";

		#endregion
	}
}