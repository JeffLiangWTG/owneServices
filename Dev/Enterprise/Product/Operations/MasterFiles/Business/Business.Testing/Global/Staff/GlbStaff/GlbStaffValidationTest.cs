using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDeviceOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsDevice = true;
			staff.IsBackupOperator = true;
			staff.IsReadOnlyDBUser = true;
			staff.IsDatabaseDeveloper = true;
			staff.GS_IsController = true;

			staff.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasError(staff.IsBackupOperatorInfo, "Is Backup Operator cannot be enabled on an Is Device Only Staff record.");
				AssertHasError(staff.IsReadOnlyDBUserInfo, "Is Database Reader cannot be enabled on an Is Device Only Staff record.");
				AssertHasError(staff.IsDatabaseDeveloperInfo, "Is Database Developer cannot be enabled on an Is Device Only Staff record.");
				AssertHasError(staff.GS_IsControllerInfo, "Is Controller cannot be enabled on an Is Device Only Staff record.");
			});
		}

		public void TestValidateSalesTeams()
		{
			#region Test Data

			var salesRep1 = Factory.NewWithValidTestData<GlbStaff>();
			var salesRep2 = Factory.NewWithValidTestData<GlbStaff>();
			var salesRep3 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep1.IsTopLevel = true;
			salesRep1.GS_IsSalesRep = true;
			salesRep2.IsTopLevel = true;
			salesRep2.GS_IsSalesRep = true;
			salesRep3.IsTopLevel = true;
			salesRep3.GS_IsSalesRep = true;

			var auGroup = Factory.NewWithValidTestData<SalesTeam>();
			auGroup.GG_Code = "AU";
			auGroup.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));

			var nsw = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, "NSW"));
			auGroup.CoveredUnlocos.Add(Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RW, nsw.PK)));

			var nswGroup = Factory.NewWithValidTestData<SalesTeam>();
			nswGroup.GG_Code = "NSW";
			foreach (var unloco in Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RW, nsw.PK)).Take(10))
			{
				nswGroup.CoveredUnlocos.Add(unloco);
			}
			Assert("nswGroup.CoveredUnlocos.Count > 0", nswGroup.CoveredUnlocos.Count > 0);

			var sydneyUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			nswGroup.CoveredUnlocos.Add(sydneyUnloco);

			var sydneyGroup = Factory.NewWithValidTestData<SalesTeam>();
			sydneyGroup.GG_Code = "SYD";
			sydneyGroup.CoveredUnlocos.Add(sydneyUnloco);

			var qld = Factory.LoadFromNaturalKey<RefCountryStates>(RefCountryStatesSchema.RW_Code, "QLD");
			var qldGroup = Factory.NewWithValidTestData<SalesTeam>();
			qldGroup.GG_Code = "QLD";
			foreach (var unloco in Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RW, qld.PK)).Take(10))
			{
				qldGroup.CoveredUnlocos.Add(unloco);
			}
			Assert("qldGroup.CoveredUnlocos.Count > 0", qldGroup.CoveredUnlocos.Count > 0);

			var usGroup = Factory.NewWithValidTestData<SalesTeam>();
			usGroup.GG_Code = "US";
			usGroup.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US"));

			var auAndUsGroup = Factory.NewWithValidTestData<SalesTeam>();
			auAndUsGroup.GG_Code = "AU+US";
			auAndUsGroup.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));
			auAndUsGroup.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US"));

			var xxCompany = Factory.NewWithValidTestData<GlbCompany>();
			xxCompany.GC_Code = "XXX";
			var auGroupForXXCompany = Factory.NewWithValidTestData<SalesTeam>();
			auGroupForXXCompany.GG_Code = "AUXX";
			auGroupForXXCompany.GG_GC = xxCompany.PK;
			auGroupForXXCompany.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));

			Factory.Save();

			#endregion

			AddSalesTeamToStaff(salesRep1, auGroup);
			salesRep1.Validation.ValidateAll();
			AssertNoRowErrors(auGroup);
			AssertNoRowErrors(salesRep1);

			AddSalesTeamToStaff(salesRep1, usGroup);
			salesRep1.Validation.ValidateAll();
			AssertNoRowErrors("Staff should be allowed to have multiple sales teams where covered countries are distinct", usGroup);
			AssertNoRowErrors(salesRep1);

			AddSalesTeamToStaff(salesRep1, auGroupForXXCompany);
			salesRep1.Validation.ValidateAll();
			AssertNoRowErrors("Staff should be allowed to have multiple sales teams where covered countries are the same when they are for different companies", auGroupForXXCompany);
			AssertNoRowErrors(salesRep1);

			AddSalesTeamToStaff(salesRep1, auAndUsGroup);
			salesRep1.Validation.ValidateAll();
			AssertHasRowError(auGroup, "Has overlapping coverage area with Sales Team (AU+US).");
			AssertHasRowError(auAndUsGroup, "Has overlapping coverage area with Sales Team (AU).");
			AssertNoRowErrors(auGroupForXXCompany);
			AssertHasRowError(salesRep1, "Sales teams have overlapping coverage area.");

			AddSalesTeamToStaff(salesRep2, auGroup);
			salesRep2.Validation.ValidateAll();
			AssertNoRowErrors(auGroup);

			AddSalesTeamToStaff(salesRep2, nswGroup);
			salesRep2.Validation.ValidateAll();
			AssertHasRowError(auGroup, "Has overlapping coverage area with Sales Team (NSW).");
			AssertHasRowError(nswGroup, "Has overlapping coverage area with Sales Team (AU).");
			AssertHasRowError(salesRep2, "Sales teams have overlapping coverage area.");

			AddSalesTeamToStaff(salesRep3, nswGroup);
			AddSalesTeamToStaff(salesRep3, qldGroup);
			salesRep3.Validation.ValidateAll();
			AssertNoRowErrors("Staff should be allowed to have multiple sales teams where covered ports are distinct", nswGroup);
			AssertNoRowErrors("Staff should be allowed to have multiple sales teams where covered ports are distinct", qldGroup);
			AssertNoRowErrors(salesRep3);

			AddSalesTeamToStaff(salesRep3, sydneyGroup);
			salesRep3.Validation.ValidateAll();
			AssertHasRowError(nswGroup, "Has overlapping coverage area with Sales Team (SYD).");
			AssertNoRowErrors(qldGroup);
			AssertHasRowError(sydneyGroup, "Has overlapping coverage area with Sales Team (NSW).");
			AssertHasRowError(salesRep3, "Sales teams have overlapping coverage area.");
		}

		static void AddSalesTeamToStaff(GlbStaff staff, SalesTeam salesTeam)
		{
			staff.SalesTeams.Add(salesTeam);
			salesTeam.Staff.Add(staff);
		}

		public void TestValidateManagers()
		{
			var roles = new StaffReportingRoleCollection()
			{
				{ "HRM", (NoResString)"Direct Manager", true, true, true }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			AssertEquals("Precondition - Should be added by FillWithValidTestData", 1, staff.Managers.Count);
			staff.Managers.FirstOrDefault().Delete();
			AssertEquals("Should have deleted manager", 0, staff.Managers.Count);
			staff.Validation.ValidateAll();
			AssertNoRowErrors(staff);
			Factory.Save();

			staff.Validation.ValidateAll();
			AssertHasRowError(staff, "The Direct Manager role is mandatory. It must be added for this staff member.");

			StaffManagerTestHelper.AddManager(staff, staff, "HRM");
			staff.Validation.ValidateAll();
			AssertNoRowErrors(staff);

			staff.Managers.FirstOrDefault().Delete();
			AssertEquals("Should have deleted manager", 0, staff.Managers.Count);
			staff.Validation.ValidateAll();
			AssertHasRowError(staff, "The Direct Manager role is mandatory. It must be added for this staff member.");

			roles = new StaffReportingRoleCollection()
			{
				{ "HRM", (NoResString)"Direct Manager", true, false, true }
			};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			staff.Validation.ValidateAll();
			AssertNoRowErrors(staff);
		}

		public void TestValidateFullName()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "hank";
			staff.Validation.ValidateAll();
			AssertNoRowErrors(staff);
			Factory.Save();

			Db.Connection.ExecuteNonQuery(@$"
update GlbStaff SET
GS_FullName='

edf', GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E'
where GS_PK='{staff.PK}'
");

			staff.Reload();
			staff.Validation.ValidateAll();
			AssertHasError(staff.GS_FullNameInfo, "Preferred Full Name starts with white space.");
		}

		public void TestValidateManagersWithoutSecurityRight()
		{
			var user = GetStaffWithoutPermissions();

			var roles = new StaffReportingRoleCollection()
			{
				{ "HRM", (NoResString)"Direct Manager", true, true, true }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(user.GS_LoginName))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				AssertEquals("Precondition - Should be added by FillWithValidTestData", 1, staff.Managers.Count);
				staff.Managers.FirstOrDefault().Delete();
				AssertEquals("Should have deleted manager", 0, staff.Managers.Count);
				staff.Validation.ValidateManagers();
				AssertNoRowErrors("This user does not have security clearance for adding a manager so they should not get the validation error.", staff);
			}
		}

		GlbStaff GetStaffWithoutPermissions()
		{
			GlbStaff result = Factory.NewWithValidTestData<GlbStaff>();
			result.GS_IsController = false;

			GlbSecurity securityRecord = result.StaffSecurityPermissionsCollection.AddNew();
			securityRecord.GU_SecurityRight = "Maintain";
			securityRecord.GU_SecurityItemIsAllowed = false;

			return result;
		}
	}
}
