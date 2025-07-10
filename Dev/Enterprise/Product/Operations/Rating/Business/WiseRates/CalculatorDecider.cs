using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.Rating.Business.WiseRates
{
	public class CalculatorDecider
	{
		readonly List<Charge> _charges;
		readonly IRateLine _parent;
		readonly ILogger _logger;
		readonly string _containerMode;
		readonly string _transportMode;
		readonly Dictionary<string, ZGuid> _chargeCodeMapping;

		public CalculatorDecider(IEnumerable<Charge> charges, IRateLine parent, ILogger logger, string containerMode, string transportMode, Dictionary<string, ZGuid> chargeCodeMappings)
		{
			_charges = charges.ToList();
			_parent = parent;
			_logger = logger;
			_containerMode = containerMode;
			_transportMode = transportMode;
			_chargeCodeMapping = chargeCodeMappings;
		}

		public Calculator Calculator
		{
			get { return calculator ?? (calculator = CalculatorFactory.GetCalculator(_parent)); }
		}
		Calculator calculator;

		public string Code
		{
			get
			{
				EnsureResolved();
				return resolver.Code;
			}
		}

		public IEnumerable<IRateLineItem> RateLineItems
		{
			get
			{
				EnsureResolved();
				return rateLineItems ?? (rateLineItems = resolver.RateLineItems);
			}
		}

		IEnumerable<IRateLineItem> rateLineItems;

		void EnsureResolved()
		{
			if (resolver == null)
			{
				foreach (var resolverType in CorrectlyOrderedResolvers())
				{
					resolver = Activator.CreateInstance(resolverType, _charges, _parent, _chargeCodeMapping) as CalculatorResolver;
					if (resolver.Resolve())
					{
						break;
					}
				}

				if (resolver is LastResortResolver)
				{
					_logger.Error(Res.GetString("3616b565-28b1-44dd-bcc5-92b2d854b570", "Could not create calculator from:{0}{1}", System.Environment.NewLine, _charges.ToYAML()));
				}
			}
		}

		CalculatorResolver resolver;

		ImmutableArray<Type> CorrectlyOrderedResolvers()
		{
			/**
			 * Note: Sarah Ho (SRH) decreed that the Cargoguide resolvers shall only
			 * produce CMB calculators. Do not add other resolvers to this list
			 * without checking with Saeed or Sarah
			 * URS Calculator matches this calculator logic when transport mode is AIR
			 */
			if (_transportMode == Core.Constants.TransportModes.Air)
			{
				return new List<Type>
				{
					typeof(InclusiveCalculatorResolver),
					typeof(CombinedCalculatorResolver),
					typeof(LastResortResolver)
				}.ToImmutableArray();
			}

			// For CargoSphere and URS with transport mode SEA and LCL container mode
			if (_transportMode == Core.Constants.TransportModes.Sea && _containerMode == Core.Constants.RateMode.LCL)
			{
				return new List<Type>
				{
					typeof(InclusiveCalculatorResolver),
					typeof(PercentageCalculatorResolver),
					typeof(FlatCalculatorResolver),
					typeof(HighestRateCalculatorResolver), // only for CargoSphere (SEA) LCL
					typeof(CombinedCalculatorResolver),
					typeof(LastResortResolver)
				}.ToImmutableArray();
			}

			// note: URS rates go through this one below.
			return new List<Type>
				{
					typeof(PercentageCalculatorResolver),
					typeof(MinimumOrPerUnitCalculatorResolver),
					typeof(FlatPlusPerUnitCalculatorResolver),
					typeof(UnitCalculatorResolver),
					typeof(FlatCalculatorResolver),
					typeof(MinimumCalculatorResolver),
					typeof(InclusiveCalculatorResolver),
					typeof(CombinedCalculatorResolver),
					typeof(LastResortResolver)
				}.ToImmutableArray();
		}
	}

	public static class CalculatorDeciderHelper
	{
		/// <summary>
		///		Convert API model charge type to freight calculation type code
		/// </summary>
		public static string GetFreightCalcTypeCode(ChargeType chargeType)
		{
			switch (chargeType & ~ChargeType.Optional)
			{
				case ChargeType.Included:
					return FreightInclusiveCalculator.FreightCalcTypes.Included;

				case ChargeType.SubjectTo:
					return FreightInclusiveCalculator.FreightCalcTypes.SubjectTo;

				case ChargeType.NotApplicable:
					return FreightInclusiveCalculator.FreightCalcTypes.NotApplicable;
			}

			return string.Empty;
		}
	}

	internal abstract class CalculatorResolver
	{
		protected readonly List<Charge> charges;
		protected readonly IRateLine parent;
		protected readonly Dictionary<string, ZGuid> chargeCodeMapping;

		protected CalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping)
		{
			this.charges = charges;
			this.parent = parent;
			this.chargeCodeMapping = chargeCodeMapping;
		}

		public abstract string Code { get; }
		public abstract bool Resolve();

		public abstract IEnumerable<IRateLineItem> RateLineItems { get; }

		protected IEnumerable<IRateLineItem> AppendMandatoryRateLineItems(IRateLine parentRateLine, Type calculatorType, IEnumerable<IRateLineItem> existingItems)
		{
			var attributes = (CalculatorPropertyAttribute[])calculatorType.GetCustomAttributes(typeof(CalculatorPropertyAttribute), true);
			var items = existingItems.ToList();
			foreach (var a in attributes.Where(x => x.IsMandatory))
			{
				if (items.All(x => x.TM_Type != a.ItemType))
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

					items.Add(new WiseLineItem(parentRateLine, a.ItemType, stringValue, 0, string.Empty, ZDecimal.Zero, 0, false));
				}
			}

			return items.ToArray();
		}
	}

	class LastResortResolver : CalculatorResolver
	{
		public LastResortResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping) : base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code
		{
			get { return NullCalculator.Code; }
		}

		public override bool Resolve()
		{
			return true;
		}

		public override IEnumerable<IRateLineItem> RateLineItems
		{
			get { return Enumerable.Empty<IRateLineItem>(); }
		}
	}

	class UnitCalculatorResolver : CalculatorResolver
	{
		public UnitCalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping) : base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code => UnitCalculator.Code;

		public override IEnumerable<IRateLineItem> RateLineItems => new[]
		{
			new WiseLineItem(parent, Calculator.Items.Operator.UNT, string.Empty, charges[0].PerUnitRate.Value, ZString.Empty, ZDecimal.Zero, 0, charges[0].Restricted)
		};

		public override bool Resolve()
		{
			return
				charges.Count == 1 &&
				charges[0].PerUnitRate.HasValue &&
				!charges[0].FlatRate.HasValue &&
				string.IsNullOrEmpty(charges[0].FreightInclusiveCarriageCharge);
		}
	}

	class FlatCalculatorResolver : CalculatorResolver
	{
		public FlatCalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping) : base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code => FlatCalculator.Code;

		public override IEnumerable<IRateLineItem> RateLineItems => new[]
		{
			new WiseLineItem(parent, Calculator.Items.Operator.BAS, string.Empty, charges[0].FlatRate.Value, ZString.Empty, ZDecimal.Zero, 0, charges[0].Restricted)
		};

		public override bool Resolve()
		{
			return charges.Count == 1 && !charges[0].PerUnitRate.HasValue && string.IsNullOrEmpty(charges[0].FreightInclusiveCarriageCharge) && charges[0].FlatRate.HasValue;
		}
	}

	class FlatPlusPerUnitCalculatorResolver : CalculatorResolver
	{
		public FlatPlusPerUnitCalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping) : base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code => FlatPlusPerUnitCalculator.Code;

		public override IEnumerable<IRateLineItem> RateLineItems => new[]
		{
			new WiseLineItem(parent, Calculator.Items.Operator.BAS, string.Empty, charges[0].FlatRate.Value, ZString.Empty, ZDecimal.Zero, 0, charges[0].Restricted),
			new WiseLineItem(parent, Calculator.Items.Operator.UNT, string.Empty, charges[0].PerUnitRate.Value, ZString.Empty, ZDecimal.Zero, 0, charges[0].Restricted)
		};

		public override bool Resolve()
		{
			return charges.Count == 1 && charges[0].PerUnitRate.HasValue && charges[0].FlatRate.HasValue;
		}
	}

	class MinimumOrPerUnitCalculatorResolver : CalculatorResolver
	{
		public MinimumOrPerUnitCalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping) : base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code => MinimumOrPerUnitCalculator.Code;

		public override IEnumerable<IRateLineItem> RateLineItems => new[]
		{
			new WiseLineItem(parent, Calculator.Items.Operator.MIN, string.Empty, charges[0].MinRate.Value, ZString.Empty, ZDecimal.Zero, 0, charges[0].Restricted),
			new WiseLineItem(parent, Calculator.Items.Operator.UNT, string.Empty, charges[0].PerUnitRate.Value, ZString.Empty, ZDecimal.Zero, 0, charges[0].Restricted)
		};

		public override bool Resolve()
		{
			return charges.Count == 1 && charges[0].PerUnitRate.HasValue && charges[0].MinRate.HasValue;
		}
	}

	class MinimumCalculatorResolver : CalculatorResolver
	{
		public MinimumCalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping) : base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code => MinimumCalculator.Code;

		public override IEnumerable<IRateLineItem> RateLineItems
		{
			get
			{
				var items = new[]
				{
					new WiseLineItem(parent, Calculator.Items.Operator.MIN, string.Empty, charges[0].MinRate.Value, ZString.Empty, ZDecimal.Zero, 0, charges[0].Restricted),
					new WiseLineItem(parent, MinimumCalculator.Items.MinimumType, CalculatorConstants.Text.MIN_ChargeCode, charges[0].MinRate.Value, ZString.Empty, ZDecimal.Zero, 0, charges[0].Restricted)
				};

				return AppendMandatoryRateLineItems(parent, typeof(MinimumCalculator), items);
			}
		}

		public override bool Resolve()
		{
			return charges.Count == 1 && !charges[0].PerUnitRate.HasValue && charges[0].MinRate.HasValue;
		}
	}

	class PercentageCalculatorResolver : CalculatorResolver
	{
		public PercentageCalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping) : base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code
		{
			get { return PercentageCalculator.Code; }
		}

		public override IEnumerable<IRateLineItem> RateLineItems
		{
			get
			{
				var items = new List<IRateLineItem>();
				var charge = charges[0];

				items.Add(new WiseLineItem(parent, CalculatorConstants.Type.PER, string.Empty, charge.Percentage.Value, string.Empty, ZDecimal.Zero, 0, charges[0].Restricted));
				items.Add(new WiseLineItem(parent, CalculatorConstants.Type.ApplyTo, CalculatorConstants.Text.ChargeCode, charge.Percentage.Value, charge.PercentageAppliesTo, ZDecimal.Zero, 0, charges[0].Restricted, chargeCodeGuid: Enterprise.Freight.Business.FreightUtilities.GetValueOrDefault(chargeCodeMapping, charge.PercentageAppliesTo)));

				if (charge.MinRate.HasValue)
				{
					items.Add(new WiseLineItem(parent, Calculator.Items.Operator.MIN, string.Empty, charge.MinRate.Value, string.Empty, 0, 0, null));
				}

				if (charge.MaxRate.HasValue)
				{
					items.Add(new WiseLineItem(parent, Calculator.Items.Operator.MAX, string.Empty, charge.MaxRate.Value, string.Empty, 0, 0, null));
				}

				return AppendMandatoryRateLineItems(parent, typeof(PercentageCalculator), items);
			}
		}

		public override bool Resolve()
		{
			return charges.Count == 1 && !string.IsNullOrWhiteSpace(charges[0].PercentageAppliesTo) && charges[0].Percentage.HasValue;
		}
	}

	class InclusiveCalculatorResolver : CalculatorResolver
	{
		public InclusiveCalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping) : base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code => FreightInclusiveCalculator.Code;

		public override IEnumerable<IRateLineItem> RateLineItems
		{
			get
			{
				var result = new List<WiseLineItem>
				{
					new WiseLineItem(
						parent, FreightInclusiveCalculator.Items.FreightCalcType,
						CalculatorDeciderHelper.GetFreightCalcTypeCode(charges[0].ChargeType),
						ZDecimal.Zero, string.Empty, ZDecimal.Zero, 0, false)
				};

				if (!string.IsNullOrWhiteSpace(charges[0].FreightInclusiveCarriageCharge))
				{
					result.Add(
						new WiseLineItem(
							parent, FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType,
							string.Empty, ZDecimal.Zero, charges[0].FreightInclusiveCarriageCharge, ZDecimal.Zero, 0, false,
							chargeCodeGuid: Enterprise.Freight.Business.FreightUtilities.GetValueOrDefault(chargeCodeMapping, charges[0].FreightInclusiveCarriageCharge)));
				}

				return result;
			}
		}

		public override bool Resolve()
		{
			return charges.Count == 1 && !string.IsNullOrEmpty(charges[0].FreightInclusiveCarriageCharge);
		}
	}

	class CombinedCalculatorResolver : CalculatorResolver
	{
		public CombinedCalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping) : base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code => CombinedCalculator.Code;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Sliding Calculator is complex, therefore decider is also not a simple thing")]
		public override IEnumerable<IRateLineItem> RateLineItems
		{
			get
			{
				var items = new List<IRateLineItem>();
				var charge = charges.First();
				var applicability = charge.Applicability ?? string.Empty;

				// The adding of the RateLineItems has moved into individual functions
				// to reduce clutter in this function. However, there are implied
				// dependencies between the methods, so do not re-arrange them
				// carelessly.

				AddMultipleEquipmentsOverMaxWeightVolumeOption(items);
				AddHigherBreakLowerCostOption(items, charge);

				if (AddEquipmentUnitCharges(items, charge, applicability))
				{
					return AppendMandatoryRateLineItems(parent, typeof(CombinedCalculator), items);
				}

				AddFlatCharges(items);
				AddMinimumCharges(items, charge, applicability);
				AddMaximumCharges(items, charge, applicability);
				AddPerUnitCharge(items);
				AddIsInclusive(items, charge);
				AddMinusPlusCharges(items, applicability);
				AddMinimumFromFirstBreaks(items);

				return AppendMandatoryRateLineItems(parent, typeof(CombinedCalculator), items);
			}
		}

		void AddMinimumFromFirstBreaks(List<IRateLineItem> items)
		{
			// This function assumes the breaks are sorted ascending

			var hasMinimumOrMinus = items
				.Any(x =>
					x.TM_Type == Calculator.Items.Operator.MIN ||
					x.TM_Type == Calculator.Items.Operator.Minus);

			if (hasMinimumOrMinus)
			{
				return;
			}

			var firstPlus = items.FirstOrDefault(x => x.TM_Type == Calculator.Items.Operator.Plus);

			if (firstPlus != null)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.MIN, string.Empty, 0, string.Empty, firstPlus.TM_Break, 0, null));

				if (!items.Any(x => x.TM_Type == Calculator.Items.UseInclusiveBreaks))
				{
					AddIsInclusive(items);
				}
			}
		}

		string AddMinusPlusCharges(List<IRateLineItem> items, string applicability)
		{
			var sortedByBreaks = charges.Where(c => c.Break.HasValue).OrderBy(c => c.Break.Value).ToArray();
			if (sortedByBreaks.Length == 1 && sortedByBreaks[0].Break.Value == 0)
			{
				var breakCharge = sortedByBreaks[0];
				var flatRate = breakCharge.FlatRate.GetValueOrDefault();
				var perUnitRate = breakCharge.PerUnitRate.GetValueOrDefault();
				var restricted = breakCharge.Restricted;
				applicability = breakCharge.Applicability ?? string.Empty;

				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Minus, applicability, perUnitRate, string.Empty, 1m, flatRate, restricted));
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Plus, applicability, perUnitRate, string.Empty, 1m, flatRate, restricted));
			}
			else
			{
				for (var i = 0; i < sortedByBreaks.Length; i++)
				{
					var breakCharge = sortedByBreaks[i];

					var flatRate = breakCharge.FlatRate.GetValueOrDefault();
					var perUnitRate = breakCharge.PerUnitRate.GetValueOrDefault();
					var breakValue = breakCharge.Break.Value;
					var restricted = breakCharge.Restricted;
					applicability = breakCharge.Applicability ?? string.Empty;

					if (breakValue == 0)
					{
						var nextBreakCharge = sortedByBreaks[i + 1];
						items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Minus, applicability, perUnitRate, string.Empty, (nextBreakCharge?.Break).GetValueOrDefault(0), flatRate, restricted));
					}
					else
					{
						items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Plus, applicability, perUnitRate, string.Empty, breakValue, flatRate, restricted));
					}
				}
			}

			return applicability;
		}

		void AddIsInclusive(List<IRateLineItem> items, Charge charge = null)
		{
			if (charge == null || // Force charge inclusive to be added
				charge.BreakOperator == CW1Constants.BreakItemCodes.GreaterThan)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.UseInclusiveBreaks, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false));
			}
		}

		void AddPerUnitCharge(List<IRateLineItem> items)
		{
			var perUnitCharge = charges.FirstOrDefault(x => x.PerUnitRate.HasValue && !x.FlatRate.HasValue && !x.Break.HasValue);
			if (perUnitCharge != null)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.UNT, string.Empty, perUnitCharge.PerUnitRate.Value, ZString.Empty, ZDecimal.Zero, 0, perUnitCharge.Restricted));
			}
		}

		void AddMaximumCharges(List<IRateLineItem> items, Charge charge, string applicability)
		{
			if (charge.MaxRate.HasValue)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.MAX, applicability, charge.MaxRate.Value, string.Empty, 0, 0, null));
			}
		}

		void AddMinimumCharges(List<IRateLineItem> items, Charge charge, string applicability)
		{
			if (charge.MinRate.HasValue || charge.MinChargeable.HasValue)
			{
				var minChargeable = charge.MinChargeable ?? 0;
				var minRate = charge.MinRate ?? 0;
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.MIN, applicability, minRate, string.Empty, minChargeable, 0, null));
			}
		}

		void AddFlatCharges(List<IRateLineItem> items)
		{
			var flatCharge = charges.FirstOrDefault(x => x.FlatRate.HasValue && !x.PerUnitRate.HasValue && !x.Break.HasValue);
			if (flatCharge != null)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.Operator.BAS, string.Empty, flatCharge.FlatRate.Value, ZString.Empty, ZDecimal.Zero, 0, flatCharge.Restricted));
			}
		}

		void AddMultipleEquipmentsOverMaxWeightVolumeOption(List<IRateLineItem> items)
		{
			var perUnitCharge = charges.FirstOrDefault(c => c.PerUnitRate.HasValue);
			// For CG FCL rates we always want to apply MultipleEquipmentsOverMaxWeightVolume rule, i.e.
			// calculate number of containers required based on the weight/volume specified by the user and
			// container payload.
			if (perUnitCharge != null)
			{
				var wiseEntry = (WiseEntry)parent.ParentRateEntry;

				if ((perUnitCharge.EquipmentUnit == WRConstants.Units.CN || perUnitCharge.Unit == WRConstants.Units.CN) &&
					wiseEntry.WiseRate.Provider == WRConstants.RateProviders.CargoGuide)
				{
					items.Add(new WiseLineItem(parent, Calculator.Items.MultipleEquipmentsOverMaxWeightVolume, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false));
				}
			}
		}

		bool AddEquipmentUnitCharges(List<IRateLineItem> items, Charge charge, string applicability)
		{
			if (charges.All(c => !string.IsNullOrEmpty(c.EquipmentUnit)))
			{
				var restricted = charge.Restricted;
				var pivotCostCharge = charges.FirstOrDefault(c => c.Unit == RatingConstants.Units.CN && c.PerUnitRate.HasValue);
				var overPivotCharge = charges.FirstOrDefault(c => (RatingConstants.Units.IsWeight(c.Unit) || RatingConstants.Units.IsVolume(c.Unit)) && c.Break.HasValue && c.PerUnitRate.HasValue);

				if (pivotCostCharge != null && overPivotCharge != null)
				{
					items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Minus, applicability, 0, string.Empty, overPivotCharge.Break.Value, pivotCostCharge.PerUnitRate.Value, restricted));
					items.Add(new WiseLineItem(parent, Calculator.Items.Operator.Plus, applicability, overPivotCharge.PerUnitRate.Value, string.Empty, overPivotCharge.Break.Value, pivotCostCharge.PerUnitRate.Value, restricted));

					items.Add(new WiseLineItem(parent, Calculator.Items.UseAccumulated, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false));
					items.Add(new WiseLineItem(parent, Calculator.Items.MultipleEquipmentsOverMaxWeightVolume, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false));
					items.Add(new WiseLineItem(parent, Calculator.Items.UseInclusiveBreaks, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false));

					return true;
				}
			}
			return false;
		}

		void AddHigherBreakLowerCostOption(List<IRateLineItem> items, Charge charge)
		{
			if (charge.IsHigherBreakLowerRate)
			{
				items.Add(new WiseLineItem(parent, Calculator.Items.HigherChargeableLowerRate, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, null));
			}
		}

		public override bool Resolve()
		{
			if (charges.Count == 0)
			{
				return false;
			}

			// Each of the charges must have set at least one of:
			// - flat rate,
			// - max rate,
			// - min rate or
			// - per unit rate.
			if (charges.Any(c => !(c.FlatRate.HasValue || c.MaxRate.HasValue || c.MinRate.HasValue || c.PerUnitRate.HasValue)))
			{
				return false;
			}

			// breakCharges are those that have both per unit rate, and a break operator
			var breakCharges = charges.Where(c => c.PerUnitRate.HasValue && !string.IsNullOrEmpty(c.BreakOperator)).ToList();

			// There are no breakCharges with a value of zero.
			if (!breakCharges.All(c => c.Break.HasValue && c.Break.Value >= 0))
			{
				return false;
			}

			// All the breakcharges must have the same operator
			if (!breakCharges.AllSame(c => c.BreakOperator))
			{
				return false;
			}

			return true;
		}
	}

	/// <summary>
	/// HRC resolver targeting CargoSphere (SEA - LCL) rates
	/// </summary>
	class HighestRateCalculatorResolver : CalculatorResolver
	{
		public HighestRateCalculatorResolver(List<Charge> charges, IRateLine parent, Dictionary<string, ZGuid> chargeCodeMapping)
			: base(charges, parent, chargeCodeMapping)
		{
		}

		public override string Code => HighestRateCalculator.Code;

		/// <summary>
		/// Warning: Must ensure <see cref="Resolve"/> is called before accessing the items because it checks conditions
		/// for wise items to be created.
		/// </summary>
		public override IEnumerable<IRateLineItem> RateLineItems
		{
			get
			{
				var result = new List<IRateLineItem>();
				var firstCharge = charges[0];
				result.Add(new WiseLineItem(
					parent,
					tm_type: Calculator.Items.Operator.UNT,
					tm_text: ZString.Empty,
					tm_relevantValue: firstCharge.PerUnitRate ?? ZDecimal.Zero,
					tm_ac: ZString.Empty,
					tm_break: ZDecimal.Zero,
					tm_flatAmount: firstCharge.FlatRate ?? ZDecimal.Zero,
					tm_callForPricing: false,
					tm_BreakWeightVolume: firstCharge.Unit,
					tm_unitMultiple: (int)firstCharge.UnitMultiplier));

				var secondCharge = charges.Last();
				result.Add(new WiseLineItem(
					parent,
					tm_type: Calculator.Items.Operator.UNT,
					tm_text: ZString.Empty,
					tm_relevantValue: secondCharge.PerUnitRate ?? ZDecimal.Zero,
					tm_ac: ZString.Empty,
					tm_break: ZDecimal.Zero,
					tm_flatAmount: secondCharge.FlatRate ?? ZDecimal.Zero,
					tm_callForPricing: false,
					tm_BreakWeightVolume: secondCharge.Unit,
					tm_unitMultiple: (int)secondCharge.UnitMultiplier));

				// A MIN item may present if MinRate(s) exist
				var minItem = CreateMINItem(firstCharge, secondCharge);
				if (minItem != null)
				{
					result.Add(minItem);
				}

				// HRC requires a mandatory item of type RatePickRule. Its empty tm_text means "HighestRate".
				var rprItem = new WiseLineItem(
					parent, tm_type: Calculator.Items.RatePickRule, tm_text: string.Empty,
					tm_relevantValue: ZDecimal.Zero, tm_ac: ZString.Empty, tm_break: ZDecimal.Zero,
					tm_flatAmount: ZDecimal.Zero, tm_callForPricing: false);
				result.Add(rprItem);

				return result;
			}
		}

		WiseLineItem CreateMINItem(Charge firstCharge, Charge secondCharge)
		{
			var higherMin = firstCharge.MinRate ?? ZDecimal.Zero;
			var secondChargeMinRate = secondCharge.MinRate ?? ZDecimal.Zero;
			if (secondChargeMinRate > higherMin)
			{
				higherMin = secondChargeMinRate;
			}

			if (higherMin > ZDecimal.Zero)
			{
				return new WiseLineItem(
					parent,
					tm_type: Calculator.Items.Operator.MIN,
					tm_text: string.Empty,
					tm_relevantValue: higherMin,
					tm_ac: ZString.Empty,
					tm_break: ZDecimal.Zero,
					tm_flatAmount: ZDecimal.Zero,
					tm_callForPricing: false);
			}

			return null;
		}

		public override bool Resolve()
		{
			if (charges.Count != 2)
			{
				return false;
			}

			// At this stage, if the pair of charges are different in units then HRC calculator is for them.
			// Otherwise, we leave them for sliding calculator.
			return charges[0].Unit != charges[1].Unit;
		}
	}
}
