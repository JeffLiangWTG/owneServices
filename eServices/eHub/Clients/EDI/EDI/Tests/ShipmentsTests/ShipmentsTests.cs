using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Transforms.EDIShipmentsInternal2EDIXmlInterchange;
using CargoWise.eHub.Clients.EDI.Transforms.EDIXmlInterchange2EDIShipmentsInternal;

namespace Cargowise.eHub.Clients.EDI.Tests.ShipmentsTests
{
	/// <summary>
    /// Summary description for ShipmentsTests
	/// </summary>
	[TestClass]
    public class ShipmentsTests
	{
		public ShipmentsTests()
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
        public void TestEDIShipmentsInternal2EDIXmlInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "ShipmentsTests.TestFiles.ShipmentsInternal.xml";
            string expectedFile = "ShipmentsTests.TestFiles.ShipmentsXMLInterchange.xml";
            mapTester.Execute<EDIShipmentsInternal2EDIXmlInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIXmlInterchange2EDIShipmentsInternal()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "ShipmentsTests.TestFiles.ShipmentsXMLInterchange.xml";
            string expectedFile = "ShipmentsTests.TestFiles.ShipmentsInternal.xml";
            mapTester.Execute<EDIXmlInterchange2EDIShipmentsInternal>(sourceFile, expectedFile);
		}
	}
}
