using System;
using Enterprise.Edifact;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class EdifactDateParserTest : TestCase
	{
		public void TestD95B()
		{
			GenericTest<Edifact.D95B.Segments.DTMSegment>(EdifactDateParser.GetDate);
		}

		public void TestD99A()
		{
			GenericTest<Edifact.D99A.Segments.DTMSegment>(EdifactDateParser.GetDate);
		}

		#region Implementation

		void GenericTest<DTMSegmentT>(Converter<DTMSegmentT, DateTime> getDate)
			where DTMSegmentT : Edifact.Auto.Segment, new()
		{
			AssertDate(getDate, "DTM+137:20060425093021:204", new DateTime(2006, 04, 25, 09, 30, 21));
			AssertDate(getDate, "DTM+137:200404250915:203", new DateTime(2004, 04, 25, 09, 15, 00));

			AssertDate(getDate, "DTM+137:20060425093021:203", new DateTime(2006, 04, 25, 09, 30, 00));
			AssertDate(getDate, "DTM+137:200404250915:204", new DateTime(2004, 04, 25, 09, 15, 00));

			AssertDateFaulure(getDate, "DTM+137:2006:203", "Unable to parse the date in a DTM segment.\r\n'2006' does not fit the specified pattern (CCYYMMDDHHMM).");

			AssertDateFaulure(getDate, "DTM+137:0000:XXX", "Unrecognised date format (XXX)");
		}

		static void AssertDate<DTMSegmentT>(Converter<DTMSegmentT, DateTime> getDate, string segmentText, DateTime expectedDate)
			where DTMSegmentT : Edifact.Auto.Segment, new()
		{
			AssertEquals(string.Format("parsing '{0}'", segmentText), expectedDate, getDate(Parse<DTMSegmentT>(segmentText)));
		}

		static void AssertDateFaulure<DTMSegmentT>(Converter<DTMSegmentT, DateTime> getDate, string segmentText, string exceptionMessage)
			where DTMSegmentT : Edifact.Auto.Segment, new()
		{
			DTMSegmentT segment = Parse<DTMSegmentT>(segmentText);

			try
			{
				getDate(segment);
				Fail(string.Format("Expected an InvalidFormatException when extracting the date from '{0}'", segmentText));
			}
			catch (InvalidFormatException ex)
			{
				AssertEquals(string.Format("Expected exception message when extracting the date from '{0}'", segmentText), exceptionMessage, ex.Message);
			}
		}

		static DTMSegmentT Parse<DTMSegmentT>(string segmentText)
			where DTMSegmentT : Edifact.Auto.Segment, new()
		{
			DTMSegmentT result = new DTMSegmentT();
			result.Parse(new UNOACharacterSet(), segmentText);
			return result;
		}

		#endregion
	}
}
