using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitCollection))]
	public class WhsItemReceiveTransportationUnitCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemReceiveTransportationUnitCollection>
	{
		#region Implementation

		protected override WhsItemReceiveTransportationUnitCollection GetCollectionToTest()
		{
			return new WhsItemReceiveTransportationUnitCollection(Factory);
		}

		#endregion
	}
}
