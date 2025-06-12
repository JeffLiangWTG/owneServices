using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.DFO.Transforms.FileWrapper_Split80;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.eHub.Clients.DFO.Tests
{
	[TestClass]
	public class FileWrapper_Split80Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFileWrapper_Split80()
		{
            List<string> exclusionXpaths = new List<string>();
            exclusionXpaths.Add("//*[local-name()='FileName']");
            ICompare comparer = new ExcludingComparer(exclusionXpaths);

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "FileWrapper_Split80.TestFiles.Input.xml";
			string expectedFile = "FileWrapper_Split80.TestFiles.Output.xml";
			mapTester.ExecuteCompiled<FileWrapper_Split80>(sourceFile, expectedFile);
		}
	}
}
