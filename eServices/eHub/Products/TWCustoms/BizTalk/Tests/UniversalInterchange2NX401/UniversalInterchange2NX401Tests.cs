using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Products.TWCustoms.Transforms.NCATK.UniversalShipment2NX401;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.eHub.Products.TWCustoms.BizTalk.Tests
{
	[TestClass]
	public class UniversalShipment2NX401Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX401WhenExport()
		{
			AssertMapping("Test_EXP.xml", "Test_EXP_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX401WhenImport()
		{
			AssertMapping("Test_IMP.xml", "Test_IMP_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX401WhenEmpty()
		{
			AssertMapping("Test_Empty_input.xml", "Test_Empty_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX401SortSequenceNumeric()
		{
			AssertMapping("Test_SortSequenceNumeric_Input.xml", "Test_SortSequenceNumeric_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX401AddressGenerateLogic()
		{
			AssertMapping("Test_NX401AddressGenerateLogic_Input.xml", "Test_NX401AddressGenerateLogic_Output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			const string filePath = "UniversalInterchange2NX401.";
			var input = filePath + "Input." + inputFile;
			var expectedOutput = filePath + "Output." + expectedOutputFile;

			mapTester.ExecuteCompiled<UniversalShipment2NX401>(input, expectedOutput);
		}

		[TestInitialize]
		public void Setup()
		{
			var ctx = new TestingMessageContext();
			ctx.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "Test11223");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectClientExists", "", "@ID", "Test11223_TCA")).Return("True");
			var extensionObjects = new Dictionary<string, object>
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};
			mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
		}
		MapTester mapTester;
	}
}
