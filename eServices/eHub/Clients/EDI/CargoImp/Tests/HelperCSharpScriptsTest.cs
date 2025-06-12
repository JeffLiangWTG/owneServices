using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CargoWise.eHub.Clients.EDI.Transforms.Native.FSU_FSA122NativeEventInternal;

namespace CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests
{
	/// <summary>
	/// Summary description for HelperCSharpScriptsTest
	/// </summary>
	[TestClass]
	public class HelperCSharpScriptsTest
	{
		public HelperCSharpScriptsTest()
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
		public void TestGetEdiEnterpriseEventCode()
		{
			Assert.AreEqual("OCR", helper.GetEdiEnterpriseEventCode("RCS"));
			Assert.AreEqual("", helper.GetEdiEnterpriseEventCode("AAA"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetVolumeOfGoods()
		{
			Assert.AreEqual("100 M3", helper.GetVolumeOfGoods("M3100DG11"));
			Assert.AreEqual("100 M3", helper.GetVolumeOfGoods("M3100"));
			Assert.AreEqual("", helper.GetVolumeOfGoods(""));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetDensityGroup()
		{
			Assert.AreEqual("11", helper.GetDensityGroup("M3100DG11"));
			Assert.AreEqual("", helper.GetDensityGroup("M3100"));
			Assert.AreEqual("", helper.GetDensityGroup(""));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetIsPartial()
		{
			Assert.AreEqual("Y", helper.GetIsPartial("P15K33.9"));
			Assert.AreEqual("", helper.GetIsPartial("T15K33.9"));
			Assert.AreEqual("", helper.GetIsPartial(""));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetNumberOfPieces()
		{
			Assert.AreEqual("15", helper.GetNumberOfPieces("P15K33.9"));
			Assert.AreEqual("1", helper.GetNumberOfPieces("P1K33.9"));
			Assert.AreEqual("9999", helper.GetNumberOfPieces("P9999K33.9"));
			Assert.AreEqual("99999", helper.GetNumberOfPieces("P99999K33.9"));
			Assert.AreEqual("15", helper.GetNumberOfPieces("T15K33.9"));
			Assert.AreEqual("15", helper.GetNumberOfPieces("T15"));
			Assert.AreEqual("", helper.GetNumberOfPieces(""));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetWeightOfGoods()
		{
			Assert.AreEqual("33.9KG", helper.GetWeightOfGoods("P15K33.9"));
			Assert.AreEqual("33.9LB", helper.GetWeightOfGoods("P15L33.9"));
			Assert.AreEqual("", helper.GetWeightOfGoods("T15A"));
			Assert.AreEqual("", helper.GetWeightOfGoods("T15"));
			Assert.AreEqual("", helper.GetWeightOfGoods(""));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFormatTime()
		{
			Assert.AreEqual("17:18", helper.FormatTime("1718"));
			Assert.AreEqual("17:185", helper.FormatTime("17185"));
			Assert.AreEqual("", helper.FormatTime("17"));
			Assert.AreEqual("", helper.FormatTime(""));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFormatDate()
		{
			Assert.AreEqual("", helper.FormatDate("", "Nov"));
			Assert.AreEqual("", helper.FormatDate("20", ""));
			Assert.AreEqual("", helper.FormatDate("", ""));

			var mock = new Mock<HelperCSharpScripts>();
			DateTime mockedCurrentDateTime = new DateTime(2009, 2, 24, 17, 18, 19);
			mock.Setup(x => x.GetCurrentDateTime).Returns(mockedCurrentDateTime);
			Assert.AreEqual("20-NoV-2008", mock.Object.FormatDate("20", "NoV"));
			Assert.AreEqual("20-Jun-2009", mock.Object.FormatDate("20", "Jun"));

			mockedCurrentDateTime = new DateTime(2009, 11, 24, 17, 18, 19);
			mock.Setup(x => x.GetCurrentDateTime).Returns(mockedCurrentDateTime);
			Assert.AreEqual("20-Feb-2010", mock.Object.FormatDate("20", "Feb"));
			Assert.AreEqual("20-Jun-2009", mock.Object.FormatDate("20", "Jun"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFormatDateTime()
		{
			Assert.AreEqual("", helper.FormatDateTime("", "", ""));

			var mock = new Mock<HelperCSharpScripts>();
			DateTime mockedCurrentDateTime = new DateTime(2009, 2, 24, 17, 18, 19);
			mock.Setup(x => x.GetCurrentDateTime).Returns(mockedCurrentDateTime);
			Assert.AreEqual("20-Nov-2008 17:18", mock.Object.FormatDateTime("20", "Nov", "1718"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetDaysToChange()
		{
			Assert.AreEqual(-1, helper.GetDaysToChange("P"));
			Assert.AreEqual(0, helper.GetDaysToChange(""));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFormatDateTimeOfDepartureOrArrival()
		{
			Assert.AreEqual("", helper.FormatDateTimeOfDepartureOrArrival("", "", "", "", ""));
			Assert.AreEqual("", helper.FormatDateTimeOfDepartureOrArrival("E", "", "", "171", ""));
			Assert.AreEqual("", helper.FormatDateTimeOfDepartureOrArrival("E", "", "AAA", "171", ""));
			Assert.AreEqual("", helper.FormatDateTimeOfDepartureOrArrival("E", "99", "Nov", "171", ""));
			Assert.AreEqual("", helper.FormatDateTimeOfDepartureOrArrival("20", "Nov", "", "9900", "P", true));

			var mock = new Mock<HelperCSharpScripts>();
			DateTime mockedCurrentDateTime = new DateTime(2009, 2, 24, 17, 18, 19);
			mock.Setup(x => x.GetCurrentDateTime).Returns(mockedCurrentDateTime);
			Assert.AreEqual("E 19-Nov-2008 17:18", mock.Object.FormatDateTimeOfDepartureOrArrival("20", "Nov", "E", "1718", "P"));
			Assert.AreEqual("19-Nov-2008 17:18", mock.Object.FormatDateTimeOfDepartureOrArrival("20", "Nov", "", "1718", "P"));
			Assert.AreEqual("2008-11-19T17:18:00", mock.Object.FormatDateTimeOfDepartureOrArrival("20", "Nov", "", "1718", "P", true));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFormatXMLStandardDateTime()
		{
			Assert.AreEqual("", helper.FormatXMLStandardDateTime("", "", ""));
			Assert.AreEqual("", helper.FormatXMLStandardDateTime("", "", "171"));
			Assert.AreEqual("", helper.FormatXMLStandardDateTime("", "AAA", "171"));
			Assert.AreEqual("", helper.FormatXMLStandardDateTime("99", "Nov", "171"));

			var mock = new Mock<HelperCSharpScripts>();
			DateTime mockedCurrentDateTime = new DateTime(2009, 2, 24, 17, 18, 19);
			mock.Setup(x => x.GetCurrentDateTime).Returns(mockedCurrentDateTime);
			Assert.AreEqual("2008-11-20T17:18:00", mock.Object.FormatXMLStandardDateTime("20", "Nov", "1718"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFormatCurrentDateTime()
		{
			var mock = new Mock<HelperCSharpScripts>();
			DateTime mockedCurrentDateTime = new DateTime(2009, 2, 24, 17, 18, 19);
			mock.Setup(x => x.GetCurrentDateTime).Returns(mockedCurrentDateTime);
			Assert.AreEqual("2009-02-24T17:18:19", mock.Object.FormatCurrentDateTime());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetOSIString()
		{
			Assert.AreEqual("aaa bbb", helper.GetOSIString("aaa", "bbb"));
			Assert.AreEqual("aaa", helper.GetOSIString("aaa", ""));
			Assert.AreEqual("bbb", helper.GetOSIString("", "bbb"));
			Assert.AreEqual("", helper.GetOSIString("", ""));

			string OSILine1 = "aaaaaaaaaa";
			string OSILine2 = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"; //140 characters
			string expected = "aaaaaaaaaa 12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789"; //130 characters
			Assert.AreEqual(expected, helper.GetOSIString(OSILine1, OSILine2));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetULDString()
		{
			Assert.AreEqual("aa, bb, cc, dd, ee", helper.GetULDString("aa", "bb", "cc", "dd", "ee"));
			Assert.AreEqual("bb, cc, dd, ee", helper.GetULDString("", "bb", "cc", "dd", "ee"));
			Assert.AreEqual("aa, bb, cc, dd", helper.GetULDString("aa", "bb", "cc", "dd", ""));
			Assert.AreEqual("aa, bb, dd, ee", helper.GetULDString("aa", "bb", "", "dd", "ee"));
			Assert.AreEqual("", helper.GetULDString("", "", "", "", ""));
		}

		HelperCSharpScripts helper = new HelperCSharpScripts();
	}
}
