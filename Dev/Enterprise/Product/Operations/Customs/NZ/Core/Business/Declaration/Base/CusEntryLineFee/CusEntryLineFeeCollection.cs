using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryLineFeeCollection : Customs.Business.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>
	{
		public CusEntryLineFeeCollection(CusEntryLine master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public CusEntryLineFee AddNew(string feeType)
		{
			CusEntryLineFee result = AddNew();
			result.CF_ChargeType = feeType;
			return result;
		}
	}
}
