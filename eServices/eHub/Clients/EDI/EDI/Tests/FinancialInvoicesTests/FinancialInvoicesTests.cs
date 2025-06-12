using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Transforms.EDIFinancialInvoicesInternal2EDIXmlInterchange;
using CargoWise.eHub.Clients.EDI.Transforms.EDIXmlInterchange2EDIFinancialInvoicesInternal;

namespace Cargowise.eHub.Clients.EDI.Tests.FinancialInvoicesTests
{
	/// <summary>
    /// Summary description for FinancialInvoicesTests
	/// </summary>
	[TestClass]
    public class FinancialInvoicesTests
	{
		public FinancialInvoicesTests()
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
        public void TestEDIFinancialInvoicesInternal2EDIXmlInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "FinancialInvoicesTests.TestFiles.FinancialInvoicesInternal.xml";
            string expectedFile = "FinancialInvoicesTests.TestFiles.FinancialInvoicesXMLInterchange.xml";
            mapTester.Execute<EDIFinancialInvoicesInternal2EDIXmlInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIXmlInterchange2EDIFinancialInvoicesInternal()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "FinancialInvoicesTests.TestFiles.FinancialInvoicesXMLInterchange.xml";
            string expectedFile = "FinancialInvoicesTests.TestFiles.FinancialInvoicesInternal.xml";
            mapTester.Execute<EDIXmlInterchange2EDIFinancialInvoicesInternal>(sourceFile, expectedFile);
		}
	}
}
