using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageContainerHelperTest : TestCaseWithFactory
	{
		public void TestFirstCartage()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = Factory.New<CommonBookedCtgMove>();
			var container = Factory.New<CommonContainer>();
			move.EW_JC_Container = container.PK;
			AssertNull(CartageContainerHelper(container).FirstCartage);
			cartage.ContainerBookedMoves.Add(move);
			AssertEquals(cartage, CartageContainerHelper(container).FirstCartage);
		}

		CartageContainerHelper CartageContainerHelper(CommonContainer container)
		{
			return cartageContainerHelper ?? (cartageContainerHelper = new CartageContainerHelper(Factory, container));
		}

		CartageContainerHelper cartageContainerHelper;
	}
}
