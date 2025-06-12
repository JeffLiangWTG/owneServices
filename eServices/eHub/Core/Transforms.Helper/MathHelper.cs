using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Core.Transforms.Helper
{
	public class MathHelper
	{
		public string RoundAwayFromZero(string inputValue)
		{
			return Round(inputValue, "0", MidpointRounding.AwayFromZero);
		}

		public string RoundAwayFromZero(string inputValue, string decimalPlaces)
		{
			return Round(inputValue, decimalPlaces, MidpointRounding.AwayFromZero);
		}

		public string RoundToEven(string inputValue)
		{
			return Round(inputValue, "0", MidpointRounding.ToEven);
		}

		public string RoundToEven(string inputValue, string decimalPlaces)
		{
			return Round(inputValue, decimalPlaces, MidpointRounding.ToEven);
		}

		string Round(string inputValue, string decimalPlaces, MidpointRounding option)
		{
			decimal value;
			int decimals;

			if (!decimal.TryParse(inputValue, out value))
			{
				return "";
			}

			if (!int.TryParse(decimalPlaces, out decimals))
			{
				return inputValue;
			}

			return Math.Round(value, decimals, option).ToString();
		}
	}
}
