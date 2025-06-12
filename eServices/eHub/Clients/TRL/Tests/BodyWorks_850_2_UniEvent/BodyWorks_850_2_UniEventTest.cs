using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRL.Transforms.BodyWorks_850_2_UniEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.TRL.Tests
{
	[TestClass]
	public class BodyWorks_850_2_UniEventTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestBodyWorks_850_2_UniEvent()
		{
			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='EventTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "BodyWorks_850_2_UniEvent.TestFiles.BodyWorks_850_2_UniEvent_input.xml";
			string expectedFile = "BodyWorks_850_2_UniEvent.TestFiles.BodyWorks_850_2_UniEvent_output.xml";
			mapTester.Execute<BodyWorks_850_2_UniEvent>(sourceFile, expectedFile);
		}
	}
}
