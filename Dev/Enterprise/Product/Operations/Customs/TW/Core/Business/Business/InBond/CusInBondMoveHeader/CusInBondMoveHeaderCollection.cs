namespace Enterprise.Customs.TW.Business
{
	public class CusInBondMoveHeaderCollection : Customs.Business.CusInBondMoveHeaderCollection
	{
		public CusInBondMoveHeaderCollection(CusInBondHeader master)
			: base(master)
		{
		}

		#region Implementation
		public new CusInBondMoveHeader this[int index] => (CusInBondMoveHeader)base[index];

		public new CusInBondMoveHeader AddNew() => (CusInBondMoveHeader)base.AddNew();
		#endregion
	}
}
