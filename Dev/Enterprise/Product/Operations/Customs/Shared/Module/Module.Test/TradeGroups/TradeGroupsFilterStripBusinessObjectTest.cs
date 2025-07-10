using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.CusRefTradeGroupCollection.FilterConstants;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(TradeGroupsFilterStripBusinessObject))]
	sealed class TradeGroupsFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCountryCodeFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var bizObj = new TradeGroupsFilterStripBusinessObject();
				var countryFilter = (ModuleNkFilter)bizObj.ModuleFilters[Constants.CountryCode];
				AssertEquals("Default value is current country", Core.Constants.CountryCodes.Australia, countryFilter.Property);
				AssertEquals("Country Code is default filter", FilterVisibility.AlwaysVisible, countryFilter.Visibility);
				AssertEquals("Country Code filter is read only", true, countryFilter.ReadOnly);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TradeGroupsFilterStripBusinessObject();
	}
}
