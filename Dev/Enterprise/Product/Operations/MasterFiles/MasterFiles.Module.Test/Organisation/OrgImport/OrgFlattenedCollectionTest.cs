using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	[TestedType(typeof(OrgFlattenedCollection))]
	sealed class OrgFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgFlattenedCollection>
	{
		protected override OrgFlattenedCollection GetCollectionToTest()
		{
			return new OrgFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgFlattened();
		}
	}
}
