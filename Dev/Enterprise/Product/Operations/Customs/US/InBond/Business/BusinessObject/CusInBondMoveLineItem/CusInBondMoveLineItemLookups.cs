namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveLineItemLookups : Customs.Business.CusInBondMoveLineItemLookups
	{
		public CusInBondMoveLineItemLookups(CusInBondMoveLineItem parent)
			: base(parent)
		{
		}

		public WeightUnitList WeightUnitList
		{
			get { return Factory.GetCachedValue<WeightUnitList>(); }
		}
	}
}
