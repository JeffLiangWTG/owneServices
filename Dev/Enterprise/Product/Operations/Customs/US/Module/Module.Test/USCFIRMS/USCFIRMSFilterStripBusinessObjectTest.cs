using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCFIRMSFilterStripBusinessObject))]
	sealed class USCFIRMSFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCFIRMSFilterStripBusinessObject();
			AssertNotNull(filter[USCFIRMS.Constants.Code]);
			AssertNotNull(filter[USCFIRMS.Constants.Name]);
			AssertNotNull(filter[USCFIRMS.Constants.DistrictPortCode]);
			AssertNotNull(filter[USCFIRMS.Constants.Address]);
			AssertNotNull(filter[USCFIRMS.Constants.City]);
			AssertNotNull(filter[USCFIRMS.Constants.State]);
			AssertNotNull(filter[USCFIRMS.Constants.ZipCode]);
			AssertNotNull(filter[USCFIRMS.Constants.Country]);
			AssertNotNull(filter[USCFIRMS.Constants.FacilityType]);
			AssertNotNull(filter[USCFIRMS.Constants.IsActive]);
			AssertNotNull(filter[USCFIRMS.Constants.LastUpdate]);
			var moduleFilter = filter[USCFIRMS.Constants.FacilityType] as ModuleTextFilter;
			AssertNotNull(moduleFilter);
			AssertEquals("List", typeof(FacilityTypeList), moduleFilter.List.GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCFIRMSFilterStripBusinessObject();
	}
}
