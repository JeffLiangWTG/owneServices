using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public class WeightBreakValidator : ValidationProvider
	{
		public WeightBreakValidator(IList items, RateLineItem rateLineItem)
			: base(rateLineItem)
		{
			this.items = items.Cast<RateLineItem>();
			currentRateLineItem = rateLineItem;
		}

		#region Weight Break Validation

		public void CheckWeightBreak()
		{
			if (currentRateLineItem != null && !currentRateLineItem.IsDeleted)
			{
				if (currentRateLineItem.RequiresWeightBreak())
				{
					MandatoryValidation.CheckNotNegative(currentRateLineItem.TM_BreakInfo);

					if (currentRateLineItem.TM_Break.IsEmpty)
					{
						currentRateLineItem.TM_BreakInfo.AddError(ErrorMessages.WeightBreakRequired);
					}
				}
				else if (!currentRateLineItem.TM_Break.IsEmpty)
				{
					currentRateLineItem.TM_BreakInfo.AddError(ErrorMessages.WeightBreakNotAllowed);
				}

				CheckPlusWeightBreakAmount();
				CheckIfLowestBreakNeedsWarning();
			}
		}

		void CheckPlusWeightBreakAmount()
		{
			if (currentRateLineItem.RateOperatorIsPlus() && !currentRateLineItem.Parent.Uses(CalculatorType.Equalization))
			{
				var currentBreak = currentRateLineItem.TM_Break;

				foreach (var item in items)
				{
					if (item != currentRateLineItem && !item.IsDeleted)
					{
						var itemType = item.TM_Type;
						var itemBreak = item.TM_Break;

						if (itemType == Calculator.Items.Operator.Plus && itemBreak == currentBreak)
						{
							currentRateLineItem.TM_BreakInfo.AddError(ErrorMessages.WeightBreakIsTheSameAsAnotherPlusRate);
							if (!item.TM_BreakInfo.HasErrors() && !item.Validation.IsValidatingAll)
							{
								item.Validation.ValidateTM_Break();
							}
						}

						if (itemType == Calculator.Items.Operator.Minus && currentBreak < itemBreak)
						{
							currentRateLineItem.TM_BreakInfo.AddError(ErrorMessages.WeightBreakLessThanMinusBreak);
						}
					}
				}
			}
		}

		void CheckIfLowestBreakNeedsWarning()
		{
			if (!currentRateLineItem.TM_BreakInfo.HasWarnings() && !currentRateLineItem.TM_BreakInfo.HasErrors() && currentRateLineItem.IsLowestBreak() && !currentRateLineItem.Parent.Uses(CalculatorType.Equalization))
			{
				if (currentRateLineItem.RateOperatorIsPlus())
				{
					currentRateLineItem.TM_BreakInfo.AddWarning(ErrorMessages.LowestBreakActsAsPlusAndMinus("-"));
				}
				else
				{
					var currentBreak = currentRateLineItem.TM_Break;

					if (!items.Any(i => i.RateOperatorIsPlus() && currentBreak == i.TM_Break))
					{
						currentRateLineItem.TM_BreakInfo.AddWarning(ErrorMessages.LowestBreakActsAsPlusAndMinus("+"));
					}
				}
			}
		}

		#endregion

		#region Implementation

		readonly IEnumerable<RateLineItem> items;
		readonly RateLineItem currentRateLineItem;

		#endregion
	}
}

