using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Enterprise.Edifact.D96B.Elements;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	class ErrorInformation
	{
		public readonly int ElementNumber;
		public readonly int OriginalSegmentNumber;
		public readonly SyntaxErrorCodedList SegmentErrorNumber;
		public readonly int SubElement;
		public readonly SyntaxErrorCodedList UcdErrorNumber;
		public string OutputSegment { get; private set; }
		public string OutputElement { get; private set; }
		public string OutputSubElement { get; private set; }

		public ErrorInformation(string originalSegmentNubmer, SyntaxErrorCodedList segmentErrorNumber, string elementNumber, string subElement, SyntaxErrorCodedList ucdErrorNumber)
		{
			if (!int.TryParse(originalSegmentNubmer, out OriginalSegmentNumber))
			{ }
			this.SegmentErrorNumber = segmentErrorNumber;
			if (!int.TryParse(elementNumber, out ElementNumber))
			{ }
			if (!int.TryParse(subElement, out SubElement))
			{ }
			this.UcdErrorNumber = ucdErrorNumber;
		}

		public string ErrorTextFromNumbers()
		{
			var error1 = "";
			var error2 = "";
			if (SegmentErrorNumber != null && !String.IsNullOrEmpty(SegmentErrorNumber.ToString()))
			{
				error1 = ErrorTextFromNumber(int.Parse(SegmentErrorNumber.ToString(), CultureInfo.InvariantCulture));
			}
			if (UcdErrorNumber != null && !String.IsNullOrEmpty(UcdErrorNumber.ToString()))
			{
				error2 = ErrorTextFromNumber(int.Parse(UcdErrorNumber.ToString(), CultureInfo.InvariantCulture));
			}
			return error2 != error1 ? (error1 + " " + error2).Trim() : error1;
		}

		internal void ExtractValuesFromOriginalOutgoingMessage(ZAMessage outGoingMessage, string[] originalSegments)
		{
			if (originalSegments.Length > OriginalSegmentNumber)
			{
				var originalSegment = originalSegments[OriginalSegmentNumber - 1];
				OutputSegment = originalSegment;
				if (ElementNumber > 0)
				{
					var elements = Regex.Split(originalSegment, @"\" + outGoingMessage.CharacterSet.ElementDelimiter);
					if (elements.Length > ElementNumber)
					{
						OutputElement = elements[ElementNumber - 1];
						if (SubElement > 0)
						{
							var subElements = Regex.Split(OutputElement, @"\" + outGoingMessage.CharacterSet.SubElementDelimiter);
							if (subElements.Length > SubElement)
							{
								OutputSubElement = subElements[SubElement - 1];
							}
						}
					}
				}
			}
		}

		string ErrorTextFromNumber(int number)
		{
			switch (number)
			{
				case 2:
					return "Syntax version or level not supported";
				case 7:
					return "Interchange recipient not actual recipient";
				case 12:
					return "Invalid value";
				case 13:
					return "Missing";
				case 14:
					return "Value not supported in this position";
				case 15:
					return "Not supported in this position";
				case 16:
					return "Too many constituents";
				case 17:
					return "No agreement";
				case 18:
					return "Unspecified error";
				case 19:
					return "Invalid decimal notation";
				case 20:
					return "Character invalid as service character";
				case 21:
					return "Invalid character(s)";
				case 22:
					return "Invalid service character(s)";
				case 23:
					return "Unknown Interchange sender";
				case 24:
					return "Too old";
				case 25:
					return "Test indicator not supported";
				case 26:
					return "Duplicate detected";
				case 27:
					return "Security function not supported";
				case 28:
					return "References do not match";
				case 29:
					return "Control count does not match number of instances received";
				case 30:
					return "Groups and messages/packages mixed";
				case 31:
					return "More than one message type in group";
				case 32:
					return "Lower level empty";
				case 33:
					return "Invalid occurrence outside message, package, or group";
				case 34:
					return "Nesting indicator not allowed";
				case 35:
					return "Too many data element or segment repetitions";
				case 36:
					return "Too many segment group repetitions";
				case 37:
					return "Invalid type of character(s)";
				case 38:
					return "Missing digit in front of decimal sign";
				case 39:
					return "Data element too long";
				case 40:
					return "Data element too short";
				case 41:
					return "Permanent communication network error";
				case 42:
					return "Temporary communication network error";
				case 43:
					return "Unknown interchange recipient";
				case 45:
					return "Trailing separator";
				case 46:
					return "Character set not supported";
				case 47:
					return "Envelope functionality not supported";
				case 48:
					return "Dependency condition violated";
			}
			return number.ToString(CultureInfo.InvariantCulture);
		}
	}
}
