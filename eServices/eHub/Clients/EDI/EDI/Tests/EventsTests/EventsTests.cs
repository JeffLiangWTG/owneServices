using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.EDIEventsInternal2EDIXmlInterchange;
using CargoWise.eHub.Clients.EDI.Transforms.EDIXmlInterchange2EDIEventsInternal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cargowise.eHub.Clients.EDI.Tests.EventsTests
{
	/// <summary>
    /// Summary description for EventsTests
	/// </summary>
	[TestClass]
    public class EventsTests
	{
		public EventsTests()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIEventsInternal2EDIXmlInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "EventsTests.TestFiles.EventsInternal.xml";
            string expectedFile = "EventsTests.TestFiles.EventsXMLInterchange.xml";
            mapTester.Execute<EDIEventsInternal2EDIXmlInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIXmlInterchange2EDIEventsInternal()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "EventsTests.TestFiles.EventsXMLInterchange.xml";
            string expectedFile = "EventsTests.TestFiles.EventsInternal.xml";
			mapTester.Execute<EDIXmlInterchange2EDIEventsInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNACCSShipmentStatusCSV2EDIEventsInternal()
		{
			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='XmlInterchange']/*[local-name()='InterchangeInfo']/*[local-name()='Date']");
			exclusionXpaths.Add("/*[local-name()='XmlInterchange']/*[local-name()='Payload']/*[local-name()='Events']/*[local-name()='Event']/*[local-name()='DateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "EventsTests.TestFiles.NACCSShipmentStatusCSV2EDIEventsInternal_output.xml";
			string expectedFile = "EventsTests.TestFiles.ediEnterprise Event.xml";

			mapTester.Execute<EDIEventsInternal2EDIXmlInterchange>(sourceFile, expectedFile);
		}
	}
}
