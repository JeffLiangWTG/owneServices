#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Urs.Api.Integration.Interfaces;
using WiseRates.Constants;
using static Enterprise.Rating.Business.UrsConstants;

namespace Enterprise.Rating.Business
{
	public static class UrsCalculatorDecider
	{
		public static ResolvedCalculator GetCalculator(IEnumerable<IUniversalRateEntryDto> ursRateEntries, IRateLine parent, ILogger logger, string transportMode)
		{
			var calc = GetResolvers(ursRateEntries, parent, transportMode, logger).FirstOrDefault(c => c != null);
			if (calc is ResolvedCalculator resolved)
			{
				return resolved;
			}
			return new ResolvedCalculator { Code = NullCalculator.Code, RateLineItems = [] };
		}

		static IEnumerable<ResolvedCalculator?> GetResolvers(IEnumerable<IUniversalRateEntryDto> ursRateEntries, IRateLine parent, string transportMode, ILogger logger)
		{
			yield return TryResolvePercentageCalculator(ursRateEntries, parent);
			yield return TryResolvePerUnitCalculator(ursRateEntries, parent);
			yield return TryResolveFlatCalculator(ursRateEntries, parent);
			yield return TryResolveCombinedCalculator(ursRateEntries, parent, transportMode, logger);
		}

		public struct ResolvedCalculator()
		{
			public string Code = NullCalculator.Code;
			public string Unit = "";
			public decimal RoundingFactor = 0;
			public byte ActualPercentage;
			public decimal UnitMultiplier;
			public IEnumerable<IRateLineItem> RateLineItems = [];
		}

		static ResolvedCalculator? TryResolvePercentageCalculator(IEnumerable<IUniversalRateEntryDto> ursRateEntries, IRateLine parent)
		{
			var rate = ursRateEntries.FirstOrDefault();
			if (ursRateEntries.Count() != 1 || rate == null || rate.Applicable != RateApplicableCode.Percentage)
			{
				return null;
			}

			List<WiseLineItem> lines = [
				new WiseLineItem(parent, CalculatorConstants.Type.PER, string.Empty, rate.Price, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(parent, CalculatorConstants.Type.ApplyTo, CalculatorConstants.Text.ChargeCode, rate.Price, UrsRatesParseHelper.FRTUniversalChargeCode, ZDecimal.Zero, 0, false)
			];

			AppendMandatoryRateLineItems(parent, typeof(PercentageCalculator), lines);
			return new ResolvedCalculator { Code = PercentageCalculator.Code, RateLineItems = lines };
		}

		static ResolvedCalculator? TryResolvePerUnitCalculator(IEnumerable<IUniversalRateEntryDto> ursRateEntries, IRateLine parent)
		{
			var perUnitRate = ursRateEntries.FirstOrDefault(e => e.BreakType == RateBreakTypeCode.Flat);
			if (perUnitRate == null || ursRateEntries.Count() > 2 || UrsRatesParseHelper.IsFlatPrice(perUnitRate))
			{
				return null;
			}

			var minOrBaseRate = ursRateEntries.FirstOrDefault(e => e.BreakType is RateBreakTypeCode.Base or RateBreakTypeCode.Min);
			if (minOrBaseRate == null && ursRateEntries.Count() == 2)
			{
				return null;
			}

			if (perUnitRate.Applicable != RateApplicableCode.UnitPrice && minOrBaseRate == null && perUnitRate.Applicable != RateApplicableCode.Zero)
			{
				return null;
			}

			var code = UnitCalculator.Code;
			List<WiseLineItem> items = [new(parent, Calculator.Items.Operator.UNT, string.Empty, perUnitRate.Price, ZString.Empty, ZDecimal.Zero, 0, false)];

			if (minOrBaseRate is { BreakType: RateBreakTypeCode.Base })
			{
				code = FlatPlusPerUnitCalculator.Code;
				items.Insert(0, new WiseLineItem(parent, Calculator.Items.Operator.BAS, string.Empty, minOrBaseRate.Price, ZString.Empty, ZDecimal.Zero, 0, false));
			}
			else if (minOrBaseRate != null)
			{
				code = MinimumOrPerUnitCalculator.Code;
				items.Insert(0, new WiseLineItem(parent, Calculator.Items.Operator.MIN, string.Empty, minOrBaseRate.Price, ZString.Empty, ZDecimal.Zero, 0, false));
			}

			return new ResolvedCalculator
			{
				Code = code,
				Unit = perUnitRate.PricingQuantityUnit,
				RoundingFactor = perUnitRate.MeasurementPrecision,
				ActualPercentage = (byte)(perUnitRate.UseVolumetric ? 0 : 100),
				UnitMultiplier = perUnitRate.PricingQuantity,
				RateLineItems = items,
			};
		}

		static ResolvedCalculator? TryResolveFlatCalculator(IEnumerable<IUniversalRateEntryDto> ursRateEntries, IRateLine parent)
		{
			var flatRate = ursRateEntries.FirstOrDefault(e => e.Applicable is RateApplicableCode.UnitPrice or RateApplicableCode.Zero or RateApplicableCode.NotApplicable);
			if (flatRate == null || ursRateEntries.Count() != 1 || !UrsRatesParseHelper.IsFlatPrice(flatRate))
			{
				return null;
			}

			return new ResolvedCalculator
			{
				Code = FlatCalculator.Code,
				RateLineItems = [
					new WiseLineItem(parent, Calculator.Items.Operator.BAS, string.Empty, flatRate.Price, ZString.Empty, ZDecimal.Zero, 0, false)
				],
			};
		}

		static ResolvedCalculator? TryResolveCombinedCalculator(IEnumerable<IUniversalRateEntryDto> ursRateEntries, IRateLine parent, string transportMode, ILogger logger)
		{
			var firstItem = ursRateEntries.FirstOrDefault();
			if (firstItem == null)
			{
				return null;
			}

			// Assumption: Every item in universalRateEntryDtos should have the same Applicable value.
			// So, only take the first item to check the value.
			var isRestricted = UrsRatesParseHelper.IsRestricted(firstItem);
			var applicability = isRestricted ? UrsRatesParseHelper.GetChargeApplicability(firstItem.Applicable) : string.Empty;

			var rateUseVolumetric = ursRateEntries.FirstOrDefault(e => e.UseVolumetric);
			var unit = rateUseVolumetric?.PricingQuantityUnit;
			var roundingFactor = rateUseVolumetric?.MeasurementPrecision ?? 0;
			var actualPercentage = (byte)(rateUseVolumetric?.UseVolumetric ?? false ? 0 : 100);

			// Pivot flow
			var items = new List<WiseLineItem>();
			var pivot = ursRateEntries.FirstOrDefault(e => e.BreakType == RateBreakTypeCode.Pivot);
			var flt = ursRateEntries.FirstOrDefault(e => e.BreakType == RateBreakTypeCode.Base);
			var flatRate = flt?.Price ?? 0;

			AddHigherBreakLowerCostOption(items, parent, transportMode);

			if (pivot != null && flt != null)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Minus, applicability, 0m, string.Empty, pivot.BreakQuantity, flt.Price, isRestricted));
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Plus, applicability, pivot.Price, string.Empty, pivot.BreakQuantity, flt.Price, isRestricted));

				items.Add(new WiseLineItem(parent, Calculator.Items.UseAccumulated, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false));
				items.Add(new WiseLineItem(parent, Calculator.Items.MultipleEquipmentsOverMaxWeightVolume, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false));
				items.Add(new WiseLineItem(parent, Calculator.Items.UseInclusiveBreaks, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false));
				AppendMandatoryRateLineItems(parent, typeof(CombinedCalculator), items);

				return new ResolvedCalculator
				{
					Code = CombinedCalculator.Code,
					Unit = pivot.PricingQuantityUnit,
					RoundingFactor = roundingFactor,
					ActualPercentage = actualPercentage,
					RateLineItems = items,
				};
			}

			// Check the BreakType of all items in universalRateEntryDtos should be one of the following values, (Base, Flat, Min, Max, BreakLess, BreakPlus)
			if (ursRateEntries.Any(e => !IsValidBreakType(e.BreakType)))
			{
				return null;
			}
			var min = ursRateEntries.FirstOrDefault(e => e.BreakType == RateBreakTypeCode.Min);
			var max = ursRateEntries.FirstOrDefault(e => e.BreakType == RateBreakTypeCode.Max);
			var perUnit = ursRateEntries.FirstOrDefault(e => e.BreakType == RateBreakTypeCode.Flat);

			// For Cargoguide, if the minimum is not defined we use the first breakweight as minimum
			if (min == null && transportMode == WRConstants.TransportModes.AIR)
			{
				var brkMin = ursRateEntries.FirstOrDefault(e => e.BreakType == RateBreakTypeCode.BreakPlus || e.BreakType == RateBreakTypeCode.BreakLess);
				AddBreakMinimumRateLineItem(items, parent, brkMin, applicability);
			}
			else
			{
				AddMinimumRateLineItem(items, parent, min, applicability);
			}

			AddBaseRateLineItem(items, parent, flt);
			AddMaximumRateLineItem(items, parent, max, applicability);
			AddPerUnitRateLineItem(items, parent, perUnit, isRestricted);

			var minus = ursRateEntries.FirstOrDefault(e => e.BreakType == RateBreakTypeCode.BreakLess);
			if (minus != null)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Minus, applicability, minus.Price, string.Empty, minus.BreakQuantity, flatRate, isRestricted));
			}

			var plusCollection = ursRateEntries.Where(e => e.BreakType == RateBreakTypeCode.BreakPlus);

			// sort plusCollection as per break quantity
			// and run AddPlusRateLineItem for each item in the loop
			plusCollection.OrderBy(e => e.BreakQuantity)
				 .ToList()
				 .ForEach(plus => {
					 var plusIsRestricted = UrsRatesParseHelper.IsRestricted(plus);
					 var plusApplicability = plusIsRestricted ? UrsRatesParseHelper.GetChargeApplicability(plus.Applicable) : string.Empty;
					 items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Plus, plusApplicability, plus.Price, string.Empty, plus.BreakQuantity, flatRate, plusIsRestricted));
				 });

			AppendMandatoryRateLineItems(parent, typeof(CombinedCalculator), items);

			return new ResolvedCalculator
			{
				Code = CombinedCalculator.Code,
				Unit = unit ?? "",
				RoundingFactor = roundingFactor,
				ActualPercentage = actualPercentage,
				RateLineItems = items,
			};
		}

		static void AddHigherBreakLowerCostOption(List<WiseLineItem> items, IRateLine parent, string transportMode)
		{
			if (transportMode == WRConstants.TransportModes.AIR)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.HigherChargeableLowerRate, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, null));
			}
		}

		static void AddMinimumRateLineItem(List<WiseLineItem> items, IRateLine parent, IUniversalRateEntryDto? min, string applicability)
		{
			if (min != null)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.MIN, applicability, min.Price, string.Empty, 0, 0, null));
			}
		}

		static void AddBreakMinimumRateLineItem(List<WiseLineItem> items, IRateLine parent, IUniversalRateEntryDto? brk, string applicability)
		{
			if (brk != null && brk.BreakType != RateBreakTypeCode.BreakLess)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.MIN, applicability, 0, string.Empty, brk.BreakQuantity, 0, null));
			}
		}

		static void AddBaseRateLineItem(List<WiseLineItem> items, IRateLine parent, IUniversalRateEntryDto? flt)
		{
			if (flt != null)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.BAS, string.Empty, flt.Price, string.Empty, 0, 0, null));
			}
		}

		static void AddMaximumRateLineItem(List<WiseLineItem> items, IRateLine parent, IUniversalRateEntryDto? max, string applicability)
		{
			if (max != null)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.MAX, applicability, max.Price, string.Empty, 0, 0, null));
			}
		}

		static void AddPerUnitRateLineItem(List<WiseLineItem> items, IRateLine parent, IUniversalRateEntryDto? perUnit, bool isRestricted)
		{
			if (perUnit != null && !UrsRatesParseHelper.IsFlatPrice(perUnit))
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.UNT, string.Empty, perUnit.Price, ZString.Empty, ZDecimal.Zero, 0, isRestricted));
			}
		}

		static void AppendMandatoryRateLineItems(IRateLine parentRateLine, Type calculatorType, List<WiseLineItem> existingItems)
		{
			var attributes = (CalculatorPropertyAttribute[])calculatorType.GetCustomAttributes(typeof(CalculatorPropertyAttribute), true);
			foreach (var a in attributes.Where(x => x.IsMandatory))
			{
				if (existingItems.All(x => x.TM_Type != a.ItemType))
				{
					var stringValue = string.Empty;
					if (a.InitialValue != null && a.MapTo.StartsWith((NoResString)"String", StringComparison.OrdinalIgnoreCase)) // matching mapping type
					{
						stringValue = a.InitialValue as string;
					}
					else if (a.InitialValue != null && a.MapTo.StartsWith((NoResString)"bool", StringComparison.OrdinalIgnoreCase)) // matching mapping type
					{
						stringValue = (bool)a.InitialValue ? "Y" : "N";
					}

					existingItems.Add(new WiseLineItem(parentRateLine, a.ItemType, stringValue, 0, string.Empty, ZDecimal.Zero, 0, false));
				}
			}
		}

		static readonly ImmutableHashSet<string> ValidBreakTypes = ImmutableHashSet.Create(
			RateBreakTypeCode.Base,
			RateBreakTypeCode.Flat,
			RateBreakTypeCode.Min,
			RateBreakTypeCode.Max,
			RateBreakTypeCode.BreakLess,
			RateBreakTypeCode.BreakPlus,
			RateBreakTypeCode.Pivot
		);

		public static bool IsValidBreakType(string breakType) => ValidBreakTypes.Contains(breakType);
	}
}
