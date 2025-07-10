using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class VoucherOfCorrectionValueBeforeCollection<TMaster> : VoucherOfCorrectionValueCollection<VoucherOfCorrectionValueBefore, TMaster>
		where TMaster : BusinessObject
	{
		public VoucherOfCorrectionValueBeforeCollection(TMaster master)
			: base(master, CusCodeDataTypeList.Codes.VOCValueBefore)
		{
		}
	}
}
