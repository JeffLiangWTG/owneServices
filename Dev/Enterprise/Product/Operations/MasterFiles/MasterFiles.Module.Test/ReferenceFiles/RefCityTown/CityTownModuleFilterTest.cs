using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Module.ReferenceFiles.RefCityTown;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCityTownCountryStateModuleFilter))]
	sealed class CityTownModuleFilterTest : CountryStateModuleFilterTest<ZString>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RefCityTownCountryStateModuleFilter("Test", RefCityTownSchema.R9_RN_NKCountry, RefCityTownSchema.R9_RW_NKState);
		}

		protected override CountryStateModuleFilter<ZString> GetNewModuleFilter()
		{
			return new RefCityTownCountryStateModuleFilter("moo", RefCityTownSchema.R9_RN_NKCountry, RefCityTownSchema.R9_RW_NKState);
		}
	}
}
