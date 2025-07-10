namespace Enterprise.Customs.TW.Business
{
	public class CusInBondContainerCollection : Customs.Business.CusInBondContainerCollection<CusInBondContainer>
	{
		public CusInBondContainerCollection(CusInBondMoveDetail master)
			: base(master)
		{
		}
	}
}
