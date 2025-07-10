using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class IFeesExtensions
	{
		public static ZDecimal GetFeeOrChargeAmount(this IFees feeAndCharges, ZString chargeType)
		{
			IFee feeOrCharge = feeAndCharges == null ? null : feeAndCharges.GetFeeFor(chargeType);

			return feeOrCharge != null ? feeOrCharge.Amount : ZDecimal.Zero;
		}

		public static void UpdateOrAddCharge(this IFees feeAndCharges, ZString chargeType, ZDecimal amount)
		{
			UpdateOrAddCharge(feeAndCharges, chargeType, amount, null);
		}

		public static void UpdateOrAddCharge(this IFees feeAndCharges, ZString chargeType, ZDecimal amount, IEnumerable<ZString> mandatoryFeesForTariff)
		{
			if (feeAndCharges != null)
			{
				IFee feeOrCharge = feeAndCharges.GetFeeFor(chargeType);

				if (feeOrCharge == null)
				{
					feeOrCharge = feeAndCharges.AddNew();
					feeOrCharge.Code = chargeType;
				}

				if (feeOrCharge != null)
				{
					if (amount == ZDecimal.Zero && (mandatoryFeesForTariff == null || !mandatoryFeesForTariff.Contains(feeOrCharge.Code) || (chargeType == Core.Constants.USCustoms.FeeCodes.Cotton)))
					{
						feeOrCharge.Delete();
					}
					else
					{
						feeOrCharge.Amount = amount;
					}
				}
			}
		}

		public static ZDecimal GetGrandTotalFee(this IFees feeAndCharges)
		{
			ZDecimal result = 0m;

			if (feeAndCharges != null)
			{
				foreach (IFee feeOrCharge in feeAndCharges)
				{
					if (Registry.Business.Customs.US.EntryChargeTypeList.IsFeeType(feeOrCharge.Code))
					{
						result += feeOrCharge.Amount;
					}
				}
			}

			return result;
		}

		public static ZDecimal GetTotal(this IFees feeAndCharges)
		{
			ZDecimal result = 0m;

			if (feeAndCharges != null)
			{
				foreach (IFee feeOrCharge in feeAndCharges)
				{
					result += feeOrCharge.Amount;
				}
			}

			return result;
		}

		/// <summary>
		/// For MPF, minimum amount is 25 and maximum amount 485. 
		/// If a calculated amount is 10$, then minimum is enforced. However, if a calculated amount is 0$, then it means the MPF is exempt and minimum amount is not enforced
		/// If a calculated amount is 500$, then maximum is enforced.
		/// </summary>
		public static void AdjustAccordingToMinimumAndMaximum(this IFees feeAndCharges, string chargeType, decimal minimumAmount, decimal maximumAmount)
		{
			IFee charge = feeAndCharges == null ? null : feeAndCharges.GetFeeFor(chargeType);

			if (charge != null)
			{
				decimal? adjustedAmount = null;

				if (charge.Amount > 0 && charge.Amount < minimumAmount)
				{
					adjustedAmount = minimumAmount;
				}
				else if (charge.Amount > maximumAmount)
				{
					adjustedAmount = maximumAmount;
				}

				if (adjustedAmount.HasValue)
				{
					charge.Amount = adjustedAmount.Value;
				}
			}
		}

		public static void ClearIfAmountLessThanThreshold(this IFees feeAndCharges, string chargeType, decimal thresholdAmount)
		{
			IFee charge = feeAndCharges == null ? null : feeAndCharges.GetFeeFor(chargeType);

			if (charge != null)
			{
				if (charge.Amount > 0 && charge.Amount < thresholdAmount)
				{
					charge.Amount = 0;
				}
			}
		}

		public static void UpdateLineAndHeaderFeeAmountLessThanThreshold(this IFees lineFeesAndCharges, string chargeType, decimal thresholdAmount, IFees headerCharges)
		{
			IFee charge = lineFeesAndCharges == null ? null : lineFeesAndCharges.GetFeeFor(chargeType);

			if (charge != null)
			{
				if (charge.Amount > 0 && charge.Amount < thresholdAmount)
				{
					if (headerCharges != null)
					{
						ZDecimal headerAmount = headerCharges.GetFeeOrChargeAmount(chargeType);
						headerCharges.UpdateOrAddCharge(chargeType, headerAmount - charge.Amount);
					}

					charge.Amount = 0;
				}
			}
		}

		public static void RoundFeesAndTaxes(this IFees feeAndCharges)
		{
			if (feeAndCharges != null)
			{
				foreach (IFee charge in feeAndCharges)
				{
					charge.Amount = charge.Amount.Round(2);
				}
			}
		}
	}
}
