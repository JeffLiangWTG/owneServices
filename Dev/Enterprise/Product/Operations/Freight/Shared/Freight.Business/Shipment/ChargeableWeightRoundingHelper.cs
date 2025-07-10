using System;
using CargoWise.Schema;
using CargoWise.Types;

using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class ChargeableWeightRoundingHelper
	{
		/// <summary>
		/// A helper function to calculate the rounded value, based on the Rounding registry item settings
		/// </summary>
		/// <returns>Rounded value</returns>

		public static decimal GetRoundedValueAir(SchemaColumn column, ZDecimal value)
		{
			AWBRounding roundingStyle = new AWBRounding();
			ChargeableWeightRoundingRegistryItem registryEntry = FreightDataRegistry.Instance.FreightChargeableWeightRoundings;

			roundingStyle.RoundingMode = registryEntry.Value[0].RoundingMode;
			roundingStyle.RoundingScale = registryEntry.Value[0].RoundingScale;

			int decimals = 0;

			int.TryParse(roundingStyle.RoundingScale, out decimals);

			var roundedValue = GetRoundedValue(roundingStyle, value);
			if (column is SchemaDecimalColumn decimalColumn
				&& !roundedValue.IsWithinSqlPrecisionAndScale(decimalColumn.Precision, decimalColumn.Scale)
				&& value.IsWithinSqlPrecisionAndScale(decimalColumn.Precision, decimalColumn.Scale))
			{
				roundedValue = DefaultNumberOfDecimals.GetRoundedValue(value, RoundingModes.Down, decimals);
			}
			return roundedValue;
		}

		#region Implementation

		static ZDecimal GetRoundedValue(AWBRounding roundingStyle, ZDecimal newValue)
		{
			ZDecimal result = newValue;

			if (roundingStyle != null)
			{
				if (roundingStyle.RoundingEnabled)
				{
					if (roundingStyle.RoundToWholeNumber)
					{
						result = roundingStyle.RoundUp ? RoundUpToWhole(newValue) : RoundDownToWhole(newValue);
					}
					else
					{
						result = roundingStyle.RoundUp ? RoundUp(newValue) : RoundDown(newValue);
					}
				}
			}
			else
			{
				result = RoundDown(newValue);
			}

			return result;
		}

		static Decimal RoundUp(Decimal weightToRound)
		{
			Decimal truncatedValue = Decimal.Truncate(weightToRound);
			Decimal addValue = weightToRound - Decimal.Truncate(weightToRound);

			if (addValue > 0)
			{
				addValue = (addValue <= 0.5M) ? 0.5M : 1M;
			}

			return truncatedValue + addValue;
		}

		static Decimal RoundDown(Decimal weightToRound)
		{
			Decimal truncatedValue = Decimal.Truncate(weightToRound);
			Decimal addValue = weightToRound - Decimal.Truncate(weightToRound);

			if (addValue > 0)
			{
				addValue = (addValue < 0.5M) ? 0M : 0.5M;
			}

			return truncatedValue + addValue;
		}

		static Decimal RoundUpToWhole(Decimal weightToRound)
		{
			Decimal truncatedValue = Decimal.Truncate(weightToRound);

			if (weightToRound > truncatedValue)
			{
				truncatedValue = truncatedValue + 1M;
			}

			return truncatedValue;
		}

		static Decimal RoundDownToWhole(Decimal weightToRound)
		{
			return Decimal.Truncate(weightToRound);
		}

		#endregion
	}
}
