using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.NOR.Transforms.IconOrder_2_UniInterWHOrder;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.NOR.Tests
{
	[TestClass]
	public class IconOrder_2_UniInterWHOrderTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestIconOrder_2_UniInterWHOrder()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("NORMELHST_IWO", "NORMELHST", "Icon - Receive Warehouse Orders", "Defaults", "Client Code")).Return("ICON");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("NORMELHST_IWO", "NORMELHST", "Icon - Receive Warehouse Orders", "Defaults", "UQ")).Return("PCE");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCountryInfo", "@result", "@name", "AUSTRALIA")).Return("AU").Repeat.Times(3);
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCountryInfo", "@result", "@name", "AUSTRALI")).Return("");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "IconOrder_2_UniInterWHOrder.TestFiles.IconOrder_2_UniInterWHOrder_input.xml";
			string expectedFile = "IconOrder_2_UniInterWHOrder.TestFiles.IconOrder_2_UniInterWHOrder_output.xml";
			mapTester.Execute<IconOrder_2_UniInterWHOrder>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
