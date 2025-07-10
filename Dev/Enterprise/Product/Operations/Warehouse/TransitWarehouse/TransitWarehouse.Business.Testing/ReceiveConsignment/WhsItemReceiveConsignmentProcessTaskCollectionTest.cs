using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveConsignmentProcessTaskCollection))]
	public class WhsItemReceiveConsignmentProcessTaskCollectionTestCase :
		ProcessTaskCollectionTest<WhsItemReceiveConsignmentProcessTaskCollection>
	{
		protected override WhsItemReceiveConsignmentProcessTaskCollection GetCollectionToTestCore()
		{
			return new WhsItemReceiveConsignmentProcessTaskCollection(Factory.New<WhsItemReceiveConsignment>());
		}
	}
}
