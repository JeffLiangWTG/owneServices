using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.RailInc.Schemas;
using CargoWise.eHub.Products.RailInc.Transforms.EDIUniversalShipment2FleetUpdate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.RailInc.Tests
{
	[TestClass]
	public class EDIUniversalShipment2FleetUpdateTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2FleetUpdate_WrongMessage()
		{
            var sourceFile = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_WrongMessage.xml";
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("FileName").Repeat.Once();

            var extensionObjects = new Dictionary<string, object>
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor}
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteAssertException<EDIUniversalShipment2FleetUpdate>(sourceFile, "Message from CW1 client did not provide a DataSource of Type ForwardingConsol or CustomsDeclaration");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIUniversalShipment2FleetUpdate_NoFileName_ThrowAnException()
        {
            var sourceFile = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_1_Input.xml";
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("").Repeat.Once();

            var extensionObjects = new Dictionary<string, object>
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor}
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteAssertException<EDIUniversalShipment2FleetUpdate>(sourceFile, "Message from CW1 client did not provide a file name");
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2FleetUpdate()
		{
            var input = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_1_Input.xml";
            var expectedOutput = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_1_Output.xml";
            RunTest(input, expectedOutput);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2FleetUpdateConsol()
		{
            var input = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_Consol_Input.xml";
            var expectedOutput = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_Consol_Output.xml";
            RunTest(input, expectedOutput);

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2FleetUpdateShipment()
		{
            var input = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_Shipment_Input.xml";
            var expectedOutput = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_Shipment_Output.xml";
            RunTest(input, expectedOutput);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2FleetUpdate_Add()
		{
            var input = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_Add_Input.xml";
            var expectedOutput = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_Add_Output.xml";
            RunTest(input, expectedOutput);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2FleetUpdate_Delete()
		{
            var input = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_Delete_Input.xml";
            var expectedOutput = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_Delete_Output.xml";
            RunTest(input, expectedOutput);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2FleetUpdate_NoContainer()
		{
			var sourceFile = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_NoContainer.xml";
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("MIQMCIMKC").Repeat.Once();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RAILINCFC").Repeat.Once();
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("FileName").Repeat.Once();

			var extensionObjects = new Dictionary<string, object>
			{
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor}
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteAssertException<EDIUniversalShipment2FleetUpdate>(sourceFile, "Shipment must have at least one container.");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2FleetUpdate_ForwardingShipment_NoContainer()
		{
			var sourceFile = "EDIUniversalShipment2FleetUpdate.TestFiles.UniversalShipment_ForwardingShipment_NoContainer.xml";
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("MIQMCIMKC").Repeat.Once();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RAILINCFC").Repeat.Once();
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("FileName").Repeat.Once();

			var extensionObjects = new Dictionary<string, object>
			{
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor}
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteAssertException<EDIUniversalShipment2FleetUpdate>(sourceFile, "Subshipment must have at least one container.");
		}

		public void RunTest(string input, string expectedOutput) {
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

            mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Generate CLU messages", "Fleet Status Code", "Fleet Status", "RLF")).Return("P").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Generate CLU messages", "Fleet Status Code", "Fleet Status", "RLD")).Return("D").Repeat.Any();

            // All other codes return "L"
            mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Generate CLU messages", "Fleet Status Code", "Fleet Status", "FL1")).Return("L").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Generate CLU messages", "Fleet Status Code", "Fleet Status", "APP")).Return("L").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Generate CLU messages", "Fleet Status Code", "Fleet Status", "")).Return("L").Repeat.Any();

            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("MIQMCIMKC").Repeat.Once();
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RAILINCFC").Repeat.Once();
            mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("FileName").Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("RLC", "RAILINCFC", "MIQMCIMKC", "BNSRL1|C00008244_0"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("RLC"), Arg<string>.Is.Equal("RAILINCFC"), Arg<string>.Is.Equal("MIQMCIMKC"), Arg<string>.Is.Anything));

            var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockDataModelAccessor },
			};

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<EDIUniversalShipment2FleetUpdate>(input, expectedOutput);
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test1_Schema_XML2FlatFile()
		{
			using (var inputMsg = TestHelper.GetEmbeddedResource("EDIUniversalShipment2FleetUpdate.TestFiles.Text.FleetUpdate_XML.xml"))
			using (var outputMsg = SchemaTester<FleetUpdate>.AssembleFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(TestHelper.GetResourceAsString("EDIUniversalShipment2FleetUpdate.TestFiles.Text.FleetUpdate_FF.txt"), sr.ReadToEnd());
		}
	}
}
