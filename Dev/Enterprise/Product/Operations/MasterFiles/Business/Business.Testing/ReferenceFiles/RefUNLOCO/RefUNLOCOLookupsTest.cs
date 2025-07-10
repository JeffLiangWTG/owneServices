using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefUNLOCOLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatesRestrictedByCountry()
		{
			var unLoco = Factory.New<RefUNLOCO>();
			unLoco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var santaSomethingInUS = new ZQuery(RefCountryStatesSchema.RW_Code, "SC");
			IList<RefCountryStates> matches = new List<RefCountryStates>(unLoco.Lookups.CountryStates.Find(santaSomethingInUS));
			AssertEquals(1, matches.Count);
			AssertEquals("US", matches[0].Country.Code);
		}
	}
}
