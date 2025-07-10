using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class OtherCharges : DocDataObject, IOtherCharges
	{
		#region IsPrepaid

		public ZBool IsPrepaid
		{
			get => isPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(IsPrepaidInfo, ref isPrepaid, value)
					&& isPrepaid)
				{
					IsCollect = false;
					IsFree = false;
					IsPayableElsewhere = false;
					IsFirstLinePrepaidLineSecondCollect = false;
					Remarks = ZString.Empty;
				}

				ValidateAll();
			}
		}

		ZBool isPrepaid;

		public ZPropertyInfo IsPrepaidInfo => GetZPropertyInfo(nameof(IsPrepaid));

		#endregion

		#region IsCollect

		public ZBool IsCollect
		{
			get => isCollect;
			set
			{
				if (SetNonPersistentPropertyValue(IsCollectInfo, ref isCollect, value)
					&& IsCollect)
				{
					IsPrepaid = false;
					IsFree = false;
					IsPayableElsewhere = false;
					IsFirstLinePrepaidLineSecondCollect = false;
					Remarks = ZString.Empty;
				}

				ValidateAll();
			}
		}

		ZBool isCollect;

		public ZPropertyInfo IsCollectInfo => GetZPropertyInfo(nameof(IsCollect));

		#endregion

		#region IsFree

		public ZBool IsFree
		{
			get => isFree;
			set
			{
				if (SetNonPersistentPropertyValue(IsFreeInfo, ref isFree, value)
					&& IsFree)
				{
					IsPrepaid = false;
					IsCollect = false;
					IsPayableElsewhere = false;
					IsFirstLinePrepaidLineSecondCollect = false;
					Remarks = ZString.Empty;
				}

				ValidateAll();
			}
		}

		ZBool isFree;

		public ZPropertyInfo IsFreeInfo => GetZPropertyInfo(nameof(IsFree));

		#endregion

		#region IsPayableElsewhere

		public ZBool IsPayableElsewhere
		{
			get => isPayableElsewhere;
			set
			{
				if (SetNonPersistentPropertyValue(IsPayableElsewhereInfo, ref isPayableElsewhere, value)
					&& IsPayableElsewhere)
				{
					IsPrepaid = false;
					IsCollect = false;
					IsFree = false;
					IsFirstLinePrepaidLineSecondCollect = false;
					Remarks = ZString.Empty;
				}

				ValidateAll();
			}
		}

		ZBool isPayableElsewhere;

		public ZPropertyInfo IsPayableElsewhereInfo => GetZPropertyInfo(nameof(IsPayableElsewhere));

		#endregion

		#region IsFirstLinePrepaidLineSecondCollect

		public ZBool IsFirstLinePrepaidLineSecondCollect
		{
			get => isFirstLinePrepaidLineSecondCollect;
			set
			{
				if (SetNonPersistentPropertyValue(IsFirstLinePrepaidLineSecondCollectInfo, ref isFirstLinePrepaidLineSecondCollect, value)
					&& IsFirstLinePrepaidLineSecondCollect)
				{
					IsPrepaid = false;
					IsCollect = false;
					IsFree = false;
					IsPayableElsewhere = false;
					Remarks = ZString.Empty;
				}

				ValidateAll();
			}
		}

		ZBool isFirstLinePrepaidLineSecondCollect;

		public ZPropertyInfo IsFirstLinePrepaidLineSecondCollectInfo => GetZPropertyInfo(nameof(IsFirstLinePrepaidLineSecondCollect));

		#endregion

		#region Remarks

		public ZString Remarks
		{
			get => remarks;
			set
			{
				if (SetNonPersistentPropertyValue(RemarksInfo, ref remarks, value)
					&& !Remarks.IsEmpty)
				{
					IsPrepaid = false;
					IsCollect = false;
					IsFree = false;
					IsPayableElsewhere = false;
					IsFirstLinePrepaidLineSecondCollect = false;
				}

				ValidateAll();
			}
		}

		ZString remarks;

		public ZPropertyInfo RemarksInfo => GetZPropertyInfo(nameof(Remarks));

		#endregion
	}
}
