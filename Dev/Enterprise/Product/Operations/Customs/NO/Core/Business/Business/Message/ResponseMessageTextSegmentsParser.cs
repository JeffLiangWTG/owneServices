using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Edifact;

namespace Enterprise.Customs.NO.Business;

static class ResponseMessageTextSegmentsParser
{
	public static IEnumerable<string> ParseInterchangeBody(UNOACharacterSet characterSet, string interchangeText)
	{
		var segmentDelimiter = characterSet.SegmentDelimiterChar;
		var escapeCharacter = characterSet.EscapeCharacterChar;

		var messageSegments = new List<string>();
		var interchangeTextAsSpan = interchangeText.AsSpan();

		int startingPosition = 0, currentHeaderSegmentPosition = -1;
		var messageTextBuilder = new StringBuilder();

		for (var currentPosition = 0; currentPosition < interchangeTextAsSpan.Length; currentPosition++)
		{
			var currentCharacter = interchangeTextAsSpan[currentPosition];

			if (currentPosition == startingPosition && IsSpaceNewLineOrCarriageReturn(currentCharacter))
			{
				startingPosition++;
				continue;
			}

			if (currentCharacter == escapeCharacter)
			{
				currentPosition++;
				continue;
			}

			if (currentCharacter != segmentDelimiter)
			{
				continue;
			}

			var sliceLength = currentPosition - startingPosition;
			var segmentLine = interchangeTextAsSpan.Slice(startingPosition, sliceLength);
			var hasHeaderPosition = HasHeaderPosition();

			if (hasHeaderPosition || segmentLine.StartsWith(HeaderSegmentAsSpan, StringComparison.OrdinalIgnoreCase))
			{
				currentHeaderSegmentPosition = !hasHeaderPosition ? currentPosition : currentHeaderSegmentPosition;
				messageTextBuilder
					.Append(segmentLine.ToArray())
					.Append(segmentDelimiter);
			}

			if (hasHeaderPosition && segmentLine.StartsWith(FooterSegmentAsSpan, StringComparison.OrdinalIgnoreCase))
			{
				messageSegments.Add(messageTextBuilder.ToString());
				messageTextBuilder.Clear();
				currentHeaderSegmentPosition = -1;
			}

			startingPosition = currentPosition + 1;
		}

		if (HasHeaderPosition() && startingPosition < interchangeTextAsSpan.Length)
		{
			var remainingBody = interchangeTextAsSpan.Slice(startingPosition);
			messageSegments.Add(remainingBody.ToString());
		}

		return messageSegments;

		bool HasHeaderPosition() => currentHeaderSegmentPosition != -1;
	}

	static ReadOnlySpan<char> HeaderSegmentAsSpan => HeaderSegmentCode.AsSpan();

	static ReadOnlySpan<char> FooterSegmentAsSpan => FooterSegmentCode.AsSpan();

	static bool IsSpaceNewLineOrCarriageReturn(char character)
	{
		return character == ' ' || character == '\n' || character == '\r';
	}

	const string HeaderSegmentCode = "UNH+";
	const string FooterSegmentCode = "UNT+";
}
