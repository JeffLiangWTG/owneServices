using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListProcessTaskCollection))]
	public class WhsItemDispatchLoadListProcessTaskCollectionTest :
		ProcessTaskCollectionTest<WhsItemDispatchLoadListProcessTaskCollection>
	{
		protected override WhsItemDispatchLoadListProcessTaskCollection GetCollectionToTestCore()
		{
			return new WhsItemDispatchLoadListProcessTaskCollection(Factory.New<WhsItemDispatchLoadList>());
		}
	}
}
