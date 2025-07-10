using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Module.Testing
{
	sealed class CusAuthorisationsFilterLookupsTest : TestCaseWithFactory
	{
		public void TestOrganisationList()
		{
			AssertEquals("OrganisationList should not be loaded by the property", false, ((IBusinessObjectCollection)lookups.OrganisationList).IsLoaded);
		}

		public void TestCurrentStatusList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.CurrentStatusList;
				var newLine = System.Environment.NewLine;
				AssertEquals("CodesAsString", $"All - Show All Records{newLine}Yes - Show Current Only{newLine}No - Show Non-current Only", list.ElementsAsString);
				AssertSame("Cached", list, lookups.CurrentStatusList);
			});
		}

		public void TestRuleCodeList()
		{
			var latviaList = CusAuthorisationHeaderProvider.GetByCountryCode(Core.Constants.CountryCodes.Latvia).GetRuleCodeListForModule(Factory).CodesAsString;
			var germanList = CusAuthorisationHeaderProvider.GetByCountryCode(Core.Constants.CountryCodes.Germany).GetRuleCodeListForModule(Factory).CodesAsString;
			AssertNotEquals(latviaList, germanList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new CusAuthorisationsFilterLookups(new CusAuthorisationsFilterStripBusinessObject());
		}

		CusAuthorisationsFilterLookups lookups;
	}
}
