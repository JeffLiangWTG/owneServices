using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveASNProcessTaskCollection))]
	public class WhsItemReceiveASNProcessTaskCollectionTestCase :
		ProcessTaskCollectionTest<WhsItemReceiveASNProcessTaskCollection>
	{
		protected override WhsItemReceiveASNProcessTaskCollection GetCollectionToTestCore()
		{
			return new WhsItemReceiveASNProcessTaskCollection(Factory.New<WhsItemReceiveASN>());
		}
	}
}
