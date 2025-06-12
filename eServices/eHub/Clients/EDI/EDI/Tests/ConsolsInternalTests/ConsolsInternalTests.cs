using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;
using CargoWise.eHub.Clients.EDI.Transforms.EDIConsolsInternal2EDIXmlInterchange;
using CargoWise.eHub.Clients.EDI.Transforms.EDIXmlInterchange2EDIConsolsInternal;

namespace Cargowise.eHub.Clients.EDI.Tests.ConsolsInternalTests
{
	/// <summary>
	/// Summary description for ConsolsInternalTests
	/// </summary>
	[TestClass]
	public class ConsolsInternalTests
	{
		public ConsolsInternalTests()
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
		public void TestEDIConsolsInternal2EDIXmlInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "ConsolsInternalTests.TestFiles.ConsolsInternal.xml";
			string expectedFile = "ConsolsInternalTests.TestFiles.ConsolsInternalWithXMLInterchange.xml";
			mapTester.Execute<EDIConsolsInternal2EDIXmlInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIXmlInterchange2EDIConsolsInternal()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "ConsolsInternalTests.TestFiles.ConsolsInternalWithXMLInterchange.xml";
			string expectedFile = "ConsolsInternalTests.TestFiles.ConsolsInternal.xml";
			mapTester.Execute<EDIXmlInterchange2EDIConsolsInternal>(sourceFile, expectedFile);
		}
	}
}
