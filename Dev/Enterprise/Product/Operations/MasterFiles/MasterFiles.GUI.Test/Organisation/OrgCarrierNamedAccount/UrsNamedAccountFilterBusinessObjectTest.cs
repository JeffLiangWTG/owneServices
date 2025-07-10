using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UrsNamedAccountFilterBusinessObject))]
	public class UrsNamedAccountFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestHasFilters_NotForModule()
		{
			var filterBizo = new UrsNamedAccountFilterBusinessObject();
			filterBizo.QueryObjectType = typeof(OrgCarrierNamedAccount);
			var moduleFilters = filterBizo.GetModuleFilters();
			AssertEquals(3, moduleFilters.Count());

			AssertNotNull(filterBizo[UrsNamedAccountFilterBusinessObject.Schema.Organisation]);
			AssertNotNull(filterBizo[UrsNamedAccountFilterBusinessObject.Schema.OrganisationName]);
			AssertNotNull(filterBizo[UrsNamedAccountFilterBusinessObject.Schema.ForeignName]);
		}

		public void TestHasFilters_ForModule()
		{
			var filterBizo = new UrsNamedAccountFilterBusinessObject(carrierFilterDisabled: true);
			filterBizo.QueryObjectType = typeof(OrgCarrierNamedAccount);
			var moduleFilters = filterBizo.GetModuleFilters();
			AssertEquals(5, moduleFilters.Count());

			AssertNotNull(filterBizo[UrsNamedAccountFilterBusinessObject.Schema.Organisation]);
			AssertNotNull(filterBizo[UrsNamedAccountFilterBusinessObject.Schema.OrganisationName]);
			AssertNotNull(filterBizo[UrsNamedAccountFilterBusinessObject.Schema.Carrier]);
			AssertNotNull(filterBizo[UrsNamedAccountFilterBusinessObject.Schema.CarrierName]);
			AssertNotNull(filterBizo[UrsNamedAccountFilterBusinessObject.Schema.ForeignName]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UrsNamedAccountFilterBusinessObject();
		}
	}
}
