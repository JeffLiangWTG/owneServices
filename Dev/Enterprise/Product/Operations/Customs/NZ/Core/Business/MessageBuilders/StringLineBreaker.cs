
namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	/// <summary>
	/// Summary description for StringLineBreaker.
	/// </summary>
	public class StringLineBreaker
	{
		public StringLineBreaker(string value, bool lineBreakOnCR, string allowedCharacters)
		{
			AllowedCharacters = allowedCharacters;
			Value = StripCharacters(value, lineBreakOnCR);
		}

		public StringLineBreaker(string value, bool lineBreakOnCR)
			: this(value, lineBreakOnCR, "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.,-()/=!\"%&*;<>")
		{
		}

		public StringLineBreaker(string value)
			: this(value, false)
		{
		}

		public string GetNextLine(int maxLength)
		{
			string result = "";
			int cRIndex = Value.IndexOf("\r");
			if (cRIndex > -1 && cRIndex <= maxLength)
			{
				result = Value.Substring(0, cRIndex);
				Value = Value.Substring(cRIndex + 1);
			}
			else
			{
				if (maxLength >= Value.Length)
				{
					result = Value;
					Value = "";
				}
				else
				{
					int lastSpace = Value.LastIndexOf(" ", maxLength);
					if (lastSpace == -1)
					{
						result = Value.Substring(0, maxLength);
						Value = Value.Substring(maxLength);
					}
					else
					{
						result = Value.Substring(0, lastSpace);
						Value = Value.Substring(lastSpace + 1);
					}
				}
			}

			return result.Trim();
		}

		public bool IsEmpty()
		{
			return (string.IsNullOrEmpty(Value.Trim()));
		}
		protected internal string AllowedCharacters;
		protected internal string Value;
		protected internal string StripCharacters(string input, bool lineBreakOnCR)
		{
			string lineBreak = "\r";
			string lineBreaksToRemove = "\n";
			if (!lineBreakOnCR)
			{
				lineBreaksToRemove += lineBreak;
			}

			string result = "";
			char[] valueCharList = input.Trim().ToCharArray();
			bool lastCharWasBlank = false;

			foreach (char forChar in valueCharList)
			{
				char thisChar = forChar;

				if (lineBreakOnCR && thisChar.ToString() == lineBreak)
				{
					if (lastCharWasBlank)
					{
						result = result.Substring(0, result.Length - 1);
					}

					result += lineBreak;
					lastCharWasBlank = true;
				}
				else
				{
					if (thisChar == ' ')
					{
						if (!lastCharWasBlank)
						{
							result += " ";
						}

						lastCharWasBlank = true;
					}
					else
					{
						if (AllowedCharacters.IndexOf(thisChar) != -1)
						{
							result += thisChar.ToString();
							lastCharWasBlank = false;
						}
					}
				}
			}

			return result.Trim();
		}
	}
}
