using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.UShipment2UEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Dakosy.BT.Tests
{
	[TestClass]
	public class UShipment2UEventTests
	{
		const string filePath = "UShipment2UEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2UEvent()
		{
			AssertMapping("Test1_Shipment_input.xml", "Test1_Shipment_output.xml");
			AssertMapping("Test2_Consol_input.xml", "Test2_Consol_output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2017-04-07T10:00:00");
			mockContextAccessor.Expect(x => x.GetContextProperty("ErrorCode", "http://cargowise.com/ehub/routing/2010/06")).Return("IRJ");
			mockContextAccessor.Expect(x => x.GetContextProperty("ErrorDescription", "http://cargowise.com/ehub/routing/2010/06")).Return("|RES=You are not registered with WTG for Dakosy messaging. Please register.|DEP=eHub");
			var extensionObjects = new Dictionary<string, object>() 
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UShipment2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
