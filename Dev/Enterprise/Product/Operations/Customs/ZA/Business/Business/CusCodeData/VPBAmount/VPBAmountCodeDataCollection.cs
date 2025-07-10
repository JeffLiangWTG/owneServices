using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.ZA;

namespace Enterprise.Customs.ZA.Business
{
	public class VPBAmountCodeDataCollection : CusCodeDataCollection<VPBAmountCodeData>
	{
		public VPBAmountCodeDataCollection(CUSDECEDIMessage master)
			: base(master, CusCodeDataTypeList.Codes.VPBAmount)
		{
		}

		internal void AddNew(ZString lineNumber, ZDecimal amount)
		{
			AddNew(VPBAmountCodeData.FormatToCY_Code(lineNumber), amount.RoundUsingCustomsValueRule().ToString());
		}
	}
}
