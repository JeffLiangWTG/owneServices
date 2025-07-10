using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusHAWBFilteredCollection : FilteredCollection<CusHAWB>
	{
		public CusHAWBFilteredCollection(CusMAWB mawb)
			: base(mawb.ChildBills)
		{
			this.mawb = mawb;
		}

		#region Implementation

		readonly CusMAWB mawb;

		protected override bool IsThisPartOfTheCollection(BusinessObject bObject)
		{
			CusHAWB houseBill = bObject as CusHAWB;
			return houseBill != null && IsMatchingFilter(houseBill);
		}

		protected override void RebuildCore()
		{
			if (mawb.InvalidBillsOnlyFilter)
			{
				mawb.ChildBills.RunPreSaveValidation();
			}
			base.RebuildCore();
		}

		protected override bool IsFilterEmpty
		{
			get
			{
				return
					mawb.CustomsCargoStatusFilter.IsEmpty &&
					mawb.CustomsMessageStatusFilter.IsEmpty &&
					!mawb.InvalidBillsOnlyFilter;
			}
		}

		protected override void ClearFilterCore()
		{
			mawb.CustomsCargoStatusFilter = ZString.Empty;
			mawb.CustomsMessageStatusFilter = ZString.Empty;
			mawb.InvalidBillsOnlyFilter = false;
		}

		protected virtual bool IsCustomsCargoStatusMatching(CusHAWB hawb, ZString status)
		{
			return hawb.CS_CustomsStatus == status;
		}

		protected virtual bool IsCustomsMessageStatusMatching(CusHAWB hawb, ZString status)
		{
			return hawb.CS_MsgStatus == status;
		}

		protected virtual bool IsInvalidBillsOnlyMatching(CusHAWB hawb)
		{
			return hawb.HasErrors || hawb.HasMessageErrors;
		}

		protected virtual bool IsMatchingFilter(CusHAWB houseBill)
		{
			bool result = IsFilterEmpty;

			if (!mawb.CustomsCargoStatusFilter.IsEmpty)
			{
				result |= IsCustomsCargoStatusMatching(houseBill, mawb.CustomsCargoStatusFilter);
			}
			if (!mawb.CustomsMessageStatusFilter.IsEmpty)
			{
				result |= IsCustomsMessageStatusMatching(houseBill, mawb.CustomsMessageStatusFilter);
			}
			if (mawb.InvalidBillsOnlyFilter)
			{
				result |= IsInvalidBillsOnlyMatching(houseBill);
			}

			return result;
		}

		#endregion // Implementation
	}
}
