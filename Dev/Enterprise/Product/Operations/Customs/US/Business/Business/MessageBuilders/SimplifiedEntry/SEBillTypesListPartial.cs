using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	partial class SEBillTypesList
	{
		public static string GetBillTypeIndicator(string type)
		{
			switch (type)
			{
				case BillTypeList.Codes.MasterBill:
					return SEBillTypesList.Codes.MasterBill;
				case BillTypeList.Codes.HouseBill:
					return SEBillTypesList.Codes.HouseBill;
				case BillTypeList.Codes.SubHouseBill:
					return SEBillTypesList.Codes.SubHouseBill;
				default:
					return "";
			}
		}
	}
}
