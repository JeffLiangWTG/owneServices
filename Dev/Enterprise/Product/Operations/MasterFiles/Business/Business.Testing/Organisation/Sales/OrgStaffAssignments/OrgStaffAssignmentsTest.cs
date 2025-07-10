using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgStaffAssignments))]
	public class OrgStaffAssignmentsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			bool oldStaffAssignmentsSalesRepValue = Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed;
			Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed = false;
			try
			{
				var staffAssignment = OrgInDB.StaffAssignments.AddNew();
				staffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
				staffAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
				Assert(staffAssignment.CanDelete);
				staffAssignment.Factory.Save();
				Assert(!staffAssignment.CanDelete);
			}
			finally
			{
				Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed = oldStaffAssignmentsSalesRepValue;
			}
		}

		#region IWorkflowTriggerEventSource

		public void TestJobHeaderCompany()
		{
			var staff = Factory.New<OrgStaffAssignments>();

			var jobHeaderCompany = ((IWorkflowTriggerEventSource)staff).JobHeaderCompany;
			AssertEquals(GlbCompany.CurrentCompany.PK, jobHeaderCompany.PK);

			var testCompany = Factory.New<GlbCompany>();
			staff.O8_GC = testCompany.PK;
			jobHeaderCompany = ((IWorkflowTriggerEventSource)staff).JobHeaderCompany;
			AssertEquals(testCompany.PK, jobHeaderCompany.PK);
		}

		public void TestParentWorkflowProviders()
		{
			var staff = Factory.New<OrgStaffAssignments>();
			var prov = ((IWorkflowTriggerEventSource)staff).ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(0, prov.Count);

			var header = Factory.New<OrgHeader>();
			staff.O8_OH = header.PK;
			prov = ((IWorkflowTriggerEventSource)staff).ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(1, prov.Count);
			AssertEquals(header, prov[0]);
		}

		#endregion

		#region IAddressBookRecipient

		public void TestIsActive()
		{
			var staffAssignment = OrgInDB.StaffAssignments.AddNew();
			AssertNull("Precondition", staffAssignment.PersonResponsible);
			AssertEquals(false, ((IAddressBookRecipient)staffAssignment).IsActive);

			staffAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			AssertNotNull("Precondition", staffAssignment.PersonResponsible);
			AssertEquals("Precondition", true, ((IAddressBookRecipient)staffAssignment.PersonResponsible).IsActive);
			AssertEquals(true, ((IAddressBookRecipient)staffAssignment).IsActive);
		}

		public void TestPK()
		{
			var staffAssignment = OrgInDB.StaffAssignments.AddNew();
			AssertNull("Precondition", staffAssignment.PersonResponsible);
			AssertEquals(ZGuid.Empty, ((IAddressBookRecipient)staffAssignment).PK);

			staffAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			AssertNotNull("Precondition", staffAssignment.PersonResponsible);
			AssertEquals(staffAssignment.PersonResponsible.PK, ((IAddressBookRecipient)staffAssignment).PK);
		}

		#endregion

		public void TestReadOnlySecurity()
		{
			bool oldStaffAssignmentsValue = Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed;

			Action<bool> setStaffAssignments = (isAllowed) =>
			{
				foreach (var pair in Env.Security.OrgDetailsModifyStaffAssignmentsLookup)
				{
					pair.Value.IsAllowed = isAllowed;
				}
			};

			try
			{
				OrgStaffAssignments nonSalesStaff = OrgInDB.StaffAssignments.AddNew();
				nonSalesStaff.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
				nonSalesStaff.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
				nonSalesStaff.O8_Department = OrgStaffAssignmentsLookups.AllServices;

				OrgStaffAssignments salesStaff = OrgInDB.StaffAssignments.AddNew();
				salesStaff.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
				salesStaff.O8_Department = OrgStaffAssignmentsLookups.AllServices;
				salesStaff.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

				Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = true;
				setStaffAssignments(true);
				Assert("Access Allowed - Not ReadOnly", !nonSalesStaff.O8_DepartmentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !nonSalesStaff.O8_GS_NKPersonResponsibleInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !nonSalesStaff.O8_RoleInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !salesStaff.O8_DepartmentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !salesStaff.O8_GS_NKPersonResponsibleInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !salesStaff.O8_RoleInfo.ReadOnly);

				setStaffAssignments(false);
				Assert("Access Allowed - Not ReadOnly", !nonSalesStaff.O8_DepartmentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !nonSalesStaff.O8_GS_NKPersonResponsibleInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !nonSalesStaff.O8_RoleInfo.ReadOnly);

				// Save required as readonly state is based on whether the user created the record themselves.
				salesStaff.Factory.Save();
				Assert("Access NOT Allowed - ReadOnly", salesStaff.O8_DepartmentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", salesStaff.O8_GS_NKPersonResponsibleInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", salesStaff.O8_RoleInfo.ReadOnly);

				Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", nonSalesStaff.O8_DepartmentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", nonSalesStaff.O8_GS_NKPersonResponsibleInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", nonSalesStaff.O8_RoleInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", salesStaff.O8_DepartmentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", salesStaff.O8_GS_NKPersonResponsibleInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", salesStaff.O8_RoleInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = oldStaffAssignmentsValue;
				setStaffAssignments(true);
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		public void TestCompanyReadonly()
		{
			OrgHeader org = OrgHeader.New(Factory);

			OrgStaffAssignmentsCollection staffAssignments = new OrgStaffAssignmentsCollection(org);
			OrgStaffAssignments assignment = staffAssignments.AddNew();
			Assert(assignment.O8_GCInfo.ReadOnly);

			org.StaffAssignments.CompanySpecific = false;
			OrgStaffAssignments otherAssignment = org.StaffAssignments.AddNew();
			Assert(!otherAssignment.O8_GCInfo.ReadOnly);
		}

		public void TestCreateAutoAdminLog()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var barryStaff = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments staffAssignment1 = org.StaffAssignments.AddNew();
			staffAssignment1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			staffAssignment1.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			staffAssignment1.O8_GS_NKPersonResponsible = barryStaff.GS_Code;
			Factory.Save();

			AssertEquals("Event should be added to StaffAssignments", 1, staffAssignment1.Logs.GetAllLogs().Count);
			ZString expectedDescription = "Staff Assignment - Initials " + barryStaff.GS_Code + " Role " + StaffAssignmentRoles.Codes.AccountManager
				+ " Department " + OrgStaffAssignmentsLookups.AllServices;
			AssertEquals("Event with correct description should be added to StaffAssignments", expectedDescription, staffAssignment1.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem).SL_Reference);

			var mariaStaff = Factory.NewWithValidTestData<GlbStaff>();

			staffAssignment1.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			staffAssignment1.O8_Department = OrgStaffAssignmentsLookups.AirFreightServices;
			staffAssignment1.O8_GS_NKPersonResponsible = mariaStaff.GS_Code;

			Factory.Save();

			AssertEquals("Event should be added to StaffAssignments", 2, staffAssignment1.Logs.GetAllLogs().Count);
			expectedDescription = "Staff Assignment - Initials " + mariaStaff.GS_Code + " (" + barryStaff.GS_Code + ")"
				+ " Role " + StaffAssignmentRoles.Codes.CustomerServiceRep + " (" + StaffAssignmentRoles.Codes.AccountManager + ")"
				+ " Department " + OrgStaffAssignmentsLookups.AirFreightServices + " (" + OrgStaffAssignmentsLookups.AllServices + ")";
			AssertEquals("Event with correct description should be added to StaffAssignments", expectedDescription, staffAssignment1.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

		public void TestDefaultValues()
		{
			OrgHeader org = OrgHeader.New(Factory);
			OrgStaffAssignments staffAssignment = org.StaffAssignments.AddNew();

			AssertEquals("Department set to all by default", "ALL", staffAssignment.O8_Department);
		}

		public void TestResponsiblePersonNameAndLoginName()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "PIR ISREK";
			staff.GS_LoginName = "((88(())";
			OrgHeader org = OrgHeader.New(Factory);
			OrgStaffAssignments staffAssignment = org.StaffAssignments.AddNew();

			AssertEquals("Precondition: Staff not set", ZString.Empty, staffAssignment.O8_GS_NKPersonResponsible);
			staffAssignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			AssertEquals("Staff set", staff.GS_Code, staffAssignment.O8_GS_NKPersonResponsible);
			AssertEquals("Staff set", staff, staffAssignment.PersonResponsible);
			AssertEquals("Staff name returned", staff.GS_FullName, staffAssignment.ResponsiblePersonName);
			AssertEquals("Staff login name returned", staff.GS_LoginName, staffAssignment.ResponsiblePersonLoginName);
		}

		public void TestRoleDescription()
		{
			OrgHeader org = OrgHeader.New(Factory);
			OrgStaffAssignments staffAssignment = org.StaffAssignments.AddNew();

			staffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			AssertEquals("Role description returned", "Sales Representative", staffAssignment.RoleDescription);
		}
	}
}
