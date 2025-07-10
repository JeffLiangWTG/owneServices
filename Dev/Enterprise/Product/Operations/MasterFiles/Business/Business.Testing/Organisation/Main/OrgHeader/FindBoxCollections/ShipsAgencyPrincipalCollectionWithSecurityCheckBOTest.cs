using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ShipsAgencyPrincipalCollectionWithSecurityCheck))]
	sealed class ShipsAgencyPrincipalCollectionWithSecurityCheckBOTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowedAccessTo()
		{
			Env.Security.AgencyPrincipalAccess.IsAllowed = false;
			SetupOrgs();
			AssertEquals(true, ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(staffPrincipal));
			AssertEquals(true, ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(groupPrincipal));
			AssertEquals(false, ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(badPrincipal));
		}

		public void TestFilter()
		{
			Env.Security.AgencyPrincipalAccess.IsAllowed = false;
			SetupOrgs();

			ShipsAgencyPrincipalCollectionWithSecurityCheck collection = new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory);
			collection.Load();

			AssertCollectionContains(staffPrincipal, collection);
			AssertCollectionContains(groupPrincipal, collection);
			AssertCollectionNotContains(badPrincipal, collection);
		}

		public void TestAllowedAccessTo_AllowAll()
		{
			Env.Security.AgencyPrincipalAccess.IsAllowed = true;
			SetupOrgs();
			AssertEquals(true, ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(staffPrincipal));
			AssertEquals(true, ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(groupPrincipal));
			AssertEquals(true, ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(badPrincipal));
		}

		public void TestFilter_AllowAll()
		{
			Env.Security.AgencyPrincipalAccess.IsAllowed = true;
			SetupOrgs();

			ShipsAgencyPrincipalCollectionWithSecurityCheck collection = new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory);
			collection.Load();

			AssertCollectionContains(staffPrincipal, collection);
			AssertCollectionContains(groupPrincipal, collection);
			AssertCollectionContains(badPrincipal, collection);
		}

		OrgHeader staffPrincipal;
		OrgHeader groupPrincipal;
		OrgHeader badPrincipal;

		#region Implementation

		void SetupOrgs()
		{
			staffPrincipal = Factory.NewWithValidTestData<OrgHeader>();
			staffPrincipal.OH_Code = "STAFF";
			staffPrincipal.OH_IsShippingProvider = true;
			staffPrincipal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			groupPrincipal = Factory.NewWithValidTestData<OrgHeader>();
			groupPrincipal.OH_Code = "GROUP";
			groupPrincipal.OH_IsShippingProvider = true;
			groupPrincipal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			badPrincipal = Factory.NewWithValidTestData<OrgHeader>();
			badPrincipal.OH_Code = "BAD";
			badPrincipal.OH_IsShippingProvider = true;
			badPrincipal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			GlbStaff staff = GlbStaff.CurrentUser;
			staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			((IOrgsAndWarehousesAccessProvider)staff).AddSecurityToAccessOrgOrWarehouse("STAFF");
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(staff);
			group.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			((IOrgsAndWarehousesAccessProvider)group).AddSecurityToAccessOrgOrWarehouse("GROUP");
			Factory.Save();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory);
		}

		#endregion
	}
}
