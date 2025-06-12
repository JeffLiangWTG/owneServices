using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Transforms.EDIWhsDocketsInternal2EDIXmlInterchange;
using CargoWise.eHub.Clients.EDI.Transforms.EDIXmlInterchange2EDIWhsDocketsInternal;

namespace Cargowise.eHub.Clients.EDI.Tests.WhsDocketsTests
{
	/// <summary>
    /// Summary description for WhsDocketsTests
	/// </summary>
	[TestClass]
	public class WhsDocketsTests
	{
        public WhsDocketsTests()
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
        public void TestEDIWhsDocketsInternal2EDIXmlInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "WhsDocketsTests.TestFiles.WhsDocketsInternal.xml";
            string expectedFile = "WhsDocketsTests.TestFiles.WhsDocketsXMLInterchange.xml";
            mapTester.Execute<EDIWhsDocketsInternal2EDIXmlInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIXmlInterchange2EDIWhsDocketsInternal()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "WhsDocketsTests.TestFiles.WhsDocketsXMLInterchange.xml";
            string expectedFile = "WhsDocketsTests.TestFiles.WhsDocketsInternal.xml";
            mapTester.Execute<EDIXmlInterchange2EDIWhsDocketsInternal>(sourceFile, expectedFile);
		}
	}
}
