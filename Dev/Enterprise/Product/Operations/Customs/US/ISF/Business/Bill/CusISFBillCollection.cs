using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFBillCollection : ActiveBusinessObjectCollection<CusISFBill>, ICusISFBillCollection<CusISFBill>
	{
		public CusISFBillCollection(CusISFHeader iSFHeader)
			: base(iSFHeader)
		{
		}

		public void UpdateBill(ZString billNumber, ZString status, ZDateTime statusDate)
		{
			bool updateMatchedDate = status == DispositionCodeList.Codes.S1;
			foreach (CusISFBill bill in this)
			{
				if ((bill.IsOceanBillOfLading || bill.IsHouseBillOfLading) && bill.BB_BillNum.EqualsIgnoringCase(billNumber))
				{
					bill.BB_CustomsStatus = status;
					if (updateMatchedDate)
					{
						bill.BB_MatchDate = statusDate;
					}
				}
			}
		}

		public CusISFBill this[ZString billType, ZString billNumber]
		{
			get
			{
				if (!billType.IsEmpty && !billNumber.IsEmpty)
				{
					foreach (CusISFBill bill in this)
					{
						if (bill.BB_BillType == billType && bill.BB_BillNum == billNumber)
						{
							return bill;
						}
					}
				}
				return null;
			}
		}

		public CusISFBill GetFirstMatchingType(ZString type)
		{
			CusISFBill result = null;
			foreach (CusISFBill reference in this)
			{
				if (reference.BB_BillType == type)
				{
					result = reference;
					break;
				}
			}
			return result;
		}
	}
}
