using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class FDARelatedBillsCollection : NonPersistentBusinessObjectCollection<FDARelatedBill>
	{
		public FDARelatedBillsCollection(FDA fda)
			: base(fda.Factory)
		{
			this.fda = fda;

			RebuildElements();
		}
		readonly FDA fda;

		void RebuildElements()
		{
			RemoveAll();

			var declaration = fda.Declaration;
			if (declaration != null)
			{
				foreach (Bill bill in declaration.Bills)
				{
					if (!bill.IsDeleted && !bill.CU_BillNum.IsEmpty && (bill.CU_BillType == BillTypeList.Codes.MasterBill || bill.CU_BillType == BillTypeList.Codes.HouseBill))
					{
						AddNew(bill);
					}
				}
			}
		}

		#region New Methods

		public FDARelatedBill AddNew(Bill bill)
		{
			var result = base.AddNew();
			result.SetBill(bill);
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FDARelatedBill(fda);
		}

		public FDARelatedBill FindByBillNumber(ZString billNo)
		{
			foreach (FDARelatedBill bill in this)
			{
				if (bill.Bill != null && bill.Bill.CU_BillNum == billNo)
				{
					return bill;
				}
			}

			return null;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
