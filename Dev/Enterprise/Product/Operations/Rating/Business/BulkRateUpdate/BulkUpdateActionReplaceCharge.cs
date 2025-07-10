using System.Collections.Generic;

namespace Enterprise.Rating.Business
{
	public class BulkUpdateActionReplaceCharge : BulkUpdateAction
	{
		public BulkUpdateActionReplaceCharge(BulkRateUpdater updater)
			: base(updater)
		{
		}

		protected override void ApplyBulkUpdateActionCore(RateEntry entry)
		{
			var lines = GetRateLinesWithActionLineChargeCode(entry);
			if (lines.Length == 1)
			{
				ReplaceRateLineValues(lines[0]);
			}
			else if (lines.Length > 1)
			{
				AddDuplicateRateLinesWithSameChargeCodeWarning(lines);
			}

			entry.RateLines.Sort();
		}

		protected override bool IsEntryNotUpdatable(RateEntry rateEntry) => base.IsEntryNotUpdatable(rateEntry)
			|| (rateEntry.IsFCLEntryWithEmptyContainer() && !Updater.ActionsLine.UsesCalculatorsSupportedOnFCLRateEntryWithEmptyContainer());

		void AddDuplicateRateLinesWithSameChargeCodeWarning(IEnumerable<RateLine> lines)
		{
			foreach (var line in lines)
			{
				line.AddRowWarning(Res.GetString("7ad104a5-8ea9-424c-bd7d-070444bb6dea", "Rate lines with the same Charge Code will not be replaced."));
			}
		}

		void ReplaceRateLineValues(RateLine rateLine)
		{
			rateLine.TL_AC = Updater.ActionsLine.TL_AC;

			var hasSecurityToOverride = !rateLine.OverrideChargeDescriptionInfo.ReadOnly;
			if (hasSecurityToOverride)
			{
				if (rateLine.OverrideChargeDescription != Updater.ActionsLine.OverrideChargeDescription)
				{
					rateLine.OverrideChargeDescription = Updater.ActionsLine.OverrideChargeDescription;
				}
				if (Updater.ActionsLine.OverrideChargeDescription)
				{
					rateLine.TL_RateDesc = Updater.ActionsLine.TL_RateDesc;
				}
			}

			// order matters, avoid calculator to reset rate line values to default by copying it first
			// then other properties like TL_WeightVolume, TL_ActualPercentage are copied later
			rateLine.TL_RateCalculator = Updater.ActionsLine.TL_RateCalculator;
			rateLine.TL_RX_NKCurrency = Updater.ActionsLine.TL_RX_NKCurrency;
			rateLine.TL_WeightVolume = Updater.ActionsLine.TL_WeightVolume;
			rateLine.UnitMultipleAsString = Updater.ActionsLine.UnitMultipleAsString;
			rateLine.TL_RoundingFactor = Updater.ActionsLine.TL_RoundingFactor;
			rateLine.UseOnlyActualWeightMeasure = Updater.ActionsLine.UseOnlyActualWeightMeasure;
			rateLine.TL_ActualPercentage = Updater.ActionsLine.TL_ActualPercentage;
			rateLine.TL_Rounding = Updater.ActionsLine.TL_Rounding;
			rateLine.TL_ContainerOwnership = Updater.ActionsLine.TL_ContainerOwnership;
			rateLine.TL_Condition = Updater.ActionsLine.TL_Condition;
			rateLine.TL_ConditionalExpression = Updater.ActionsLine.TL_ConditionalExpression;
			rateLine.TL_ConditionalExpressionDescription = Updater.ActionsLine.TL_ConditionalExpressionDescription;
			rateLine.TL_UnitFactor = Updater.ActionsLine.TL_UnitFactor;

			rateLine.RateLineItems.Clone(Updater.ActionsLine);
		}
	}
}
