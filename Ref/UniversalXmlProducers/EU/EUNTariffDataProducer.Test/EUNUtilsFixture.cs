using System;
using System.Globalization;
using Moq;
using NPOI.SS.UserModel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class EUNUtilsFixture
	{
		[Test]
		public void TestNormalizeExportTariffCode()
		{
			Assert.That(EUNUtils.NormalizeExportTariffCode("01"), Is.EqualTo("01000000"));
			Assert.That(EUNUtils.NormalizeExportTariffCode("0123456789"), Is.EqualTo("01234567"));
		}

		[Test]
		public void GetFromOADate()
		{
			var minDateStartTime = new DateTime(1900, 01, 01, 00, 00, 00);
			var minDateEndTime = new DateTime(1900, 01, 01, 23, 59, 00);
			var maxDateTime = new DateTime(2079, 06, 06, 23, 59, 00);

			Assert.Multiple(() =>
			{
				var parsedDate = EUNUtils.GetFromOADate(null);
				Assert.AreEqual(minDateStartTime, parsedDate, "null cell");

				var column = new Mock<ICell>();
				column.Setup(x => x.CellType).Returns(CellType.Numeric);
				column.Setup(x => x.NumericCellValue).Returns(0);
				parsedDate = EUNUtils.GetFromOADate(column.Object);
				Assert.AreEqual(minDateStartTime, parsedDate, "cell with value 0 as startDate");

				parsedDate = EUNUtils.GetFromOADate(column.Object, isStartDate: false);
				Assert.AreEqual(maxDateTime, parsedDate, "cell with value 0 as endDate");

				column.Setup(x => x.NumericCellValue).Returns(-10);
				parsedDate = EUNUtils.GetFromOADate(column.Object);
				Assert.AreEqual(minDateStartTime, parsedDate, "cell with negative value as startDate");

				column.Setup(x => x.NumericCellValue).Returns(-10);
				parsedDate = EUNUtils.GetFromOADate(column.Object, isStartDate: false);
				Assert.AreEqual(minDateEndTime, parsedDate, "cell with negative value as endDate");

				column.Setup(x => x.NumericCellValue).Returns(26299);
				parsedDate = EUNUtils.GetFromOADate(column.Object);
				Assert.AreEqual(new DateTime(1972, 1, 1), parsedDate, "cell with value 26299 as startDate");

				column.Setup(x => x.NumericCellValue).Returns(26299);
				parsedDate = EUNUtils.GetFromOADate(column.Object, isStartDate: false);
				Assert.AreEqual(new DateTime(1972, 01, 01, 23, 59, 00), parsedDate, "cell with value 26299 as endDate");

				column.Setup(x => x.CellType).Returns(CellType.String);
				column.Setup(x => x.StringCellValue).Returns("31-12-1899");
				parsedDate = EUNUtils.GetFromOADate(column.Object);
				Assert.AreEqual(minDateStartTime, parsedDate, "cell with value under minDateTime as startDate");

				column.Setup(x => x.CellType).Returns(CellType.String);
				column.Setup(x => x.StringCellValue).Returns("31-12-1899");
				parsedDate = EUNUtils.GetFromOADate(column.Object, isStartDate: false);
				Assert.AreEqual(minDateEndTime, parsedDate, "cell with value under minDateTime as endDate");

				column.Setup(x => x.StringCellValue).Returns("31-12-2079");
				parsedDate = EUNUtils.GetFromOADate(column.Object);
				Assert.AreEqual(maxDateTime, parsedDate, "cell with value above maxDateTime as startDate");

				column.Setup(x => x.StringCellValue).Returns("31-12-2079");
				parsedDate = EUNUtils.GetFromOADate(column.Object, isStartDate: false);
				Assert.AreEqual(maxDateTime, parsedDate, "cell with value above maxDateTime as endDate");

				column.Setup(x => x.StringCellValue).Returns("01-01-2023");
				parsedDate = EUNUtils.GetFromOADate(column.Object);
				Assert.AreEqual(new DateTime(2023, 1, 1), parsedDate, "cell with value 01-01-2023 as startDate");

				column.Setup(x => x.StringCellValue).Returns("01-01-2023");
				parsedDate = EUNUtils.GetFromOADate(column.Object, isStartDate: false);
				Assert.AreEqual(new DateTime(2023, 01, 01, 23, 59, 00), parsedDate, "cell with value 01-01-2023 as endDate");

				column.Setup(x => x.StringCellValue).Returns("aaa");
				try
				{
					_ = EUNUtils.GetFromOADate(column.Object);
					Assert.Fail("This should throw exception");
				}
				catch (Exception ex)
				{
					Assert.AreEqual(ex.Message, "Invalid date format was present expected:[dd-MM-yyyy] got:[aaa]", "cell with invalid data type as startDate");
				}
				Assert.Throws<ArgumentException>(() => EUNUtils.GetFromOADate(column.Object, isStartDate: false), "cell with invalid data type as endDate");

				column.Setup(x => x.StringCellValue).Returns("12-31-2023");
				try
				{
					_ = EUNUtils.GetFromOADate(column.Object);
					Assert.Fail("This should throw exception");
				}
				catch (Exception ex)
				{
					Assert.AreEqual(ex.Message, "Invalid date format was present expected:[dd-MM-yyyy] got:[12-31-2023]", "cell with invalid data type as startDate");
				}
			});
		}

		[Test]
		public void GetFromOADateForEnUsCulture()
		{
			var column = new Mock<ICell>();
			column.Setup(x => x.CellType).Returns(CellType.String);
			column.Setup(x => x.StringCellValue).Returns("31-12-1977");

			var savedCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
				var expectedDate = new DateTime(1977, 12, 31);
				var parsedDate = EUNUtils.GetFromOADate(column.Object);

				Assert.AreEqual(expectedDate, parsedDate);
			}
			finally
			{
				CultureInfo.CurrentCulture = savedCulture;
			}
		}

		[Test]
		public void TestGetParsedDateTime_USCulture()
		{
			var savedCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
				var expectedDate = new DateTime(1977, 12, 31);
				var defaultDate = new DateTime(2023, 01, 01);
				var parsedDate = EUNUtils.GetParsedDateTime("31-12-1977");

				Assert.AreEqual(expectedDate, parsedDate, "valid date");

				Assert.Throws<ArgumentException>(() => EUNUtils.GetParsedDateTime("invalid"), "US culture - invalid string");

				Assert.Throws<ArgumentException>(() => EUNUtils.GetParsedDateTime("12-31-1977"), "invalid date");
			}
			finally
			{
				CultureInfo.CurrentCulture = savedCulture;
			}
		}

		[Test]
		public void TestGetParsedDateTime_DECulture()
		{
			var savedCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = new CultureInfo("de-DE", false);
				var expectedDate = new DateTime(1977, 12, 31);
				var defaultDate = new DateTime(2023, 01, 01);
				var parsedDate = EUNUtils.GetParsedDateTime("31-12-1977");

				Assert.AreEqual(expectedDate, parsedDate, "valid date");

				Assert.Throws<ArgumentException>(() => EUNUtils.GetParsedDateTime("invalid"), "US culture - invalid string");

				Assert.Throws<ArgumentException>(() => EUNUtils.GetParsedDateTime("12-31-1977"), "invalid date");

			}
			finally
			{
				CultureInfo.CurrentCulture = savedCulture;
			}
		}

		[Test]
		public void NormalizeTariffCode_WhenTariffCodeIsLongerThan10Chars()
		{
			var tariffCode = "0123456000000000";
			var normalizedTariffCode = EUNUtils.NormalizeTariffCode(tariffCode);

			Assert.That(normalizedTariffCode, Is.EqualTo("0123456000000000"));
		}
	}
}
