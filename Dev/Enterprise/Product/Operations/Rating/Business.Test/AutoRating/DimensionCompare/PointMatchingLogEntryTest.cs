using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test
{
	public class PointMatchingLogEntryTest : TestCase
	{
		public void TestGetLogStringWhenPointRejected()
		{
			var logEntry = new PointMatchingLogEntry("FRT-CMB-KG-Costing KLMROY_WW", nameof(MeasureType.Weight), "Commodity", string.Empty, "GEN", "COS", true);

			AssertEquals("Matching Log", "RateLine FRT-CMB-KG-Costing KLMROY_WW will not rate  by Weight	reason: expected 'GEN' Commodity while rate is for 'COS' Commodity", logEntry.GetLogString());
		}
	}

	public class PointMatchingLogTest : RatingTestCase
	{
		public void TestGetLog()
		{
			var pointMatchingLog = new PointMatchingLog(Factory)
			{
				new PointMatchingLogEntry("ODOC-UNT-KG-Client Rate CONSIGNEE1", nameof(MeasureType.Weight), string.Empty, string.Empty, "1644", string.Empty, false),
				new PointMatchingLogEntry("ODOC-UNT-KG-Client Rate CONSIGNEE1", nameof(MeasureType.Volume), string.Empty, string.Empty, "10.633", string.Empty, false),
				new PointMatchingLogEntry("ODOC-UNT-KG-Client Rate CONSIGNEE1", nameof(MeasureType.Chargeable), string.Empty, string.Empty, "1772.167", string.Empty, false)
			};

			var actualLog = pointMatchingLog.GetLog();
			var expectedLog = @"			RateLine ODOC-UNT-KG-Client Rate CONSIGNEE1
				Job's info:
				: 1644 Weight
				: 10.633 Volume
				: 1772.167 Chargeable";

			AssertEquals("The generated log should match the expected output.", expectedLog, actualLog);
		}
	}
}
