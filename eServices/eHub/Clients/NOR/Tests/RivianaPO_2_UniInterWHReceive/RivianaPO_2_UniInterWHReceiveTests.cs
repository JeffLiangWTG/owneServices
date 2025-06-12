using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.NOR.Transforms.RivianaPO_2_UniInterWHReceive;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.NOR.Tests
{
	[TestClass]
	public class RivianaPO_2_UniInterWHReceiveTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRivianaPO_2_UniInterWHReceive()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("NORMELHST_RWR", "NORMELHST", "Riviana-Sunrice - Receive Warehouse Receipts", "Defaults", "Client Code")).Return("SUNRICE");
			mockCodeMapper.Expect(x => x.GetRecipientCode("NORMELHST_RWR", "NORMELHST", "Riviana-Sunrice - Receive Warehouse Receipts", "Unit of Measurement", "EA")).Return("PCE").Repeat.Times(3);

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "RivianaPO_2_UniInterWHReceive.TestFiles.RivianaPO_2_UniInterWHReceive_input.xml";
			string expectedFile = "RivianaPO_2_UniInterWHReceive.TestFiles.RivianaPO_2_UniInterWHReceive_output.xml";
			mapTester.Execute<RivianaPO_2_UniInterWHReceive>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
