using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.TPT.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.TPT.Tests
{
	[TestClass]
	public class UniShip2SubmitInstructionTests
	{
		const string filePath = "UniShip2SubmitInstruction.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip2SubmitInstruction()
		{
			List<string> referenceList = new List<string> { "C00001536_MRN11111111_Shipping_", "C00001536_MRN22222222_Shipping_", "C00001536_MRN33333333_Shipping_", "C00001004_EXPREF00002_Shipping_" };
			TestMapping("Test1_input_ShippingOrder.xml", "Test1_output.xml", "121", referenceList);

			referenceList = new List<string> { "C00001537_IR0334434344_Landing_", "C00001537_IR0334434569_Landing_", "C00001004_IMPREF00001_Transhipment_ZA3WC" };
			TestMapping("Test2_input_LandingOrder.xml", "Test2_output.xml", "121", referenceList);

			referenceList = new List<string> { "C00001537_IR0334434344_Transhipment_ZADUR", "C00001537_IR0334434569_Transhipment_ZADUR" };
			TestMapping("Test3_input_Transhipment.xml", "Test3_output.xml", "121", referenceList);

			referenceList = new List<string> { "C00001536_MRN11111111_Shipping_", "C00001536_MRN22222222_Shipping_", "C00001536_MRN33333333_Shipping_", "C00001004_EXPREF00002_Shipping_" };
			TestMapping("Test4_input_ShippingOrder_Amendment.xml", "Test4_output.xml", "121", referenceList);

			referenceList = new List<string> { "C00001536_MRN11111111_Shipping_", "C00001536_MRN22222222_Shipping_", "C00001536_MRN33333333_Shipping_", "C00001004_EXPREF00002_Shipping_" };
			TestMapping("Test5_input_MixedPackingLine.xml", "Test5_output.xml", "121", referenceList);

			referenceList = new List<string> { "C00001537_IR0334434344_Transhipment_ZADUR", "C00001537_IR0334434569_Transhipment_ZADUR" };
			TestMapping("Test6_input_Transhipment.xml", "Test6_output.xml", "121", referenceList);
		}

		public void TestMapping(string sourceFile, string expectedFile, string interchangeNumber, List<string> referenceList)
		{
			var input = filePath + sourceFile;
			var expectedOutput = filePath + expectedFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockUnitConvertorMapper = MockRepository.GenerateStrictMock<UnitConverter>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
			mockDataModelAccessor.Expect(x=>x.GetClientRegistrationCode("TESTSENDER","JNB", "TPT")).Return("ABCD");

			for (int i = 0; i < referenceList.Count; i++)
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.TPT.Transforms", "@maxlength", "14")).Return(i.ToString());
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("TPTMSG", "TPT", "TESTSENDER", i.ToString(), referenceList[i])).Repeat.Any();
			}

			mockCodeMapper.Expect(x => x.GetRecipientCode("TPT_SI", "TPT_SI", "Service Instruction to TPT", "Package Type", "TPT Code", "UNT")).Return("UNT").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("TPT_SI", "TPT_SI", "Service Instruction to TPT", "Package Type", "TPT Code", "BAG")).Return("BG").Repeat.Any();

			mockDateMapper.Stub(x => x.CurrentDateTimeUTC("yyyy-MM-dd HH:mm:ss")).Return("2015-07-09 09:30:10");

			var extensionObjects = new Dictionary<string, object>() 
			{ 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockUnitConvertorMapper },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniShip2SubmitInstruction>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockUnitConvertorMapper.VerifyAllExpectations();
		}
	}
}
