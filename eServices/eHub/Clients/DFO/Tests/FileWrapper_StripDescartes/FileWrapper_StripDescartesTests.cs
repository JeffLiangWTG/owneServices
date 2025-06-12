using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.DFO.Transforms.FileWrapper_StripDescartes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.DFO.Tests
{
	[TestClass]
	public class FileWrapper_StripDescartesTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFileWrapper_StripDescartes()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "FileWrapper_StripDescartes.TestFiles.Input.xml";
			string expectedFile = "FileWrapper_StripDescartes.TestFiles.Output.xml";
			mapTester.Execute<FileWrapper_StripDescartes>(sourceFile, expectedFile);
		}
	}
}
