using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class EquipmentHireCalculator : Calculator
	{
		public EquipmentHireCalculator(IRateLine master)
			: base(master)
		{
		}

		// Not moved to RatingCalculatorCodes since EQH calculaor is currently disabled.
		public const string Code = "EQH";

		protected override bool IsBreakUnitAvailableCore()
		{
			return true;
		}

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			MandatoryValidation.CheckEntered(lineItem.TM_TextInfo);

			if (!lineItem.TM_Text.IsEmpty)
			{
				foreach (var item in Line.ChildRateLineItems)
				{
					if (lineItem != item && lineItem.TM_Text == item.TM_Text)
					{
						lineItem.TM_TextInfo.AddError(Res.GetString("201965a1-bf2c-412a-8d2b-57cd54f8002d", "You can only specify one rate for each equipment type. You should not specify the same equipment type more than once."));
						return;
					}
				}
			}

			ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, lineItem.Lookups.EquipmentTypes);
		}

		public override void ValidateTM_BreakWeightVolume(RateLineItem lineItem)
		{
			MandatoryValidation.CheckEntered(lineItem.TM_BreakWeightVolumeInfo);
			if (!lineItem.TM_BreakWeightVolume.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(lineItem.TM_BreakWeightVolumeInfo, lineItem.Lookups.TimeUnits);
			}
		}

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			if (RateLineBizO.RateLineItems.Any())
			{
				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
				foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
				{
					var unitDescription = UnitDescriptionInternal(item.TM_BreakWeightVolume, addPlural: true);
					var unit = ResString.GetMultilingualString("4107a369-c97a-4e4b-9c66-41d3a7497713", "per {0}", unitDescription);

					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item, item.EquipmentTypeDesc, unit));
				}
			}

			return result;
		}

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
		}

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			CalculatorHelper.CloneAndUpdateRateLineItemsWithPercentAndBaseRate<EquipmentHireCalculator>(ctbCalc, clone, RateLineItems, rateTypeToUpdate);
		}
	}
}

