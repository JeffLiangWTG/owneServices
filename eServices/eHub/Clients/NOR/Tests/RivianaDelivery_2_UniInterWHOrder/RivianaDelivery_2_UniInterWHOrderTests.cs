using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.NOR.Transforms.RivianaDelivery_2_UniInterWHOrder;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.NOR.Tests
{
	[TestClass]
	public class RivianaDelivery_2_UniInterWHOrderTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRivianaDelivery_2_UniInterWHOrder()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("NORMELHST_RWO", "NORMELHST", "Riviana-Sunrice - Receive Warehouse Orders", "Defaults", "Client Code")).Return("SUNRICE");
			mockCodeMapper.Expect(x => x.GetRecipientCode("NORMELHST_RWO", "NORMELHST", "Riviana-Sunrice - Receive Warehouse Orders", "Unit of Measurement", "EA")).Return("PCE").Repeat.Times(3);

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "RivianaDelivery_2_UniInterWHOrder.TestFiles.RivianaDelivery_2_UniInterWHOrder_input.xml";
			string expectedFile = "RivianaDelivery_2_UniInterWHOrder.TestFiles.RivianaDelivery_2_UniInterWHOrder_output.xml";
			mapTester.Execute<RivianaDelivery_2_UniInterWHOrder>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
