using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCreditorGroupCollection))]
	sealed class OrgCreditorGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIOrgCreditorGroupCollection()
		{
			AssertNotNull(new OrgCreditorGroupCollection(Factory) is IOrgCreditorGroupCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgCreditorGroupCollection(Factory);
		}
	}
}
