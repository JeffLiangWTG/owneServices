using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(BondedEntryKeyLookupCollection))]
	internal class BondedEntryKeyLookupCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BondedEntryKeyLookupCollection(Factory);
		}
	}
}
