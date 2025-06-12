using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.NOR.Transforms.UniShipWHReceive_2_RivianaPO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.NOR.Tests
{
	[TestClass]
	public class UniShipWHReceive_2_RivianaPOTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShipWHReceive_2_RivianaPO()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniShipWHReceive_2_RivianaPO.TestFiles.UniShipWHReceive_2_RivianaPO_input.xml";
			string expectedFile = "UniShipWHReceive_2_RivianaPO.TestFiles.UniShipWHReceive_2_RivianaPO_output.xml";
			mapTester.Execute<UniShipWHReceive_2_RivianaPO>(sourceFile, expectedFile);
		}
	}
}
