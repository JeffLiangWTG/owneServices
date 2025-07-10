using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class StampDutyCargoDescCollection : CusInBondCargoDescCollection<StampDutyCargoDesc>
	{
		public StampDutyCargoDescCollection(NctsHeader master) : base(master)
		{
		}
	}
}
