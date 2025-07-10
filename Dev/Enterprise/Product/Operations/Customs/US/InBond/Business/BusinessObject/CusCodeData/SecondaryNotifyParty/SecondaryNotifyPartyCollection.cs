namespace Enterprise.Customs.US.InBond.Business
{
	public class SecondaryNotifyPartyCollection : Customs.Business.CusCodeDataCollection<SecondaryNotifyParty>
	{
		public SecondaryNotifyPartyCollection(CusInBondMoveDetail master)
			: base(master, CusCodeDataTypeList.Codes.SecondaryNotifyParty)
		{
		}
	}
}
