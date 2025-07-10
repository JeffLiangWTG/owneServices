using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsCartonGroupCollection))]
	class WhsCartonGroupCollectionTestCase : WhsActiveBusinessObjectCollectionTestCase<WhsCartonGroupCollection>
	{
		#region Implementation

		#region GetCollectionToTest

		protected override WhsCartonGroupCollection GetCollectionToTest()
		{
			return new WhsCartonGroupCollection(Factory);
		}

		#endregion 

		#endregion
	}
}
