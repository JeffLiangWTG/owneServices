using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.EDIAgencyBillsOfLadingInternal2EDIXmlInterchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Transforms.EDIXmlInterchange2EDIAgencyBillsOfLadingInternal;
using System.Collections.Generic;

namespace Tests
{
	[TestClass]
	public class AgencyBillsOfLadingInternalTests
	{
		public AgencyBillsOfLadingInternalTests()
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
		public void TestEDIAgencyBillsOfLadingInternal2EDIXmlInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "AgencyBillsOfLadingInternalTests.TestFiles.AgencyBillsOfLadingInternal.xml";
			string expectedFile = "AgencyBillsOfLadingInternalTests.TestFiles.AgencyBillOfLadingWithXMLInterchange.xml";
			mapTester.Execute<EDIAgencyBillsOfLadingInternal2EDIXmlInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIXmlInterchange2EDIAgencyBillsOfLadingInternal()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "AgencyBillsOfLadingInternalTests.TestFiles.AgencyBillOfLadingWithXMLInterchange.xml";
			string expectedFile = "AgencyBillsOfLadingInternalTests.TestFiles.AgencyBillsOfLadingInternal.xml";
			mapTester.Execute<EDIXmlInterchange2EDIAgencyBillsOfLadingInternal>(sourceFile, expectedFile);
		}
	}
}
