using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class FDARelatedBillsGenPivotCollection : CustomsGenPivotCollection<FDARelatedBillsGenPivot, FDA, Bill>
	{
		public FDARelatedBillsGenPivotCollection(FDA master)
			: base(master)
		{
		}

		internal void AddMissingPivotIfOnlyOneBill()
		{
			if (Count == 0)
			{
				var master = Master;
				var invoiceLine = master.InvoiceLine;
				var declaration = invoiceLine == null ? null : invoiceLine.Declaration;
				if (declaration != null)
				{
					var bills = declaration.Bills.OfType<Bill>().Where(bill => !bill.IsDeleted && !bill.CU_BillNum.IsEmpty && (bill.CU_BillType == BillTypeList.Codes.MasterBill || bill.CU_BillType == BillTypeList.Codes.HouseBill)).ToArray();
					if (bills.Length == 1)
					{
						AddPivotFor(bills[0]);
					}
					else if (bills.Length == 2)
					{
						var houseBills = bills.Where(x => x.IsHouseBill).ToArray();
						if (houseBills.Length == 1)
						{
							AddPivotFor(houseBills[0]);
						}
					}
				}
			}
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			if (!runDefault)
			{
				runDefault = true;
				AddMissingPivotIfOnlyOneBill();
			}
		}
		bool runDefault;

		protected override string RelationType
		{
			get { return GenPivotTypeDecider.Types.FDARelatedBillsGenPivot; }
		}
	}
}
