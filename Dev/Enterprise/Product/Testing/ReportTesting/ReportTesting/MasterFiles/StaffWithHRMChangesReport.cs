using System;
using System.IO;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Tasks.DbSecurityAdmin;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Staff With HRM Changes Report")]
	[UseSnapshotProtection(true)]
	public class StaffWithHRMChangesTemplateTest : TemplateTestCase
	{
		void SetPrintUserPermission()
		{
			DocumentsDataRegistry.Instance.EnforceDataAccessOnReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var otherFactoryToAvoidCaching = new BusinessObjectFactory();

			var group = otherFactoryToAvoidCaching.NewWithValidTestData<GlbGroup>();
			var hrmRole = group.Roles.AddNew();
			hrmRole.GGR_RoleName = DbRoleTypes.CwHRMStaffRole;

			var staff = otherFactoryToAvoidCaching.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "HRMStaff";
			staff.GS_Code = "HRD";

			var link = otherFactoryToAvoidCaching.New<GlbGroupLink>();
			link.GK_GS = staff.PK;
			link.GK_GG = group.PK;

			otherFactoryToAvoidCaching.Save();

			staff = Factory.Load<GlbStaff>(staff.PK);
			staff.IsReadOnlyDBUser = true;

			new DbUserManager().SetPasswordForStaff(staff, "Pa$$w0rd!");
			Factory.Save();

			var task = Factory.NewWithValidTestData<ReportScheduleTask>();
			task.S5_GS_NKPrintUser = staff.GS_Code;

			Report.SetScheduleTask(task);
		}

		Mock<IServiceTaskNudger> serviceTaskNudgerMock;
		IDisposable serviceTaskNudgerDisposable;

		protected override void SetUp()
		{
			connectionThatCanAccessHrmSchema = Db.NewAdminConnection();
			serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			serviceTaskNudgerMock.Setup(nudger => nudger.NudgeServiceTask("DSA", null))
				.Callback(() =>
				{
					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					adminTask.RunTask(CancellationToken.None);
				});

			serviceTaskNudgerDisposable = ObjectFactory.Substitute(serviceTaskNudgerMock.Object);

			base.SetUp();

			SetPrintUserPermission();
		}

		protected override void TearDown()
		{
			connectionThatCanAccessHrmSchema?.RollbackTransaction();
			connectionThatCanAccessHrmSchema?.Dispose();
			connectionThatCanAccessHrmSchema = null;
			serviceTaskNudgerDisposable?.Dispose();
			base.TearDown();
		}

		DbConnection connectionThatCanAccessHrmSchema;

		public void TestOnlyGetStaffWithJobTitleChanges()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var steve = factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";

			var steveBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			steveBasis.GSW_GS_Staff = steve.PK;
			steveBasis.GSW_WorkingBasis = "PER";
			steveBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			oldJob.GEH_GS_Staff = bob.PK;
			oldJob.GEH_JobTitle = "Designer";
			oldJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			oldJob.GEH_IsApproved = true;

			var newJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			newJob.GEH_GS_Staff = bob.PK;
			newJob.GEH_JobTitle = "C Suite Developer";
			newJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			newJob.GEH_IsApproved = true;

			var steveJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			steveJob.GEH_GS_Staff = steve.PK;
			steveJob.GEH_JobTitle = "Senior Developer";
			steveJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			steveJob.GEH_IsApproved = true;

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["Job Title Changes"]).AddOption("Job Title Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertContainsValue(sheetContent, "Designer", "C Suite Developer", "Y");
				AssertNotContainsValue(sheetContent, "BOB", "STV");
			}
		}

		public void TestOnlyGetStaffWithTeamChanges()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var steve = factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";

			var steveBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			steveBasis.GSW_GS_Staff = steve.PK;
			steveBasis.GSW_WorkingBasis = "PER";
			steveBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var hrm = factory.NewWithValidTestData<GlbTeam>();
			hrm.GST_Code = "HRM";
			hrm.GST_Name = "Human Resource Management";
			hrm.GST_IsActive = true;

			var oldTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			oldTeam.GET_GS_Staff = bob.PK;
			oldTeam.GET_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			oldTeam.GET_GST_NKTeamCode = "HRM";
			oldTeam.GET_IsApproved = true;

			var glow = factory.NewWithValidTestData<GlbTeam>();
			glow.GST_Code = "GLW";
			glow.GST_Name = "GLOW";
			glow.GST_IsActive = true;

			var newTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			newTeam.GET_GS_Staff = bob.PK;
			newTeam.GET_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			newTeam.GET_GST_NKTeamCode = "GLW";
			newTeam.GET_IsApproved = true;

			var steveTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			steveTeam.GET_GS_Staff = steve.PK;
			steveTeam.GET_EffectiveDate = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			steveTeam.GET_GST_NKTeamCode = "GLW";
			steveTeam.GET_IsApproved = true;

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["Team Changes"]).AddOption("Team Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertContainsValue(sheetContent, "Human Resource Management", "GLOW", "Y");
				AssertNotContainsValue(sheetContent, "BOB", "STV");
			}
		}

		public void TestGetOnlyStaffWithPeopleLeaderChanges()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var luke = factory.NewWithValidTestData<GlbStaff>();
			luke.GS_Code = "LUK";

			var lukeBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			lukeBasis.GSW_GS_Staff = luke.PK;
			lukeBasis.GSW_WorkingBasis = "PER";
			lukeBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var yoda = factory.NewWithValidTestData<GlbStaff>();
			yoda.GS_Code = "YOD";
			yoda.GS_FullName = "Yoda";

			var obiwan = factory.NewWithValidTestData<GlbStaff>();
			obiwan.GS_Code = "OB1";
			obiwan.GS_FullName = "Obi Wan Kenobi";

			var vader = factory.NewWithValidTestData<GlbStaff>();
			vader.GS_Code = "VAD";
			vader.GS_FullName = "Darth Vader";

			var vaderBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			vaderBasis.GSW_GS_Staff = vader.PK;
			vaderBasis.GSW_WorkingBasis = "PER";
			vaderBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var emperor = factory.NewWithValidTestData<GlbStaff>();
			emperor.GS_Code = "PAL";
			emperor.GS_FullName = "Emperor Palpatine";

			var oldDRM = luke.Managers.AddNew();
			oldDRM.GSM_GS_Manager = obiwan.PK;
			oldDRM.GSM_GS_Staff = luke.PK;
			oldDRM.GSM_ManagerType = "DRM";
			oldDRM.GSM_EffectiveDate = new ZDateTime(2023, 1, 1);
			oldDRM.GSM_EndDate = new ZDateTime(2024, 5, 5);
			oldDRM.GSM_IsApproved = true;

			var newDRM = luke.Managers.AddNew();
			newDRM.GSM_GS_Manager = yoda.PK;
			newDRM.GSM_GS_Staff = luke.PK;
			newDRM.GSM_ManagerType = "DRM";
			newDRM.GSM_EffectiveDate = new ZDateTime(2024, 6, 6);
			newDRM.GSM_IsApproved = true;

			var vaderDRM = vader.Managers.AddNew();
			vaderDRM.GSM_GS_Manager = emperor.PK;
			vaderDRM.GSM_GS_Staff = vader.PK;
			vaderDRM.GSM_ManagerType = "PPL";
			vaderDRM.GSM_EffectiveDate = new ZDateTime(2024, 1, 1);
			vaderDRM.GSM_IsApproved = true;

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["People Leader Changes"]).AddOption("People Leader Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "LUK");
				AssertContainsValue(sheetContent, "Obi Wan Kenobi", "Yoda", "Y");
				AssertNotContainsValue(sheetContent, "LUK", "VAD");
			}
		}

		public void TestOnlyGetStaffWithFTEChanges()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var dan = factory.NewWithValidTestData<GlbStaff>();
			dan.GS_Code = "DAN";

			var danBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			danBasis.GSW_GS_Staff = dan.PK;
			danBasis.GSW_WorkingBasis = "PER";
			danBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var rich = factory.NewWithValidTestData<GlbStaff>();
			rich.GS_Code = "RIC";

			var richBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			richBasis.GSW_GS_Staff = rich.PK;
			richBasis.GSW_WorkingBasis = "PER";
			richBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			oldRem.GSR_GS_Staff = dan.PK;
			oldRem.GSR_RN_NKCountry = "AU";
			oldRem.GSR_RX_NKCurrency = "AUD";
			oldRem.GSR_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			oldRem.GSR_FullTimeEquivalent = 0.8;

			var newRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			newRem.GSR_GS_Staff = dan.PK;
			newRem.GSR_RN_NKCountry = "AU";
			newRem.GSR_RX_NKCurrency = "AUD";
			newRem.GSR_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			newRem.GSR_FullTimeEquivalent = 1;

			var steveRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			steveRem.GSR_GS_Staff = rich.PK;
			steveRem.GSR_RN_NKCountry = "AU";
			steveRem.GSR_RX_NKCurrency = "AUD";
			steveRem.GSR_EffectiveDate = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["FTE Changes"]).AddOption("FTE Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "DAN");
				AssertContainsValue(sheetContent, "0.8", "1", "Y");
				AssertNotContainsValue(sheetContent, "DAN", "RIC");
			}
		}

		public void TestOnlyGetStaffWithEmploymentEntityChanges()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var steve = factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";

			var steveBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			steveBasis.GSW_GS_Staff = steve.PK;
			steveBasis.GSW_WorkingBasis = "PER";
			steveBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldCompany = factory.NewWithValidTestData<GlbCompany>();
			oldCompany.GC_Code = "GC6";
			oldCompany.GC_Name = "Old Company";

			var oldBranch = factory.NewWithValidTestData<GlbBranch>();
			oldBranch.GB_Code = "GB6";
			oldBranch.GB_BranchName = "Old Branch";
			oldBranch.GB_GC = oldCompany.PK;

			var oldEmpEntity = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			oldEmpEntity.GHB_GS_Staff = bob.PK;
			oldEmpEntity.GHB_GB_Branch = oldBranch.PK;
			oldEmpEntity.GHB_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);

			var newCompany = factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "GC7";
			newCompany.GC_Name = "New Company";

			var newBranch = factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_Code = "GB7";
			newBranch.GB_BranchName = "New Branch";
			newBranch.GB_GC = newCompany.PK;

			var newEmpEntity = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			newEmpEntity.GHB_GS_Staff = bob.PK;
			newEmpEntity.GHB_GB_Branch = newBranch.PK;
			newEmpEntity.GHB_EffectiveDate = new ZDateTimeOffset(2024, 6, 1, 0, 0, 0, offset);

			var steveEmpEntity = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			steveEmpEntity.GHB_GS_Staff = steve.PK;
			steveEmpEntity.GHB_GB_Branch = newBranch.PK;
			steveEmpEntity.GHB_EffectiveDate = new ZDateTimeOffset(2024, 5, 1, 0, 0, 0, offset);

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["Employing Entity Changes"]).AddOption("Employing Entity Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertContainsValue(sheetContent, "GB6", "GB7", "Y");
				AssertNotContainsValue(sheetContent, "BOB", "STV");
			}
		}

		public void TestOnlyGetStaffWithBeneficiaryChanges()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var steve = factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";

			var steveBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			steveBasis.GSW_GS_Staff = steve.PK;
			steveBasis.GSW_WorkingBasis = "PER";
			steveBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldCompany = factory.NewWithValidTestData<GlbCompany>();
			oldCompany.GC_Code = "GC6";
			oldCompany.GC_Name = "Old Company";

			var oldBranch = factory.NewWithValidTestData<GlbBranch>();
			oldBranch.GB_Code = "GB6";
			oldBranch.GB_BranchName = "Old Branch";
			oldBranch.GB_GC = oldCompany.PK;

			var oldBenEntity = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			oldBenEntity.GBB_GS_Staff = bob.PK;
			oldBenEntity.GBB_GB_Branch = oldBranch.PK;
			oldBenEntity.GBB_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);

			var newCompany = factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "GC7";
			newCompany.GC_Name = "New Company";

			var newBranch = factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_Code = "GB7";
			newBranch.GB_BranchName = "New Branch";
			newBranch.GB_GC = newCompany.PK;

			var newBenEntity = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			newBenEntity.GBB_GS_Staff = bob.PK;
			newBenEntity.GBB_GB_Branch = newBranch.PK;
			newBenEntity.GBB_EffectiveDate = new ZDateTimeOffset(2024, 5, 1, 0, 0, 0, offset);

			var steveBenEntity = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			steveBenEntity.GBB_GS_Staff = steve.PK;
			steveBenEntity.GBB_GB_Branch = newBranch.PK;
			steveBenEntity.GBB_EffectiveDate = new ZDateTimeOffset(2024, 5, 1, 0, 0, 0, offset);

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["Beneficiary Entity Changes"]).AddOption("Beneficiary Entity Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertContainsValue(sheetContent, "GB6", "GB7", "Y");
				AssertNotContainsValue(sheetContent, "BOB", "STV");
			}
		}

		public void TestIgnoreChangesFromNonSpecifiedFields()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var steve = factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";

			var steveBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			steveBasis.GSW_GS_Staff = steve.PK;
			steveBasis.GSW_WorkingBasis = "PER";
			steveBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			oldJob.GEH_GS_Staff = bob.PK;
			oldJob.GEH_JobTitle = "Designer";
			oldJob.GEH_JobFamily = "DSG";
			oldJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			oldJob.GEH_IsApproved = true;

			var newJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			newJob.GEH_GS_Staff = bob.PK;
			newJob.GEH_JobTitle = "C Suite Developer";
			newJob.GEH_JobFamily = "EXE";
			newJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			newJob.GEH_IsApproved = true;

			var hrm = factory.NewWithValidTestData<GlbTeam>();
			hrm.GST_Code = "HRM";
			hrm.GST_Name = "Human Resource Management";
			hrm.GST_IsActive = true;

			var oldTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			oldTeam.GET_GS_Staff = steve.PK;
			oldTeam.GET_EffectiveDate = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			oldTeam.GET_GST_NKTeamCode = "HRM";
			oldTeam.GET_IsApproved = true;

			var glow = factory.NewWithValidTestData<GlbTeam>();
			glow.GST_Code = "GLW";
			glow.GST_Name = "GLOW";
			glow.GST_IsActive = true;

			var steveTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			steveTeam.GET_GS_Staff = steve.PK;
			steveTeam.GET_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			steveTeam.GET_GST_NKTeamCode = "GLW";
			steveTeam.GET_IsApproved = true;

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["Team Changes"]).AddOption("Team Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "STV");
				AssertContainsValue(sheetContent, "Human Resource Management", "GLOW", "Y");
				AssertNotContainsValue(sheetContent, "STV", "BOB");
			}
		}

		public void TestIgnoreChangesOutsideOfRange()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var steve = factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";

			var steveBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			steveBasis.GSW_GS_Staff = steve.PK;
			steveBasis.GSW_WorkingBasis = "PER";
			steveBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var bobOldJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			bobOldJob.GEH_GS_Staff = bob.PK;
			bobOldJob.GEH_JobTitle = "Designer";
			bobOldJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			bobOldJob.GEH_IsApproved = true;

			var bobNewJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			bobNewJob.GEH_GS_Staff = bob.PK;
			bobNewJob.GEH_JobTitle = "C Suite Developer";
			bobNewJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			bobNewJob.GEH_IsApproved = true;

			var bobFutureJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			bobFutureJob.GEH_GS_Staff = bob.PK;
			bobFutureJob.GEH_JobTitle = "CEO";
			bobFutureJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 7, 6, 0, 0, 0, offset);
			bobFutureJob.GEH_IsApproved = true;

			var steveOldJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			steveOldJob.GEH_GS_Staff = steve.PK;
			steveOldJob.GEH_JobTitle = "Developer";
			steveOldJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			steveOldJob.GEH_IsApproved = true;

			var steveNewJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			steveNewJob.GEH_GS_Staff = steve.PK;
			steveNewJob.GEH_JobTitle = "Senior Developer";
			steveNewJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 7, 7, 0, 0, 0, offset);
			steveNewJob.GEH_IsApproved = true;

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["Job Title Changes"]).AddOption("Job Title Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertContainsValue(sheetContent, "Designer", "C Suite Developer", "Y");
				AssertNotContainsValue(sheetContent, "C Suite Developer", "CEO");
				AssertNotContainsValue(sheetContent, "BOB", "STV");
			}
		}

		public void TestGetOnlyOneStaffRowWithSpecifiedFieldChange()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var bobOldJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			bobOldJob.GEH_GS_Staff = bob.PK;
			bobOldJob.GEH_JobTitle = "Designer";
			bobOldJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			bobOldJob.GEH_IsApproved = true;

			var bobNewJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			bobNewJob.GEH_GS_Staff = bob.PK;
			bobNewJob.GEH_JobTitle = "C Suite Developer";
			bobNewJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 3, 3, 0, 0, 0, offset);
			bobNewJob.GEH_IsApproved = true;

			var bobNewestJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			bobNewestJob.GEH_GS_Staff = bob.PK;
			bobNewestJob.GEH_JobTitle = "CEO";
			bobNewestJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			bobNewestJob.GEH_IsApproved = true;

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["Job Title Changes"]).AddOption("Job Title Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertContainsValue(sheetContent, "C Suite Developer", "CEO", "Y");
				AssertNotContainsValue(sheetContent, "C Suite Developer", "Designer", "C Suite Developer");
			}
		}

		public void TestChangesInRangeAreFlagged()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			oldJob.GEH_GS_Staff = bob.PK;
			oldJob.GEH_JobTitle = "Designer";
			oldJob.GEH_JobFamily = "DSG";
			oldJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			oldJob.GEH_IsApproved = true;

			var newJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			newJob.GEH_GS_Staff = bob.PK;
			newJob.GEH_JobTitle = "C Suite Developer";
			newJob.GEH_JobFamily = "EXE";
			newJob.GEH_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			newJob.GEH_IsApproved = true;

			var hrm = factory.NewWithValidTestData<GlbTeam>();
			hrm.GST_Code = "HRM";
			hrm.GST_Name = "Human Resource Management";
			hrm.GST_IsActive = true;

			var oldTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			oldTeam.GET_GS_Staff = bob.PK;
			oldTeam.GET_EffectiveDate = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			oldTeam.GET_GST_NKTeamCode = "HRM";
			oldTeam.GET_IsApproved = true;

			var glow = factory.NewWithValidTestData<GlbTeam>();
			glow.GST_Code = "GLW";
			glow.GST_Name = "GLOW";
			glow.GST_IsActive = true;

			var newTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			newTeam.GET_GS_Staff = bob.PK;
			newTeam.GET_EffectiveDate = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);
			newTeam.GET_GST_NKTeamCode = "GLW";
			newTeam.GET_IsApproved = true;

			var rem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem.GSR_GS_Staff = bob.PK;
			rem.GSR_RN_NKCountry = "AU";
			rem.GSR_RX_NKCurrency = "AUD";
			rem.GSR_EffectiveDate = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			rem.GSR_FullTimeEquivalent = 0.5;

			factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2024, 6, 6, 0, 0, 0, offset);
			((OptionGroup)Report.FilterCollection["Job Title Changes"]).AddOption("Job Title Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertContainsValue(sheetContent, "Designer", "C Suite Developer", "Y");
				AssertContainsValue(sheetContent, "Human Resource Management", "GLOW", "N");
				AssertContainsValue(sheetContent, "0.5", "N");
			}
		}

		static (int Row, int Column) GetIndex(string code, ExcelWorkSheet report)
		{
			if (string.IsNullOrEmpty(code))
			{
				throw new ArgumentNullException(nameof(code));
			}

			for (var row = 0; row < report.RowCount; row++)
			{
				for (var col = 0; col < report.ColumnCount; col++)
				{
					if (report[row, col].ToString().Contains(code))
					{
						return (row, col);
					}
				}
			}

			throw new ArgumentException($"Could not find cell that contains {code}", nameof(code));
		}

		static void AssertContainsValue(ExcelWorkSheet report, params object[] rowTuple)
		{
			var (codeRow, codeColumn) = GetIndex(rowTuple[0].ToString(), report);

			CombineAssertions(() =>
			{
				for (var i = 0; i < rowTuple.Length; i++)
				{
					AssertContains(rowTuple[i].ToString(), report[codeRow, codeColumn + i].ToString());
				}
			});
		}

		static void AssertNotContainsValue(ExcelWorkSheet report, params object[] rowTuple)
		{
			var (codeRow, codeColumn) = GetIndex(rowTuple[0].ToString(), report);

			CombineAssertions(() =>
			{
				for (var i = 0; i < rowTuple.Length - 1; i++)
				{
					AssertNotContains(rowTuple[i + 1].ToString(), report[codeRow, codeColumn + i].ToString());
				}
			});
		}
	}
}
