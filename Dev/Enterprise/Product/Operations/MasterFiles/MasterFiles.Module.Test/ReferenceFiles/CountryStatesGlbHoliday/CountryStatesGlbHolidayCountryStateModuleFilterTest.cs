using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CountryStatesGlbHolidayCountryStateModuleFilter))]
	sealed class CountryStatesGlbHolidayCountryStateModuleFilterTest : CountryStateModuleFilterTest<ZGuid>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CountryStatesGlbHolidayCountryStateModuleFilter("Test", GlbHolidaySchema.GH_ParentID, GlbHolidaySchema.GH_ParentTableCode);
		}

		protected override CountryStateModuleFilter<ZGuid> GetNewModuleFilter()
		{
			return new CountryStatesGlbHolidayCountryStateModuleFilter("moo", GlbHolidaySchema.GH_ParentID, GlbHolidaySchema.GH_ParentTableCode);
		}
	}
}
