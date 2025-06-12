using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.AUCustomsNEXDOC.Configuration;
using CargoWise.eHub.Products.AUCustomsNEXDOC.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.AUCustomsNEXDOC.Tests
{
	[TestClass]
	public class UInterchange2RexOwnership_Tests
	{
		const string filePath = "Maps.UInterchange2RexOwnership.TestFiles.";
		 
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUInterchange2RexForwardOwnership_HoldUntilStatus()
		{
			AssertMapping("Test1_RexForwardOwnership_input.xml", "Test1_RexForwardOwnership_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUInterchange2RexForwardOwnership_NotHoldUntilStatus()
		{
			AssertMapping("Test2_RexForwardOwnership_input.xml", "Test2_RexForwardOwnership_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUInterchange2RexAcknowledgeOwnership()
		{
			AssertMapping("Test3_RexAcknowledgeOwnership_input.xml", "Test3_RexAcknowledgeOwnership_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUInterchange2RexTransferOwnership()
		{
			AssertMapping("Test4_RexTransferOwnership_input.xml", "Test4_RexTransferOwnership_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUInterchange2RexWithdrawOwnership()
		{
			AssertMapping("Test5_RexWithdrawOwnership_input.xml", "Test5_RexWithdrawOwnership_output.xml");
		}

		public static void AssertMapping(string inputFile, string expectedOutputFile)
		{
			string input = filePath + inputFile;
			string expectedOutput = filePath + expectedOutputFile;

			var mockNEXDOCDataModelAccessor = MockRepository.GenerateStrictMock<NEXDOCDataModelAccessor>();
			var mockConfigurationAccessor = MockRepository.GenerateStrictMock<ConfigurationAccessor>();

			var senderID = "T_____AUC";
			var recipientID = "NEXDOCS";

			mockNEXDOCDataModelAccessor.Expect(x => x.GetVendorToken("NEXDOCS")).Return("bb955551e4af485480bfbc88f3485569");
			mockNEXDOCDataModelAccessor.Expect(x => x.GetInstallationToken("NEXDOCS")).Return("VENDOR_ONLY_TEST_ea88dd3ea0f4415fa35786112985e055");
			mockNEXDOCDataModelAccessor.Expect(x => x.GetInstallationPassword("NEXDOCS")).Return("Password!23");

			mockConfigurationAccessor.Expect(x => x.GetClientGroupToken(senderID)).Return("2d353433393339373234343437383935");
			mockConfigurationAccessor.Expect(x => x.GetClientToken("T__AUC", "JRN")).Return("e4cf7d22cdbf4b6b94cd60f2c1fb2251");

			Messages.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", senderID);
			Messages.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID);

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockNEXDOCDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", new Messages() },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockConfigurationAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			mapTester.ExecuteCompiled<UInterchange2RexOwnership>(input, expectedOutput);

			mockNEXDOCDataModelAccessor.VerifyAllExpectations();
			mockConfigurationAccessor.VerifyAllExpectations();
		}
	}
}
