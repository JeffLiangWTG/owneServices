using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefAccessorialFilterBusinessObject))]
	class RefAccessorialFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCodeFilter()
			=> AssertModuleTextFilter("Code", "ABC", (carrier1, carrier2) =>
			{
				carrier1.ASI_Code = "ABC";
				carrier2.ASI_Code = "XYZ";

				return new[] { carrier1 };
			});

		public void TestDescriptionFilter()
			=> AssertModuleTextFilter("Description", "ABC Courier Company", (carrier1, carrier2) =>
			{
				carrier1.ASI_Description = "ABC Courier Company 1";
				carrier2.ASI_Description = "ABC Courier Company 2";

				return new[] { carrier1, carrier2 };
			});

		void AssertModuleTextFilter(string searchProperty, string searchKeyword, Func<RefAccessorial, RefAccessorial, IEnumerable<RefAccessorial>> getExpected)
		{
			var carrier1 = Factory.NewWithValidTestData<RefAccessorial>();
			var carrier2 = Factory.NewWithValidTestData<RefAccessorial>();
			var expectedCarriers = getExpected(carrier1, carrier2);

			Factory.Save();

			var filterBO = GetNewFilterStripBusinessObject();

			if (filterBO != null && filterBO[searchProperty] is ModuleTextFilter filter)
			{
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = searchKeyword;
				filter.IsActive = true;
			}

			var filterCollection = new RefAccessorialCollection(Factory);
			AssertContainsExactElementsInAnyOrder(expectedCarriers, filterCollection.Find(filterBO.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			=> new RefAccessorialFilterBusinessObject();
	}
}
