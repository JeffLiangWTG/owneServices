using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefMessagingBussCarrierInfoFilterBusinessObject))]
	class RefMessagingBussCarrierInfoFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCodeFilter()
			=> AssertModuleTextFilter("Code", "INABC", (carrier1, carrier2) =>
			{
				carrier1.ZMC_CarrierCode = "INABC";
				carrier2.ZMC_CarrierCode = "AUABC";

				return new[] { carrier1 };
			});

		public void TestNameFilter()
			=> AssertModuleTextFilter("Name", "ABC Courier Company", (carrier1, carrier2) =>
			{
				carrier1.ZMC_CarrierName = "ABC Courier Company 1";
				carrier2.ZMC_CarrierName = "ABC Courier Company 2";

				return new[] { carrier1, carrier2 };
			});

		public void TestCountryFilter()
			=> AssertModuleTextFilter("Country", "IN", (carrier1, carrier2) =>
			{
				carrier1.ZMC_CountryCode = "UK";
				carrier2.ZMC_CountryCode = "IN";

				return new[] { carrier2 };
			});

		void AssertModuleTextFilter(string searchProperty, string searchKeyword, Func<RefMessagingBussCarrierInfo, RefMessagingBussCarrierInfo, IEnumerable<RefMessagingBussCarrierInfo>> getExpected)
		{
			var carrier1 = Factory.NewWithValidTestData<RefMessagingBussCarrierInfo>();
			var carrier2 = Factory.NewWithValidTestData<RefMessagingBussCarrierInfo>();
			var expectedCarriers = getExpected(carrier1, carrier2);

			Factory.Save();

			var filterBO = GetNewFilterStripBusinessObject();

			if (filterBO != null && filterBO[searchProperty] is ModuleTextFilter filter)
			{
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = searchKeyword;
				filter.IsActive = true;
			}

			var filterCollection = new RefMessagingBussCarrierInfoCollection(Factory);
			AssertContainsExactElementsInAnyOrder(expectedCarriers, filterCollection.Find(filterBO.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			=> new RefMessagingBussCarrierInfoFilterBusinessObject();
	}
}
