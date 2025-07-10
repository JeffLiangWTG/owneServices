using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CountryStatesGlbHolidayFilterBusinessObject))]
	sealed class CountryStatesGlbHolidayFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestModuleFilters()
		{
			var filterBizo = new CountryStatesGlbHolidayFilterBusinessObject();
			filterBizo.QueryObjectType = typeof(CountryStatesGlbHolidayBizo);
			var moduleFilters = filterBizo.ModuleFilters;
			AssertEquals(6, moduleFilters.Count());

			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "Holiday Name"));
			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "Recurring"));
			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "Active Status"));
			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "CountryState"));
			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "Is Working Day"));
			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "Custom SQL Filter"));
		}

		public void TestActiveStatusQuery()
		{
			var filterBizo = new CountryStatesGlbHolidayFilterBusinessObject();
			filterBizo.QueryObjectType = typeof(CountryStatesGlbHolidayBizo);
			// Needed to poke filters into action
			var moduleFilters = filterBizo.ModuleFilters;
			var filter = (ModuleTextFilter)filterBizo[FilterDescriptions.ActiveStatus];
			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					filter.Property = FilterStripBusinessObject.StatusActive;
					AssertEquals("GH_IsActive = 1".ToUpper(), filterBizo.Filter.LiteralTextADO.ToUpper());

					filter.Property = FilterStripBusinessObject.StatusInactive;
					AssertEquals("GH_IsActive = 0".ToUpper(), filterBizo.Filter.LiteralTextADO.ToUpper());

					filter.Property = FilterStripBusinessObject.StatusAll;
					AssertEquals("GH_IsActive = 1 or GH_IsActive = 0".ToUpper(), filterBizo.Filter.LiteralTextADO.ToUpper());
				}
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CountryStatesGlbHolidayFilterBusinessObject();
		}
	}
}
