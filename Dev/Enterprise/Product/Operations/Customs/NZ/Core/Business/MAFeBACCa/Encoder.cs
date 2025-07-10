using System;
using System.Text;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	static class Base64Encoder
	{
		public static string Base64Encode(string content)
		{
			return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content), Base64FormattingOptions.InsertLineBreaks);
		}

		public static string Base64Decode(string content)
		{
			byte[] utf8Characters = Convert.FromBase64String(content);
			return Base64Decode(utf8Characters);
		}

		public static string Base64Decode(byte[] utf8Characters)
		{
			return Base64Decode(utf8Characters, false);
		}

		public static string Base64Decode(byte[] utf8Characters, bool stripUTF8PrefixChar)
		{
			Decoder utf8Decoder = new UTF8Encoding(false).GetDecoder();

			int unicodeCharacterCount = utf8Decoder.GetCharCount(utf8Characters, 0, utf8Characters.Length);
			char[] unicodeCharaters = new char[unicodeCharacterCount];

			utf8Decoder.GetChars(utf8Characters, 0, utf8Characters.Length, unicodeCharaters, 0);
			if (stripUTF8PrefixChar)
			{
				return new String(unicodeCharaters, 1, unicodeCharaters.Length - 1);
			}
			else
			{
				return new String(unicodeCharaters);
			}
		}
	}
}
