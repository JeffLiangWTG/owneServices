using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.DFO.Transforms.FileWrapper_ReduceFHL4;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.DFO.Tests
{
	[TestClass]
	public class FileWrapper_ReduceFHL4Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFileWrapper_ReduceFHL4()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "FileWrapper_ReduceFHL4.TestFiles.Full_Input.xml";
			string expectedFile = "FileWrapper_ReduceFHL4.TestFiles.Full_Output.xml";
			mapTester.Execute<FileWrapper_ReduceFHL4>(sourceFile, expectedFile);

			sourceFile = "FileWrapper_ReduceFHL4.TestFiles.ReducedHBS_Input.xml";
			expectedFile = "FileWrapper_ReduceFHL4.TestFiles.ReducedHBS_Output.xml";
			mapTester.Execute<FileWrapper_ReduceFHL4>(sourceFile, expectedFile);

			sourceFile = "FileWrapper_ReduceFHL4.TestFiles.Mini_Input.xml";
			expectedFile = "FileWrapper_ReduceFHL4.TestFiles.Mini_Output.xml";
			mapTester.Execute<FileWrapper_ReduceFHL4>(sourceFile, expectedFile);
		}
	}
}
