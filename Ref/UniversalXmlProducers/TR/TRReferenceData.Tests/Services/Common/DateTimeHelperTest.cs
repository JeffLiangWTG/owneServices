using System;
using CargoWise.RefDbRepo.TRReferenceData.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Tests
{
    public class DateTimeHelperTest
    {

		[Test]
		public void TestParseDate_WithValidDateString_ReturnsDateTime()
		{
			string dateAsString = "2022-02-20";
			var expected = new DateTime(2022, 2, 20);

			DateTime? result = DateTimeHelper.ParseDate(dateAsString);

			Assert.That(expected, Is.EqualTo(result));
		}

		[Test]
		public void TestParseDate_WithNull_ReturnsNull()
		{
			Assert.That(DateTimeHelper.ParseDate(null), Is.Null);
			Assert.That(DateTimeHelper.ParseDate(""), Is.Null);
		}

		[Test]
		public void TestParseDate_WithInvalidDateString_ThrowsFormatException()
		{
			string invalidDate = "invalid-date";

			var ex = Assert.Throws<FormatException>(() => DateTimeHelper.ParseDate(invalidDate));
			Assert.That(ex.Message, Does.Contain("Parse DateTime Error"));
		}

		[Test]
		public void TestParseDate_WithDifferentDateFormats_ReturnsCorrectDate()
		{
			string[] validDates = { "27 March 2024", "March 27, 2024" };

			foreach (var date in validDates)
			{
				DateTime? result = DateTimeHelper.ParseDate(date);
				var expected = new DateTime(2024, 3, 27);

				Assert.That(expected, Is.EqualTo(result));
			}
		}
	}
}
