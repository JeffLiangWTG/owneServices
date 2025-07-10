using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FDARelatedBill : NonPersistentBusinessObject
		, IObsoleteValidation
	{
		public FDARelatedBill(FDA fda)
			: base(fda.Factory)
		{
			this.fda = fda;
		}
		readonly FDA fda;

		#region Related objects

		public void SetBill(Bill bill)
		{
			Bill = bill;
		}

		public Bill Bill
		{
			get { return bill == null || bill.IsDeleted ? null : bill; }
			set
			{
				bill = value;
				IsForFDALine = fda.BillsForFDALine.Contains(bill);
			}
		}
		Bill bill;

		public FDARelatedBillsGenPivot Pivot
		{
			get { return Bill != null ? fda.BillsForFDALine.GetRelatedPivot(Bill) : null; }
		}

		#endregion

		#region Properties

		#region BillNumber

		public ZString BillNumber
		{
			get { return Bill == null ? ZString.Empty : Bill.CU_BillUniqueCode; }
		}

		public ZPropertyInfo BillNumberInfo
		{
			get { return GetZPropertyInfo(nameof(BillNumber)); }
		}
		#endregion

		#region IsForFDALine

		[BusinessObjectTestExclude()]
		public ZBool IsForFDALine
		{
			get { return isForFDALine; }
			set
			{
				if (isForFDALine != value)
				{
					isForFDALine = value;

					if (Bill != null)
					{
						if (isForFDALine)
						{
							fda.BillsForFDALine.AddPivotFor(Bill);
						}
						else
						{
							fda.BillsForFDALine.DeletePivotFor(Bill);
						}
					}
					if (!IsValidationSuspended)
					{
						ValidateIsForFDALine();
					}
					IsForFDALineInfo.RefreshBinding();
				}
			}
		}
		ZBool isForFDALine;

		public ZPropertyInfo IsForFDALineInfo
		{
			get { return GetZPropertyInfo(nameof(IsForFDALine)); }
		}

		#endregion

		#endregion

		public void ValidateIsForFDALine()
		{
			IsForFDALineInfo.ClearAllNotifications();
			if (IsForFDALine)
			{
				FDARelatedBillsGenPivot pivot = Pivot;
				if (pivot != null)
				{
					pivot.Validation.ValidateXX_Relation2ID();
					IsForFDALineInfo.AddAllNotificationsFrom(pivot.XX_Relation2IDInfo);
				}
			}
		}

		protected override void AddToFactoryCache()
		{
			//DO NOT Allow memory to be held up by factory
		}
	}
}
