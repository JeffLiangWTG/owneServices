using System.Text;
using CargoWise.Types;
using Enterprise.Edifact.D98A.Elements;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	public class ResponseEmailBuilder
	{
		public ResponseEmailBuilder()
		{
			HeaderString = new StringBuilder();
			BodyString = new StringBuilder();
		}

		public readonly StringBuilder HeaderString;
		public readonly StringBuilder BodyString;

		public void OutputBodyHeader(string headerTitle)
		{
			OutputBodyLine();
			OutputBodyLine(headerTitle);
			OutputBodyLine("----------------------------------------------------------------------");
		}

		public void OutputHeaderLine(string tagForLine, string lineToOutput)
		{
			if (!string.IsNullOrEmpty(lineToOutput))
			{
				HeaderString.Append(tagForLine.PadRight(15) + (tagForLine.Length != 0 ? ": " : "  ") + lineToOutput + "\r\n");
			}
		}

		public void OutputHeaderLine(string lineToOutput)
		{
			HeaderString.Append(lineToOutput + "\r\n");
		}

		public void OutputHeaderLine()
		{
			HeaderString.Append("\r\n");
		}

		public void OutputBodyLine(string lineToOutput)
		{
			BodyString.Append(lineToOutput + "\r\n");
		}

		public void OutputBodyLine()
		{
			BodyString.Append("\r\n");
		}

		public void OutputHeaderBreakLine()
		{
			HeaderString.Append("----------------------------------------------------------------------\r\n");
		}

		public void OutputMessageStatus(string messageStatusCode)
		{
			string fullText = GetProcessingIndicatorFromCode(messageStatusCode);
			int position = fullText.IndexOf(",");
			if (position != -1)
			{
				OutputHeaderLine("Message Status", "(" + messageStatusCode + ") " + fullText.Substring(0, position) + ".");
				fullText = fullText.Substring(position + 1).TrimStart();
				position = fullText.IndexOf("-");
				if (position != -1)
				{
					OutputHeaderLine("", fullText.Substring(0, position) + ".");
					OutputHeaderLine(fullText.Substring(position + 1).TrimStart() + ".");
				}
				else
				{
					OutputHeaderLine("", fullText + ".");
				}
			}
			else
			{
				OutputHeaderLine("Message Status", "(" + messageStatusCode + ") " + fullText + ".");
			}
		}

		string GetProcessingIndicatorFromCode(string processingIndicatorCode)
		{
			ZString result = new ResponseStatusList().GetDescriptionFromCode(processingIndicatorCode);
			if (result.IsEmpty)
			{
				result = "ERROR: Processing Indicator (" + processingIndicatorCode + ") Not Recognised";
			}
			return result;
		}

		public void OutputTextLiteralElements(TextLiteralElements textLiteralElements)
		{
			if (!string.IsNullOrEmpty(textLiteralElements.FreeText1))
			{
				OutputBodyLine(textLiteralElements.FreeText1);
			}
			if (!string.IsNullOrEmpty(textLiteralElements.FreeText2))
			{
				OutputBodyLine(textLiteralElements.FreeText2);
			}
			if (!string.IsNullOrEmpty(textLiteralElements.FreeText3))
			{
				OutputBodyLine(textLiteralElements.FreeText3);
			}
			if (!string.IsNullOrEmpty(textLiteralElements.FreeText4))
			{
				OutputBodyLine(textLiteralElements.FreeText4);
			}
		}

		public string GetNoteText(TextLiteralElements textLiteralElements)
		{
			StringBuilder builder = new StringBuilder();
			if (!string.IsNullOrEmpty(textLiteralElements.FreeText1))
			{
				builder.Append(textLiteralElements.FreeText1 + "\r\n");
			}
			if (!string.IsNullOrEmpty(textLiteralElements.FreeText2))
			{
				builder.Append(textLiteralElements.FreeText2 + "\r\n");
			}
			if (!string.IsNullOrEmpty(textLiteralElements.FreeText3))
			{
				builder.Append(textLiteralElements.FreeText3 + "\r\n");
			}
			if (!string.IsNullOrEmpty(textLiteralElements.FreeText4))
			{
				builder.Append(textLiteralElements.FreeText4 + "\r\n");
			}
			ZString result = builder.ToString();
			return result.TrimEnd();
		}
	}
}
