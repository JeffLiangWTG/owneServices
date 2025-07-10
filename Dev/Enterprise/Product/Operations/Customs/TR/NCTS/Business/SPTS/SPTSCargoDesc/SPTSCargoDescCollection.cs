namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSCargoDescCollection : Customs.Business.CusInBondCargoDescCollection<SPTSCargoDesc>
	{
		public SPTSCargoDescCollection(SPTSContainer master)
			: base(master)
		{
		}
	}
}
