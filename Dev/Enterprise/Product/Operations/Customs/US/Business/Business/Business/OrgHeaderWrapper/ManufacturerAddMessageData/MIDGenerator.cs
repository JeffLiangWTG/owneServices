using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class MIDGenerator
	{
		ZString uS_FirmName;
		ZString uS_Country;
		ZString uS_Street;
		ZString uS_City;

		public ZString GenerateMID(ZString firmName, ZString street, ZString city, ZString country)
		{
			this.uS_FirmName = firmName.Trim().ReplaceIgnoringCase("/", " ");
			this.uS_Country = country;
			this.uS_Street = street;
			this.uS_City = city;

			return GenerateMID();
		}

		ZString GenerateMID()
		{
			ZStringBuilder result = new ZStringBuilder();

			result.Append(uS_Country);

			result.Append(GetPortionFromFirmName());

			result.Append(GetPortionFromStreet());

			result.Append(GetPortionFromCity());

			return result.ToString().ToUpper();
		}

		ZString GetPortionFromFirmName()
		{
			if (uS_Country == Core.Constants.CountryCodes.Macau)
			{
				uS_FirmName = Regex.Replace(uS_FirmName, "FABRICA DE ", "", RegexOptions.IgnoreCase);
				uS_FirmName = Regex.Replace(uS_FirmName, "ARTIGOS DE VESTUARIO ", "", RegexOptions.IgnoreCase);
			}
			else if (uS_Country == Core.Constants.CountryCodes.Indonesia)
			{
				uS_FirmName = Regex.Replace(uS_FirmName, "^P[.]?T[.]? ", "", RegexOptions.IgnoreCase);
			}
			else if (uS_Country == Core.Constants.CountryCodes.Portugal)
			{
				uS_FirmName = Regex.Replace(uS_FirmName, "COMPANHIA TEXTIL ", "", RegexOptions.IgnoreCase);
			}
			else if (uS_Country == Core.Constants.CountryCodes.Russia)
			{
				uS_FirmName = Regex.Replace(uS_FirmName, "^J[.]?S[.]?C[.]? ", "", RegexOptions.IgnoreCase);
				uS_FirmName = Regex.Replace(uS_FirmName, "^O[.]?A[.]?O[.]? ", "", RegexOptions.IgnoreCase);
				uS_FirmName = Regex.Replace(uS_FirmName, "^O[.]?O[.]?O[.]? ", "", RegexOptions.IgnoreCase);
				uS_FirmName = Regex.Replace(uS_FirmName, "^Z[.]?A[.]?O[.]? ", "", RegexOptions.IgnoreCase);
			}
			ZString[] nameWords = uS_FirmName.Split(' ');

			ZString first = ZString.Empty, second = ZString.Empty;

			int index = 0;

			while (index < nameWords.Length)
			{
				int nextIndex = index + 1;

				ZString currentWord = nameWords[index].KeepAlphanumericCharacters();

				ZString wordToAttach = currentWord;

				if (currentWord.Length == 1)// A single word initial. Group of initials together (up to 3 letters) can be part of MID
				{
					wordToAttach = FindGroupOfInitialsFromIndexUpToThreeLetters(index, nameWords);

					nextIndex = index + wordToAttach.Length;
				}

				if (wordToAttach.Length > 1 && !ShouldBeIgnored(wordToAttach))
				{
					if (first.IsEmpty)
					{
						first = wordToAttach.Left(3);
					}
					else if (second.IsEmpty)
					{
						second = wordToAttach.Left(3);
					}
					else
					{
						break;
					}
				}

				index = nextIndex;
			}

			return first + second;
		}

		ZString FindGroupOfInitialsFromIndexUpToThreeLetters(int index, ZString[] nameWords)
		{
			ZString result = ZString.Empty;

			while (index < nameWords.Length && result.Length < 3)
			{
				ZString currentWord = nameWords[index].KeepAlphanumericCharacters();
				if (currentWord.Length == 1)
				{
					result += currentWord;
				}
				index++;
			}

			return result;
		}

		ZString GetPortionFromStreet()
		{
			ZString[] streetWords = uS_Street.Split(' ');

			ZInt largestNumber = 0;

			ZString largestNumberWord = ZString.Empty;

			for (int index = 0; index < streetWords.Length; index++)
			{
				ZString currentWord = streetWords[index].KeepChars("1234567890");

				if (!currentWord.IsEmpty && currentWord.IsNumbersOnlyOrEmpty)
				{
					ZInt parsed = ZInt.ParseSafe(currentWord, -1);

					if (parsed > largestNumber)
					{
						largestNumber = parsed;
						largestNumberWord = currentWord;
					}
				}
			}

			return largestNumberWord.Left(4);
		}

		ZString GetPortionFromCity()
		{
			switch (uS_Country)
			{
				case Core.Constants.CountryCodes.HongKong:
					return HongKongSityCode;
				case Core.Constants.CountryCodes.Macau:
					return MacaoSityCode;
				case Core.Constants.CountryCodes.Singapore:
					return SingaporeSityCode;
				case Core.Constants.CountryCodes.Vatican:
					return VaticanSityCode;
				case Core.Constants.CountryCodes.Monaco:
					return MonacoSityCode;
				case Core.Constants.CountryCodes.SanMarino:
					return SanMarinoSityCode;
				case Core.Constants.CountryCodes.Andorra:
					return AndorraMarinoSityCode;
				default:
					break;
			}

			ZString cityWord = ZString.Empty;
			ZString[] cityWords = uS_City.Split(' ');

			foreach (ZString city in cityWords)
			{
				if (!ShouldBeIgnored(city))
				{
					cityWord += city.KeepAlphabeticCharacters().Left(3 - cityWord.Length);
				}

				if (cityWord.Length == 3)
				{
					break;
				}
			}

			return cityWord;
		}
		const string HongKongSityCode = "HON";
		const string MacaoSityCode = "MAC";
		const string SingaporeSityCode = "SIN";
		const string VaticanSityCode = "VAT";
		const string MonacoSityCode = "MON";
		const string SanMarinoSityCode = "SAN";
		const string AndorraMarinoSityCode = "AND";

		bool ShouldBeIgnored(ZString currentWord)
		{
			return currentWord.EqualsIgnoringCase("a") ||
				currentWord.EqualsIgnoringCase("an") ||
				currentWord.EqualsIgnoringCase("and") ||
				currentWord.EqualsIgnoringCase("of") ||
				currentWord.EqualsIgnoringCase("the");
		}
	}
}
