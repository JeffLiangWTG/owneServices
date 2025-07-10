using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(BulkRateUpdaterContainerType))]
	public class BulkRateUpdaterContainerTypeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var update = new BulkRateUpdater();
			return update.ContainerTypes.AddNew();
		}

		public void TestSettingContainerCodeUpdatesContainer()
		{
			var update = new BulkRateUpdater();
			var containerType = update.ContainerTypes.AddNew();

			AssertNull(containerType.Container);

			containerType.RC_Code = "20GP";

			AssertNotNull(containerType.Container);
			AssertEquals(Helper.Containers["20GP"].PK, containerType.Container.PK);

			containerType.RC_Code = "40GP";

			AssertNotNull(containerType.Container);
			AssertEquals(Helper.Containers["40GP"].PK, containerType.Container.PK);

			containerType.RC_Code = "code that doesn't exist";

			AssertNull(containerType.Container);
		}

		TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;
	}
}
