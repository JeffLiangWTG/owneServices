using System;
using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	public static class EntryNumberCheckDigitCalculator
	{
		public static int GetCheckDigit(string filerCode, string entryNumberWithoutCheckDigit, int checkDigitAddition)
		{
			var numberRepresentation = GetNumberRepresentation(filerCode);
			numberRepresentation.AddRange(GetNumberRepresentation(entryNumberWithoutCheckDigit));

			int result = 0;

			int positionFromRight = 0;

			for (int index = numberRepresentation.Count - 1; index >= 0; index--)
			{
				positionFromRight++;
				int number = numberRepresentation[index];

				if (positionFromRight % 2 == 0)
				{
					result += number;
				}
				else
				{
					result += number * 2 > 9 ? (number * 2 + 1) % 10 : number * 2;
				}
			}

			int returnValue = (10 - (result % 10) + checkDigitAddition) % 10;
			return returnValue;
		}

		static List<int> GetNumberRepresentation(string numbersOrLetters)
		{
			var result = new List<int>();

			foreach (char oneChar in numbersOrLetters)
			{
				if (char.IsDigit(oneChar))
				{
					result.Add(int.Parse(char.ToString(oneChar)));
				}
				else if (char.IsLetter(oneChar))
				{
					result.Add(GetNumericValue(char.ToUpper(oneChar)));
				}
			}

			return result;
		}

		static int GetNumericValue(char alphabetLetter)
		{
			switch (alphabetLetter)
			{
				case 'A':
				case 'J':
					return 1;

				case 'B':
				case 'K':
				case 'S':
					return 2;

				case 'C':
				case 'L':
				case 'T':
					return 3;

				case 'D':
				case 'M':
				case 'U':
					return 4;

				case 'E':
				case 'N':
				case 'V':
					return 5;

				case 'F':
				case 'O':
				case 'W':
					return 6;

				case 'G':
				case 'P':
				case 'X':
					return 7;

				case 'H':
				case 'Q':
				case 'Y':
					return 8;

				case 'I':
				case 'R':
				case 'Z':
					return 9;

				default:
					throw new ArgumentException("Passed Alphabetic letter is invalid");
			}
		}
	}
}
