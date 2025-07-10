using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsProductStyleClassificationCollection))]
	class WhsProductStyleClassificationCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsProductStyleClassificationCollection>
	{
		#region Implementation

		protected override WhsProductStyleClassificationCollection GetCollectionToTest()
		{
			var productStyle = Factory.New<WhsProductStyle>();
			return new WhsProductStyleClassificationCollection(productStyle);
		}

		#endregion
	}
}
