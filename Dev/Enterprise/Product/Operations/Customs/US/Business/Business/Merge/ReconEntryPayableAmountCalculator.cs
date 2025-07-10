using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class ReconEntryPayableAmountCalculator
	{
		public void Calculate(ReconDeclaration reconDeclaration)
		{
			ReconEntryHeader reconEntry = reconDeclaration.ReconEntry;
			IFees charges = reconEntry.Charges;

			foreach (IFee charge in charges)
			{
				charge.Amount = 0m;
			}

			foreach (ReconOriginalEntryHeader originalEntry in reconDeclaration.OriginalEntries)
			{
				foreach (CusEntryHeaderCharges charge in originalEntry.ReconCharges)
				{
					var originalCharge = originalEntry.OriginalCharges.GetCharge(charge.C1_ChargeType);
					if (originalCharge == null && charge.C1_ChargeType != Core.Constants.USCustoms.FeeCodes.ReconciliationInterest)
					{
						originalEntry.OriginalCharges.AddNew(charge.C1_ChargeType, 0m);
					}
				}

				foreach (ReconEntryOriginalCharge charge in originalEntry.OriginalCharges)
				{
					var reconCharge = originalEntry.ReconCharges.GetChargeWithThisCode(charge.CY_Code);
					if (reconCharge == null && charge.CY_Code != Core.Constants.USCustoms.FeeCodes.MPC)
					{
						originalEntry.ReconCharges.AddNew(charge.CY_Code, 0m);
					}
				}
				originalEntry.OriginalCharges.Sort(ReconEntryOriginalCharge.Schema.CY_Code, System.ComponentModel.ListSortDirection.Descending);
				originalEntry.ReconCharges.Sort(CusEntryHeaderChargesSchema.Constants.C1_ChargeType, System.ComponentModel.ListSortDirection.Descending);

				foreach (IFee reconCharge in originalEntry.ReconCharges)
				{
					ZDecimal existingAmount = charges.GetFeeOrChargeAmount(reconCharge.Code);

					charges.UpdateOrAddCharge(reconCharge.Code, reconCharge.Amount + existingAmount);
				}

				foreach (IFee originalCharge in originalEntry.OriginalCharges)
				{
					ZDecimal existingAmount = charges.GetFeeOrChargeAmount(originalCharge.Code);

					charges.UpdateOrAddCharge(originalCharge.Code, existingAmount - originalCharge.Amount);
				}
			}

			reconEntry.CH_TotalPaid = charges.GetTotal();
		}
	}
}
