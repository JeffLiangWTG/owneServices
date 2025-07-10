using System;
using System.Collections;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class OverlappingCalculatorFixture
	{
		[Test]
		public void GetEndDate()
		{
			var expireTime1 = new DateTime(2020, 1, 1);
			var expireTime2 = new DateTime(2021, 1, 1).ToUTCDateTimeOffset();
			var calculator = new OverlappingCalculator(false);
			Assert.AreEqual(expireTime1, calculator.GetEndDate(expireTime1));
			Assert.AreEqual(expireTime2, calculator.GetEndDate(expireTime2));

			calculator = new OverlappingCalculator(true);
			Assert.AreEqual(expireTime1.AddMinutes(-1), calculator.GetEndDate(expireTime1));
			Assert.AreEqual(expireTime2.AddMinutes(-1), calculator.GetEndDate(expireTime2));
		}

		[TestCaseSource(nameof(IsOverlappedTestCases))]
		public void IsOverlapped(DateTimeRange datetimeRange1, DateTimeRange dateTimeRange2, bool endDateExclusive, bool isOverlapped)
		{
			var calculator = new OverlappingCalculator(endDateExclusive);
			Assert.AreEqual(isOverlapped, calculator.IsOverlapped([datetimeRange1, dateTimeRange2]));
			Assert.AreEqual(isOverlapped, calculator.IsOverlapped([datetimeRange1], dateTimeRange2));
		}

		static IEnumerable IsOverlappedTestCases
		{
			get
			{
				yield return new TestCaseData(new DateTimeRange(new DateTime(1990, 1, 1), new DateTime(2000, 1, 1)), new DateTimeRange(new DateTime(2000, 1, 1), new DateTime(2010, 1, 1)), false, false)
				{
					TestName = "{m}_IsOverlapped_1"
				};
				yield return new TestCaseData(new DateTimeRange(new DateTime(1990, 1, 1), new DateTime(2000, 1, 1)), new DateTimeRange(new DateTime(2000, 1, 1), new DateTime(2010, 1, 1)), true, true)
				{
					TestName = "{m}_IsOverlapped_2"
				};
				yield return new TestCaseData(new DateTimeRange(new DateTime(1990, 1, 1), new DateTime(2000, 1, 1)), new DateTimeRange(new DateTime(1995, 1, 1), new DateTime(2005, 1, 1)), false, true)
				{
					TestName = "{m}_IsOverlapped_3"
				};
				yield return new TestCaseData(new DateTimeRange(new DateTime(1990, 1, 1), new DateTime(2000, 1, 1)), new DateTimeRange(new DateTime(1995, 1, 1), new DateTime(2005, 1, 1)), true, true)
				{
					TestName = "{m}_IsOverlapped_4"
				};
			}
		}
	}
}
