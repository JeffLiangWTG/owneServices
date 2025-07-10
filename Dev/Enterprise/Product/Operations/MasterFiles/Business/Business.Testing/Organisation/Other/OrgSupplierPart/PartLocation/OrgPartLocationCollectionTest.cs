using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartLocationCollection))]
	sealed class OrgPartLocationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgSupplierPart parent = Factory.New<OrgSupplierPart>();
			return new OrgPartLocationCollection(parent, Factory);
		}
	}
}
