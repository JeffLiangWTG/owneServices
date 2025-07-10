using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageContainerHelper))]
	public class CartageContainerHelperBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CartageContainerHelper(Factory, GetContainer());
		}

		CommonContainer GetContainer()
		{
			return container ?? (container = Factory.New<CommonContainer>());
		}

		CommonContainer container;
	}
}
