using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefJobEquipmentFilterBusinessObject))]
	class RefJobEquipmentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCodeFilter()
			=> AssertModuleTextFilter("Code", "EquA", (jobEquipment1, jobEquipment2) =>
			{
				jobEquipment1.JEQ_Code = "EquA";
				jobEquipment2.JEQ_Code = "EquB";
				return new[] { jobEquipment1 };
			});

		public void TestDescriptionFilter()
			=> AssertModuleTextFilter("Name", "Equipment Combination", (jobEquipment1, jobEquipment2) =>
			{
				jobEquipment1.JEQ_Description = "Equipment Combination A";
				jobEquipment2.JEQ_Description = "Equipment Combination B";
				return new[] { jobEquipment1, jobEquipment2 };
			});

		public void TestActiveStatus()
		{
			using (var testModule = new RefJobEquipmentModule())
			{
				var filter = testModule.FilterBusinessObject["Active Status"] as ModuleTextFilter;
				AssertNotNull("The 'Active Status' should be exists in the Job Equipment module", filter);
				Assert("The 'Active Status' should be visible always", filter.Visibility == FilterVisibility.AlwaysApplied);
			}
		}

		public void TestActiveStatusFilter()
		{
			using (var testModule = new RefJobEquipmentModule())
			{
				var filter = testModule.FilterBusinessObject["Active Status"] as ModuleTextFilter;
				AssertNotNull("The 'Active Status' should be exists in the Job Equipment module", filter);

				var activeJobEquipment = Factory.NewWithValidTestData<JobEquipment>();
				var inactiveJobEquipment = Factory.NewWithValidTestData<JobEquipment>();

				activeJobEquipment.JEQ_IsActive = true;
				inactiveJobEquipment.JEQ_IsActive = false;

				Factory.Save();

				filter.Property = "Active";
				filter.IsActive = true;

				var result = Factory.Load<JobEquipment>(testModule.FilterBusinessObject.Filter);
				AssertCollectionContains(activeJobEquipment, result);
				AssertCollectionNotContains(inactiveJobEquipment, result);

				filter.Property = "Inactive";
				filter.IsActive = true;

				result = Factory.Load<JobEquipment>(testModule.FilterBusinessObject.Filter);
				AssertCollectionContains(inactiveJobEquipment, result);
				AssertCollectionNotContains(activeJobEquipment, result);

				filter.Property = "All";
				filter.IsActive = true;

				result = Factory.Load<JobEquipment>(testModule.FilterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { inactiveJobEquipment, activeJobEquipment }, result);
			}
		}

		void AssertModuleTextFilter(string searchProperty, string searchKeyword, Func<JobEquipment, JobEquipment, IEnumerable<JobEquipment>> getExpected)
		{
			var jobEquipment1 = Factory.NewWithValidTestData<JobEquipment>();
			var jobEquipment2 = Factory.NewWithValidTestData<JobEquipment>();
			var expectedCarriers = getExpected(jobEquipment1, jobEquipment2);

			Factory.Save();

			var filterBO = GetNewFilterStripBusinessObject();

			if (filterBO != null && filterBO[searchProperty] is ModuleTextFilter filter)
			{
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = searchKeyword;
				filter.IsActive = true;
			}

			var filterCollection = new JobEquipmentCollection(Factory);
			AssertContainsExactElementsInAnyOrder(expectedCarriers, filterCollection.Find(filterBO.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new RefJobEquipmentFilterBusinessObject();
	}
}
