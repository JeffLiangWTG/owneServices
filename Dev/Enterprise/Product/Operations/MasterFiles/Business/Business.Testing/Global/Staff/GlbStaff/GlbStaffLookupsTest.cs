using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGenders()
		{
			var lookups = Factory.New<GlbStaff>().Lookups;
			Assert("Male", lookups.Gender.ContainsCode(Core.Constants.Genders.Man));
			Assert("Female", lookups.Gender.ContainsCode(Core.Constants.Genders.Woman));
			Assert("Non-Binary", lookups.Gender.ContainsCode(Core.Constants.Genders.NonBinary));
			Assert("NotSpecified", lookups.Gender.ContainsCode(Core.Constants.Genders.NotSpecified));
			Assert("Custom", lookups.Gender.ContainsCode(Core.Constants.Genders.Custom));
			Assert("Agender", lookups.Gender.ContainsCode(Core.Constants.Genders.Agender));
		}

		public void TestLookups()
		{
			AssertNotNull(Bizo.Lookups.StaffEmploymentTypes);
			AssertEquals(true, Bizo.Lookups.CommissionBasisTypes.ContainsCode(CommissionBasisType.Codes.PRF));
			AssertEquals(true, Bizo.Lookups.CommissionBasisTypes.ContainsCode(CommissionBasisType.Codes.REV));
			AssertNotNull(Bizo.Lookups.StaffEmploymentTypes);
			AssertNotNull(Bizo.Lookups.CompleteGroupList);
			AssertNotNull(Bizo.Lookups.CompleteStaffList);
			AssertNotNull(Bizo.Lookups.CompleteSalesTeamList);
			AssertEquals("NationalityTypes", typeof(RefCountryCollection), Bizo.Lookups.NationalityTypes.GetType());
			Assert("Gender", Bizo.Lookups.Gender.ContainsCode(Core.Constants.Genders.Woman));
			AssertNotNull(Bizo.Lookups.Departments);
			AssertNotNull(Bizo.Lookups.Branches);
		}

		public void TestDomainNames()
		{
			var domain1 = ObjectFactory.Get<IDomainCredentials>();
			domain1.DomainName = "domain1";
			var domain2 = ObjectFactory.Get<IDomainCredentials>();
			domain2.DomainName = "domain2";
			var domain3 = ObjectFactory.Get<IDomainCredentials>();
			domain3.DomainName = "domain3";

			var aDRegistry = ObjectFactory.Get<IADRegistry>();
			aDRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1, domain2, domain3 };

			AssertNotNull(Bizo.Lookups.DomainNames);
			AssertEquals(3, Bizo.Lookups.DomainNames.Count);
			Assert(Bizo.Lookups.DomainNames.ContainsCode("domain1"));
			Assert(Bizo.Lookups.DomainNames.ContainsCode("domain2"));
			Assert(Bizo.Lookups.DomainNames.ContainsCode("domain3"));
		}

		GlbStaff Bizo
		{
			get { return Factory.New<GlbStaff>(); }
		}
	}
}
