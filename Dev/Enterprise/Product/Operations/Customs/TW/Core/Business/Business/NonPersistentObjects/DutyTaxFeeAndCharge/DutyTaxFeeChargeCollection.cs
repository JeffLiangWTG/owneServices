using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class DutyTaxFeeChargeCollection : NonPersistentBusinessObjectCollection<DutyTaxFeeCharge>
	{
		public DutyTaxFeeChargeCollection(CusEntryHeader supporter)
			: base(supporter.Factory)
		{
			header = supporter;
		}

		readonly CusEntryHeader header;

		int oldHashCode = -1;

		public override int GetHashCode()
		{
			unchecked
			{
				var charges = header.Charges.Cast<CusEntryHeaderCharges>();
				var entryLines = header.MergedLines.Cast<CusEntryLine>();
				var hashCode = charges.Count().GetHashCode() ^ entryLines.Count().GetHashCode();
				charges.ForEach(x => hashCode = hashCode ^ x.C1_ChargeType.GetHashCode() ^ x.C1_ChargeAmount.GetHashCode() ^ x.C1_MethodOfPayment.GetHashCode());

				foreach (var entryLine in entryLines)
				{
					entryLine.Fees.Cast<CusEntryLineFee>().ForEach(x => hashCode = (hashCode * 397) ^ (x.CF_ChargeType, x.CF_ChargeAmount, x.CF_MethodOfPayment).GetHashCode());
				}
				return hashCode;
			}
		}

		#region Implement
		void RebuildElements()
		{
			RemoveAll();

			var list = new List<DutyCalculationIntermediateResult>();
			header.Charges.Cast<CusEntryHeaderCharges>().ForEach(x => list.Add(new DutyCalculationIntermediateResult { RateCode = x.C1_ChargeType, Amount = x.C1_ChargeAmount, PaymentMethod = x.C1_MethodOfPayment }));

			foreach (var entryLine in header.MergedLines.Cast<CusEntryLine>())
			{
				entryLine.Fees.Cast<CusEntryLineFee>().Where(x => !x.CF_ChargeAmount.IsEmpty).ForEach(x => list.Add(new DutyCalculationIntermediateResult { RateCode = x.CF_ChargeType, Amount = x.CF_ChargeAmount, PaymentMethod = x.CF_MethodOfPayment }));
			}
			var aggregateCharges = list.GetGroupedDuties(header.IsImport).ToList();
			foreach (var charge in aggregateCharges)
			{
				AddCharge(charge.TypeCode, charge.Amount, charge.PaymentMethod);
			}
		}

		public bool ShouldRebuildElements()
		{
			var hashCode = GetHashCode();
			bool result = hashCode != oldHashCode;
			if (hashCode != oldHashCode)
			{
				oldHashCode = hashCode;
				RebuildElements();
			}
			return result;
		}

		void AddCharge(ZString rateCode, ZDecimal amount, ZString paymentMethod)
		{
			var charge = (DutyTaxFeeCharge)CreateNonPersistentBusinessObject();
			charge.ChargeType = rateCode;
			charge.ChargeAmount = amount;
			charge.MethodOfPayment = paymentMethod;
			Add(charge);
		}

		#endregion

		#region override

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DutyTaxFeeCharge(header);
		}

		#endregion
	}
}
