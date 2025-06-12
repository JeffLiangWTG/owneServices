using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRL.Transforms.TRLX12_4010_850_2_UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.TRL.Tests
{
	[TestClass]
	public class TRLX12_4010_850_2_UniversalEventTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTRLX12_4010_850_2_UniversalEvent()
		{
			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='EventTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "TRLX12_4010_850_2_UniversalEvent.TestFiles.TRLX12_4010_850_2_UniversalEvent_input.xml";
			string expectedFile = "TRLX12_4010_850_2_UniversalEvent.TestFiles.TRLX12_4010_850_2_UniversalEvent_output.xml";
			mapTester.Execute<TRLX12_4010_850_2_UniversalEvent>(sourceFile, expectedFile);
		}
	}
}
