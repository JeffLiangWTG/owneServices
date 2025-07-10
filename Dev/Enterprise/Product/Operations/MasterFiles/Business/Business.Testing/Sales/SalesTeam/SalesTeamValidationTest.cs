using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal class SalesTeamValidationTest : GlbGroupValidationTest
	{
		public void TestValidateGG_GC_ChecksSecurityCheckpoint()
		{
			Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed = false;
			string expectedSecurityErrorMessage = string.Format(@"You do not have the appropriate security rights to view or edit Sales Teams outside your current login company ({0}).

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 

{1}", GlbCompany.CurrentCompany.GC_Code, Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.DisplayTextPathToSecurityRight);

			var team = Factory.NewWithValidTestData<SalesTeam>();

			team.GG_GC = GlbCompany.CurrentCompany.PK;
			AssertNoErrors(team.GG_GCInfo);

			team.GG_GC = ZGuid.Empty;
			AssertMandatoryValidationError(team.GG_GCInfo, true);

			team.GG_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			AssertHasError(team.GG_GCInfo, expectedSecurityErrorMessage);

			Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed = true;
			team.GG_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			AssertNoErrors(team.GG_GCInfo);
		}

		public void TestValidateGroupMembers()
		{
			#region Test Data

			var salesRep1 = Factory.NewWithValidTestData<GlbStaff>();
			var salesRep2 = Factory.NewWithValidTestData<GlbStaff>();
			var salesRep3 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep1.GS_IsSalesRep = true;
			salesRep2.GS_IsSalesRep = true;
			salesRep3.GS_IsSalesRep = true;

			var auGroup = Factory.NewWithValidTestData<SalesTeam>();
			auGroup.GG_Code = "AU";
			auGroup.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));

			var nsw = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, "NSW"));
			auGroup.CoveredUnlocos.Add(Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RW, nsw.PK)));

			var nswGroup = Factory.NewWithValidTestData<SalesTeam>();
			nswGroup.IsTopLevel = true;
			nswGroup.GG_Code = "NSW";
			foreach (var unloco in Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RW, nsw.PK)).Take(10))
			{
				nswGroup.CoveredUnlocos.Add(unloco);
			}
			Assert("nswGroup.CoveredUnlocos.Count > 0", nswGroup.CoveredUnlocos.Count > 0);

			var sydneyUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			nswGroup.CoveredUnlocos.Add(sydneyUnloco);

			var sydneyGroup = Factory.NewWithValidTestData<SalesTeam>();
			sydneyGroup.IsTopLevel = true;
			sydneyGroup.GG_Code = "SYD";
			sydneyGroup.CoveredUnlocos.Add(sydneyUnloco);

			var qld = Factory.LoadFromNaturalKey<RefCountryStates>(RefCountryStatesSchema.RW_Code, "QLD");
			var qldGroup = Factory.NewWithValidTestData<SalesTeam>();
			qldGroup.IsTopLevel = true;
			qldGroup.GG_Code = "QLD";
			foreach (var unloco in Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RW, qld.PK)).Take(10))
			{
				qldGroup.CoveredUnlocos.Add(unloco);
			}
			Assert("qldGroup.CoveredUnlocos.Count > 0", qldGroup.CoveredUnlocos.Count > 0);

			var usGroup = Factory.NewWithValidTestData<SalesTeam>();
			usGroup.IsTopLevel = true;
			usGroup.GG_Code = "US";
			usGroup.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US"));

			var auAndUsGroup = Factory.NewWithValidTestData<SalesTeam>();
			auAndUsGroup.IsTopLevel = true;
			auAndUsGroup.GG_Code = "AU+US";
			auAndUsGroup.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));
			auAndUsGroup.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US"));

			var xxCompany = Factory.NewWithValidTestData<GlbCompany>();
			xxCompany.GC_Code = "XXX";
			var auGroupForXXCompany = Factory.NewWithValidTestData<SalesTeam>();
			auGroupForXXCompany.IsTopLevel = true;
			auGroupForXXCompany.GG_Code = "AUXX";
			auGroupForXXCompany.GG_GC = xxCompany.PK;
			auGroupForXXCompany.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));

			Factory.Save();

			#endregion

			AddStaffToSalesTeam(salesRep1, auGroup);
			auGroup.Validation.ValidateAll();
			AssertNoRowErrors(salesRep1);
			AssertNoRowErrors(auGroup);

			AddStaffToSalesTeam(salesRep1, usGroup);
			usGroup.Validation.ValidateAll();
			AssertNoRowErrors("Staff should be allowed to be in multiple sales teams where covered countries are distinct", salesRep1);
			AssertNoRowErrors(usGroup);

			AddStaffToSalesTeam(salesRep1, auGroupForXXCompany);
			auGroupForXXCompany.Validation.ValidateAll();
			AssertNoRowErrors("Staff should be allowed to have multiple sales teams where covered countries are the same when they are for different companies", salesRep1);
			AssertNoRowErrors(auGroupForXXCompany);

			AddStaffToSalesTeam(salesRep1, auAndUsGroup);
			auAndUsGroup.Validation.ValidateAll();
			AssertHasRowError(salesRep1, "Already assigned to Sales Team (AU) which has overlapping coverage area with this team.");
			AssertHasRowError(auAndUsGroup, "There are staff that are assigned to other Sales Teams that have overlapping coverage area with this team.");

			AddStaffToSalesTeam(salesRep2, auGroup);
			auGroup.Validation.ValidateAll();
			AssertNoRowErrors(salesRep2);
			AssertNoRowErrors(auGroup);

			AddStaffToSalesTeam(salesRep2, nswGroup);
			nswGroup.Validation.ValidateAll();
			AssertHasRowError(salesRep2, "Already assigned to Sales Team (AU) which has overlapping coverage area with this team.");
			AssertHasRowError(nswGroup, "There are staff that are assigned to other Sales Teams that have overlapping coverage area with this team.");

			AddStaffToSalesTeam(salesRep3, nswGroup);
			AddStaffToSalesTeam(salesRep3, qldGroup);
			qldGroup.Validation.ValidateAll();
			AssertNoRowErrors("Staff should be allowed to be in multiple sales teams where covered ports are distinct", salesRep3);
			AssertNoRowErrors(qldGroup);

			AddStaffToSalesTeam(salesRep3, sydneyGroup);
			sydneyGroup.Validation.ValidateAll();
			AssertHasRowError(salesRep3, "Already assigned to Sales Team (NSW) which has overlapping coverage area with this team.");
			AssertHasRowError(sydneyGroup, "There are staff that are assigned to other Sales Teams that have overlapping coverage area with this team.");
		}

		static void AddStaffToSalesTeam(GlbStaff staff, SalesTeam salesTeam)
		{
			staff.SalesTeams.Add(salesTeam);
			salesTeam.Staff.Add(staff);
		}

		protected override bool IsContactTypeAllowed => false;

		protected override GlbGroup GetNewBusinessObject()
		{
			return Factory.New<SalesTeam>();
		}
	}
}
