using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class EstablishmentCodeValidator : ValidationProvider
	{
		public EstablishmentCodeValidator()
		{
		}

		public EstablishmentCodeValidator(IFactoryProvider factoryProvider) : base(factoryProvider)
		{
		}
		protected internal bool CodeIsNNNNA(ZString code)
		{
			bool result = false;
			if (code.Length == 5)
			{
				result = (Char.IsDigit(code[0]) && Char.IsDigit(code[1]) && Char.IsDigit(code[2]) && Char.IsDigit(code[3]) && Char.IsLetter(code[4]));
			}
			return result;
		}

		protected internal bool CodeIsANNNA(ZString code)
		{
			bool result = false;
			if (code.Length == 5)
			{
				result = (Char.IsLetter(code[0]) && Char.IsDigit(code[1]) && Char.IsDigit(code[2]) && Char.IsDigit(code[3]) && Char.IsLetter(code[4]));
			}
			return result;
		}

		protected internal bool CodeIsAANNA(ZString code)
		{
			bool result = false;
			if (code.Length == 5)
			{
				result = (Char.IsLetter(code[0]) && Char.IsLetter(code[1]) && Char.IsDigit(code[2]) && Char.IsDigit(code[3]) && Char.IsLetter(code[4]));
			}
			return result;
		}

		protected internal bool EstablishmentCodeValidNNNNA(ZString code, out char correctCheckSum)
		{
			int value1 = int.Parse(code[0].ToString());
			int value2 = int.Parse(code[1].ToString());
			int value3 = int.Parse(code[2].ToString());
			int value4 = int.Parse(code[3].ToString());
			correctCheckSum = GetCheckSum(value1, value2, value3, value4);
			return correctCheckSum == code[4];
		}

		protected internal bool EstablishmentCodeValidANNNA(ZString code, out char correctCheckSum)
		{
			int value1 = GetANNNAValueForChar(code[0]);
			int value2 = int.Parse(code[1].ToString());
			int value3 = int.Parse(code[2].ToString());
			int value4 = int.Parse(code[3].ToString());
			correctCheckSum = GetCheckSum(value1, value2, value3, value4);
			return correctCheckSum == code[4];
		}

		protected internal bool EstablishmentCodeValidAANNA(ZString code, out char correctCheckSum)
		{
			int value1 = GetAANNAValueForChar(code[0]);
			int value2 = GetAANNAValueForChar(code[1]);
			int value3 = int.Parse(code[2].ToString());
			int value4 = int.Parse(code[3].ToString());
			correctCheckSum = GetCheckSum(value1, value2, value3, value4);
			return correctCheckSum == code[4];
		}

		protected char GetCheckSum(int value1, int value2, int value3, int value4)
		{
			int weightedValue = ((value1 * 10) + (value2 * 9) + (value3 * 8) + (value4 * 7));
			weightedValue %= 11;
			weightedValue++;
			return GetChecksumChar(weightedValue);
		}

		protected int GetANNNAValueForChar(char c)
		{
			switch (c)
			{
				case 'A':
					return 11;
				case 'B':
					return 12;
				case 'C':
					return 13;
				case 'D':
					return 14;
				case 'E':
					return 15;
				case 'F':
					return 16;
				case 'G':
					return 17;
				case 'H':
					return 18;
				case 'I':
					return 19;
				case 'J':
					return 20;
				case 'K':
					return 21;
				case 'L':
					return 22;
				case 'M':
					return 23;
				case 'N':
					return 24;
				case 'O':
					return 25;
				case 'P':
					return 26;
				case 'Q':
					return 27;
				case 'R':
					return 28;
				case 'S':
					return 29;
				case 'T':
					return 30;
				case 'U':
					return 31;
				case 'V':
					return 32;
				case 'W':
					return 33;
				case 'X':
					return 34;
				case 'Y':
					return 35;
				case 'Z':
					return 36;
			}
			return -1;
		}

		protected int GetAANNAValueForChar(char c)
		{
			switch (c)
			{
				case 'A':
					return 1;
				case 'B':
					return 2;
				case 'C':
					return 3;
				case 'D':
					return 4;
				case 'E':
					return 5;
				case 'F':
					return 6;
				case 'G':
					return 7;
				case 'H':
					return 8;
				case 'I':
					return 9;
				case 'J':
					return 0;
				case 'K':
					return 1;
				case 'L':
					return 2;
				case 'M':
					return 3;
				case 'N':
					return 4;
				case 'O':
					return 5;
				case 'P':
					return 6;
				case 'Q':
					return 7;
				case 'R':
					return 8;
				case 'S':
					return 9;
				case 'T':
					return 0;
				case 'U':
					return 1;
				case 'V':
					return 2;
				case 'W':
					return 3;
				case 'X':
					return 4;
				case 'Y':
					return 5;
				case 'Z':
					return 6;
			}
			return -1;
		}

		protected char GetChecksumChar(int value)
		{
			switch (value)
			{
				case 1:
					return 'A';
				case 2:
					return 'B';
				case 3:
					return 'C';
				case 4:
					return 'D';
				case 5:
					return 'E';
				case 6:
					return 'H';
				case 7:
					return 'J';
				case 8:
					return 'K';
				case 9:
					return 'M';
				case 10:
					return 'N';
				case 11:
					return 'P';
			}
			return 'Z';
		}
	}
}
