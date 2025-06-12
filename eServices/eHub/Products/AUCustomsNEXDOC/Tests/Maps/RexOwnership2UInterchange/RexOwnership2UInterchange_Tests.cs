using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.AUCustomsNEXDOC.Tests
{
	[TestClass]
	public class RexOwnership2UniversalInterchange_Tests
	{
		const string filePath = "Maps.RexOwnership2UInterchange.TestFiles.";
		 
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRexForwardOwnership2UniversalInterchange_Success()
		{
			AssertMapping("Test1_RexForwardOwnership_Success_input.xml", "Test1_RexForwardOwnership_Success_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRexAcknowledgeOwnership2UInterchange_Success()
		{
			AssertMapping("Test2_RexAcknowledgeOwnership_Success_input.xml", "Test2_RexAcknowledgeOwnership_Success_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRexAcknowledgeOwnership2UInterchange_Fault()
		{
			AssertMapping("Test3_RexAcknowledgeOwnership_Fault_input.xml", "Test3_RexAcknowledgeOwnership_Fault_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRexTransferOwnership2UniversalInterchange_Success()
		{
			AssertMapping("Test4_RexTransferOwnership_Success_input.xml", "Test4_RexTransferOwnership_Success_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRexWithdrawOwnership2UniversalInterchange_Success()
		{
			AssertMapping("Test5_RexWithdrawOwnership_Success_input.xml", "Test5_RexWithdrawOwnership_Success_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRexAcknowledgeOwnership2UInterchange_RexNumber_Fault()
		{
			AssertMapping("Test6_RexAcknowledgeOwnership_RexNumber_Fault_input.xml", "Test6_RexAcknowledgeOwnership_RexNumber_Fault_output.xml");
		}

		public static void AssertMapping(string inputFile, string expectedOutputFile)
		{
			string input = filePath + inputFile;
			string expectedOutput = filePath + expectedOutputFile;

			var senderID = "T_____AUC";
			var recipientID = "NEXDOCS";
			var dataMapperMock = MockRepository.GenerateStrictMock<DateMapper>();

			dataMapperMock.Expect(x => x.CurrentDateTimeUTC(Arg<string>.Is.Anything)).Return("2018-02-07T21:20:29");
			Messages.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", senderID);
			Messages.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID);
			Messages.SetContextProperty("RexNumber", "", "REX0000028829");
			Messages.SetContextProperty("JobNumber", "", "B0000001");

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", new Messages() },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", dataMapperMock }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			mapTester.ExecuteCompiled<RexOwnership2UInterchange>(input, expectedOutput);
		}
	}
}
