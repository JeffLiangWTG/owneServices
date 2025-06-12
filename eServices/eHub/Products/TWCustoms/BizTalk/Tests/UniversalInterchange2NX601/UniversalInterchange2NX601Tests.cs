using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Products.TWCustoms.Transforms.NCATK.UniversalShipment2NX601;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.eHub.Products.TWCustoms.BizTalk.Tests
{
	[TestClass]
	public class UniversalShipment2NX601Tests
	{
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test2NX601IMP()
        {
            AssertMapping("Test_IMP.xml", "Test_IMP_output.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test2NX601ImportFullData()
        {
            AssertMapping("Test_IMP_FullData.xml", "Test_IMP_FullData_output.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test2NX601WhenEmpty()
        {
            AssertMapping("Test_Empty_input.xml", "Test_Empty_output.xml");
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestLocalProcessorAddressNonOverride()
		{
			AssertMapping("Test_LocalProcessorAddress_NonOverride.xml", "Test_LocalProcessorAddress_NonOverride_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestLocalProcessorAddressOverride()
		{
			AssertMapping("Test_LocalProcessorAddress_Override.xml", "Test_LocalProcessorAddress_Override_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestWI00447900()
		{
			AssertMapping("Test_WI00447900_input.xml", "Test_WI00447900_output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
            const string filePath = "UniversalInterchange2NX601.";
			var input = filePath + "Input." + inputFile;
			var expectedOutput = filePath + "Output." + expectedOutputFile;

            mapTester.ExecuteCompiled<UniversalShipment2NX601>(input, expectedOutput);
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
