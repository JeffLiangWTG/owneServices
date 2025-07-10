namespace Enterprise.Customs.NL.NCTS.Business
{
	public class NctsDepartureHeaderContainerCollection : EU.NCTS.Business.NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>
	{
		public NctsDepartureHeaderContainerCollection(NctsHeader master) : base(master)
		{
		}
	}
}
