using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsProductStyleColourCollection))]
	class WhsProductStyleColourCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsProductStyleColourCollection>
	{
		#region Implementation

		protected override WhsProductStyleColourCollection GetCollectionToTest()
		{
			var productStyle = Factory.New<WhsProductStyle>();
			return new WhsProductStyleColourCollection(productStyle);
		}

		#endregion
	}
}
