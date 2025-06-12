using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.JPCustoms.Transforms.SAS111FlatFile2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class SAS111FlatFile2UniversalEventTest
	{

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFile2UniversalEventTest1()
		{
			string sourceFile = "SAS111FlatFile2UniversalEvent_input.sample_sas111xml.xml";
			string outputFile = "SAS111FlatFile2UniversalEvent_output.sample_sas111xml.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS111FlatFile2UniversalEvent>(sourceFile, outputFile);

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFile2UniversalEventTest2()
		{
			string sourceFile = "SAS111FlatFile2UniversalEvent_input.sample_sas112xml.xml";
			string outputFile = "SAS111FlatFile2UniversalEvent_output.sample_sas112xml.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS111FlatFile2UniversalEvent>(sourceFile, outputFile);

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFile2UniversalEventTest3()
		{
			string sourceFile = "SAS111FlatFile2UniversalEvent_input.SAS111_DNLxml.xml";
			string outputFile = "SAS111FlatFile2UniversalEvent_output.SAS111_DNLxml.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS111FlatFile2UniversalEvent>(sourceFile, outputFile);

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFile2UniversalEventTest4()
		{
			string sourceFile = "SAS111FlatFile2UniversalEvent_input.SAS111_DNUxml.xml";
			string outputFile = "SAS111FlatFile2UniversalEvent_output.SAS111_DNUxml.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS111FlatFile2UniversalEvent>(sourceFile, outputFile);

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFile2UniversalEventTest5()
		{
			string sourceFile = "SAS111FlatFile2UniversalEvent_input.SAS111_HLDxml.xml";
			string outputFile = "SAS111FlatFile2UniversalEvent_output.SAS111_HLDxml.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS111FlatFile2UniversalEvent>(sourceFile, outputFile);

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFile2UniversalEventTest6()
		{
			string sourceFile = "SAS111FlatFile2UniversalEvent_input.SAS112_DNLxml.xml";
			string outputFile = "SAS111FlatFile2UniversalEvent_output.SAS112_DNLxml.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS111FlatFile2UniversalEvent>(sourceFile, outputFile);

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFile2UniversalEventTest7()
		{
			string sourceFile = "SAS111FlatFile2UniversalEvent_input.SAS112_DNUxml.xml";
			string outputFile = "SAS111FlatFile2UniversalEvent_output.SAS112_DNUxml.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS111FlatFile2UniversalEvent>(sourceFile, outputFile);

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFile2UniversalEventTest8()
		{
			string sourceFile = "SAS111FlatFile2UniversalEvent_input.SAS112_HLDxml.xml";
			string outputFile = "SAS111FlatFile2UniversalEvent_output.SAS112_HLDxml.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS111FlatFile2UniversalEvent>(sourceFile, outputFile);

		}

	}
}