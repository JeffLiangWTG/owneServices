using System;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class AWBRoundingHelper
	{
		/// <summary>
		/// A helper function to calculate the rounded value, based on the AWB Rounding registry item settings
		/// </summary>
		/// <param name="awbType"></param>
		/// <param name="registryEntry">The setting location whose configuration we wish to apply</param>
		/// <param name="newValue">Value to be rounded</param>		
		/// <returns>Rounded value</returns>
		public static Decimal GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB awbType, Decimal newValue)
		{
			AWBRounding awbRoundingStyle = GetAWBRoundingStyleForType(awbType, FreightDataRegistry.Instance.AWBRoundings);
			return GetRoundedValue(awbRoundingStyle, newValue);
		}

		#region Implementation

		static Decimal GetRoundedValue(AWBRounding awbRoundingStyle, Decimal newValue)
		{
			Decimal result = newValue;

			if (awbRoundingStyle != null)
			{
				if (awbRoundingStyle.RoundingEnabled)
				{
					if (awbRoundingStyle.RoundToWholeNumber)
					{
						result = awbRoundingStyle.RoundUp ? RoundUpToWhole(newValue) : RoundDownToWhole(newValue);
					}
					else
					{
						result = awbRoundingStyle.RoundUp ? RoundUp(newValue) : RoundDown(newValue);
					}
				}
			}
			else
			{
				result = RoundDown(newValue);
			}

			return result;
		}

		static AWBRounding GetAWBRoundingStyleForType(ExportAWBHeader.TypeOfAWB awbType, AWBRoundingRegistryItem registryEntry)
		{
			switch (awbType)
			{
				case ExportAWBHeader.TypeOfAWB.AgentMaster:
					return registryEntry.Value[AWBRounding.Keys.AgentMaster];
				case ExportAWBHeader.TypeOfAWB.DirectMaster:
					return registryEntry.Value[AWBRounding.Keys.DirectMaster];
				case ExportAWBHeader.TypeOfAWB.MasterHouse:
					return registryEntry.Value[AWBRounding.Keys.MasterHouse];
				case ExportAWBHeader.TypeOfAWB.House:
					return registryEntry.Value[AWBRounding.Keys.House];
				default:
					return null;
			}
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
