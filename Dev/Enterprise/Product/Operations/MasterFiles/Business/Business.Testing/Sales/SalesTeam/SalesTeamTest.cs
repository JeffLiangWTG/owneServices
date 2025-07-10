using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesTeam))]
	sealed class SalesTeamTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SalesTeam = (SalesTeam)GetNewBusinessObject();
		}

		SalesTeam SalesTeam;

		#endregion

		#region Set Default Values

		public void TestDefaultValues()
		{
			var team = Factory.New<SalesTeam>();
			AssertEquals(false, team.IsGlobal);
			AssertEquals(GlbCompany.CurrentCompany.PK, team.GG_GC);
		}

		#endregion

		#region Related Business Objects

		public void TestSalesRep()
		{
			GlbStaff salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_IsSalesRep = true;
			SalesTeam.Staff.Add(salesRep);
			AssertEquals("Sales team contains sales rep", true, SalesTeam.Staff.Contains(salesRep));
		}

		public void TestCovers()
		{
			var auCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var auTeam = Factory.New<SalesTeam>();
			auTeam.CoveredCountries.Add(auCountry);

			var usCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var usTeam = Factory.New<SalesTeam>();
			usTeam.CoveredCountries.Add(usCountry);

			var sydUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var sydTeam = Factory.New<SalesTeam>();
			sydTeam.CoveredUnlocos.Add(sydUnloco);

			AssertEquals(true, auTeam.Covers(auCountry));
			AssertEquals(false, auTeam.Covers(usCountry));
			AssertEquals(true, auTeam.Covers(sydUnloco));
			AssertEquals(false, auTeam.Covers((RefCountry)null));
			AssertEquals(false, auTeam.Covers((RefUNLOCO)null));

			AssertEquals(false, usTeam.Covers(auCountry));
			AssertEquals(true, usTeam.Covers(usCountry));
			AssertEquals(false, usTeam.Covers(sydUnloco));
			AssertEquals(false, usTeam.Covers((RefCountry)null));
			AssertEquals(false, usTeam.Covers((RefUNLOCO)null));

			AssertEquals(false, sydTeam.Covers(auCountry));
			AssertEquals(false, sydTeam.Covers(usCountry));
			AssertEquals(true, sydTeam.Covers(sydUnloco));
			AssertEquals(false, sydTeam.Covers((RefCountry)null));
			AssertEquals(false, sydTeam.Covers((RefUNLOCO)null));
		}

		public void TestShouldDetachSalesReps()
		{
			SalesTeam.GG_IsActive = true;
			SalesTeam.Staff.Add(SalesRep);
			SalesTeam.Staff.Add(SalesRep);
			Factory.Save();
			Assert("Sales Team can keep all Sales Reps", !SalesTeam.ShouldDetachSalesReps);

			SalesTeam.GG_IsActive = false;
			Assert("Sales Team should detach all Sales Reps", SalesTeam.ShouldDetachSalesReps);
		}

		GlbStaff SalesRep
		{
			get
			{
				GlbStaff salesRep = Factory.NewWithValidTestData<GlbStaff>();
				salesRep.GS_IsSalesRep = true;
				return salesRep;
			}
		}

		#endregion

		#region Security

		public void TestReadOnlySecurity()
		{
			var salesTeam = Factory.NewWithValidTestData<SalesTeam>();

			Env.Security.GroupsModify.IsAllowed = false;
			Env.Security.SalesTeamsNew.IsAllowed = false;
			Env.Security.SalesTeamsEdit.IsAllowed = false;
			AssertEquals(true, salesTeam.GG_CodeInfo.ReadOnly);

			Env.Security.GroupsModify.IsAllowed = true;
			Env.Security.SalesTeamsNew.IsAllowed = false;
			Env.Security.SalesTeamsEdit.IsAllowed = false;
			AssertEquals(true, salesTeam.GG_CodeInfo.ReadOnly);

			Env.Security.GroupsModify.IsAllowed = false;
			Env.Security.SalesTeamsNew.IsAllowed = true;
			Env.Security.SalesTeamsEdit.IsAllowed = false;
			AssertEquals(false, salesTeam.GG_CodeInfo.ReadOnly);

			Env.Security.GroupsModify.IsAllowed = false;
			Env.Security.SalesTeamsNew.IsAllowed = false;
			Env.Security.SalesTeamsEdit.IsAllowed = true;
			AssertEquals(true, salesTeam.GG_CodeInfo.ReadOnly);

			Factory.Save();

			Env.Security.GroupsModify.IsAllowed = false;
			Env.Security.SalesTeamsNew.IsAllowed = false;
			Env.Security.SalesTeamsEdit.IsAllowed = false;
			AssertEquals(true, salesTeam.GG_CodeInfo.ReadOnly);

			Env.Security.GroupsModify.IsAllowed = true;
			Env.Security.SalesTeamsNew.IsAllowed = false;
			Env.Security.SalesTeamsEdit.IsAllowed = false;
			AssertEquals(true, salesTeam.GG_CodeInfo.ReadOnly);

			Env.Security.GroupsModify.IsAllowed = false;
			Env.Security.SalesTeamsNew.IsAllowed = true;
			Env.Security.SalesTeamsEdit.IsAllowed = false;
			AssertEquals(true, salesTeam.GG_CodeInfo.ReadOnly);

			Env.Security.GroupsModify.IsAllowed = false;
			Env.Security.SalesTeamsNew.IsAllowed = false;
			Env.Security.SalesTeamsEdit.IsAllowed = true;
			AssertEquals(false, salesTeam.GG_CodeInfo.ReadOnly);
		}

		#endregion

		#region Validation

		public void TestLightValidationDisabled()
		{
			var salesTeam = Factory.New<SalesTeam>();
			AssertEquals(false, salesTeam.LightValidationEnabled);
		}

		#endregion

		public void TestCurrentGroupLink()
		{
			SalesTeam salesTeam = Factory.New<SalesTeam>();
			AssertNull("Group link to the sales team should be null", salesTeam.CurrentGroupLink);
			GlbGroupLink link = Factory.New<GlbGroupLink>();
			salesTeam.CurrentGroupLink = link;
			AssertEquals("Group link to the sales team should be link", link, salesTeam.CurrentGroupLink);
		}

		public void TestParentTeamValidation()
		{
			var grandparentTeam = Factory.NewWithValidTestData<SalesTeam>();
			var parentTeam = Factory.NewWithValidTestData<SalesTeam>();
			var childTeam = Factory.NewWithValidTestData<SalesTeam>();

			childTeam.ParentTeamPk = ZGuid.NewZGuid();
			AssertHasError(childTeam.ParentTeamPkInfo, "Enter a valid Parent Group.");

			childTeam.ParentTeamPk = parentTeam.PK;
			parentTeam.ParentTeamPk = grandparentTeam.PK;

			AssertNoErrors(grandparentTeam);
			AssertNoErrors(parentTeam);
			AssertNoErrors(childTeam);
		}

		#region HumanReadableName

		public void TestHumanReadableNameCore()
		{
			SalesTeam.GG_Code = "ELF";
			AssertEquals("HumanReadableNameCore is human readable", "Sales Team (ELF)", SalesTeam.HumanReadableName);
		}

		#endregion
	}
}
