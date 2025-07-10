using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ExceptedQuantityUtilities
	{
		public enum UNDGPackType { SingleUNDGPack, MultiUNDGPack }

		public enum ExceptedQuantityMeasurementType { Weight, Volume }

		public static bool IsSubstancePermittedInLimitedQuantities(UNDGSubstance substance)
		{
			if (substance == null)
			{
				return false;
			}

			return ExceptedQuantityCodes.Contains(substance.DG_ExceptedQuantityCode);
		}

		public static IReadOnlyCollection<ZString> ExceptedQuantityCodes => exceptedQuantityCodes ?? (exceptedQuantityCodes = new ZString[]
		{
			UNDGSubstanceLookups.ExceptedQuantity.Code.E1,
			UNDGSubstanceLookups.ExceptedQuantity.Code.E2,
			UNDGSubstanceLookups.ExceptedQuantity.Code.E3,
			UNDGSubstanceLookups.ExceptedQuantity.Code.E4,
			UNDGSubstanceLookups.ExceptedQuantity.Code.E5
		});

		[ThreadStatic]
		static IReadOnlyCollection<ZString> exceptedQuantityCodes;

		public static IReadOnlyDictionary<string, decimal> MaximumInnerQuantityDictionary => maximumInnerQuantityDictionary ?? (maximumInnerQuantityDictionary = new Dictionary<string, decimal>
		{
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E1, 0.03M },
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E2, 0.03M },
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E3, 0.03M },
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E4, 0.001M },
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E5, 0.001M }
		});

		[ThreadStatic]
		static Dictionary<string, decimal> maximumInnerQuantityDictionary;

		public static IReadOnlyDictionary<string, decimal> MaximumOuterQuantityDictionary => maximumOuterQuantityDictionary ?? (maximumOuterQuantityDictionary = new Dictionary<string, decimal>
		{
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E1, 1M },
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E2, 0.5M },
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E3, 0.3M },
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E4, 0.5M },
			{ UNDGSubstanceLookups.ExceptedQuantity.Code.E5, 0.3M }
		});

		[ThreadStatic]
		static Dictionary<string, decimal> maximumOuterQuantityDictionary;

		public static bool DoesValueExceedMaximumNetQuantityAllowedPerMultiUNDGPack(string substanceExceptedCode, decimal unitValue)
		{
			if (MaximumInnerQuantityDictionary.TryGetValue(substanceExceptedCode, out decimal maximumValue))
			{
				return unitValue > maximumValue;
			}
			else
			{
				return false;
			}
		}

		public static bool DoesValueExceedMaximumNetQuantityAllowedPerSingleUNDGPack(string substanceExceptedCode, decimal unitValue)
		{
			if (MaximumOuterQuantityDictionary.TryGetValue(substanceExceptedCode, out decimal maximumValue))
			{
				return unitValue > maximumValue;
			}
			else
			{
				return false;
			}
		}

		public static string GetMostRestrictiveExceptedQuantityCodeOfSubstances(IEnumerable<UNDGSubstance> undgSubstances)
		{
			if (undgSubstances == null)
			{
				return string.Empty;
			}

			return undgSubstances
				.Where(substance => substance != null)
				.OrderByDescending(s => ExceptedQuantityCodes.IndexOf(code => code == s.DG_ExceptedQuantityCode))
				.First()
				.DG_ExceptedQuantityCode;
		}
	}
}
