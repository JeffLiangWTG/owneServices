using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgStaffAssignmentsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUniqueAssignment()
		{
			var company2 = Factory.New<GlbCompany>();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var staffAssign1 = org.StaffAssignments.AddNew();
			staffAssign1.O8_Department = "FIA";
			staffAssign1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			var staffAssign2 = org.StaffAssignments.AddNew();
			staffAssign2.O8_Department = "FEA";
			staffAssign2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			org.StaffAssignments.RunPreSaveValidation();
			AssertNoErrors("No errors on either staff assignment", staffAssign1.O8_RoleInfo);
			AssertNoErrors("No errors on either staff assignment", staffAssign2.O8_RoleInfo);

			staffAssign2.O8_Department = "FIA";
			org.StaffAssignments.RunPreSaveValidation();
			AssertHasErrors("Error on staff assignment as it's duplicated", staffAssign1.O8_RoleInfo);
			AssertHasErrors("Error on staff assignment as it's duplicated", staffAssign2.O8_RoleInfo);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var nonCompanySpecificCollection = new OrgStaffAssignmentsCollection(org2);
			nonCompanySpecificCollection.CompanySpecific = false;
			var nonCompanySpecificStaffAssign1 = nonCompanySpecificCollection.AddNew();
			nonCompanySpecificStaffAssign1.O8_Department = "FIA";
			nonCompanySpecificStaffAssign1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			nonCompanySpecificStaffAssign1.O8_GC = GlbCompany.CurrentCompany.PK;

			var nonCompanySpecificStaffAssign2 = nonCompanySpecificCollection.AddNew();
			nonCompanySpecificStaffAssign2.O8_Department = "FIA";
			nonCompanySpecificStaffAssign2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			nonCompanySpecificStaffAssign2.O8_GC = company2.PK;

			nonCompanySpecificStaffAssign1.Header.StaffAssignments.Load();
			nonCompanySpecificStaffAssign2.Header.StaffAssignments.Load();
			nonCompanySpecificCollection.RunPreSaveValidation();
			AssertNoErrors("No errors on either staff assignment", nonCompanySpecificStaffAssign1.O8_RoleInfo);
			AssertNoErrors("No errors on either staff assignment", nonCompanySpecificStaffAssign2.O8_RoleInfo);

			nonCompanySpecificStaffAssign2.O8_GC = GlbCompany.CurrentCompany.PK;
			nonCompanySpecificStaffAssign2.Header.StaffAssignments.Load();
			nonCompanySpecificCollection.RunPreSaveValidation();
			AssertHasErrors("Error on staff assignment as it's duplicated", nonCompanySpecificStaffAssign2.O8_RoleInfo);
		}

		public void TestUniqueAssignment_ProductSpecific()
		{
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgStaffAssignmentsCollection = new OrgStaffAssignmentsCollection(org2);
			orgStaffAssignmentsCollection.CompanySpecific = false;
			var orgStaffAssignments1 = orgStaffAssignmentsCollection.AddNew();
			orgStaffAssignments1.O8_Department = "FIA";
			orgStaffAssignments1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignments1.O8_Product = "";

			var orgStaffAssignments2 = orgStaffAssignmentsCollection.AddNew();
			orgStaffAssignments2.O8_Department = "FIA";
			orgStaffAssignments2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignments2.O8_Product = "AAA";

			orgStaffAssignments1.Header.StaffAssignments.Load();
			orgStaffAssignments2.Header.StaffAssignments.Load();
			orgStaffAssignmentsCollection.RunPreSaveValidation();
			AssertNoErrors(orgStaffAssignments1.O8_RoleInfo);
			AssertNoErrors(orgStaffAssignments2.O8_RoleInfo);

			orgStaffAssignments1.O8_Product = "AAA";
			orgStaffAssignments1.Header.StaffAssignments.Load();
			orgStaffAssignmentsCollection.RunPreSaveValidation();
			AssertHasError("Duplicated product", orgStaffAssignments1.O8_RoleInfo, "This staff assignment already has a Staff member assigned to it.");
			AssertHasError("Duplicated product", orgStaffAssignments2.O8_RoleInfo, "This staff assignment already has a Staff member assigned to it.");

			orgStaffAssignments1.O8_Product = "BBB";
			orgStaffAssignments1.Header.StaffAssignments.Load();
			orgStaffAssignmentsCollection.RunPreSaveValidation();
			AssertNoErrors("Unique product", orgStaffAssignments1.O8_RoleInfo);
			AssertNoErrors("Unique product", orgStaffAssignments2.O8_RoleInfo);
		}

		public void TestDepartment()
		{
			OrgStaffAssignments staffAssignment = Factory.New<OrgStaffAssignments>();

			staffAssignment.O8_Department = "";
			AssertHasErrors(staffAssignment.O8_DepartmentInfo);

			staffAssignment.O8_Department = "ZUB";
			AssertHasErrors(staffAssignment.O8_DepartmentInfo);

			staffAssignment.O8_Department = "ALL";
			AssertNoErrors(staffAssignment.O8_DepartmentInfo);
		}

		public void TestRole()
		{
			var org = Factory.New<OrgHeader>();
			var staffAssignment = org.StaffAssignments.AddNew();

			staffAssignment.RunPreSaveValidation();
			AssertHasErrors(staffAssignment.O8_RoleInfo);

			staffAssignment.O8_Role = "ZUB";
			AssertHasErrors(staffAssignment.O8_RoleInfo);

			staffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			AssertNoErrors(staffAssignment.O8_RoleInfo);
		}

		public void TestRoleSecurity()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "BLAH";
			var staffAssignment = org.StaffAssignments.AddNew();
			staffAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed = false;
			staffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			AssertHasError(staffAssignment.O8_RoleInfo, "You do not have the required security for the selected role.");

			Factory.Save();
			staffAssignment.Validation.ValidateO8_Role();
			AssertNoError(staffAssignment.O8_RoleInfo, "You do not have the required security for the selected role.");

			staffAssignment.O8_Role = StaffAssignmentRoles.Codes.Controller;
			Factory.Save();
			AssertNoError(staffAssignment.O8_RoleInfo, "You do not have the required security for the selected role.");

			staffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			AssertEquals("O8_RoleInfo.HasChanges", true, staffAssignment.O8_RoleInfo.HasChanges);
			AssertHasError(staffAssignment.O8_RoleInfo, "You do not have the required security for the selected role.");

			Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed = true;
			staffAssignment.Validation.ValidateO8_Role();
			AssertNoError(staffAssignment.O8_RoleInfo, "You do not have the required security for the selected role.");
		}

		public void TestO8_GCValidation()
		{
			bool previousValue = Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed;

			try
			{
				ZString errorString = string.Format("You do not have required security rights to maintain staff assignments for other companies.\r\nThis can be changed by your system administrator at:\r\n{0}", Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.DisplayTextPathToSecurityRight);

				OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
				OrgStaffAssignments testAssignment = testOrg.StaffAssignments.AddNew();
				testAssignment.O8_GS_NKPersonResponsible = "ABC";

				Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = false;
				testAssignment.Validation.ValidateO8_GC();
				AssertNoError(testAssignment.O8_GCInfo, errorString);

				testAssignment.O8_GC = ZGuid.Empty;
				testAssignment.Validation.ValidateO8_GC();
				AssertHasError(testAssignment.O8_GCInfo, errorString);

				Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = true;
				testAssignment.Validation.ValidateO8_GC();
				AssertNoError(testAssignment.O8_GCInfo, errorString);

				Factory.Save();
				Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = false;
				testAssignment.Validation.ValidateO8_GC();
				AssertNoError(testAssignment.O8_GCInfo, errorString);
			}
			finally
			{
				Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = previousValue;
			}
		}

		public void TestResponsiblePerson()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgStaffAssignments testAssignment = testOrg.StaffAssignments.AddNew();

			testAssignment.RunPreSaveValidation();
			AssertHasErrors(testAssignment.O8_GS_NKPersonResponsibleInfo);

			testAssignment.O8_GS_NKPersonResponsible = "XYZ";
			AssertHasErrors(testAssignment.O8_GS_NKPersonResponsibleInfo);

			testAssignment.O8_GS_NKPersonResponsible = "E"; // EDISupport
			AssertNoErrors(testAssignment.O8_GS_NKPersonResponsibleInfo);

			testAssignment.O8_GS_NKPersonResponsible = "";
			testAssignment.ValidateResponsiblePerson();
			AssertHasError(testAssignment.O8_GS_NKPersonResponsibleInfo, (NoResString)"Sales Representative must be entered because System -> Registry -> Organizations -> Make Sales Rep Mandatory is ON and current organization is marked as Receivables or Sales. Please enter a Sales representative or turn off this setting in the Registry.");
		}
	}
}
