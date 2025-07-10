using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(StmMenuItemFilteredCollection))]
	sealed class StmMenuItemFilteredCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmMenuItemFilteredCollection(Factory);
		}
	}
}
