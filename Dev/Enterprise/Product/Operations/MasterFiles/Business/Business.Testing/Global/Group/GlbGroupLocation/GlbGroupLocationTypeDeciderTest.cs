using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbGroupLocationTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new GlbGroupLocationTypeDecider();
			AssertEquals(typeof(GlbGroupLocation), typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForLoad()
		{
			var salesTeam = Factory.NewWithValidTestData<SalesTeam>();
			salesTeam.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));
			salesTeam.CoveredUnlocos.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var teamLocations = otherFactory.Load<GlbGroupLocation>(new ZQuery(GlbGroupLocationSchema.GGL_GG, salesTeam.PK));
			AssertType(typeof(GlbGroupCountry), teamLocations.First(x => x.GGL_PortOrCountry == "AU"));
			AssertType(typeof(GlbGroupUnloco), teamLocations.First(x => x.GGL_PortOrCountry == "AUSYD"));
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new GlbGroupLocationTypeDecider();
			AssertEquals(null, typeDecider.GetTypeForNew());
		}
	}
}
