using System;
using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.Xslt.ExtensionObjects.Legacy
{
    public class MathHelper : IMathHelper
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
            if (!decimal.TryParse(inputValue, out decimal value))
            {
                return "";
            }

            if (!int.TryParse(decimalPlaces, out int decimals))
            {
                return inputValue;
            }

            return Math.Round(value, decimals, option).ToString();
        }
    }
}
