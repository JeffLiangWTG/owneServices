using System;
using System.Collections.Generic;
using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.Xslt.ExtensionObjects.Legacy
{
    public class UnitConverter : IUnitConverter
    {
        public string Convert(string value, string fromUnit, string toUnit)
        {
            string result = string.Empty;

            if (Decimal.TryParse(value, out decimal decimalValue))
            {
                result = Convert(decimalValue, fromUnit, toUnit).ToString();
            }

            return result;
        }

        internal decimal Convert(decimal value, string fromUnit, string toUnit)
        {
            decimal result = 0M;
            fromUnit = fromUnit.ToUpper();
            toUnit = toUnit.ToUpper();

            if (fromUnit == toUnit)
            {
                result = value * 1M;
            }
            else
            {
                if (UnitConversionRates.TryGetValue(fromUnit, out decimal inputRate) && UnitConversionRates.TryGetValue(toUnit, out decimal outputRate))
                {
                    result = value * inputRate / outputRate;
                }
            }

            return Math.Round(result, 4, MidpointRounding.AwayFromZero);
        }

        static Dictionary<string, decimal> unitConversionRates;

        static Dictionary<string, decimal> UnitConversionRates
        {
            get
            {
                if (unitConversionRates == null)
                {
                    unitConversionRates = new Dictionary<string, decimal>
                    {
                        //Length - based on metre
                        { "M", 1.0m },
                        { "IN", 0.0254m },
                        { "CM", 0.01m },
                        { "FT", 0.3048m },
                        { "YD", 0.9144m },

                        //Weight - based on kilogram
                        { "KG", 1.0m },
                        { "LB", 0.453592m },
                        { "TL", 1016.0469088m },
                        { "T", 1000m },
                        { "TN", 907.18474m },
                        { "DT", 100m },
                        { "G", 0.001m },
                        { "HG", 0.1m },
                        { "KT", 1000000m },
                        { "LT", 0.373241722m },
                        { "MC", 0.0002m },
                        { "MG", 0.000001m },
                        { "OT", 0.031103477m },
                        { "OZ", 0.028349523m },

                        //Volume - based on cubicmetre
                        { "M3", 1.0m },
                        { "CF", 0.028316847m },
                        { "CI", 1.638706399858543e-5m },
                        { "CY", 0.764554857583232m },
                        { "D3", 0.001m },
                        { "L", 0.001m },
                        { "ML", 1000m },
                        { "TE", 0.127551m }
                    };
                }

                return unitConversionRates;
            }
        }
    }
}
