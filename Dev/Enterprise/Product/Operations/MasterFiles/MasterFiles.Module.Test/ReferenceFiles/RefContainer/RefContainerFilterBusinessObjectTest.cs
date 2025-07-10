using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefContainerFilterBusinessObject))]
	sealed class RefContainerFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region CheckBoxFiltersTests

		public void TestStatusActiveFilter()
		{
			var activeContainer = Factory.NewWithValidTestData<RefContainer>();
			var inactiveContainer = Factory.NewWithValidTestData<RefContainer>();
			inactiveContainer.RC_IsActive = false;

			using (var refContainerModule = new RefContainerModuleForTest())
			{
				var filterBusinessObject = (RefContainerFilterBusinessObject)refContainerModule.FilterBusinessObject;

				var filter = (ModuleTextFilter)filterBusinessObject["Active Status"];
				AssertNotNull(filter);
				AssertEquals(FilterVisibility.AlwaysApplied, filter.Visibility);

				filter.IsActive = true;
				AssertEquals("Default value", "Active", filter.Property);

				var containers = new RefContainerCollection(Factory);
				containers.AdditionalFilter = filterBusinessObject.Filter;
				AssertCollectionContains(activeContainer, containers);
				AssertCollectionNotContains(inactiveContainer, containers);

				filter.Property = "Inactive";
				filter.IsActive = true;
				containers.AdditionalFilter = filterBusinessObject.Filter;
				AssertCollectionNotContains(activeContainer, containers);
				AssertCollectionContains(inactiveContainer, containers);

				filter.Property = "All";
				filter.IsActive = true;
				containers.AdditionalFilter = filterBusinessObject.Filter;
				AssertCollectionContains(activeContainer, containers);
				AssertCollectionContains(inactiveContainer, containers);
			}
		}

		#endregion

		#region TextFiltersTests

		public void TestContainerCodeFilter()
		{
			RefContainer containerCode1 = Factory.NewWithValidTestData<RefContainer>();
			RefContainer containerCode2 = Factory.NewWithValidTestData<RefContainer>();
			containerCode1.RC_Code = "SEA";
			containerCode2.RC_Code = "AIR";

			Factory.Save();

			RefContainerFilterBusinessObject filter = new RefContainerFilterBusinessObject();
			((ModuleTextFilter)filter["Container Code"]).Property = "SEA";
			((ModuleTextFilter)filter["Container Code"]).IsActive = true;

			RefContainerCollection containers = new RefContainerCollection(Factory, filter.Filter);

			AssertCollectionContains(containerCode1, containers);
			AssertCollectionNotContains(containerCode2, containers);
		}

		public void TestDescriptionFilter()
		{
			RefContainer description1 = Factory.NewWithValidTestData<RefContainer>();
			RefContainer description2 = Factory.NewWithValidTestData<RefContainer>();
			description1.RC_Description = "SEA";
			description2.RC_Description = "AIR";

			Factory.Save();

			RefContainerFilterBusinessObject filter = new RefContainerFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "SEA";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			RefContainerCollection containers = new RefContainerCollection(Factory, filter.Filter);

			AssertCollectionContains(description1, containers);
			AssertCollectionNotContains(description2, containers);
		}

		public void TestTrasportModeFilter()
		{
			RefContainer trasportMode1 = Factory.NewWithValidTestData<RefContainer>();
			RefContainer trasportMode2 = Factory.NewWithValidTestData<RefContainer>();
			trasportMode1.RC_ShippingMode = "SEA";
			trasportMode2.RC_ShippingMode = "AIR";

			Factory.Save();

			RefContainerFilterBusinessObject filter = new RefContainerFilterBusinessObject();
			((ModuleTextFilter)filter["Transport Mode"]).Property = "SEA";
			((ModuleTextFilter)filter["Transport Mode"]).IsActive = true;

			RefContainerCollection containers = new RefContainerCollection(Factory, filter.Filter);

			AssertCollectionContains(trasportMode1, containers);
			AssertCollectionNotContains(trasportMode2, containers);
		}

		public void TestContainerTypeFilter()
		{
			var container1 = Factory.NewWithValidTestData<RefContainer>();
			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container1.RC_ContainerType = Constants.ContainerTypes.Refrigerated;
			container2.RC_ContainerType = Constants.ContainerTypes.DryStorage;
			Factory.Save();

			var filter = new RefContainerFilterBusinessObject();
			((ModuleTextFilter)filter["Container Type"]).Property = Constants.ContainerTypes.Refrigerated;
			((ModuleTextFilter)filter["Container Type"]).IsActive = true;

			var containers = new RefContainerCollection(Factory, filter.Filter);
			AssertCollectionContains(container1, containers);
			AssertCollectionNotContains(container2, containers);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefContainerFilterBusinessObject();
		}

		#endregion
	}
}
