using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.CargoImp.FSU_FSA122NativeEventInternal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cargowise.eHub.Clients.EDI.Tests.NativeFSU_FSA122NativeEventInternalTest
{
	[TestClass]
	public class NativeEventInternalTests
	{
		public NativeEventInternalTests()
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
		public void TestFSUFSA122NativeEventInternal_FSU_BKD_OSI()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA122NativeEventInternalTest.TestFiles.FSU_BKD_OSI.xml";
			string expectedFile = "FSUFSA122NativeEventInternalTest.TestFiles.FSU_BKD_OSI_NativeEventInternal.xml";
			mapTester.Execute<NativeFSU_FSA122NativeEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122NativeEventInternal_FSU_MANOnly()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA122NativeEventInternalTest.TestFiles.FSU_MANOnly.xml";
			string expectedFile = "FSUFSA122NativeEventInternalTest.TestFiles.FSU_MANOnly_NativeEventInternal.xml";
			mapTester.Execute<NativeFSU_FSA122NativeEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122NativeEventInternal_FSU12_MultipleCarrierAndDuplicateStatusRecords()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA122NativeEventInternalTest.TestFiles.FSU12_MultipleCarrierAndDuplicateStatusRecords.xml";
			string expectedFile = "FSUFSA122NativeEventInternalTest.TestFiles.FSU12_MultipleCarrierAndDuplicateStatusRecords_NativeEventInternal.xml";
			mapTester.Execute<NativeFSU_FSA122NativeEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122NativeEventInternal_FSU12_OneCarrierAndAllStatusRecords()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

            string sourceFile = "FSUFSA122NativeEventInternalTest.TestFiles.FSU12_OneCarrierAndAllStatusRecords.xml";
            string expectedFile = "FSUFSA122NativeEventInternalTest.TestFiles.FSU12_OneCarrierAndAllStatusRecords_NativeEventInternal.xml";
			mapTester.Execute<NativeFSU_FSA122NativeEventInternal>(sourceFile, expectedFile);
		}

		ICompare Comparer
		{
			get
			{
				if (comparer == null)
				{
					List<string> exclusionXpaths = new List<string>();
					exclusionXpaths.Add("/*[local-name()='NativeEventInternal']/*[local-name()='Body']/*[local-name()='NativeXMLEvent']/*[local-name()='ContextCollection']/*[local-name()='Context' and *[local-name()='Type']/text()='TimeOfDeparture']/*[local-name()='Value']");
					exclusionXpaths.Add("/*[local-name()='NativeEventInternal']/*[local-name()='Body']/*[local-name()='NativeXMLEvent']/*[local-name()='ContextCollection']/*[local-name()='Context' and *[local-name()='Type']/text()='TimeOfArrival']/*[local-name()='Value']");
					exclusionXpaths.Add("/*[local-name()='NativeEventInternal']/*[local-name()='Body']/*[local-name()='NativeXMLEvent']/*[local-name()='ContextCollection']/*[local-name()='Context' and *[local-name()='Type']/text()='FlightDate']/*[local-name()='Value']");
					exclusionXpaths.Add("/*[local-name()='NativeEventInternal']/*[local-name()='Body']/*[local-name()='NativeXMLEvent']/*[local-name()='EventTime']");
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;

	}
}
