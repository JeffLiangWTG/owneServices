using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.ProcessManagement.Module.Test
{
	public class WorkItemCRMSecurityProviderTest : CRMSecurityProviderTest<WorkItem>
	{
		protected override CRMSecurityProvider<WorkItem> GetNewProviderForTest() => new WorkItemCRMSecurityProvider();

		protected override IEnumerable<WorkItem> GetTestObjectWithoutStaffAssignment()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_SystemCreateUserInfo.SetValueFromString("AAA");
			return new WorkItem[] { workItem };
		}

		protected override IEnumerable<WorkItem> GetTestObjectWithOrgStaffAssignment()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			yield return workItem;
		}

		protected override IEnumerable<WorkItem> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org) => System.Array.Empty<WorkItem>();

		protected override void AddStaffAssignmentForCompany(WorkItem obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
		}

		protected override IEnumerable<WorkItem> GetTestObjectWithBizObjStaffAssignment()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			return new WorkItem[] { workItem };
		}
	}
}
