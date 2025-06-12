using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;
using CargoWise.eHub.Clients.EDI.Transforms.EDIInvoicesInternal2EDIXmlInterchange;
using CargoWise.eHub.Clients.EDI.Transforms.EDIXmlInterchange2EDIInvoicesInternal;

namespace Cargowise.eHub.Clients.EDI.Tests.InvoicesInternal
{
    /// <summary>
    /// Summary description for InvoicesInternalTests
    /// </summary>
    [TestClass]
    public class InvoicesInternalTests
    {
        public InvoicesInternalTests()
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
        public void TestEDIInvoicesInternal2EDIXmlInterchange()
        {
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "InvoicesInternalTests.TestFiles.InvoicesInternal.xml";
            string expectedFile = "InvoicesInternalTests.TestFiles.XmlInterchange.xml";
            mapTester.Execute<EDIInvoicesInternal2EDIXmlInterchange>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIXmlInterchange2EDIInvoicesInternal()
        {
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "InvoicesInternalTests.TestFiles.XmlInterchange.xml";
            string expectedFile = "InvoicesInternalTests.TestFiles.InvoicesInternal.xml";
            mapTester.Execute<EDIXmlInterchange2EDIInvoicesInternal>(sourceFile, expectedFile);
        }
    }
}
