namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageInternalCartageBehaviorStrategy : InternalCartageBehaviorStrategy
	{
		public CartageInternalCartageBehaviorStrategy()
		{
		}

		protected override void RebuildLegs(CommonCartage cartage)
		{
			InternalCartageManagerHelper.Refresh(cartage, cartage.CartageInternalType);
		}
	}
}
