using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryHeaderCharge : Customs.Business.CusEntryHeaderCharges
	{
		public CusEntryHeaderCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsWaivable => C1_ChargeType == EntryChargeTypeList.Codes.EntryFee || C1_ChargeType == EntryChargeTypeList.Codes.EntryFeeGST;
	}
}
