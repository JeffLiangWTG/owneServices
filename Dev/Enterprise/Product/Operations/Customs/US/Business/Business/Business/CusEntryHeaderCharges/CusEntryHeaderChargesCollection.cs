using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class CusEntryHeaderChargesCollection : Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>, IFees
	{
		public CusEntryHeaderChargesCollection(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public ZDecimal GetTotalCustomsFeeAmount()
		{
			ZDecimal result = 0m;

			foreach (CusEntryHeaderCharges charge in this)
			{
				if (!CusFeeCodeConstants.IsExciseTax(charge.C1_ChargeType))
				{
					result += charge.C1_ChargeAmount;
				}
			}

			return result;
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			base.OnNonCommittedAdded(bizOAdded);
			SynchroniseOnNonCommittedAdded((CusEntryHeaderCharges)bizOAdded);
		}

		void SynchroniseOnNonCommittedAdded(CusEntryHeaderCharges charge)
		{
			var reconOriginalEntry = ((CusEntryHeader)EntryHeader).ReconOriginalEntry;
			if (charge != null && !charge.C1_ChargeType.IsEmpty && reconOriginalEntry != null)
			{
				ZString chargeType = charge.C1_ChargeType;
				var existingCharge = reconOriginalEntry.OriginalCharges.GetCharge(chargeType);
				if (existingCharge == null)
				{
					reconOriginalEntry.OriginalCharges.AddNew(chargeType);
				}
			}
		}

		protected override bool AllowNewCore => base.AllowNewCore && !ShouldDutiesFeesBeCalculatedFroRecon;

		protected override bool AllowRemoveCore => base.AllowRemoveCore && !ShouldDutiesFeesBeCalculatedFroRecon;

		bool ShouldDutiesFeesBeCalculatedFroRecon
		{
			get
			{
				var entryHeader = (CusEntryHeader)EntryHeader;
				return entryHeader.IsReconImportEntry && !((IReconOriginalChargeParent)entryHeader).DefaultValueForOverridenForNewChild;
			}
		}

		public bool HasDuplicate(string chargeType)
		{
			BusinessObject[] allCharges = Find(new ZQuery(CusEntryHeaderChargesSchema.C1_ChargeType, chargeType));
			return allCharges.Length > 1;
		}

		#region IFees Members

		IFee IFees.GetFeeFor(ZString code) => GetChargeWithThisCode(code);

		IFee IFees.AddNew() => AddNew();

		#endregion
	}
}
