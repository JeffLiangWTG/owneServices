using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Gateway.Transforms.dbo_SelectInboxMessagesByStatus2SendRequestStream;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Gateway.Tests
{
	[TestClass]
	public class dbo_SelectInboxMessagesByStatus2SendRequestStreamTests
	{
		private MapTester _mapTester;
		private Assembly _testFixture;

		[TestInitialize]
		public void TestSetup()
		{
			_testFixture = Assembly.GetExecutingAssembly();
			_mapTester = new MapTester(_testFixture);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_dbo_SelectInboxMessagesByStatus2SendRequestStream()
		{
			string source = "TestFiles.dbo_SelectInboxMessagesByStatus1.xml";
			string expected = "TestFiles.SendRequestStream.xml";
			_mapTester.Execute<dbo_SelectInboxMessagesByStatus2SendRequestStream>(source, expected);
		}
	}
}
