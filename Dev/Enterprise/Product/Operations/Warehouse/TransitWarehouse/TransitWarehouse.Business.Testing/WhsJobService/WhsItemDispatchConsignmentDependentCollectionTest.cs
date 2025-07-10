using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsJobServiceDependentCollection))]
	public class WhsItemDispatchConsignmentDependentCollectionTest : BusinessObjectCollectionTestCaseBase<WhsItemDispatchConsignment>
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new WhsJobServiceDependentCollection(Factory.New<WhsItemDispatchConsignment>(), Factory);
	}
}
