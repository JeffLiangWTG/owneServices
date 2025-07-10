using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Calculator.Items.Operator.MIN, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.Operator.BAS, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.Operator.UNT, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.Operator.MAX, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.Operator.Minus, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.Operator.Plus, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Items.UseAccumulated, RateLineItem.Schema.TM_Text, InitialValue = false, IsMandatory = true, MapTo = "Bool1", RelatedTo = "IsAccumulated")]
	[CalculatorProperty(Items.HigherChargeableLowerRate, RateLineItem.Schema.TM_Text, InitialValue = false, IsMandatory = true, MapTo = "Bool2", RelatedTo = "UseHigherChargeableLowerRateRule")]
	[CalculatorProperty(Items.UseInclusiveBreaks, RateLineItem.Schema.TM_Text, InitialValue = false, IsMandatory = true, MapTo = "Bool3", RelatedTo = "UseInclusiveBreaks")]
	[CalculatorProperty(Items.MultipleEquipmentsOverMaxWeightVolume, RateLineItem.Schema.TM_Text, InitialValue = false, IsMandatory = false, MapTo = "Bool6", RelatedTo = "MultipleEquipmentsOverMaxWeightVolume")]
	[CalculatorProperty(Items.BreaksPer, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String3", RelatedTo = "BreaksPer")]
	// WARNING: Anybody adding a new CalculatorProperties here *MUST* also check
	// RateLineItemMapperView to ensure that existing calculators that inherit
	// BaseCombinedCalculator do NOT have these new field accidentally appearing
	// in them. Also do a test in: RateLineItemMapperViewTest
	public abstract class BaseCombinedCalculator : Calculator
	{
		protected BaseCombinedCalculator(IRateLine master)
			: base(master)
		{
		}

		#region Initialisation

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var higherChargeableRuleNeedsToBeSet = Line.FindRateLineItem(Items.HigherChargeableLowerRate) == null;

			var itemList = base.CheckOrCreateItems();

			if (higherChargeableRuleNeedsToBeSet && RatingDataRegistry.Instance.UseHigherWeightOrUnitLowerRateRuleDefault.Value)
			{
				var item = (RateLineItem)Line.FindRateLineItem(Items.HigherChargeableLowerRate);
				if (item != null)
				{
					using (item.SuspendSettingHasChanges())
					using (item.GetValidationSuspender())
					{
						UseHigherChargeableLowerRateRule = true;
					}
				}
			}

			return itemList;
		}

		#endregion

		#region Properties

		public ZDecimal Minimum
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.MIN]; }
			set { this[Calculator.Items.Operator.MIN] = value; }
		}

		public ZDecimal BaseRate
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.BAS]; }
			set { this[Calculator.Items.Operator.BAS] = value; }
		}

		public ZDecimal PerUnit
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.UNT]; }
			set { this[Calculator.Items.Operator.UNT] = value; }
		}

		public ZDecimal Maximum
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.MAX]; }
			set { this[Calculator.Items.Operator.MAX] = value; }
		}

		public override ZString BreakUnit
		{
			get
			{
				if (IsBreakUnitAvailable)
				{
					foreach (IRateLineItem item in Line.ChildRateLineItems)
					{
						if (item.RateOperatorIsMinus())
						{
							if (!item.TM_BreakWeightVolume.IsEmpty)
							{
								return item.TM_BreakWeightVolume;
							}

							break;
						}
					}
				}

				return base.BreakUnit;
			}
		}

		/// <summary>
		///		When the calculator is per container with over-pivot rate
		///		(for example $1000 per container + 2$ per each KG over 900 KG), the unit
		///		represents the pivot unit (i.e. KG in this case). But, in order autorating
		///		to properly find related measures (i.e. continers with weight per each container)
		///		it has to return Container unit for autorating engine.
		///
		///		This smells but can't do anythig without major refactoring of over-pivot calculator or
		///		autorating engine.
		/// </summary>
		protected internal override ZString Unit => MultipleEquipmentsOverMaxWeightVolume
			? (ZString)RatingConstants.Units.CN
			: base.Unit;

		public override ZBool IsAccumulated
		{
			get => !BreaksPer.IsEmpty ? ZBool.True : (ZBool)this[Items.UseAccumulated];
			set => this[Items.UseAccumulated] = value;
		}

		internal ZPropertyInfo IsAccumulatedInfo => Bool1Info;

		public override ZBool Bool1
		{
			get => !BreaksPer.IsEmpty ? ZBool.True : base.Bool1;
			set => base.Bool1 = value;
		}

		public virtual ZBool UseHigherChargeableLowerRateRule
		{
			get => !BreaksPer.IsEmpty ? ZBool.False : (ZBool)this[Items.HigherChargeableLowerRate];
			set => this[Items.HigherChargeableLowerRate] = value;
		}

		internal ZPropertyInfo UseHigherChargeableLowerRateRuleInfo => Bool2Info;

		public override ZBool Bool2
		{
			get => !BreaksPer.IsEmpty ? ZBool.False : base.Bool2;
			set => base.Bool2 = value;
		}

		public override ZBool UseInclusiveBreaks
		{
			get { return IsAccumulated ? (ZBool)true : (ZBool)this[BaseCombinedCalculator.Items.UseInclusiveBreaks]; }
			set { this[BaseCombinedCalculator.Items.UseInclusiveBreaks] = value; }
		}

		public override ZBool MultipleEquipmentsOverMaxWeightVolume
		{
			get
			{
				var attributeValue = this[Items.MultipleEquipmentsOverMaxWeightVolume];
				if (attributeValue != null)
				{
					return (ZBool)this[Items.MultipleEquipmentsOverMaxWeightVolume];
				}
				else
				{
					return false;
				}
			}
			set
			{
				this[Items.MultipleEquipmentsOverMaxWeightVolume] = value;
			}
		}

		public override CodeDescriptionPairList List3 => RateLineBizO.Lookups.BreaksPer;

		public override ZString BreaksPer
		{
			get => (ZString)this[Items.BreaksPer];
			set => this[Items.BreaksPer] = value;
		}

		bool IsBreaksPerContainerTypeOrClass => BreaksPer.EqualsIgnoringCase(Items.BreaksPerContainerTypeOrClass);

		bool IsBreaksPerContainer => BreaksPer.EqualsIgnoringCase(Items.BreaksPerContainer);

		protected virtual bool IsBreakUnitDifferentToChargeableUnit
		{
			get
			{
				var breakUnit = BreakUnit;
				var result = !breakUnit.IsEmpty
					&& !IsCalculatePerTopPackWithBreakUnitsMode
					&& IsSliding()
					&& Line != null
					&& !Line.TL_WeightVolume.IsEmpty
					&& breakUnit != Line.TL_WeightVolume
					&& !Line.ParentRateEntry.IsFreightEntry();

				return result;
			}
		}

		bool IsCalculatePerTopPackWithBreakUnitsMode
		{
			get
			{
				var breakUnit = BreakUnit;
				return !breakUnit.IsEmpty
						&& breakUnit != Line.TL_WeightVolume
						&& CalculationPerTopPackWithBreakUnitsAvailable;
			}
		}

		protected virtual bool CalculationPerTopPackWithBreakUnitsAvailable => UnitHelper.IsTopPack(Line.TL_WeightVolume, Line.Factory);

		protected ZDecimal GetValueByType(IEnumerable<IRateLineItem> items, string itemType) => items.FirstOrDefault(x => x.TM_Type == itemType)?.TM_RelevantValue ?? 0m;

		#endregion

		#region Validation

		protected override void ValidateTM_TypeCore(RateLineItem lineItem)
		{
			ValidateGridCalculator(lineItem, true, true, true);
		}

		public override void ValidateTM_BreakWeightVolume(RateLineItem lineItem)
		{
			if (!lineItem.TM_BreakWeightVolume.IsEmpty && lineItem.IsLowestBreak())
			{
				ListValidation.ErrorIfInvalidCode(lineItem.TM_BreakWeightVolumeInfo);
			}
		}

		#endregion

		#region Quotation Lines

		protected QuotationLineList GetQuotationLinesForIterator(GetQuotationLinesParam flags, IEnumerable<IRateLineItem> items, ZString description)
		{
			var result = new QuotationLineList();
			var rateLineItems = items as IRateLineItem[] ?? items.ToArray();
			var minimumLine = QuotationLine.Minimum(Line, 0, rateLineItems, ZString.Empty);
			var baseRateLine = QuotationLine.BaseRate(Line, 0, rateLineItems, ZString.Empty);
			var perUnitLine = QuotationLine.PerUnit(Line, 0, rateLineItems, ZString.Empty);
			var maximumLine = QuotationLine.Maximum(Line, rateLineItems, ZString.Empty);
			var oneLineChargeOptions = description.IsEmpty ? RateDescriptionFlags(flags) : 0;

			if (IsSliding(rateLineItems))
			{
				items = GetSortedBreaks(rateLineItems).ToList();
				result.Add(description.IsEmpty ? QuotationLine.Header(Line, RateDescriptionFlags(flags)) : QuotationLine.New(Line, description));
				result.Add(minimumLine);
				result.Add(baseRateLine);
				AddQuoteItemsToResult(flags, items, result);
				result.Add(maximumLine);
			}
			else if (maximumLine == null && minimumLine == null && baseRateLine == null)
			{
				result.Add(QuotationLine.PerUnit(Line, oneLineChargeOptions, rateLineItems, description));
			}
			else if (maximumLine == null && perUnitLine == null && minimumLine == null)
			{
				result.Add(QuotationLine.BaseRate(Line, oneLineChargeOptions, rateLineItems, description));
			}
			else if (maximumLine == null && perUnitLine == null && baseRateLine == null)
			{
				result.Add(QuotationLine.Minimum(Line, oneLineChargeOptions, rateLineItems, description));
			}
			else if (minimumLine == null && perUnitLine == null && baseRateLine == null)
			{
				result.Add(QuotationLine.BaseRate(Line, oneLineChargeOptions, rateLineItems, description));
			}
			else
			{
				result.Add(description.IsEmpty ? QuotationLine.Header(Line, RateDescriptionFlags(flags)) : QuotationLine.New(Line, description));
				result.Add(minimumLine);
				result.Add(baseRateLine);
				result.Add(perUnitLine);
				result.Add(maximumLine);
			}

			AddDescriptionFromResult(description, result);

			return result;
		}

		void AddQuoteItemsToResult(GetQuotationLinesParam flags, IEnumerable<IRateLineItem> items, QuotationLineList result)
		{
			foreach (var item in items)
			{
				if (Line.ChargeCode.AC_SuppressOnQuoteIfZero && item.RateOperatorIsMinus() && item.TM_RelevantValue.IsEmpty && item.TM_FlatAmount.IsEmpty)
				{
					continue;
				}
				if (!item.RequiresWeightBreak())
				{
					continue;
				}
				result.AddRange(GetQuotationLinesWithPossibleFlatAmountOrMinimum(flags, item, items));
			}
		}

		static void AddDescriptionFromResult(ZString description, QuotationLineList result)
		{
			if (!description.IsEmpty)
			{
				for (var i = 1; i < result.Count; i++)
				{
					result[i].Shift();
				}
			}
		}

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			return GetQuotationLinesForIterator(flags, Line.ChildRateLineItems.ToList(), ZString.Empty);
		}

		protected virtual MultilingualString GetPlusUnit(IRateLineItem item, bool shouldAddContainerCode = false)
		{
			if (IsAccumulated && !item.IsOverPivotRate())
			{
				return ResString.GetMultilingualString("c9180def-77ea-412a-9cb1-93f22d1861fd", "per additional {0}", UnitDescription(!IsAccumulated));
			}
			else
			{
				return ResString.GetMultilingualString("bc46eb21-710e-4871-9d45-530f46c7b6a6", "per {0}", UnitDescription(!IsAccumulated, addContainerCode: shouldAddContainerCode));
			}
		}

		protected virtual QuotationLine GetBreakMinimumQuotationLine(IRateLineItem item)
		{
			return null;
		}

		static ZDecimal? GetFlatAmount(bool isAccumulated, IRateLineItem item, IEnumerable<IRateLineItem> items)
		{
			if (isAccumulated)
			{
				var prevItem = item.PrevRateLineItem(items);
				if (prevItem != null && prevItem.TM_FlatAmount == item.TM_FlatAmount && prevItem.TM_RelevantValue.IsEmpty)
				{
					return null;
				}
			}

			return item.TM_FlatAmount;
		}

		protected QuotationLine GetFlatAmountQuotationLine(IRateLineItem item, IEnumerable<IRateLineItem> items)
		{
			var flat = GetFlatAmount(IsAccumulated, item, items);

			if (flat == null)
			{
				return null;
			}

			var flatAmountLine = QuotationLine.NewWithValue(Line, 0, flat.Value, FlatAmountText, (NoResString)ZString.Empty);
			if (flatAmountLine != null)
			{
				flatAmountLine.Shift();
			}

			return flatAmountLine;
		}

		static string FlatAmountText => Res.GetString("738fb356-271b-40a4-bba6-07f922954d9a", "Flat Amount");

		protected QuotationLineList GetQuotationLinesWithPossibleFlatAmountOrMinimum(GetQuotationLinesParam flags, IRateLineItem item, IEnumerable<IRateLineItem> items)
		{
			var result = new QuotationLineList();
			var description = ItemBreakDescription(item);

			if (item.TM_CallForPricing)
			{
				result.Add(QuotationLine.New(Line, QuotationLineType.NoCurrency, description, CallForPricingReason(item), (NoResString)ZString.Empty));
			}
			else
			{
				var useFlatAmountAsFirstLine = item.TM_RelevantValue.IsEmpty && !item.TM_FlatAmount.IsEmpty;
				if (!useFlatAmountAsFirstLine)
				{
					var type = RateDescriptionFlags(flags);
					var shouldAddContainerCode = Calculator.ShouldAddContainerCode(type);

					var unit = item.RateOperatorIsMinus()
						? GetMinusUnit(shouldAddContainerCode)
						: GetPlusUnit(item, shouldAddContainerCode);

					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item, description, unit));
					result.Add(GetFlatAmountQuotationLine(item, items));
				}
				else
				{
					MultilingualString unit = (NoResString)ZString.Empty;
					if (!Line.ConversionFactor.IsEmpty && QuantityUnit.IsWeight(Line.TL_WeightVolume) && !Line.UseOnlyActualWeightMeasure())
					{
						unit = UnitText(Line.ConversionFactor.ToLongString());
					}
					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item.TM_FlatAmount, description, unit));
				}
				result.Add(GetBreakMinimumQuotationLine(item));
			}

			return result;
		}

		MultilingualString GetMinusUnit(bool shouldAddContainerCode) => ResString.GetMultilingualString("47250fd5-c065-4382-b5b3-68b097785cf2", "per {0}", UnitDescription(true, addContainerCode: shouldAddContainerCode));

		public override DocLineAmount GetDocLineAmount()
			=> GetDocLineAmountForIterator(Line.ChildRateLineItems.ToList(), ZString.Empty);

		protected DocLineAmount GetDocLineAmountForIterator(IEnumerable<IRateLineItem> items, ZString description)
		{
			var result = new DocLineAmount();

			var rateLineItems = items as IRateLineItem[] ?? items.ToArray();

			var min = QuotationLine.GetValue(Calculator.Items.Operator.MIN, rateLineItems);
			var baseRate = QuotationLine.GetValue(Calculator.Items.Operator.BAS, rateLineItems);
			var unit = QuotationLine.GetValue(Calculator.Items.Operator.UNT, rateLineItems);
			var max = QuotationLine.GetValue(Calculator.Items.Operator.MAX, rateLineItems);

			if (IsSliding(rateLineItems))
			{
				result.SetMin(Line.TL_RX_NKCurrency, min);
				result.SetFlat(Line.TL_RX_NKCurrency, baseRate, description);
				result.SetMax(Line.TL_RX_NKCurrency, max);

				var unitDescription = Line.Calculator.UnitDescription(true);
				result.SetUnit(Line.TL_RX_NKCurrency, unitDescription, unit, description);

				foreach (var item in items)
				{
					if (Line.ChargeCode.AC_SuppressOnQuoteIfZero && item.RateOperatorIsMinus() && item.TM_RelevantValue.IsEmpty && item.TM_FlatAmount.IsEmpty)
					{
						continue;
					}
					if (!item.RequiresWeightBreak())
					{
						continue;
					}
					var docLineAmount1 = GetDocLineAmountWithPossibleFlatAmountOrMinimum(item, items, description);
					result = result + docLineAmount1;
				}
			}
			else if (max == default && min == default && baseRate == default)
			{
				var unitDescription = Line.Calculator.UnitDescription(true);
				result.SetUnit(Line.TL_RX_NKCurrency, unitDescription, unit, description);
			}
			else if (max == default && unit == default && min == default)
			{
				result.SetFlat(Line.TL_RX_NKCurrency, baseRate, description);
			}
			else if (max == default && unit == default && baseRate == default)
			{
				result.SetMin(Line.TL_RX_NKCurrency, min);
			}
			else if (min == default && unit == default && baseRate == default)
			{
				result.SetFlat(Line.TL_RX_NKCurrency, baseRate, description);
			}
			else
			{
				result.SetMin(Line.TL_RX_NKCurrency, min);
				result.SetFlat(Line.TL_RX_NKCurrency, baseRate, description);

				var unitDescription = Line.Calculator.UnitDescription(true);
				result.SetUnit(Line.TL_RX_NKCurrency, unitDescription, unit, description);

				result.SetMax(Line.TL_RX_NKCurrency, max);
			}

			return result;
		}

		DocLineAmount GetDocLineAmountWithPossibleFlatAmountOrMinimum(IRateLineItem item, IEnumerable<IRateLineItem> items, string description)
		{
			var result = new DocLineAmount();
			var itemBreakDescription = ItemBreakDescription(item);

			if (!item.TM_CallForPricing)
			{
				var additionalDescription = GetAdditionalDescription(Calculator.GetQuotationLinesParam.ShowEquipmentType, item.ParentRateLine.ParentRateEntry);
				additionalDescription = additionalDescription.Replace("\r\n", "");

				var descriptionList = new[] { additionalDescription.Trim('-', ' ').ToString(), description, itemBreakDescription.ToString() };
				itemBreakDescription = string.Join(" - ", descriptionList.Where(x => !string.IsNullOrEmpty(x)));

				var useFlatAmountAsFirstLine = item.TM_RelevantValue.IsEmpty && !item.TM_FlatAmount.IsEmpty;
				if (!useFlatAmountAsFirstLine)
				{
					var unit = item.RateOperatorIsMinus()
						? GetMinusUnit(shouldAddContainerCode: false)
						: GetPlusUnit(item, shouldAddContainerCode: false);

					var flat = GetFlatAmount(IsAccumulated, item, items);
					var flatAmount = flat != null ? flat.Value : ZDecimal.Zero;
					result.SetSliding(Line.TL_RX_NKCurrency, itemBreakDescription, unit, item.TM_RelevantValue, flatAmount);
				}
				else
				{
					MultilingualString unit = (NoResString)ZString.Empty;
					if (!Line.ConversionFactor.IsEmpty && QuantityUnit.IsWeight(Line.TL_WeightVolume) && !Line.UseOnlyActualWeightMeasure())
					{
						unit = UnitText(Line.ConversionFactor.ToLongString());
					}

					result.SetSliding(Line.TL_RX_NKCurrency, itemBreakDescription, unit, 0m, item.TM_FlatAmount);
				}
			}

			return result;
		}

		static ResourceString UnitText(string unit) => ResString.GetMultilingualString("34623d5d-db8d-4008-a820-bace4cb50432", "({0})", unit);

		#endregion

		#region Calculation

		protected override (IEnumerable<CalculationResult> results, string error) CalculateResult(AutoRatingCalculatorParameters parameters)
		{
			if (Line.IsPivotBreakOverrideApplicable())
			{
				var itemsToOverride = Line.ChildRateLineItems.Where(i => i.RateOperatorIsMinus() || i.RateOperatorIsPlus()).ToArray();

				//There can't be only 1 minus break, if 1 minus break specified, there has to be atleast 1 plus break operator
				//There can be only 1 plus break possible, but if that break specified, it won't make any sense to override it as it will be same rate from 0 to nolimit
				if ((itemsToOverride.Length == 2 && itemsToOverride[0].TM_Break == itemsToOverride[1].TM_Break))
				{
					var maxPivotBreak = parameters.GetPivotBreakForSlidingCalculator(Line);

					if (maxPivotBreak > 0)
					{
						try
						{
							foreach (var item in itemsToOverride)
							{
								item.OverrideBreak(maxPivotBreak);
							}

							return base.CalculateResult(parameters);
						}
						finally
						{
							foreach (var item in itemsToOverride)
							{
								item.ResetBreakToOriginalValue();
							}
						}
					}
				}
			}

			return base.CalculateResult(parameters);
		}

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			CalculateForItems(Line.ChildRateLineItems.ToList(), calcOutput);
		}

		protected void CalculateForItems(IEnumerable<IRateLineItem> items, CalculatorOutput calcOutput)
		{
			InitMinMaxBase(items, calcOutput);

			if (IsCalculatePerTopPackWithBreakUnitsMode)
			{
				// This is a case when line unit is some kind of container/packaging (e.g. container, box, pallet, etc.)
				// and the break unit is weight/volume. For example, this is possible setup when line unit is CN and break unit is KG:
				// Container Weight is less than 1000 kg => $1000 per Container
				// Container Weight is more than 1000 kg => $900 per Container
				// Container Weight is more than 2000 kg => $800 per Container
				CalculatePerTopPackWithWeightVolumeBreaks(items, calcOutput);
			}
			else if (IsCalculatePerEquipmentAndWeightMode)
			{
				// This is a case when we want to calculate per both - container and its weight.
				// In this case, the Flat Rate on breaks will be per container rate and Unit Rate will be per weight.
				// The unit on the line will be weight unit.
				// Usually, it is used for over-pivot rates, like we calculate per container, but if the weight exceeds threshold, we want
				// to charge per each KG over those threshold. In this way a calculator will have this setup:
				// -500 KG, Base Rate $1000, Per Unit Rate 0$
				// +500 KG, Base Rate $1000, Per Unit Rate 5$
				// IsAccumulated: true
				// MultipleEquipmentsOverMaxWeightVolume: true
				//
				// So, if a container is 700 KG, it will calculate as the following:
				// $500 * 0$ per KG + 200 KG * 5$ per KG + Base $1000 = $1000 + 200KG * 5$
				CalculatePerEquipmentAndWeight(items, calcOutput);
			}
			else if (IsSliding(items))
			{
				// Here all other cases are covered when the break unit is the same as line unit. For example:
				// Weight is lees than 1000 KG => $3 per KG
				// Weight is more than 1000 KG => $2 per KG
				// Weight is more than 2000 KG => $1 per KG
				if (IsAccumulated)
				{
					CalculateAccumulated(items, calcOutput);
				}
				else
				{
					CalculateNonAccumulated(items, calcOutput);
				}
			}
			else
			{
				// Here we cover cases when there are no breaks at all. I.e. we have just a single per unit rate with UNT operator
				if (!Unit.IsEmpty)
				{
					var perUnit = GetValueByType(items, Items.Operator.UNT);
					CalculatePerUnit(calcOutput, new Quantity(perUnit, Unit));
				}
			}
		}

		protected void InitMinMaxBase(IEnumerable<IRateLineItem> items, CalculatorOutput calcOutput)
		{
			calcOutput.Minimum = GetValueByType(items, Items.Operator.MIN);
			calcOutput.Maximum = GetValueByType(items, Items.Operator.MAX);
			calcOutput.BaseRate = GetValueByType(items, Items.Operator.BAS);
		}

		#region Calculate per Top Pack with weight/volume break

		/// <summary>
		/// "Per Top Pack" refer to either containers for FCL jobs, or pack lines. See WI00095747 for the introduction of the unit "PK" for "Any Pack Type".
		///
		/// For Fully Containerized jobs' matching criteria includes the "Container/ULD Type and/or Class" due to the container/ULD being the most outer pack
		///
		/// Pack Types are used to further consolidate the cargo whether the cargo is containerized or loose
		/// and is now also considered as part of the jobs' matching criteria due to the pack type
		/// being considered as the outer pack of each pack line on a job.
		/// Pack Type matching is only considered when the charge is chargeable per package.
		/// </summary>
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CalculatePerTopPackWithWeightVolumeBreaks(IEnumerable<IRateLineItem> items, CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var breakUnit = BreakUnit;
			var topPacks = GetTopPacks(parameters);

			if (!CanCalculatePerTopPackWithWeightVolumeBreaks(parameters, topPacks, breakUnit, out var error))
			{
				calcOutput.FailureMessage = error;
				return;
			}

			foreach (var topPack in topPacks)
			{
				var amountForBreakSearch = topPack.GetPerPackAmount(breakUnit, parameters, Line);
				var chargeableUnit = GetChargeableUnit(Unit);
				var chargeableAmount = new Quantity(topPack.Count, chargeableUnit, reference: topPack.Reference);

				CalculateNonAccumulated(items, amountForBreakSearch, chargeableAmount, Unit, calcOutput);
			}
		}

		bool CanCalculatePerTopPackWithWeightVolumeBreaks(AutoRatingCalculatorParameters parameters, IEnumerable<TopLevelPack> topPacks, string breakUnit, out string error)
		{
			error = string.Empty;

			if (parameters.Criteria.ConsumerType == JobInvoicingConsumerTypes.eManifest)
			{
				return true;
			}

			var breakIsWeightVolume = Constants.Weight.ContainsCode(breakUnit) || Constants.Volume.ContainsCode(breakUnit);
			var breakIsPackage = breakUnit != QuantityUnit.TU && !breakIsWeightVolume;

			var anyZeroWeightVolumePackage = topPacks != null &&
				(topPacks.Any(x => breakIsWeightVolume && x.Weight == 0 && x.Volume == 0) || topPacks.Any(x => breakIsPackage && x.Packages == 0));

			if (topPacks == null || anyZeroWeightVolumePackage)
			{
				if (parameters.Criteria.ConsumerType == JobInvoicingConsumerTypes.Shipment ||
					parameters.Criteria.ConsumerType == JobInvoicingConsumerTypes.QuotedBooking)
				{
					error = Res.GetString("8029355f-e2d8-4c53-8e7c-83585d34938c",
						"Weight / Volume / Packages information was not specified for Pack Lines on this job. Rating based on container weight / volume / packages cannot be performed.");
				}
				else if (parameters.Criteria.ConsumerType == JobInvoicingConsumerTypes.Brokerage)
				{
					error = Res.GetString("3c0c6da6-3637-44f4-b2c0-dd4b08809d80",
						"Weight information was not specified for the Containers on this job. Please enter weight details on the Containers tab (on the Declaration screen) and then autorate again.");
				}
				else
				{
					if (anyZeroWeightVolumePackage)
					{
						error = Res.GetString("9b794364-511d-4276-99d2-fb4dda086704",
							"{0} - Weight / Volume / Packages information was not specified on this job.", parameters.Criteria.ConsumerType.Description);
					}
					else
					{
						error = Res.GetString("d6de776e-2eab-4b42-8ac2-7f75f9dcfd05",
							"{0} doesn't support rating based on container weight / volume / packages.",
							parameters.Criteria.ConsumerType.Description);
					}
				}
			}
			else if (parameters.Criteria.ConsumerType == JobInvoicingConsumerTypes.Brokerage && !Constants.Weight.ContainsCode(breakUnit))
			{
				error = Res.GetString("8d723fc5-8505-4542-82d3-97fdb01980dc",
					"declaration charge ({0}) as it is based on container volume or packages. Declaration charges can only be based on container weight.",
					Line.ChargeCode.AC_Code);
			}

			if (breakUnit == QuantityUnit.TU & !topPacks.All(x => x.Teu > 0))
			{
				error = Res.GetString("25aa5c91-9672-4fe3-8377-7f7dbb95e913",
					"containers in this job missing TEU information.");
			}

			return string.IsNullOrEmpty(error);
		}

		/// <summary>
		/// Only for units "CN", "PK", or a package type, and where the break is weight.
		/// </summary>
		IEnumerable<TopLevelPack> GetTopPacks(AutoRatingCalculatorParameters parameters)
		{
			var topLevelPacks = new List<TopLevelPack>();

			var measureType = parameters.GetMeasureTypes(Line).FirstOrDefault();
			// Note: MeasureType.Unit is for units that are a specific Package Type (like "Box" or "Palette"), or a distance.
			// MeasureType.Package is for a unit of "PK" - "Any Package Type", or "JP" - "Job Package".
			if (measureType == MeasureType.Package)
			{
				measureType = MeasureType.Unit;
			}

			var parts = parameters.GetParts(measureType, Line);

			if (parts == null)
			{
				ErrorReporter.ReportOnce("GetTopPacks_NullReferenceException", "parameters.GetParts() returns null");   // Error report
				parts = Enumerable.Empty<IRateablePart>();
			}

			foreach (var part in parts)
			{
				if (part is IRateableContainer container)
				{
					if (container.ContainerCount <= 0)
					{
						continue;
					}

					topLevelPacks.Add(new TopLevelPack
					{
						Weight = container.ContainerGrossWeight.AmountFor(QuantityUnit.KG).Amount,
						Volume = container.ContainerVolumeInM3,
						Teu = container.TEU,
						Packages = container.ContainerPackages,
						Reference = container.Reference,
						Count = container.ContainerCount
					});
				}
				else if (measureType == MeasureType.WarehousePackage)
				{
					if (!part.PackageCount.HasValue || part.PackageCount == 0)
					{
						continue;
					}

					topLevelPacks.Add(new TopLevelPack
					{
						Weight = part.WeightMeasure.Actual,
						Volume = part.VolumeMeasure.Actual,
						Count = part.PackageCount.Value
					});
				}
			}

			return topLevelPacks;
		}

		ZString GetChargeableUnit(string unit)
		{
			// For containers we use the container code as a Unit so that the unit is 20GP, 40GP and not CN
			if (unit == QuantityUnit.CN && Line.ParentRateEntry.Container != null)
			{
				unit = Line.ParentRateEntry.Container.RC_Code;
			}

			return unit;
		}

		class TopLevelPack
		{
			public decimal Weight { get; set; }
			public decimal Volume { get; set; }
			public decimal Teu { get; set; }
			public decimal Packages { get; set; }
			public string Reference { get; set; }
			public decimal Count { get; set; }

			public Quantity GetPerPackAmount(string unit, AutoRatingCalculatorParameters parameters, IRateLine rateLine)
			{
				if (Constants.Weight.ContainsCode(unit) || Constants.Volume.ContainsCode(unit))
				{
					var weight = new Quantity(Weight / Count, QuantityUnit.KG, reference: Reference);
					var volume = new Quantity(Volume / Count, QuantityUnit.M3, reference: Reference);
					var chargeable = parameters.CalculateChargeable(rateLine, weight, volume, unit);
					return chargeable;
				}

				if (unit == QuantityUnit.TU)
				{
					return new Quantity(Teu, unit, reference: Reference);
				}

				return new Quantity(Packages, unit, reference: Reference);
			}
		}

		#endregion

		#region Calculate Per Eqipment

		bool IsCalculatePerEquipmentAndWeightMode => MultipleEquipmentsOverMaxWeightVolume && QuantityUnit.IsWeight(Line.TL_WeightVolume);

		void CalculatePerEquipmentAndWeight(IEnumerable<IRateLineItem> items, CalculatorOutput calcOutput)
		{
			if (!QuantityUnit.IsWeight(Line.TL_WeightVolume))
			{
				calcOutput.FailureMessage = Res.GetString(
					"7CD92098-4586-40E0-9F03-EB4CA13CBAA7",
					"Not autorated. When using 'Multiple Equipment over Max Weight/Volume', only weight unit is supported as chargeable.");
				return;
			}

			// It is essential to use chargeable containers rather than containers from measures so that we calculate based
			// on matched containers rather than all containers. AutoRating does matching rate lines to measure parts prior to calculation,
			// and we must respect this here.
			var containers = calcOutput.Parameters.GetChargeableContainers(Line);
			if (!containers.Any())
			{
				calcOutput.FailureMessage = Res.GetString(
					"C834B30B-6877-4CC0-99A2-2BA928E6FBFA",
					"Not autorated. When using 'Multiple Equipment over Max Weight/Volume', container information should be specified for job.");
				return;
			}

			foreach (var container in containers)
			{
				if (container.ContainerWeightInKG == 0)
				{
					calcOutput.FailureMessage = Res.GetString(
						"F5771D3E-511F-44C5-8C55-21BA71B81828",
						"Not autorated. When using 'Multiple Equipment over Max Weight/Volume', weight should be specified on containers.");
					return;
				}

				var occupiedContainersCount = calcOutput.Parameters.GetOccupiedContainerCount(Line, container);
				var containerCount = occupiedContainersCount > 0
					? occupiedContainersCount
					: container.ContainerCount > 0
						? container.ContainerCount
						: 1;

				// Make sure the weight is in the rate unit
				var totalWeight = new Quantity(container.ContainerWeightInKG, Constants.Weight.Kilograms);
				totalWeight = calcOutput.Parameters.CalculateChargeable(Line, totalWeight, targetUnit: Line.TL_WeightVolume);

				var perContainerChargeableQuantity = new Quantity(
					totalWeight.Amount / containerCount,
					totalWeight.Unit,
					totalWeight.Source,
					totalWeight.Reference);

				perContainerChargeableQuantity = AdjustChargeable(calcOutput.Parameters, perContainerChargeableQuantity);

				var perContainerChargeable = RoundAmount(perContainerChargeableQuantity);

				for (var i = 1; i <= containerCount; i++)
				{
					if (IsSliding())
					{
						if (IsAccumulated)
						{
							CalculateAccumulated(items, perContainerChargeable, calcOutput);
						}
						else
						{
							CalculateNonAccumulated(items, perContainerChargeable, perContainerChargeable, Line.TL_WeightVolume, calcOutput);
						}
					}
					else
					{
						var perUnit = GetValueByType(items, Items.Operator.UNT);
						CalculatePerUnit(calcOutput, new Quantity(perUnit, Line.TL_WeightVolume), perContainerChargeable);
					}
				}

				var mergedBases = MergePaymentBases(calcOutput.PaymentBases);
				calcOutput.Set(mergedBases.ToList());
			}
		}

		#endregion

		#region Calculate Non Accumulated

		void CalculateNonAccumulated(IEnumerable<IRateLineItem> items, CalculatorOutput calcOutput)
		{
			if (Line.TL_UnitFactor == UnitFactorList.Codes.PacksWeight && RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.Value)
			{
				var measureType = RatingCache.GetMeasureTypeFromUnit(Line.TL_WeightVolume);
				if (measureType != MeasureType.Weight)
				{
					calcOutput.IsEmpty = true;
					return;
				}
			}

			var breakAmount = ChargeableAmountForBreakSearch(calcOutput.Parameters);
			var chargeableAmount = ChargeableAmount(calcOutput.Parameters);
			CalculateNonAccumulated(items, breakAmount, chargeableAmount, Unit, calcOutput);
		}

		protected void CalculateNonAccumulated(IEnumerable<IRateLineItem> items, Quantity breakAmount, Quantity chargeableAmount, ZString unit, CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var item = GetBreakItem(items, parameters, breakAmount);
			if (item == null)
			{
				calcOutput.IsEmpty = true;
				return;
			}

			if (UseHigherChargeableLowerRateRule)
			{
				var nextItem = item.NextRateLineItem(items);
				if (nextItem != null && IsNextBreakCheaperThanCurrentBreak(item, nextItem, breakAmount))
				{
					item = nextItem;
					chargeableAmount = new Quantity(nextItem.TM_Break, breakAmount.Unit, breakAmount.Source, breakAmount.Reference, Res.GetString("d7a2f7e3-9e20-4d27-bb26-1d5e2e08485a", "HBLR is applied"));
				}
			}

			if (parameters.Criteria.WeightBreakOverride.HasValue && parameters.Criteria.WeightBreakOverride.Value != 0m && chargeableAmount.IsWeight() && _Rating.Sell)
			{
				chargeableAmount = new Quantity(parameters.Criteria.WeightBreakOverride.Value, parameters.Criteria.WeightBreakOverrideUnit);
			}

			var perUnit = item.TM_RelevantValue;
			calcOutput.Minimum = Math.Max(calcOutput.Minimum, item.TM_BreakMinimum);

			var breakDescription = !string.IsNullOrEmpty(breakAmount.Description)
				? (string)breakAmount.Description
				: IsBreakUnitDifferentToChargeableUnit || perUnit == 0
					? (string)breakAmount.Unit
					: string.Empty;

			var perUnitDescription = !string.IsNullOrEmpty(breakDescription)
				? Res.GetString("8d8879db-3637-4110-8660-76246f464fe8", "for {0} {1}", breakAmount.Amount.ToString("G26", Culture.CurrentCompanyCountryCulture), breakDescription)
				: string.Empty;

			if (string.IsNullOrEmpty(unit))
			{
				ErrorReporter.ReportOnce
				(
					"EmptyUnitInBaseCombinedCalculator",
					$@"Rate Type: {ParentRatingHeader.RateTypeSafe()}
Service Provider/Client: {ParentRatingHeader.Header?.NameAndCode ?? ""}
RateEntry: {ParentRateEntry.TI_RateCategory}-{ParentRateEntry.TI_Mode} {ParentRateEntry.TI_OriginLRC}>{ParentRateEntry.TI_ViaLRC}>{ParentRateEntry.TI_DestinationLRC}
Charge: {Line.ChargeCode.AC_Code}
Calculator: {Line.RateCalculatorType}"
				);

				var errorMessage = Res.GetString("3b29bd58-8935-4528-9588-5f50bb8d331f",
							"Unit is blank for CMB Calculator");
				calcOutput.FailureMessage = errorMessage;
				return;
			}

			CalculatePerUnit(calcOutput, new Quantity(perUnit, unit, description: perUnitDescription), chargeableAmount);

			if (MultipleEquipmentsOverMaxWeightVolume)
			{
				// In this mode the break amount and price is related to equipment weight while the base rate is related to the base price for
				// the equipment itself. So, basically the base price is per equipment price and thus must be calculated as Per Unit rather than as Flat.
				CalculatePerUnit(calcOutput, new Quantity(item.TM_FlatAmount, Unit), new Quantity(1, Unit));
			}
			else
			{
				AddFlatAmountToLastCalculation(calcOutput, item.TM_FlatAmount, perUnitDescription);
			}
		}

		bool IsNextBreakCheaperThanCurrentBreak(IRateLineItem currentItem, IRateLineItem nextItem, Quantity breakAmount)
		{
			var currentBreakTotal = currentItem.TM_FlatAmount + breakAmount.Amount / UnitMultiplier * currentItem.TM_RelevantValue;
			var nextBreakTotalNextItem = nextItem.TM_FlatAmount + nextItem.TM_Break / UnitMultiplier * nextItem.TM_RelevantValue;

			return currentBreakTotal > nextBreakTotalNextItem;
		}

		#endregion

		#region Calculate Accumulated

		void CalculateAccumulated(IEnumerable<IRateLineItem> items, CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var chargeableAmount = BreakUnit == Line.TL_WeightVolume
				? ChargeableAmount(parameters)
				: ChargeableAmountForBreakSearch(parameters);

			if (chargeableAmount.IsEmpty)
			{
				return;
			}

			if (IsBreaksPerContainerTypeOrClass || IsBreaksPerContainer)
			{
				var chargeableByContainer = parameters.GetChargeableByContainer(Line);
				if (IsBreaksPerContainer)
				{
					foreach (var chargeable in chargeableByContainer)
					{
						CalculateAccumulated(items, chargeable, calcOutput);
					}

					var mergedBases = MergePaymentBases(calcOutput.PaymentBases);
					calcOutput.Set(mergedBases.ToList());
				}
				else
				{
					var itemBreakMultiplier = Math.Max(chargeableByContainer.Count(), 1);
					CalculateAccumulated(items, chargeableAmount, calcOutput, itemBreakMultiplier, addFlatPaymentBasis: false);
					AddFlatPaymentBasisPerContainer(items, chargeableByContainer, calcOutput);
				}
			}
			else
			{
				CalculateAccumulated(items, chargeableAmount, calcOutput);
			}
		}

		protected void CalculateAccumulated(IEnumerable<IRateLineItem> items, Quantity chargeableAmount, CalculatorOutput calcOutput, int itemBreakMultiplier = 1, bool addFlatPaymentBasis = true)
		{
			var lineItems = GetSortedBreaks(items).ToList();
			var isFirstIteration = true;

			int i;
			for (i = 0; i < lineItems.Count; i++)
			{
				var item = lineItems[i];
				var unitPrice = new Quantity(item.TM_RelevantValue, chargeableAmount.Unit);

				if (item.RateOperatorIsMinus())
				{
					var amount = Math.Min(chargeableAmount.Amount, item.TM_Break * itemBreakMultiplier);
					CalculatePerUnit(calcOutput, unitPrice, new Quantity(amount, chargeableAmount.Unit));
				}
				else if (item.RateOperatorIsPlus())
				{
					var itemBreak = isFirstIteration ? 0m : item.TM_Break * itemBreakMultiplier;

					if (UseInclusiveBreaks ? chargeableAmount.Amount <= itemBreak : chargeableAmount.Amount < itemBreak)
					{
						break;
					}

					if (item.TM_CallForPricing)
					{
						var reason = Res.GetString("7430f346-5e6f-4774-a014-702033d9dca6", "Not autorated. This charge has Rate Restriction for the relevant break, and also is set to 'Use Accumulated' amounts and therefore cannot be autorated.");

						calcOutput.CalculationLog.Steps.Clear();
						calcOutput.FailureMessage = reason;
						return;
					}

					if (i + 1 < lineItems.Count)
					{
						var nextLineItemBreak = lineItems[i + 1].TM_Break * itemBreakMultiplier;
						var amount = Math.Min(chargeableAmount.Amount - itemBreak, nextLineItemBreak - itemBreak);
						CalculatePerUnit(calcOutput, unitPrice, new Quantity(amount, chargeableAmount.Unit));
					}
					else
					{
						var amount = chargeableAmount.Amount - itemBreak;
						CalculatePerUnit(calcOutput, unitPrice, new Quantity(amount, chargeableAmount.Unit));
					}
				}

				isFirstIteration = false;
			}

			var flatAmount = i > 0 ? lineItems[i - 1].TM_FlatAmount : (ZDecimal)0m;

			if (MultipleEquipmentsOverMaxWeightVolume)
			{
				// In this mode the break amount and price is related to equipment weight while the base rate is related to the base price for
				// the equipment itself. So, basically the base price is per equipment price and thus must be calculated as Per Unit rather than as Flat.
				CalculatePerUnit(calcOutput, new Quantity(flatAmount, Unit), new Quantity(1, Unit));
			}
			else if (addFlatPaymentBasis)
			{
				AddFlatAmountToLastCalculation(calcOutput, flatAmount);
			}
		}

		void AddFlatPaymentBasisPerContainer(IEnumerable<IRateLineItem> items, IEnumerable<Quantity> chargeableByContainer, CalculatorOutput calcOutput)
		{
			var results = new List<PaymentBasis>();
			foreach (var chargeable in chargeableByContainer)
			{
				var breakItem = GetBreakItemByAmount(chargeable, items, UseInclusiveBreaks);

				if (breakItem != null)
				{
					AddFlatAmountToLastCalculation(calcOutput, breakItem.TM_FlatAmount);
				}
			}

			calcOutput.Add(results);
		}

		#endregion

		#region Chargeable Amount

		protected override Quantity ChargeableAmountInternal(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			var chargeable = base.ChargeableAmountInternal(parameters, unit);
			var adjustedChargeable = AdjustChargeable(parameters, chargeable);
			return adjustedChargeable;
		}

		protected override Quantity ChargeableAmountForBreakSearch(AutoRatingCalculatorParameters parameters)
		{
			var criteria = parameters.Criteria;

			if (criteria.WeightBreakOverride.HasValue && criteria.WeightBreakOverride.Value != 0m && Constants.Weight.ContainsCode(BreakUnit))
			{
				return new Quantity(criteria.WeightBreakOverride.Value, criteria.WeightBreakOverrideUnit);
			}

			if (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.Value)
			{
				switch (Line.TL_UnitFactor.ToString())
				{
					case UnitFactorList.Codes.PacksWeight:
						return parameters.GetPacksWeightForBreakSearch(Line, BreakUnit);
				}
			}

			if (IsBreakUnitDifferentToChargeableUnit)
			{
				var result = ChargeableAmountInternal(parameters, BreakUnit);
				return RoundAmount(result);
			}

			return base.ChargeableAmountForBreakSearch(parameters);
		}

		/// <summary>
		/// Adjusts the chargeable amount according to a MIN break with a
		/// zero rate.
		/// </summary>
		Quantity AdjustChargeable(AutoRatingCalculatorParameters parameters, Quantity chargeable)
		{
			if (adjustedChargeableCache.TryGetValue(chargeable, out var cachedValue))
			{
				return cachedValue;
			}

			var minItem = Line.FindRateLineItem(Items.Operator.MIN);
			if (minItem == null || minItem.TM_Break.IsEmpty)
			{
				adjustedChargeableCache[chargeable] = chargeable;
				return chargeable;
			}

			var minChargeable = new Quantity(
				minItem.TM_Break,
				Line.TL_WeightVolume,
				source: (NoResString)"Rate",
				reference: (NoResString)"Minimum chargeable amount",
				label: (NoResString)"Min"
			);

			if (!minChargeable.UnitType.HasValue || minChargeable.UnitType != chargeable.UnitType)
			{
				adjustedChargeableCache[chargeable] = chargeable;
				return chargeable;
			}

			if (minChargeable.UnitType.Value == ZUnitType.Weight)
			{
				// value if there is only one break.
				var overridenPivotWeight = parameters.GetPivotBreakForSlidingCalculator(Line);
				if (overridenPivotWeight > 0)
				{
					minChargeable = new Quantity(
						overridenPivotWeight,
						Constants.Weight.Kilograms,
						(NoResString)"Job Container",
						(NoResString)"Overriden pivot weight");
				}
			}

			var minChargeableSameUnit = minChargeable.Convert(chargeable.Unit);
			var adjustedChargeable = minChargeableSameUnit.Amount > chargeable.Amount
				? minChargeable
				: chargeable;

			if (chargeable.Amount != adjustedChargeable.Amount)
			{
				parameters.Logger.Log(LogType.Information,
					Invariant(
						$"Minimum Chargeable Weight {adjustedChargeable} is used instead of Chargeable Weight {chargeable} because charge {Line.ChargeCode?.AC_Code} uses Minimum Weight as the Minimum Chargeable.")); // Just a log string
			}

			adjustedChargeableCache[chargeable] = adjustedChargeable;
			return adjustedChargeable;
		}

		readonly Dictionary<Quantity, Quantity> adjustedChargeableCache = new Dictionary<Quantity, Quantity>();

		#endregion

		#endregion

		#region Base Calculator

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		protected override bool IsBreakUnitAvailableCore()
		{
			if (CalculationPerTopPackWithBreakUnitsAvailable)
			{
				return true;
			}

			var canHaveDistanceUnitAsBreakUnit = !IsAccumulated && !Unit.IsEmpty && !QuantityUnit.IsDistance(Unit) && !Line.ParentRateEntry.IsFreightEntry();
			if (canHaveDistanceUnitAsBreakUnit)
			{
				return true;
			}

			return false;
		}

		#endregion
	}
}
