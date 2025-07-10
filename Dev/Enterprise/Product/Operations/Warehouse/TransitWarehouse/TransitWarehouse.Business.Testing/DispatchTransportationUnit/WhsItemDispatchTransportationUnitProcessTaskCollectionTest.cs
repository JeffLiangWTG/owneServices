using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitProcessTaskCollection))]
	public class WhsItemDispatchTransportationUnitProcessTaskCollectionTest : ProcessTaskCollectionTest<WhsItemDispatchTransportationUnitProcessTaskCollection>
	{
		protected override WhsItemDispatchTransportationUnitProcessTaskCollection GetCollectionToTestCore()
		{
			return new WhsItemDispatchTransportationUnitProcessTaskCollection(Factory.New<WhsItemDispatchTransportationUnit>());
		}
	}
}
