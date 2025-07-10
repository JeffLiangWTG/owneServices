using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Module.ReferenceFiles.RefUNLOCO;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefUNLOCOCountryStateModuleFilter))]
	sealed class UNLOCOModuleFilterTest : CountryStateModuleFilterTest<ZGuid>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RefUNLOCOCountryStateModuleFilter("Test", RefUNLOCOSchema.RL_RN_NKCountryCode, RefUNLOCOSchema.RL_RW);
		}

		protected override CountryStateModuleFilter<ZGuid> GetNewModuleFilter()
		{
			return new RefUNLOCOCountryStateModuleFilter("moo", RefUNLOCOSchema.RL_RN_NKCountryCode, RefUNLOCOSchema.RL_RW);
		}
	}
}
