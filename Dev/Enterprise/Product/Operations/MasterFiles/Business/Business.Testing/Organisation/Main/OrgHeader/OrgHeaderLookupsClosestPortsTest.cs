using System.Collections;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderLookupsClosestPortsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainsFilters()
		{
			var lookups = new OrgHeaderLookupsWithUserFilters(OrgHeader.New(Factory));
			(lookups.ClosestPorts.FilterBusinessObjectDefaults as IEnumerable).GetEnumerator(); // Mock Add FilterBusinessObjectDefault to filter
			var cpFilters = lookups.ClosestPorts.FilterBusinessObjectDefaults;
			CombineAssertions("Filter defaults should exist", () =>
			{
				AssertEquals("Should contain UNLOCO Identifiers", true, cpFilters.ContainsDefaultFor("UNLOCO Identifiers:Property0"));
				AssertEquals("Should contain Country Filter", true, cpFilters.ContainsDefaultFor("CountryState:Property1"));
				AssertEquals("Should contain State Filter", true, cpFilters.ContainsDefaultFor("CountryState:Property2"));
			});
		}
	}
}
