using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class OverseasFreightAndInsuranceCalculator
	{
		public Money GetCharge(IChargeApportionee chargeApportionee, string[] chargeCodes)
		{
			Money result = Money.Empty;

			if (chargeApportionee.CurrencyConverter != null)
			{
				foreach (string chargeCode in chargeCodes)
				{
					JobComInvCharge[] sortedCharges = GetSortedCharges(chargeApportionee, chargeCode);

					bool hasAdjustedCharge = false;

					foreach (BaseJobComInvHeaderCharge charge in sortedCharges)
					{
						if (charge.J7_AdjustedCharge)
						{
							hasAdjustedCharge = true;
							result = chargeApportionee.CurrencyConverter.Add(result, charge.Money);
						}
						else if (!hasAdjustedCharge || !charge.J7_IsDutiable)
						{
							result = chargeApportionee.CurrencyConverter.Add(result, charge.Money);
						}
					}
				}
			}

			return result;
		}

		JobComInvCharge[] GetSortedCharges(IChargeApportionee chargeApportionee, string chargeCode)
		{
			List<JobComInvCharge> charges = new List<JobComInvCharge>();
			charges.AddRange(chargeApportionee.Charges.GetCharge(chargeCode));
			charges.AddRange(chargeApportionee.ApportionedCharges.GetCharge(chargeCode));

			charges.Sort(new AdjustedNormalChargeComparer());
			return charges.ToArray();
		}

		class AdjustedNormalChargeComparer : IComparer<JobComInvCharge>
		{
			public int Compare(JobComInvCharge x, JobComInvCharge y)
			{
				if (x.J7_AdjustedCharge != y.J7_AdjustedCharge)
				{
					if (x.J7_AdjustedCharge)
					{
						return -1;
					}
					else
					{
						return 1;
					}
				}
				return 0;
			}
		}
	}
}
