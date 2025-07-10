using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchConsignmentProcessTaskCollection))]
	public class WhsItemDispatchConsignmentProcessTaskCollectionTest :
		ProcessTaskCollectionTest<WhsItemDispatchConsignmentProcessTaskCollection>
	{
		protected override WhsItemDispatchConsignmentProcessTaskCollection GetCollectionToTestCore()
		{
			return new WhsItemDispatchConsignmentProcessTaskCollection(Factory.New<WhsItemDispatchConsignment>());
		}
	}
}
