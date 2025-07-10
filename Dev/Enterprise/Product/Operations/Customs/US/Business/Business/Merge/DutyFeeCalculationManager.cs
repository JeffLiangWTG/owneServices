using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class DutyFeeCalculationManager : IDutyFeeCalculator
	{
		public DutyFeeCalculationManager(IDutyDataLineHeaderProvider declaration)
		{
			this.declaration = declaration;
		}

		readonly IDutyDataLineHeaderProvider declaration;

		public void Calculate()
		{
			Calculate(declaration.EntriesToCalculateDutyFeeTax);
		}

		public void Calculate(IEnumerable<IDutyDataLineHeader> entries)
		{
			var lineCalculator = new LineDutyFeeCalculator();

			var mpfCalculator = new MPFCalculator(declaration.Factory);
			foreach (IDutyDataLineHeader entry in entries)
			{
				entry.OnCalculating();

				foreach (IEntryLineOrInvoiceLineDutyData line in entry.DutyDataLines)
				{
					if (!entry.AreDutyFeeKnownAndImported)
					{
						try
						{
							lineCalculator.Calculate(line, new FeeCalculator(declaration.Factory, entry.IsHMFApplicable, SetFeeResult));
						}
						catch (CustomsMergeException cex)
						{
							line.CalculateException = cex.Message;
						}
					}

					line.RollUpFees(entry);

					if (!line.HasMPF && !line.IsMPFOverridden && mpfCalculator.ShouldHaveFee(line))
					{
						line.HasMPF = true;
					}
				}

				if (!entry.AreDutyFeeKnownAndImported && entry.IsCottonFeeDeMinimusApplicable)
				{
					foreach (IEntryLineOrInvoiceLineDutyData line in entry.DutyDataLines)
					{
						// Threshold amount is 2.01$ and if a calculated fee is less than that, fee is exempt.
						line.UpdateLineAndHeaderFeeAmountLessThanThreshold(Core.Constants.USCustoms.FeeCodes.Cotton, CottonFeeCalculator.ThresholdCottonFeeAmount);
					}
				}

				var originalLineHeader = entry as ReconOriginalDutyDataLineHeader;
				if (originalLineHeader == null || !entry.CalculateChangedLinesOnly)
				{
					//This should happen after all lines' fees are calculated, but before detached entry lines are to be deleted
					CalculateEntryLevelFeeAndAdjustLineFeesAccordingToMaxAmount(entry);
				}

				//As a result of a derived duty calculation
				if (!entry.AreDutyFeeKnownAndImported)
				{
					entry.DeleteDetachedEntryLines();
				}
			}
		}

		void SetFeeResult(FeeResult feeResult, string feeCode, IFeeCalculationDataProvider invoiceLine)
		{
			var amount = feeResult.IsRequired ? feeResult.Amount : ZDecimal.Zero;
			invoiceLine.SetFeeResult(feeCode, amount, new FeeCalculationInternalData());
		}

		void CalculateEntryLevelFeeAndAdjustLineFeesAccordingToMaxAmount(IDutyDataLineHeader entry)
		{
			var mpfOverridden = entry.OverridenTotalMPFPayable;

			if (mpfOverridden.HasValue)
			{
				entry.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, mpfOverridden.Value);
			}
			else
			{
				SetMPFMinimumIfThereIsAtLeastOneEntryLineSubjectToMPF(entry);

				var currentMPFRate = new FeeCalculationHelper(declaration.Factory, entry.DateForMPFCalculation).GetCurrentRate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
				if (currentMPFRate != null)
				{
					entry.FeeAndCharges.AdjustAccordingToMinimumAndMaximum(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, currentMPFRate.ZZF_Minimum, currentMPFRate.ZZF_Maximum);
				}
			}

			if (!entry.AreDutyFeeKnownAndImported)
			{
				entry.CalculateAndStoreEntryLevelFees((feeType, feeAmount) => entry.FeeAndCharges.UpdateOrAddCharge(feeType, feeAmount));
			}

			entry.FeeAndCharges.RoundFeesAndTaxes();

			entry.UpdateHMFAccordingToMinimumThresholdRule();
		}

		void SetMPFMinimumIfThereIsAtLeastOneEntryLineSubjectToMPF(IDutyDataLineHeader entry)
		{
			var totalRoundedMPF = entry.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);

			if (totalRoundedMPF == 0m)
			{
				if (entry is IDutyDataLineHeader dutyDataLineHeader &&
					dutyDataLineHeader.DutyDataLines.Cast<IEntryLineOrInvoiceLineDutyData>().Any(x =>
					!(
						x.GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) != 0 ||
						!(x.FeeDataProviders.FirstOrDefault()?.IsFeeOverriden(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) ?? false)
					)))
				{
					entry.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 0m);
				}
				else
				{
					bool hasMPF = DoEntryLinesHaveMPF(entry);

					var currentRate = new FeeCalculationHelper(declaration.Factory, entry.DateForMPFCalculation).GetCurrentRate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
					if (hasMPF && currentRate != null)
					{
						entry.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, currentRate.ZZF_Minimum);
					}
				}
			}
		}

		bool DoEntryLinesHaveMPF(IDutyDataLineHeader entry)
		{
			foreach (IEntryLineOrInvoiceLineDutyData line in entry.DutyDataLines)
			{
				if (line.HasMPF)
				{
					return true;
				}
			}
			return false;
		}
	}
}
