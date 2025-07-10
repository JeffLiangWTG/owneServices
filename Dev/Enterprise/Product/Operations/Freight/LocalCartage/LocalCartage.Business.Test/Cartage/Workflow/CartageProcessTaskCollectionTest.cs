using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageProcessTaskCollection))]
	class CartageProcessTaskCollectionTest : ProcessTaskCollectionTest<CartageProcessTaskCollection>
	{
		protected override CartageProcessTaskCollection GetCollectionToTestCore()
		{
			return new CartageProcessTaskCollection(Cartage);
		}

		CommonCartage Cartage
		{
			get
			{
				if (cartage == null)
				{
					cartage = Factory.NewWithValidTestData<CommonCartage>();
				}

				return cartage;
			}
		}

		CommonCartage cartage;
	}
}
