using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty("ExcludeHolidays", IsCalculatedField = true, MapTo = "String1", RelatedTo = "ExcludeHolidays")]
	[CalculatorProperty(Items.ExcludeHolidays, RateLineItem.Schema.TM_Text, IsMandatory = true)]
	public class TimeCalculator : BaseCombinedCalculator
	{
		public TimeCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.Time;

		#region Initialisation

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var shouldSetAccumlated = FindRateLineItem(BaseCombinedCalculator.Items.UseAccumulated) == null;
			var shouldSetExcludeHolidays = FindRateLineItem(Items.ExcludeHolidays) == null;

			var itemList = base.CheckOrCreateItems();

			if (shouldSetAccumlated)
			{
				IsAccumulated = true;
			}
			if (shouldSetExcludeHolidays)
			{
				var excludeHolidaysItem = itemList.FirstOrDefault(item => item.TM_Type == Items.ExcludeHolidays) as RateLineItem;

				if (excludeHolidaysItem != null)
				{
					using (excludeHolidaysItem.SuspendSettingHasChanges())
					using (excludeHolidaysItem.GetValidationSuspender())
					{
						if (Env.Registry.Rating.ExcludeHolidaysInTimeRating)
						{
							ExcludeHolidays = Items.ExcludeWeekendsAndPublicHolidays;
						}
					}
				}
			}

			return itemList;
		}

		#endregion

		#region Overrides

		protected override bool CalculationPerTopPackWithBreakUnitsAvailable
		{
			get { return false; }
		}

		protected override bool IsBreakUnitAvailableCore()
		{
			return true;
		}

		#endregion

		#region Properties

		#region TimeUnit

		public ZString TimeUnit
		{
			get
			{
				var item = Line.ChildRateLineItems.FirstOrDefault(x => x.RateOperatorIsUNT() || x.IsLowestBreak());
				if (item != null && !item.TM_BreakWeightVolume.IsEmpty)
				{
					return item.TM_BreakWeightVolume;
				}

				return QuantityUnit.DY;
			}
		}

		#endregion

		[MaxLength(3)]
		public ZString ExcludeHolidays
		{
			get { return (ZString)this[Items.ExcludeHolidays]; }
			set { this[Items.ExcludeHolidays] = value; }
		}

		public ZPropertyInfo ExcludeHolidaysInfo
		{
			get
			{
				var excludeHolidaysItem = FindRateLineItem(Items.ExcludeHolidays);
				if (excludeHolidaysItem != null)
				{
					return excludeHolidaysItem.TM_TextInfo;
				}
				else
				{
					return DummyRateLineItem.TM_TextInfo;
				}
			}
		}

		public override TimeInfo.Exclusion ExcludedHolidays
		{
			get
			{
				switch (ExcludeHolidays)
				{
					case Items.ExcludeWeekendsAndPublicHolidays:
						return TimeInfo.Exclusion.WeekendsPublicHolidays;

					case Items.ExcludeSundaysAndPublicHolidays:
						return TimeInfo.Exclusion.Sundays | TimeInfo.Exclusion.PublicHolidays;

					default:
						return 0;
				}
			}
		}

		#endregion

		#region Validation

		public override void ValidateTM_BreakWeightVolume(RateLineItem lineItem)
		{
			if (!lineItem.TM_BreakWeightVolumeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(lineItem.TM_BreakWeightVolumeInfo);
				if (!lineItem.TM_BreakWeightVolume.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(lineItem.TM_BreakWeightVolumeInfo, lineItem.Lookups.TimeUnits);
				}
			}

			if (Line.TL_WeightVolume == lineItem.TM_BreakWeightVolume)
			{
				lineItem.Parent.Validation.ValidateTL_WeightVolume();
			}
		}

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			base.ValidateTM_Text(lineItem);

			if (lineItem.TM_Type == Items.ExcludeHolidays && !lineItem.TM_Text.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, List1);
			}
		}

		#endregion

		#region Lists

		CodeDescriptionPairList fHolidayExclusionList;
		public override CodeDescriptionPairList List1
		{
			get
			{
				if (fHolidayExclusionList == null)
				{
					fHolidayExclusionList = new CodeDescriptionPairList();
					fHolidayExclusionList.AddPair(Items.ExcludeWeekendsAndPublicHolidays, Items.ExcludeWeekendsAndPublicHolidaysDescription);
					fHolidayExclusionList.AddPair(Items.ExcludeSundaysAndPublicHolidays, Items.ExcludeSundaysAndPublicHolidaysDescription);
				}
				return fHolidayExclusionList;
			}
		}

		#endregion

		#region Quotation Lines

		protected internal override MultilingualString UnitDescription(bool showConversionFactor, ZString unit = default, bool addContainerCode = false)
		{
			var unitDescription = UnitDescriptionInternal(Line.TL_WeightVolume, unitMultiplier: Line.TL_WeightVolumeMultiple, addContainerCode: addContainerCode);
			var timeUnitDescription = UnitDescriptionInternal(TimeUnit, addContainerCode: addContainerCode);

			return ResString.GetMultilingualString("cc1515b3-57a0-4fa1-ba70-ac5e3f1b996c", "{0} x {1}", unitDescription, timeUnitDescription);
		}

		protected override ZString BreakUnitDescription
		{
			get { return UnitDescriptionInternal(TimeUnit, addPlural: true); }
		}

		protected override MultilingualString GetPlusUnit(IRateLineItem item, bool shouldAddContainerCode = false)
		{
			return ResString.GetMultilingualString("51c980cc-4f59-4601-842a-2fd57dfb7e5e", "per {0}", UnitDescription(false));
		}

		#endregion

		#region Calculation

		protected override (IEnumerable<CalculationResult> results, string error) CalculateResult(AutoRatingCalculatorParameters parameters)
		{
			var timeUnit = TimeUnit;
			var amounts = new TimeChargeableCalculationStrategy(this).GetMultipleChargeableAmounts(parameters, timeUnit);
			if (amounts.Count == 0)
			{
				var emptyResult = new CalculationResult(Line, Enumerable.Empty<PaymentBasis>(), Unit);
				return (new[] { emptyResult }, string.Empty);
			}

			var items = Line.ChildRateLineItems.ToList();
			var lineUnit = Line.TL_WeightVolume;
			bool isSliding = IsSliding(items);
			List<PaymentBasis> allPayments = new List<PaymentBasis>();
			var lineUnitAmount = ChargeableAmount(parameters);
			bool isTimePerContainer = amounts[0].ContainerCount > 0;
			bool useContainerCount = false;

			if (lineUnit == QuantityUnit.CN && isTimePerContainer)
			{
				// Use container count from amounts rather than container count from lineUnitAmount
				// since some containers may not have the service.
				// E.g., there may be 3 20GP containers, but only one container has the service.
				// The lineUnitAmout will be 3, but the ContainerCount on the RepeatedQuantity has the real count of 1.
				useContainerCount = true;
			}
			else if (lineUnit == QuantityUnit.SV)
			{
				// The line unit is per service so the lineUnitAmount is the service count.
				// However, the repeat count on the amounts is also the service count.
				// We can ignore the lineUnitAmount, which already happens in the main loop below,
				// to avoid double counting.
			}
			else
			{
				var sum = RepeatedQuantity.Sum(amounts, timeUnit);
				amounts = new[] { new RepeatedQuantity(sum, lineUnitAmount.Amount, 0) };
			}

			foreach (var repeatedQuantity in amounts.Where(x => !x.Quantity.IsEmpty))
			{
				var amount = repeatedQuantity.Quantity;
				var repeatCount = repeatedQuantity.RepeatCount;
				if (useContainerCount)
				{
					repeatCount *= repeatedQuantity.ContainerCount;
				}
				var calcLog = new CalculationLog();
				InitializeCalculationLog(this, calcLog, parameters, Line, lineUnit);
				var calcOutput = new CalculatorOutput(parameters, calcLog);
				InitMinMaxBase(items, calcOutput);
				if (isSliding)
				{
					if (IsAccumulated)
					{
						CalculateAccumulated(items, amount, calcOutput);
					}
					else
					{
						CalculateNonAccumulated(items, amount, amount, TimeUnit, calcOutput);
					}
				}
				else
				{
					var perUnit = GetValueByType(items, Items.Operator.UNT);
					CalculatePerUnit(calcOutput, new Quantity(perUnit, TimeUnit), amount);
				}

				AdjustPaymentDescriptions(calcOutput, isSliding || IsAccumulated, amount, repeatCount, lineUnitAmount.Unit);

				AddBaseMinMax(calcOutput);

				if (calcOutput.PaymentBases.Any())
				{
					allPayments.AddRange(calcOutput.PaymentBases);
				}
			}

			var result = new CalculationResult(Line, allPayments, Unit);
			SetAttributes(result, parameters, Line, lineUnit);
			return (new[] { result }, string.Empty);
		}

		protected void AdjustPaymentDescriptions(
			CalculatorOutput calcOutput,
			bool isSlidingOrAccumulated,
			Quantity timeAmount,
			decimal repeatCount,
			string lineUnit)
		{
			string newChargeableDescriptionReference = !timeAmount.Reference.IsEmpty
				? " (" + timeAmount.Reference + ')'
				: string.Empty;

			var parameters = calcOutput.Parameters;
			var bases = new List<PaymentBasis>();
			foreach (var basis in calcOutput.PaymentBases)
			{
				var newChargeable = new Quantity(repeatCount * basis.Chargeable.Amount, GetMultiplicationUnits(lineUnit, basis.Chargeable.Unit), reference: timeAmount.Reference);
				ZString newChargeableDescription;

				newChargeableDescription = GetChargeableDescription(
					repeatCount,
					UnitDescriptionInternal(lineUnit, addPlural: true) + newChargeableDescriptionReference,
					basis.Chargeable.Amount,
					basis.ChargeableUnitDescription ?? basis.Chargeable.Unit);

				var changedBasis = parameters.Criteria.CreatePaymentBasis(basis.RateInfo, newChargeable, basis.RateInfo.UnitDescription, newChargeableDescription);
				bases.Add(changedBasis);
			}

			calcOutput.Set(bases);
		}

		ZString GetChargeableDescription(ZDecimal chargeableAmount, ZString chargeableDescription, ZDecimal timeAmount, ZString timeDescription)
		{
			if (chargeableAmount == 1 && timeAmount == 1)
			{
				return ZString.Empty;
			}

			return Res.GetString("ae7d5f3f-cab6-450e-827d-6a21c5322515", "{0} {1} x {2} {3}", chargeableAmount.ToString("G26", Culture.CurrentCompanyCountryCulture), chargeableDescription, timeAmount.ToString("G26", Culture.CurrentCompanyCountryCulture), timeDescription);
		}

		ZString GetMultiplicationUnits(ZString chargeableUnit, ZString perUnitChargeableUnit)
		{
			return Res.GetString("30859ef4-7292-4540-bbbb-f2ee321fba4a", "{0} x {1}", chargeableUnit, perUnitChargeableUnit);
		}

		Quantity TimeAmount(AutoRatingCalculatorParameters parameters)
		{
			return TimeAmountInternal(parameters, TimeUnit);
		}

		protected override Quantity ChargeableAmountForBreakSearch(AutoRatingCalculatorParameters parameters)
		{
			return TimeAmount(parameters);
		}

		public override ZString BreakUnit => TimeUnit;

		protected override bool IsBreakUnitDifferentToChargeableUnit => false;

		#endregion

		#region Calculator Description Conversion

		protected override bool IsCalculatorDescriptionSupported => true;

		protected override string ConvertCalculatorDescription()
		{
			var convertor = new TimeCalculatorDescriptionConverter(this);
			return convertor.Convert();
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;
	}
}

