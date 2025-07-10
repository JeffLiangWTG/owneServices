using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDebtorGroupCollection))]
	sealed class OrgDebtorGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIOrgDebtorGroupCollection()
		{
			AssertNotNull(new OrgDebtorGroupCollection(Factory) is IOrgDebtorGroupCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgDebtorGroupCollection(Factory);
		}
	}
}
