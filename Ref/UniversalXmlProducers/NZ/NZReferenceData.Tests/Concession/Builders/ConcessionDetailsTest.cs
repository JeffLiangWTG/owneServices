using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	class ConcessionDetailsTest
	{
		[Test]
		public void TestConstructor_CantParse()
		{
			Assert.Throws(Is.TypeOf<RefDataParseException>().And.Message.EqualTo("Can't process data line. Line string: 100025L~~Dec 31 1979 12:00AM~May  8 2002 12:00AM"), () => _ = new ConcessionDetails("100025L~~Dec 31 1979 12:00AM~May  8 2002 12:00AM"));
		}

		[Test]
		public void TestCode()
		{
			var concessionDetails = new ConcessionDetails("100242C~Dec 31 1979 12:00AM~Jun 30 1998 12:00AM");
			Assert.AreEqual("100242C", concessionDetails.Code);
		}

		[Test]
		public void TestStartDate()
		{
			var concessionDetails = new ConcessionDetails("100242C~Dec 31 1979 12:00AM~Jun 30 1998 12:00AM");
			Assert.AreEqual(DateTime.Parse("Dec 31 1979 12:00AM", CultureInfo.InvariantCulture), concessionDetails.StartDate);
		}

		[Test]
		public void TestStartDate_UnableToParse()
		{
			Assert.Throws(Is.TypeOf<RefDataParseException>().And.Message.EqualTo("Unable to parse start date. Line string: 100242C~Dec 31 19719 12:00AM~Jun 30 1998 12:00AM"), () => _ = new ConcessionDetails("100242C~Dec 31 19719 12:00AM~Jun 30 1998 12:00AM"));
		}

		[Test]
		public void TestEndDate()
		{
			var concessionDetails = new ConcessionDetails("100242C~Dec 31 1979 12:00AM~Jun 30 1998 12:00AM");
			Assert.AreEqual(DateTime.Parse("Jun 30 1998 12:00AM", CultureInfo.InvariantCulture), concessionDetails.EndDate);
		}

		[Test]
		public void TestEndDate_UnableToParse()
		{
			Assert.Throws(Is.TypeOf<RefDataParseException>().And.Message.EqualTo("Unable to parse end date. Line string: 100242C~Dec 31 1979 12:00AM~Jun 33 1998 12:00AM"), () => _ = new ConcessionDetails("100242C~Dec 31 1979 12:00AM~Jun 33 1998 12:00AM"));
		}

		[Test]
		public void TestGetConcessionDetails()
		{
			var lines = new[] { "100242C~Dec 31 1979 12:00AM~Jun 30 2025 12:00AM", "100243C~Dec 31 1979 12:00AM~Jun 30 1998 12:00AM" };
			CollectionAssert.AreEquivalent(new[] { "100242C" }, ConcessionDetails.GetConcessionDetails(lines, Mock.Of<ILogger>(), DateProvider).Select(x => x.Key), "100243C expired before ActiveDate, so not output it");
		}

		[Test]
		public void TestGetConcessionDetails_LogError_Parse()
		{
			var loggerMock = new Mock<ILogger>();
			var lines = new[] { "100242C~Dec 31 1979 12:00AM~Jun 30 2025 12:00AM", "100243C~Dec 32 1979 12:00AM~Jun 30 1998 12:00AM" };
			var linesParsed = ConcessionDetails.GetConcessionDetails(lines, loggerMock.Object, DateProvider);
			Assert.AreEqual(1, linesParsed.Count);
			loggerMock.Verify(x => x.LogError("Error during parsing, ConcessionDetails skipped: Unable to parse start date. Line string: 100243C~Dec 32 1979 12:00AM~Jun 30 1998 12:00AM"), Times.Once);
		}

		[Test]
		public void TestConcessionDetails_LogError_Duplicate()
		{
			var loggerMock = new Mock<ILogger>();
			var lines = new[] { "100242C~Dec 31 1979 12:00AM~Jun 30 2025 12:00AM", "100242C~Dec 31 1979 12:00AM~Jun 30 2026 12:00AM" };
			var linesParsed = ConcessionDetails.GetConcessionDetails(lines, loggerMock.Object, DateProvider);
			Assert.AreEqual(1, linesParsed.Count);
			loggerMock.Verify(x => x.LogError("Error during parsing, duplicate ConcessionDetails line: 100242C~Dec 31 1979 12:00AM~Jun 30 2026 12:00AM"), Times.Once);
		}

		IDateProvider DateProvider => Mock.Of<IDateProvider>(x => x.Today == new DateTime(2025, 1, 1) && x.ActiveDate == new DateTime(2020, 1, 1));
	}
}
