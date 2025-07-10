using CargoWise.Types;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondCargoDescCollection : Customs.Business.CusInBondCargoDescCollection<CusInBondCargoDesc>
	{
		public CusInBondCargoDescCollection(CusInBondContainer master)
			: base(master)
		{
		}

		public ZInt TotalPieceCount
		{
			get
			{
				var result = ZInt.Zero;
				foreach (CusInBondCargoDesc commodity in this)
				{
					result += commodity.BY_PieceCount;
				}
				return result;
			}
		}

		protected override void SetDefaultsForNewElementCore(CusInBondCargoDesc newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (Count == 0)
			{
				DefaultFromBill(newElement);
			}
			else if (Count > 0)
			{
				DefaultFromPreviousCommodity(newElement, this[Count - 1]);
			}
			DefaultForFTZ(newElement);
		}

		void DefaultForFTZ(CusInBondCargoDesc newCommodity)
		{
			var bill = newCommodity.Bill;
			if (bill != null)
			{
				var header = bill.Header;
				if (header != null && header.BH_FTZMove)
				{
					newCommodity.BY_Description = bill.B0_MasterBillNumber;
				}
			}
		}

		void DefaultFromPreviousCommodity(CusInBondCargoDesc newCommodity, CusInBondCargoDesc previousCommodity)
		{
			if (!previousCommodity.IsDeleted)
			{
				newCommodity.BY_GrossWeightUnit = previousCommodity.BY_GrossWeightUnit;
				newCommodity.BY_ManifestUnitCode = previousCommodity.BY_ManifestUnitCode;
			}
		}

		void DefaultFromBill(CusInBondCargoDesc newCommodity)
		{
			var bill = newCommodity.Bill;
			if (bill != null)
			{
				newCommodity.BY_GrossWeightUnit = bill.B0_WeightUQ;
				newCommodity.BY_ManifestUnitCode = bill.B0_ManifestUQ.Left(CusInBondCargoDesc.Schema.BY_ManifestUnitCodeMaxLength);
			}
		}

		protected override bool AllowNew
		{
			get
			{
				var master = (CusInBondContainer)Relationship.Master;
				return master != null && ((CargoWise.EntityFramework.IBusinessObjectInternals)master).Row.RowState != System.Data.DataRowState.Detached && !master.IsDeleted && !master.ShouldSynchronise;
			}
		}
	}
}
