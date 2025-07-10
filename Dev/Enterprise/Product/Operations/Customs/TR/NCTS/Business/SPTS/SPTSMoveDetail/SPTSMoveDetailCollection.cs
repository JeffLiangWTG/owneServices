using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSMoveDetailCollection : CusInBondMoveDetailCollection<SPTSMoveDetail>
	{
		public SPTSMoveDetailCollection(SPTSDepartureMovementHeader master)
			: base(master)
		{
		}
	}
}
