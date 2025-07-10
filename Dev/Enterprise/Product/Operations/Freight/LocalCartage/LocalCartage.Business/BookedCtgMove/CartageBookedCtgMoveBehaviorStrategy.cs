using System;
using System.Linq;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageBookedCtgMoveBehaviorStrategy : CommonBookedCtgMoveBehaviorStrategy
	{
		public CartageBookedCtgMoveBehaviorStrategy()
		{
		}

		public override void CartageBookedMoveLinkCreated(CommonBookedCtgMove bookedMove)
		{
			CommonCartage cartage = bookedMove.Cartage;

			using (bookedMove.SuspendSettingHasChanges())
			{
				var cartageMoves = cartage.BookedMovesCollection;
				bookedMove.EW_DisplayOrder = cartageMoves.Any() ? Convert.ToInt16(cartageMoves.Max(m => m.EW_DisplayOrder) + 1) : Convert.ToInt16(1);
				bookedMove.DefaultAddresses();
				bookedMove.CreateDefaultLegs();
				bookedMove.SetDropModeToCartageDropModeFallbackIfEmpty();
				bookedMove.Cartage.RefreshBindingIncludingChildren();
			}
		}

		public override void CartageBookedMoveLinkBroken(CommonBookedCtgMove bookedMove)
		{
		}
	}
}
