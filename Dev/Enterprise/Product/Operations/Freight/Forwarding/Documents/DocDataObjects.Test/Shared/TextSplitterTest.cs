using System;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.Shared
{
	sealed class TextSplitterTest : TestCase
	{
		public void TestSplitText_Empty()
		{
			var res = TextSplitter.Split(null, 10);
			AssertEquals(Array.Empty<string>(), res);

			res = TextSplitter.Split(string.Empty, 10);
			AssertEquals(Array.Empty<string>(), res);

			res = TextSplitter.Split("     ", 10);
			AssertEquals(Array.Empty<string>(), res);

			res = TextSplitter.Split("Lorem ipsum dolor sit amet", 0);
			AssertEquals(Array.Empty<string>(), res);
		}

		public void TestSplitText_SingleLine()
		{
			const string text = "Lorem ipsum dolor sit amet.";

			var res = TextSplitter.Split(text, 10);

			AssertArrayEqualsByElements("split text", new[]
			{
				"Lorem",
				"ipsum",
				"dolor sit",
				"amet.",
			}, res);
		}

		public void TestSplitText_Multiline()
		{
			const string text =
@"By submitting or cancelling this eBooking you confirm that you have read, understood and agree to all the terms and conditions for this Airline available here:
https://lufthansa-cargo.com/rate-and-price-conditions
https://lufthansa-cargo.com/general-terms-and-conditions-of-carriage-of-cargo";

			var res = TextSplitter.Split(text, 90);

			AssertArrayEqualsByElements("split text", new[]
			{
				"By submitting or cancelling this eBooking you confirm that you have read, understood and",
				"agree to all the terms and conditions for this Airline available here:",
				"https://lufthansa-cargo.com/rate-and-price-conditions",
				"https://lufthansa-cargo.com/general-terms-and-conditions-of-carriage-of-cargo",
			}, res);
		}
	}
}
