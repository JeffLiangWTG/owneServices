using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsJobServiceDependentCollection))]
	public class WhsVASOrderDependentCollectionTest : BusinessObjectCollectionTestCaseBase<WhsItemReceiveConsignment>
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new WhsJobServiceDependentCollection(Factory.New<WhsItemReceiveConsignment>(), Factory);
	}
}
