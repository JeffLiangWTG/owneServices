using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitProcessTaskCollection))]
	public class WhsItemReceiveTransportationUnitProcessTaskCollectionTestCase :
		ProcessTaskCollectionTest<WhsItemReceiveTransportationUnitProcessTaskCollection>
	{
		protected override WhsItemReceiveTransportationUnitProcessTaskCollection GetCollectionToTestCore()
		{
			return new WhsItemReceiveTransportationUnitProcessTaskCollection(Factory.New<WhsItemReceiveTransportationUnit>());
		}
	}
}
