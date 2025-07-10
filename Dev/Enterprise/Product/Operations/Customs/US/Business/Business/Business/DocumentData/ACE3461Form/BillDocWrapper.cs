using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class BillDocWrapper : NonPersistentBusinessObject
	{
		public BillDocWrapper(Bill bill, ZString billType)
		{
			this.bill = bill;
			this.billType = billType;
		}
		readonly Bill bill;
		public const string MasterBill = "MB";
		public const string HouseBill = "HB";
		public const string InBondBill = "IB";
		public const string RegularBill = "RB";

		readonly ZString billType;
		JobDeclaration Declaration
		{
			get { return bill.Declaration; }
		}

		public ZString IsMaster
		{
			get { return billType == MasterBill ? "X" : ""; }
		}

		public ZString IsHouse
		{
			get { return billType == HouseBill ? "X" : ""; }
		}

		public ZString IsInBond
		{
			get { return billType == InBondBill ? "X" : ""; }
		}

		public ZString IsRegular
		{
			get { return billType == RegularBill ? "X" : ""; }
		}

		public ZString SCAC
		{
			get
			{
				var result = ZString.Empty;
				if (Declaration != null)
				{
					if (Declaration.IsConsumptionFTZ)
					{
						result = "FTZ" + Declaration.JE_MasterBill;
					}
					else
					{
						result = bill.US_UI_NKBillIssuerSCAC;
					}
				}
				return result;
			}
		}

		public ZString InBondNumber
		{
			get { return billType == InBondBill ? bill.ITNumber : ZString.Empty; }
		}

		public ZString BillNumber
		{
			get { return bill.CU_BillNum; }
		}

		public ZDecimal Quantity
		{
			get { return bill.CU_NoOfPacks; }
		}

		public ZString UnitOfMeasure
		{
			get { return bill.CU_PackType; }
		}

		public ZString IsNonAMS
		{
			get { return ((IBillDetails)bill).IsNonAMS ? "X" : ""; }
		}

		public ZString IsSplitBill
		{
			get { return ((IBillDetails)bill).IsSplit ? "X" : ""; }
		}
	}
}
