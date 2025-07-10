using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	public static class GSTNumberValidation
	{
		[ThreadSafe]
		static readonly ZInt[] WeightFactor1 = new ZInt[] { 3, 2, 7, 6, 5, 4, 3, 2 };
		[ThreadSafe]
		static readonly ZInt[] WeightFactor2 = new ZInt[] { 7, 4, 3, 2, 5, 2, 7, 6 };

		public static bool ValidateGSTNumber(ZString gSTRaw)
		{
			var newGSTRaw = gSTRaw.Replace("-", "").Replace(" ", "");
			if (ZInt.TryParse(newGSTRaw, out ZInt number))
			{
				if ((number < 10000000) || (number > 150000000))
				{
					return false;
				}
				else
				{
					var trailing = number % 10;
					var checkDigit = GetCheckDigit(number, WeightFactor1);
					if (checkDigit >= 0 && checkDigit <= 9)
					{
						if (checkDigit == trailing)
						{
							return true;
						}
						else
						{
							return false;
						}
					}
					else
					{
						checkDigit = GetCheckDigit(number, WeightFactor2);
						if (checkDigit == 10 || checkDigit != trailing)
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
			}
			else
			{
				return false;
			}
		}

		static ZInt GetCheckDigit(ZInt number, ZInt[] weightFactor)
		{
			var tempNumber = number / 10;
			var sum = 0;
			for (int i = 7; i >= 0; i--)
			{
				var reminder = tempNumber % 10;
				tempNumber = tempNumber / 10;
				sum += reminder * weightFactor[i];
			}
			return sum % 11 == 0 ? 0 : 11 - sum % 11;
		}
	}
}
