using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitCollection))]
	class WhsItemDispatchTransportationUnitCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemDispatchTransportationUnitCollection>
	{
		#region Implementation

		protected override WhsItemDispatchTransportationUnitCollection GetCollectionToTest()
		{
			return new WhsItemDispatchTransportationUnitCollection(Factory);
		}

		#endregion
	}
}
