using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffChangeRequest))]
	class GlbStaffChangeRequestTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2022, 6, 1)]
		public void TestWorkPattern_OriginalReturnsNullWhenNoneExists()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var changeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();

			var current = Factory.NewWithValidTestData<GlbWorkPattern>();
			current.GWP_EffectiveDate = new ZDateTimeOffset(2021, 1, 1);
			current.GWP_GS_Staff = staff.PK;
			current.GWP_IsApproved = true;

			var requested = Factory.NewWithValidTestData<GlbWorkPattern>();
			requested.GWP_EffectiveDate = new ZDateTimeOffset(2020, 1, 3); // Note effective prior to 'current'
			requested.GWP_GS_Staff = staff.PK;
			requested.GWP_IsApproved = false;
			requested.GWP_GCR_ChangeRequest = changeRequest.PK;

			Factory.Save();

			AssertEquals(null, changeRequest.OriginalWorkPattern);
		}

		[TestDate(2022, 6, 1)]
		public void TestManager_OriginalReturnsNullWhenNoneExists()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var changeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();

			var current = Factory.NewWithValidTestData<GlbStaffManager>();
			current.GSM_EffectiveDate = new ZDateTime(2021, 1, 1);
			current.GSM_GS_Staff = staff.PK;
			current.GSM_IsApproved = true;

			var requested = Factory.NewWithValidTestData<GlbStaffManager>();
			requested.GSM_EffectiveDate = new ZDateTime(2020, 1, 3); // Note effective prior to 'current'
			requested.GSM_GS_Staff = staff.PK;
			requested.GSM_IsApproved = false;
			requested.GSM_GCR_ChangeRequest = changeRequest.PK;

			Factory.Save();

			AssertEquals(null, changeRequest.OriginalManagementLink);
		}

		[TestDate(2022, 6, 1)]
		public void TestTeam_OriginalReturnsNullWhenNoneExists()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var changeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();

			var current = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			current.GET_EffectiveDate = new ZDateTimeOffset(2021, 1, 1);
			current.GET_GS_Staff = staff.PK;
			current.GET_IsApproved = true;

			var requested = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			requested.GET_EffectiveDate = new ZDateTimeOffset(2020, 1, 3); // Note effective prior to 'current'
			requested.GET_GS_Staff = staff.PK;
			requested.GET_IsApproved = false;
			requested.GET_GCR_ChangeRequest = changeRequest.PK;

			Factory.Save();

			AssertEquals(null, changeRequest.OriginalTeam);
		}

		[TestDate(2022, 6, 1)]
		public void TestEmployment_OriginalReturnsNullWhenNoneExists()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var changeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();

			var current = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			current.GEH_EffectiveDate = new ZDateTimeOffset(2021, 1, 1);
			current.GEH_GS_Staff = staff.PK;
			current.GEH_IsApproved = true;

			var requested = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			requested.GEH_EffectiveDate = new ZDateTimeOffset(2020, 1, 3); // Note effective prior to 'current'
			requested.GEH_GS_Staff = staff.PK;
			requested.GEH_IsApproved = false;
			requested.GEH_GCR_ChangeRequest = changeRequest.PK;

			Factory.Save();

			AssertEquals(null, changeRequest.OriginalEmployment);
		}

		[TestDate(2019, 6, 1)]
		public void TestEmployment()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var changeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();

			var current = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			current.GEH_EffectiveDate = new ZDateTimeOffset(2019, 1, 1);
			current.GEH_GS_Staff = staff.PK;
			current.GEH_IsApproved = true;

			var original = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			original.GEH_EffectiveDate = new ZDateTimeOffset(2020, 1, 1);
			original.GEH_GS_Staff = staff.PK;
			original.GEH_IsApproved = true;

			var rejected = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			rejected.GEH_EffectiveDate = new ZDateTimeOffset(2021, 1, 1);
			rejected.GEH_GS_Staff = staff.PK;
			rejected.GEH_IsApproved = false;

			var requested = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			requested.GEH_EffectiveDate = new ZDateTimeOffset(2022, 1, 3);
			requested.GEH_GS_Staff = staff.PK;
			requested.GEH_IsApproved = false;
			requested.GEH_GCR_ChangeRequest = changeRequest.PK;

			Factory.Save();

			AssertEquals(original, changeRequest.OriginalEmployment);
			AssertEquals(requested, changeRequest.RequestedEmployment);
			AssertEquals(staff, changeRequest.Staff);
		}

		[TestDate(2019, 6, 1)]
		public void TestManager()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var changeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();

			var original = Factory.NewWithValidTestData<GlbStaffManager>();
			original.GSM_EffectiveDate = new ZDateTime(2020, 1, 1);
			original.GSM_ManagerType = "DRM";
			original.GSM_GS_Staff = staff.PK;
			original.GSM_IsApproved = true;

			var wrongManagerType = Factory.NewWithValidTestData<GlbStaffManager>();
			wrongManagerType.GSM_EffectiveDate = new ZDateTime(2020, 1, 2);
			wrongManagerType.GSM_ManagerType = "REM";
			wrongManagerType.GSM_GS_Staff = staff.PK;
			wrongManagerType.GSM_IsApproved = true;

			var current = Factory.NewWithValidTestData<GlbStaffManager>();
			current.GSM_EffectiveDate = new ZDateTime(2019, 1, 1);
			current.GSM_GS_Staff = staff.PK;
			current.GSM_ManagerType = "DRM";
			current.GSM_IsApproved = true;
			current.GSM_EndDate = original.GSM_EffectiveDate;

			var rejected = Factory.NewWithValidTestData<GlbStaffManager>();
			rejected.GSM_EffectiveDate = new ZDateTime(2021, 1, 1);
			rejected.GSM_ManagerType = "DRM";
			rejected.GSM_GS_Staff = staff.PK;
			rejected.GSM_IsApproved = false;

			var requested = Factory.NewWithValidTestData<GlbStaffManager>();
			requested.GSM_EffectiveDate = new ZDateTime(2022, 1, 3);
			requested.GSM_ManagerType = "DRM";
			requested.GSM_GS_Staff = staff.PK;
			requested.GSM_IsApproved = false;
			requested.GSM_GCR_ChangeRequest = changeRequest.PK;

			Factory.Save();

			AssertEquals(original, changeRequest.OriginalManagementLink);
			AssertEquals(requested, changeRequest.RequestedManagementLink);
			AssertEquals(staff, changeRequest.Staff);
		}

		[TestDate(2019, 6, 1)]
		public void TestWorkPattern()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var changeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();

			var current = Factory.NewWithValidTestData<GlbWorkPattern>();
			current.GWP_EffectiveDate = new ZDateTimeOffset(2019, 1, 1);
			current.GWP_GS_Staff = staff.PK;
			current.GWP_IsApproved = true;

			var original = Factory.NewWithValidTestData<GlbWorkPattern>();
			original.GWP_EffectiveDate = new ZDateTimeOffset(2020, 1, 1);
			original.GWP_GS_Staff = staff.PK;
			original.GWP_IsApproved = true;

			var rejected = Factory.NewWithValidTestData<GlbWorkPattern>();
			rejected.GWP_EffectiveDate = new ZDateTimeOffset(2021, 1, 1);
			rejected.GWP_GS_Staff = staff.PK;
			rejected.GWP_IsApproved = false;

			var requested = Factory.NewWithValidTestData<GlbWorkPattern>();
			requested.GWP_EffectiveDate = new ZDateTimeOffset(2022, 1, 3);
			requested.GWP_GS_Staff = staff.PK;
			requested.GWP_IsApproved = false;
			requested.GWP_GCR_ChangeRequest = changeRequest.PK;

			Factory.Save();

			AssertEquals(original, changeRequest.OriginalWorkPattern);
			AssertEquals(requested, changeRequest.RequestedWorkPattern);
			AssertEquals(staff, changeRequest.Staff);
		}

		[TestDate(2019, 6, 1)]
		public void TestTeam()
		{
			var team = Factory.NewWithValidTestData<GlbTeam>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var changeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();

			var currentTeam = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			currentTeam.GET_EffectiveDate = new ZDateTimeOffset(2019, 1, 1);
			currentTeam.GET_GS_Staff = staff.PK;
			currentTeam.GET_IsApproved = true;
			currentTeam.GET_GST_NKTeamCode = team.GST_Code;

			var originalTeam = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			originalTeam.GET_EffectiveDate = new ZDateTimeOffset(2020, 1, 1);
			originalTeam.GET_GS_Staff = staff.PK;
			originalTeam.GET_IsApproved = true;
			originalTeam.GET_GST_NKTeamCode = team.GST_Code;

			var rejectedTeam = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			rejectedTeam.GET_EffectiveDate = new ZDateTimeOffset(2021, 1, 1);
			rejectedTeam.GET_GS_Staff = staff.PK;
			rejectedTeam.GET_IsApproved = false;
			rejectedTeam.GET_GST_NKTeamCode = team.GST_Code;

			var requestedTeam = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			requestedTeam.GET_EffectiveDate = new ZDateTimeOffset(2022, 1, 3);
			requestedTeam.GET_GS_Staff = staff.PK;
			requestedTeam.GET_IsApproved = false;
			requestedTeam.GET_GCR_ChangeRequest = changeRequest.PK;
			requestedTeam.GET_GST_NKTeamCode = team.GST_Code;

			Factory.Save();

			AssertEquals(originalTeam, changeRequest.OriginalTeam);
			AssertEquals(requestedTeam, changeRequest.RequestedTeam);
			AssertEquals(staff, changeRequest.Staff);
		}

		public void TestRaisedBy()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var requestor = Factory.NewWithValidTestData<GlbStaff>();

			var changeRequestTemplate = Factory.New<GlbStaffChangeRequestTemplate>();
			changeRequestTemplate.GSG_TemplateName = "AAA";
			changeRequestTemplate.GSG_Code = "TCODE";

			var changeRequest = Factory.New<GlbStaffChangeRequest>();
			changeRequest.GCR_GSG_Template = changeRequestTemplate.PK;
			changeRequest.GCR_SystemCreateUser = requestor.GS_Code;

			var teamLink = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			teamLink.GET_GCR_ChangeRequest = changeRequest.PK;
			teamLink.GET_GS_Staff = staff.PK;
			teamLink.GET_GST_NKTeamCode = Factory.NewWithValidTestData<GlbTeam>().GST_Code;

			AssertEquals(requestor, changeRequest.RaisedBy);
			AssertEquals(staff, changeRequest.Staff);
		}

		public void TestDocManagerSupportImplementation()
		{
			var changeRequest = Factory.New<GlbStaffChangeRequest>();
			AssertNotNull(changeRequest.DocManagerInfo);
			AssertEquals(changeRequest.DocManagerInfo.DocManagerCode, Core.Constants.DocManagerCodes.ChangeRequest);
		}

		public void TestWorkflowProviderImplementation()
		{
			var changeRequest = Factory.New<GlbStaffChangeRequest>();
			AssertNotNull(changeRequest.WorkflowItems);
			AssertEquals(changeRequest.WorkflowType, WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode);
			AssertNull(changeRequest.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var changeRequestTemplate = Factory.New<GlbStaffChangeRequestTemplate>();
			changeRequestTemplate.GSG_TemplateName = "AAA";
			changeRequestTemplate.GSG_Code = "TCODE";

			var changeRequest = Factory.New<GlbStaffChangeRequest>();
			changeRequest.GCR_GSG_Template = changeRequestTemplate.PK;
			changeRequest.GCR_Status = "APP";
			var columnValues = ((IColumnValueRankerInternals)changeRequest.GetTemplateSelectionCriteria()).ColumnValues.ToArray();
			AssertEquals(1, columnValues.Length);
			AssertEquals(columnValues[0].ColumnName, "P0_SubType1");
			AssertEquals(columnValues[0].Values[0], "TCODE");
			AssertEquals(columnValues[0].Values[1], string.Empty);
		}

		public void TestTemplate()
		{
			var changeRequest = Factory.New<GlbStaffChangeRequest>();
			AssertNull("Template should not be set", changeRequest.Template);

			var changeRequestTemplate = Factory.New<GlbStaffChangeRequestTemplate>();
			changeRequest.GCR_GSG_Template = changeRequestTemplate.PK;

			AssertEquals("Template should be set", changeRequestTemplate, changeRequest.Template);
		}
	}
}
