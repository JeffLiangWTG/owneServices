using System.Linq;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageCartageBehaviorStrategy : StandaloneCartageBehaviorStrategy
	{
		public CartageCartageBehaviorStrategy()
		{
		}

		protected override void RebuildLegs(CommonCartage cartage)
		{
			if (cartage.IsLoose)
			{
				foreach (var bookedMove in cartage.LooseBookedMoves)
				{
					bookedMove.CartageLegs.DeleteAll();
					bookedMove.DefaultAddresses();
					bookedMove.CreateDefaultLegs();
				}
			}
			else
			{
				cartage.LooseBookedMoves.DeleteAll();
			}

			if (cartage.IsContainerised)
			{
				foreach (var container in cartage.Containers)
				{
					var move = cartage.GetBookedMoves(container)[0];
					move.CartageLegs.DeleteAll();
					move.DefaultAddresses();
					move.CreateDefaultLegs();
				}
			}
			else
			{
				foreach (CommonContainer container in cartage.Containers.ToArray())
				{
					container.Delete();
				}
			}
		}
	}
}
