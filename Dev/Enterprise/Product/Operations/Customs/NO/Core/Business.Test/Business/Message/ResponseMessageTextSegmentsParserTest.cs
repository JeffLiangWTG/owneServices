using Enterprise.Edifact;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ResponseMessageTextSegmentsParser))]
sealed class ResponseMessageTextSegmentsParserTest  : TestCase
{
	public void TestParseInterchangeBody_WithEscapeCharFollowedByDelimiter()
	{
		const string messageOne =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"UNT+9+740428'";

		const string messageTwo =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"TDT+8++3+:::DESTINY?'UNH++WXYZ:2'" +
			"UNT+9+740428'";

		const string messageWithInterchange = "UNB+UNOA:1+NO000101:NEP+NO011701:NEP+230126:0853+23012608531967'"
			+ messageOne
			+ messageTwo
			+ "UNZ+1+23012608531967'";

		var messages = ResponseMessageTextSegmentsParser.ParseInterchangeBody(new UNOACharacterSet(), messageWithInterchange);
		AssertContainsExactElementsInAnyOrder(new[] { messageOne, messageTwo }, messages);
	}

	public void TestParseInterchangeBody_WithEmptyOrLessThanThreeCharSegment()
	{
		const string message =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"''''" +
			"NAD+1+2+test'" +
			"AB'" +
			"TDT+5+2+2+:::VALUE'" +
			"X'" +
			"UNT+9+740428'";

		var messages = ResponseMessageTextSegmentsParser.ParseInterchangeBody(new UNOACharacterSet(), message);
		AssertContainsExactElementsInExactOrder(new[] { message }, messages);
	}

	public void TestParseInterchangeBody_WithHeaderSegmentInTheValue()
	{
		const string message =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"TDT+8++3+:::DESTINY?'UNH++WXYZ:2'" +
			"UNT+9+740428'";
		const string messageWithInterchange = "UNB+UNOA:1+NO000101:NEP+NO011701:NEP+230126:0853+23012608531967'"
			+ message
			+ "UNZ+1+23012608531967'";

		var messages = ResponseMessageTextSegmentsParser.ParseInterchangeBody(new UNOACharacterSet(), messageWithInterchange);
		AssertContainsExactElementsInExactOrder(new[] { message }, messages);
	}

	public void TestParsInterchangeBody_WithMultipleHeaderNodes()
	{
		const string message =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059104'" +
			"BGM+932++137:20240826:102+32'" +
			"NAD+1+2+test'" +
			"TDT+5+2+2+:::VALUE'" +
			"UNT+9+740428'";
		var messages = ResponseMessageTextSegmentsParser.ParseInterchangeBody(new UNOACharacterSet(), message);
		AssertContainsExactElementsInExactOrder(new[] { message }, messages);
	}

	public void TestParseInterchangeBody_WithMultipleFooterNodes()
	{
		const string message =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"UNT+9+740428'" +
			"UNT+9+740555'";

		const string expectedMessage =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"UNT+9+740428'";

		var messages = ResponseMessageTextSegmentsParser.ParseInterchangeBody(new UNOACharacterSet(), message);
		AssertContainsExactElementsInExactOrder(new[] { expectedMessage }, messages);
	}

	public void TestParseInterchangeBody_WithHeaderAndFooterElementInTextAsPartOfInvalidNode()
	{
		const string message =
			"UNH+740428+CUSRES:1:902:UN:NEP+9333600952024092600059103'" +
			"BGM+932++137:20240826:102+32'" +
			"UNHAA+TDT+8++3+:::WXYZ:2'" +
			"UNTAAA+9+740428'" +
			"UNT+9+740555'";

		var messages = ResponseMessageTextSegmentsParser.ParseInterchangeBody(new UNOACharacterSet(), message);
		AssertContainsExactElementsInExactOrder(new [] { message }, messages);
	}
}
