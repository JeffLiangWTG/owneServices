using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class VoucherOfCorrectionValueAfterCollection<TMaster> : VoucherOfCorrectionValueCollection<VoucherOfCorrectionValueAfter, TMaster>
		where TMaster : BusinessObject
	{
		public VoucherOfCorrectionValueAfterCollection(TMaster master)
			: base(master, CusCodeDataTypeList.Codes.VOCValueAfter)
		{
		}
	}
}
