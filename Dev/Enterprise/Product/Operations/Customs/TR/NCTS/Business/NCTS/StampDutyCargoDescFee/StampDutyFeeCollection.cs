using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class StampDutyFeeCollection : CusInBondFeeCollection<StampDutyFee>
	{
		public StampDutyFeeCollection(CusInBondCargoDesc cusInBondCargoDesc) : base(cusInBondCargoDesc)
		{
		}
	}
}
