using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ProcessManagement.Module.Test
{
	public class ProjectCRMSecurityProviderTest : CRMSecurityProviderTest<Project>
	{
		protected override CRMSecurityProvider<Project> GetNewProviderForTest() => new ProjectCRMSecurityProvider();

		protected override IEnumerable<Project> GetTestObjectWithoutStaffAssignment()
		{
			var project = Factory.NewWithValidTestData<Project>();
			return new Project[] { project };
		}

		protected override IEnumerable<Project> GetTestObjectWithOrgStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org.StaffAssignments.AddNew();
			staffAssignments.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			var address = org.Addresses.AddNewMainAddress();

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "test";

			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_OA_ClientAddress = address.PK;
			project.WKP_OC_Contact = contact.PK;

			return new Project[] { project };
		}

		protected override IEnumerable<Project> GetTestObjectWithBizObjStaffAssignment()
		{
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_GS_NKProjectManager = GlbStaff.CurrentUser.GS_Code;
			return new Project[] { project };
		}

		protected override IEnumerable<Project> GetTestObjectsWithoutOrg()
		{
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_OA_ClientAddress = ZGuid.Empty;
			return new Project[] { project };
		}

		protected override void AddStaffAssignmentForCompany(Project obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.ClientAddress.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}

		protected override IEnumerable<Project> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			OrgContact contact;
			if (org.Contacts.Count > 0)
			{
				contact = org.Contacts[0];
			}
			else
			{
				contact = org.Contacts.AddNew();
				contact.OC_ContactName = "test";
			}
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_OA_ClientAddress = org.Addresses[0].PK;
			project.WKP_OC_Contact = contact.PK;

			return new Project[] { project };
		}
	}
}
