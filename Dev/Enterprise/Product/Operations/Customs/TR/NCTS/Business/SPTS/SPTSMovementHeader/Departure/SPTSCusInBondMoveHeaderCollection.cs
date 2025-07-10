namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSCusInBondMoveHeaderCollection : Customs.Business.CusInBondMoveHeaderCollection
	{
		public SPTSCusInBondMoveHeaderCollection(SPTSHeader master)
			: base(master)
		{
		}

		#region Implementation
		public new SPTSDepartureMovementHeader this[int index] => (SPTSDepartureMovementHeader)base[index];

		public new SPTSDepartureMovementHeader AddNew() => (SPTSDepartureMovementHeader)base.AddNew();
		#endregion
	}
}
