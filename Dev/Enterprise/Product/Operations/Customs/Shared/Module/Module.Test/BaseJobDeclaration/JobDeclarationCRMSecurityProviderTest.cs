using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module.Testing
{
	class JobDeclarationCRMSecurityProviderTest : CRMSecurityProviderTest<BaseJobDeclaration>
	{
		protected override CRMSecurityProvider<BaseJobDeclaration> GetNewProviderForTest() => new JobDeclarationCRMSecurityProvider();
		protected override IEnumerable<BaseJobDeclaration> GetTestObjectWithoutStaffAssignment()
		{
			var obj = Factory.NewWithValidTestData<BaseJobDeclaration>();
			return new BaseJobDeclaration[] { obj };
		}

		protected override IEnumerable<BaseJobDeclaration> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;

			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			var address = org1.Addresses.AddNewMainAddress();
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = address.PK;
			jobHeader1.JH_GS_NKRepSales = "U00";

			var baseJobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobHeader1.JH_ParentID = baseJobDeclaration.PK;
			jobHeader1.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			
			return new BaseJobDeclaration[] { baseJobDeclaration };
		}

		protected override IEnumerable<BaseJobDeclaration> GetTestObjectWithBizObjStaffAssignment()
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			var baseJobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobHeader1.JH_ParentID = baseJobDeclaration.PK;
			jobHeader1.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			return new BaseJobDeclaration[] { baseJobDeclaration };
		}

		protected override IEnumerable<BaseJobDeclaration> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = "U01";
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;

			var baseJobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobHeader1.JH_ParentID = baseJobDeclaration.PK;
			jobHeader1.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			return new BaseJobDeclaration[] { baseJobDeclaration };
		}

		protected override void AddStaffAssignmentForCompany(BaseJobDeclaration obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var job = obj.Factory.LoadTop1<JobHeader>(new CargoWise.EntityFramework.ZQuery(JobHeaderSchema.JH_ParentID, obj.PK))
				?? obj.Factory.LoadTop1<JobHeader>(new CargoWise.EntityFramework.ZQuery(JobHeaderSchema.JH_ParentID, obj.JE_JS));

			var assignment = job.LocalChargesAddr.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}

		protected override IEnumerable<BaseJobDeclaration> GetTestObjectWithTaskAssignment()
		{
			var result = new List<BaseJobDeclaration>(base.GetTestObjectWithTaskAssignment());
			var baseJobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var workflowItem1 = baseJobDeclaration.WorkflowItems.AddNew();
			workflowItem1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			result.Add(baseJobDeclaration);

			return result;
		}
	}
}
