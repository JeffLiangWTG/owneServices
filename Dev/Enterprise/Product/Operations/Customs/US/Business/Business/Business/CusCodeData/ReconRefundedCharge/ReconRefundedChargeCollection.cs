using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconRefundedChargeCollection : CusCodeDataCollection<ReconRefundedCharge>
	{
		public ReconRefundedChargeCollection(BusinessObject parent)
			: base(parent, CusCodeDataTypeList.Codes.ReconRefundedCharge)
		{
		}

		public ReconRefundedCharge AddNewIfNotExists(ZString chargeType)
		{
			return GetFirstElementHaving(chargeType) ?? AddNew(chargeType);
		}
	}
}
