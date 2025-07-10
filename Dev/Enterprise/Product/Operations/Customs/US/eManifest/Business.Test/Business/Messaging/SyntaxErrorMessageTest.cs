using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(SyntaxErrorMessage))]
	sealed class SyntaxErrorMessageTest : EDIMessageAbstractTest<SyntaxErrorMessage>
	{
		public void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.USeManifest, message.EM_ApplicationCode);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
			AssertEquals("EM_MessageType", MessageTypes.Codes.SyntaxError, message.EM_MessageType);
		}

		public void TestSegmentAndElementLevelSyntaxErrorsFormatted()
		{
			const string originalText = @"UNH+17+CUSCAR:D:03B:UN'BGM+85:::STANDARD+CWEBMAN0000004+22'DTM+132::203'LOC+60+:77'RFF+:MAN17'RFF+**:12317'NAD+CA+CWEB:172'DOC'UNT+9+17'";
			const string responseText = @"UNH+17+CONTRL:D:03B:UN'UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4'UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4'UCM+17+CUSCAR:D:03B:UN+4'UCS+5'UCD+13+2:1'UCD+12+2:1'UCS+6'UCD+12+2:1'UCS+8+15'UNT+11+17'UNZ+1+17'";
			const string expectedTextFormatted = @"Source Message Interpretation:

1 - UNH+17+CUSCAR:D:03B:UN'
2 - BGM+85:::STANDARD+CWEBMAN0000004+22'
3 - DTM+132::203'
4 - LOC+60+:77'

{0} 5 - RFF+:MAN17'
{0} Error Message: Missing,  Component Value: '', Position: L: 5; P: 2,1.
{0} Error Message: Invalid value,  Component Value: '', Position: L: 5; P: 2,1.

{0} 6 - RFF+**:12317'
{0} Error Message: Invalid value,  Component Value: '**', Position: L: 6; P: 2,1.

7 - NAD+CA+CWEB:172'

{0} 8 - DOC'
{0} Error Message: Not supported in this position,  Component Value: '', Position: L: 8.

9 - UNT+9+17'

Message Text:

UNH+17+CONTRL:D:03B:UN
UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4
UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4
UCM+17+CUSCAR:D:03B:UN+4
UCS+5
UCD+13+2:1
UCD+12+2:1
UCS+6
UCD+12+2:1
UCS+8+15
UNT+11+17
UNZ+1+17
";
			AssertFormattedMessageText(originalText, responseText, expectedTextFormatted, "17");
		}

		public void TestMessageLevelSyntaxErrorsFormatted()
		{
			const string originalText = @"UNH+18+CUSCAR:D:03B:UN'BGM+85:::STANDARD+CWEBMAN0000004+22'DTM+132::203'LOC+60+:77'RFF+:MAN17'NAD+CA+CWEB:172'DOC'UNT+7+18'";
			const string responseText = @"UNH+12+CONTRL:D:03B:UN'UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4'UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4'UCM+18+CUSCAR:D:03B:UN+4+29'UNT+5+12'";
			const string expectedTextFormatted = @"Source Message Interpretation:

1 - UNH+18+CUSCAR:D:03B:UN'
2 - BGM+85:::STANDARD+CWEBMAN0000004+22'
3 - DTM+132::203'
4 - LOC+60+:77'
5 - RFF+:MAN17'
6 - NAD+CA+CWEB:172'
7 - DOC'
8 - UNT+7+18'

{0} Error Message: Control count does not match number of instances received

Message Text:

UNH+12+CONTRL:D:03B:UN
UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4
UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4
UCM+18+CUSCAR:D:03B:UN+4+29
UNT+5+12
";
			AssertFormattedMessageText(originalText, responseText, expectedTextFormatted, "18");
		}

		void AssertFormattedMessageText(string originalText, string responseText, string expectedTextFormatted, string messageNum)
		{
			var original = GetOriginalMessage(messageNum, originalText);
			original.EM_MessageType = MessageTypes.Codes.eManifest;
			var response = GetReceivedMessage(messageNum, responseText);
			AssertMultilineASCIIEquals("Original Formatted", originalText.Replace("\'", "\r\n"), original.EM_FormattedMessageText);
			AssertMultilineASCIIEquals("Response Formatted", string.Format(expectedTextFormatted, "##"), response.EM_FormattedMessageText);
		}
	}
}
