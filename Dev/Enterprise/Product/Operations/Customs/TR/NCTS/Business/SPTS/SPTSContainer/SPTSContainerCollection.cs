using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSContainerCollection : CusInBondContainerCollection<SPTSContainer>
	{
		public SPTSContainerCollection(SPTSHeader master)
			: base(master)
		{
		}

		public SPTSContainerCollection(CusInBondMoveDetail master)
			: base(master)
		{
		}

		public SPTSContainerCollection(SPTSBill master)
			: base(master)
		{
		}
	}
}
