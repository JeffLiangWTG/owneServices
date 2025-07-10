using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefContainerCollection))]
	sealed class RefContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<RefContainerCollection>
	{
		public void TestAirFilter()
		{
			RefContainerCollection containers = new RefContainerCollection(Factory, "AIR");
			AssertEquals("AIR", containers.FilterBusinessObjectDefaults["Transport Mode" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			RefContainer newContainer = containers.AddNew();
			AssertEquals("AIR", newContainer.RC_ShippingMode);
		}

		public void TestSeaFilter()
		{
			RefContainerCollection containers = new RefContainerCollection(Factory, "SEA");
			AssertEquals("SEA", containers.FilterBusinessObjectDefaults["Transport Mode" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			RefContainer newContainer = containers.AddNew();
			AssertEquals("SEA", newContainer.RC_ShippingMode);
		}

		public void TestRoadFilter()
		{
			RefContainerCollection containers = new RefContainerCollection(Factory, "ROA");
			AssertEquals("ROA", containers.FilterBusinessObjectDefaults["Transport Mode" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			RefContainer newContainer = containers.AddNew();
			AssertEquals("ROA", newContainer.RC_ShippingMode);
		}
	}
}
