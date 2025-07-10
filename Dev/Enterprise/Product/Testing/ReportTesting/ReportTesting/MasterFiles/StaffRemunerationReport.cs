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
	[TemplateName("Staff Remuneration Report")]
	[UseSnapshotProtection(true)]
	public class StaffRemunerationReport : TemplateTestCase
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

		public void TestOnlyIncludeStaffDepartedWithinRange()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_FullName = "Bob Belcher";
			bob.GS_EmploymentDate = new ZDateTime(2021, 5, 1);
			bob.GS_DepartureDate = new ZDateTime(2024, 5, 5);

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var steve = factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_FullName = "Steve Ozzie";
			steve.GS_EmploymentDate = new ZDateTime(2021, 5, 1);
			steve.GS_DepartureDate = new ZDateTime(2023, 5, 1);

			var steveBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			steveBasis.GSW_GS_Staff = steve.PK;
			steveBasis.GSW_WorkingBasis = "PER";
			steveBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "Departed";
			((DateField)Report.FilterCollection["Departed From Date"]).Value = new DateTime(2023, 1, 1);
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("STV", sheetContent[5, 1].ToString());
				AssertContains("Steve Ozzie", sheetContent[5, 2].ToString());
				AssertEquals(new DateTime(2023, 5, 1), DateTime.FromOADate((double)sheetContent[5, 11]));
				AssertNotContains("BOB", sheetContent.ToString());
				AssertNotContains("Bob Belcher", sheetContent.ToString());
			}
		}

		public void TestOnlyIncludeValidStaffWorkingBasis()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

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
			steveBasis.GSW_WorkingBasis = "UDF";
			steveBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Departed From Date"]).Value = new DateTime(2023, 1, 1);
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("BOB", sheetContent[5, 1].ToString());
				AssertNotContains("STV", sheetContent[5, 1].ToString());
			}
		}

		public void TestIncludeOnlyActiveStaff()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_FullName = "Bob Belcher";
			bob.GS_IsActive = false;

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var steve = factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_FullName = "Steve Ozzie";
			steve.GS_IsActive = true;

			var steveBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			steveBasis.GSW_GS_Staff = steve.PK;
			steveBasis.GSW_WorkingBasis = "PAR";
			steveBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "Active";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 1, 1);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("STV", sheetContent[5, 1].ToString());
				AssertContains("Steve Ozzie", sheetContent[5, 2].ToString());
				AssertNotContains("BOB", sheetContent.ToString());
				AssertNotContains("Bob Belcher", sheetContent.ToString());
			}
		}

		public void TestGetMostRecentEmploymentInformation()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			oldJob.GEH_GS_Staff = bob.PK;
			oldJob.GEH_JobTitle = "Dev";
			oldJob.GEH_JobFamily = "BRD";
			oldJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);

			var newJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			newJob.GEH_GS_Staff = bob.PK;
			newJob.GEH_JobTitle = "Associate";
			newJob.GEH_JobFamily = "PRD";
			newJob.GEH_DepartureReason = "DML";
			newJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 4, 4);

			var futureJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			futureJob.GEH_GS_Staff = bob.PK;
			futureJob.GEH_JobTitle = "Manager";
			futureJob.GEH_JobFamily = "MGT";
			futureJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 10, 10);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("DML", sheetContent[5, 12].ToString());
				AssertContains("Associate", sheetContent[5, 13].ToString());
				AssertContains("PRD", sheetContent[5, 14].ToString());
				AssertNotContains("Dev", sheetContent[5, 13].ToString());
				AssertNotContains("BRD", sheetContent[5, 14].ToString());
				AssertNotContains("Manager", sheetContent[5, 13].ToString());
				AssertNotContains("MGT", sheetContent[5, 14].ToString());
			}
		}

		public void TestIgnoreUnapprovedChangeRequests()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var currentJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			currentJob.GEH_GS_Staff = bob.PK;
			currentJob.GEH_JobTitle = "Dev";
			currentJob.GEH_JobFamily = "BRD";
			currentJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);
			currentJob.GEH_IsApproved = true;

			var newJob = factory.NewWithValidTestData<GlbEmploymentHistory>();
			newJob.GEH_GS_Staff = bob.PK;
			newJob.GEH_JobTitle = "Associate";
			newJob.GEH_JobFamily = "PRD";
			newJob.GEH_DepartureReason = "DML";
			newJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 4, 4);
			newJob.GEH_IsApproved = false;

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("Dev", sheetContent[5, 13].ToString());
				AssertContains("BRD", sheetContent[5, 14].ToString());
				AssertNotContains("DML", sheetContent[5, 12].ToString());
				AssertNotContains("Associate", sheetContent[5, 13].ToString());
				AssertNotContains("PRD", sheetContent[5, 14].ToString());
			}
		}

		public void TestGetCurrentBeneficiaryEntity()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldCompany = factory.NewWithValidTestData<GlbCompany>();
			oldCompany.GC_Code = "GC6";
			oldCompany.GC_Name = "Old Company";

			var oldBranch = factory.NewWithValidTestData<GlbBranch>();
			oldBranch.GB_Code = "GB6";
			oldBranch.GB_BranchName = "Old Branch";
			oldBranch.GB_GC = oldCompany.PK;

			var oldDepartment = factory.NewWithValidTestData<GlbDepartment>();
			oldDepartment.GE_Code = "GE6";
			oldDepartment.GE_Desc = "Old Department";

			var oldBenEntity = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			oldBenEntity.GBB_GS_Staff = bob.PK;
			oldBenEntity.GBB_GB_Branch = oldBranch.PK;
			oldBenEntity.GBB_GE_Department = oldDepartment.PK;
			oldBenEntity.GBB_EffectiveDate = new ZDateTimeOffset(2022, 5, 1);

			var newCompany = factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "GC7";
			newCompany.GC_Name = "New Company";

			var currentBranch = factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "GB7";
			currentBranch.GB_BranchName = "New Branch";
			currentBranch.GB_GC = newCompany.PK;

			var currentDepartment = factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "GE7";
			currentDepartment.GE_Desc = "New Department";

			var currentBenEntity = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBenEntity.GBB_GS_Staff = bob.PK;
			currentBenEntity.GBB_GB_Branch = currentBranch.PK;
			currentBenEntity.GBB_GE_Department = currentDepartment.PK;
			currentBenEntity.GBB_EffectiveDate = new ZDateTimeOffset(2023, 5, 1);

			var futureCompany = factory.NewWithValidTestData<GlbCompany>();
			futureCompany.GC_Code = "GC8";

			var futureBranch = factory.NewWithValidTestData<GlbBranch>();
			futureBranch.GB_Code = "GB8";
			futureBranch.GB_GC = newCompany.PK;

			var futureDepartment = factory.NewWithValidTestData<GlbDepartment>();
			futureDepartment.GE_Code = "GE8";
			futureDepartment.GE_Desc = "Future Department";

			var futureBenEntity = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			futureBenEntity.GBB_GS_Staff = bob.PK;
			futureBenEntity.GBB_GB_Branch = futureBranch.PK;
			futureBenEntity.GBB_GE_Department = futureDepartment.PK;
			futureBenEntity.GBB_EffectiveDate = new ZDateTimeOffset(2023, 10, 10);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("GB7", sheetContent[5, 15].ToString());
				AssertContains("GE7", sheetContent[5, 16].ToString());
				AssertContains("GC7", sheetContent[5, 17].ToString());
				AssertNotContains("GB6", sheetContent[5, 15].ToString());
				AssertNotContains("GE6", sheetContent[5, 16].ToString());
				AssertNotContains("GC6", sheetContent[5, 17].ToString());
				AssertNotContains("GB8", sheetContent[5, 15].ToString());
				AssertNotContains("GE8", sheetContent[5, 16].ToString());
				AssertNotContains("GC8", sheetContent[5, 17].ToString());
			}
		}

		public void TestGetCurrentEmployingEntity()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldCompany = factory.NewWithValidTestData<GlbCompany>();
			oldCompany.GC_Code = "GC6";
			oldCompany.GC_Name = "Old Company";

			var oldBranch = factory.NewWithValidTestData<GlbBranch>();
			oldBranch.GB_Code = "GB6";
			oldBranch.GB_BranchName = "Old Branch";
			oldBranch.GB_GC = oldCompany.PK;

			var oldDepartment = factory.NewWithValidTestData<GlbDepartment>();
			oldDepartment.GE_Code = "GE6";
			oldDepartment.GE_Desc = "Old Department";

			var oldEmpEntity = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			oldEmpEntity.GHB_GS_Staff = bob.PK;
			oldEmpEntity.GHB_GB_Branch = oldBranch.PK;
			oldEmpEntity.GHB_GE_Department = oldDepartment.PK;
			oldEmpEntity.GHB_EffectiveDate = new ZDateTimeOffset(2022, 5, 1);

			var newCompany = factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "GC7";
			newCompany.GC_Name = "New Company";

			var currentBranch = factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "GB7";
			currentBranch.GB_BranchName = "New Branch";
			currentBranch.GB_GC = newCompany.PK;

			var currentDepartment = factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "GE7";
			currentDepartment.GE_Desc = "New Department";

			var currentEmpEntity = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			currentEmpEntity.GHB_GS_Staff = bob.PK;
			currentEmpEntity.GHB_GB_Branch = currentBranch.PK;
			currentEmpEntity.GHB_GE_Department = currentDepartment.PK;
			currentEmpEntity.GHB_EffectiveDate = new ZDateTimeOffset(2023, 5, 1);

			var futureCompany = factory.NewWithValidTestData<GlbCompany>();
			futureCompany.GC_Code = "GC8";

			var futureBranch = factory.NewWithValidTestData<GlbBranch>();
			futureBranch.GB_Code = "GB8";
			futureBranch.GB_GC = newCompany.PK;

			var futureDepartment = factory.NewWithValidTestData<GlbDepartment>();
			futureDepartment.GE_Code = "GE8";
			futureDepartment.GE_Desc = "Future Department";

			var futureEmpEntity = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			futureEmpEntity.GHB_GB_Branch = futureBranch.PK;
			futureEmpEntity.GHB_GE_Department = futureDepartment.PK;
			futureEmpEntity.GHB_EffectiveDate = new ZDateTimeOffset(2023, 1, 10);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("GB7", sheetContent[5, 18].ToString());
				AssertContains("GE7", sheetContent[5, 19].ToString());
				AssertContains("GC7", sheetContent[5, 20].ToString());
				AssertNotContains("GB6", sheetContent[5, 18].ToString());
				AssertNotContains("GE6", sheetContent[5, 19].ToString());
				AssertNotContains("GC6", sheetContent[5, 20].ToString());
				AssertNotContains("GB8", sheetContent[5, 15].ToString());
				AssertNotContains("GE8", sheetContent[5, 16].ToString());
				AssertNotContains("GC8", sheetContent[5, 17].ToString());
			}
		}

		public void TestGetAllCurrentCostCenters()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldBranch = factory.NewWithValidTestData<GlbBranch>();
			oldBranch.GB_Code = "GB6";
			oldBranch.GB_BranchName = "Branch 6";

			var oldDepartment = factory.NewWithValidTestData<GlbDepartment>();
			oldDepartment.GE_Code = "TC1";
			oldDepartment.GE_Desc = "Old Cost Center";

			var oldCostCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			oldCostCenter.GSK_GS_Staff = bob.PK;
			oldCostCenter.GSK_GB_Branch = oldBranch.PK;
			oldCostCenter.GSK_GE_Department = oldDepartment.PK;
			oldCostCenter.GSK_StartDate = new ZDate(2022, 1, 1);
			oldCostCenter.GSK_EndDate = new ZDate(2022, 12, 12);

			var currentBranch1 = factory.NewWithValidTestData<GlbBranch>();
			currentBranch1.GB_Code = "GB7";
			currentBranch1.GB_BranchName = "Branch 7";

			var currentDepartment1 = factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment1.GE_Code = "TC2";
			currentDepartment1.GE_Desc = "Cost Center 2";

			var currentBranch2 = factory.NewWithValidTestData<GlbBranch>();
			currentBranch2.GB_Code = "GB8";
			currentBranch2.GB_BranchName = "Branch 8";

			var currentDepartment2 = factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment2.GE_Code = "TC3";
			currentDepartment2.GE_Desc = "Cost Center 3";

			var currentCostCenter1 = factory.NewWithValidTestData<GlbStaffCostCentre>();
			currentCostCenter1.GSK_GS_Staff = bob.PK;
			currentCostCenter1.GSK_GB_Branch = currentBranch1.PK;
			currentCostCenter1.GSK_GE_Department = currentDepartment1.PK;
			currentCostCenter1.GSK_StartDate = new ZDate(2023, 1, 1);

			var currentCostCenter2 = factory.NewWithValidTestData<GlbStaffCostCentre>();
			currentCostCenter2.GSK_GS_Staff = bob.PK;
			currentCostCenter2.GSK_GB_Branch = currentBranch2.PK;
			currentCostCenter2.GSK_GE_Department = currentDepartment2.PK;
			currentCostCenter2.GSK_StartDate = new ZDate(2023, 3, 2);

			var currentDepartment3 = factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment3.GE_Code = "TC4";

			var currentCostCenter3 = factory.NewWithValidTestData<GlbStaffCostCentre>();
			currentCostCenter3.GSK_GS_Staff = bob.PK;
			currentCostCenter3.GSK_GE_Department = currentDepartment3.PK;
			currentCostCenter3.GSK_StartDate = new ZDate(2023, 2, 2);

			var futureDepartment = factory.NewWithValidTestData<GlbDepartment>();
			futureDepartment.GE_Code = "TC5";

			var futureCostCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			futureCostCenter.GSK_GS_Staff = bob.PK;
			futureCostCenter.GSK_GE_Department = futureDepartment.PK;
			futureCostCenter.GSK_StartDate = new ZDate(2023, 10, 10);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("TC2 (GB7), TC4, TC3 (GB8)", sheetContent[5, 21].ToString());
				AssertNotContains("TC1", sheetContent.ToString());
				AssertNotContains("GB6", sheetContent.ToString());
				AssertNotContains("TC5", sheetContent.ToString());
			}
		}

		public void TestGetCurrentHomeDepartmentAndBranch()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var currentBranch = factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "GB7";

			var currentDepartment = factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "GE7";

			var bob = factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_GB_HomeBranch = currentBranch.PK;
			bob.GS_GE_HomeDepartment = currentDepartment.PK;

			var bobBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			bobBasis.GSW_GS_Staff = bob.PK;
			bobBasis.GSW_WorkingBasis = "PER";
			bobBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("GB7", sheetContent[5, 22].ToString());
				AssertContains("GE7", sheetContent[5, 23].ToString());
			}
		}

		public void TestGetCurrentRemPackage()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var staffBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			staffBasis.GSW_GS_Staff = staff.PK;
			staffBasis.GSW_WorkingBasis = "PER";
			staffBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var prevRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			prevRem.GSR_GS_Staff = staff.PK;
			prevRem.GSR_RN_NKCountry = "AU";
			prevRem.GSR_RX_NKCurrency = "EUR";
			prevRem.GSR_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);
			prevRem.GSR_FullTimeEquivalent = 0.80;

			var currentRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			currentRem.GSR_GS_Staff = staff.PK;
			currentRem.GSR_RN_NKCountry = "AU";
			currentRem.GSR_RX_NKCurrency = "AUD";
			currentRem.GSR_EffectiveDate = new ZDateTimeOffset(2023, 5, 20);
			currentRem.GSR_FullTimeEquivalent = 1;

			var futureRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			futureRem.GSR_GS_Staff = staff.PK;
			futureRem.GSR_RN_NKCountry = "US";
			futureRem.GSR_RX_NKCurrency = "USD";
			futureRem.GSR_EffectiveDate = new ZDateTimeOffset(2023, 10, 10);
			futureRem.GSR_FullTimeEquivalent = 1;

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertEquals(new DateTime(2023, 5, 20), DateTime.FromOADate((double)sheetContent[5, 24]));
				AssertContains("AUD", sheetContent[5, 25].ToString());
				AssertContains("1", sheetContent[5, 26].ToString());
				AssertNotEquals(new DateTime(2022, 1, 1), DateTime.FromOADate((double)sheetContent[5, 24]));
				AssertNotContains("EUR", sheetContent[5, 25].ToString());
				AssertNotContains("0.80", sheetContent[5, 26].ToString());
				AssertNotContains("USD", sheetContent[5, 25].ToString());
			}
		}

		public void TestGetCurrentJobClassification()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var staffBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			staffBasis.GSW_GS_Staff = staff.PK;
			staffBasis.GSW_WorkingBasis = "PER";
			staffBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var prevClass = factory.NewWithValidTestData<GlbStaffClassification>();
			prevClass.GSL_GS_Staff = staff.PK;
			prevClass.GSL_Classification = "BT1";
			prevClass.GSL_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var currentClass = factory.NewWithValidTestData<GlbStaffClassification>();
			currentClass.GSL_GS_Staff = staff.PK;
			currentClass.GSL_Classification = "BT2";
			currentClass.GSL_EffectiveDate = new ZDateTimeOffset(2023, 5, 20);

			var futureClass = factory.NewWithValidTestData<GlbStaffClassification>();
			futureClass.GSL_GS_Staff = staff.PK;
			futureClass.GSL_Classification = "BT3";
			futureClass.GSL_EffectiveDate = new ZDateTimeOffset(2023, 10, 10);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertEquals(new DateTime(2023, 5, 20), DateTime.FromOADate((double)sheetContent[5, 27]));
				AssertContains("BT2", sheetContent[5, 28].ToString());
				AssertContains("Business Development - Technical 2", sheetContent[5, 29].ToString());
				AssertNotEquals(new DateTime(2022, 1, 1), DateTime.FromOADate((double)sheetContent[5, 27]));
				AssertNotContains("BT1", sheetContent[5, 28].ToString());
				AssertNotContains("Business Development - Technical 1", sheetContent[5, 29].ToString());
				AssertNotContains("BT3", sheetContent[5, 28].ToString());
			}
		}

		public void TestGetCurrentWorkingBasis()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var prevBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			prevBasis.GSW_GS_Staff = staff.PK;
			prevBasis.GSW_WorkingBasis = "PAR";
			prevBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var currentBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			currentBasis.GSW_GS_Staff = staff.PK;
			currentBasis.GSW_WorkingBasis = "PER";
			currentBasis.GSW_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);

			var futureBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			futureBasis.GSW_GS_Staff = staff.PK;
			futureBasis.GSW_WorkingBasis = "CAS";
			futureBasis.GSW_EffectiveDate = new ZDateTimeOffset(2023, 10, 10);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertEquals(new DateTime(2023, 1, 1), DateTime.FromOADate((double)sheetContent[5, 30]));
				AssertContains("PER", sheetContent[5, 31].ToString());
				AssertNotEquals(new DateTime(2022, 1, 1), DateTime.FromOADate((double)sheetContent[5, 30]));
				AssertNotContains("PAR", sheetContent[5, 31].ToString());
				AssertNotContains("CAS", sheetContent[5, 31].ToString());
			}
		}

		public void TestGetCurrentPerformanceReview()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var staffBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			staffBasis.GSW_GS_Staff = staff.PK;
			staffBasis.GSW_WorkingBasis = "PER";
			staffBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var prevReview = factory.NewWithValidTestData<GlbStaffReview>();
			prevReview.GSV_GS_Staff = staff.PK;
			prevReview.GSV_GS_NKReviewer = "JTR";
			prevReview.GSV_Score = 2;
			prevReview.GSV_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var currentReview = factory.NewWithValidTestData<GlbStaffReview>();
			currentReview.GSV_GS_Staff = staff.PK;
			currentReview.GSV_GS_NKReviewer = "JT2";
			currentReview.GSV_Score = 3;
			currentReview.GSV_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertEquals(new DateTime(2023, 1, 1), DateTime.FromOADate((double)sheetContent[5, 32]));
				AssertContains("JT2", sheetContent[5, 33].ToString());
				AssertContains("3", sheetContent[5, 34].ToString());
				AssertNotEquals(new DateTime(2022, 1, 1), DateTime.FromOADate((double)sheetContent[5, 32]));
				AssertNotContains("JTR", sheetContent[5, 33].ToString());
				AssertNotContains("2", sheetContent[5, 34].ToString());
			}
		}

		public void TestGetCurrentRemEntitlements()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var staffBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			staffBasis.GSW_GS_Staff = staff.PK;
			staffBasis.GSW_WorkingBasis = "PER";
			staffBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var currentRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			currentRem.GSR_GS_Staff = staff.PK;
			currentRem.GSR_RN_NKCountry = "AU";
			currentRem.GSR_RX_NKCurrency = "AUD";
			currentRem.GSR_EffectiveDate = new ZDateTimeOffset(2023, 5, 20);

			var baseSalary = factory.NewWithValidTestData<GlbStaffEntitlement>();
			baseSalary.GSI_GSR_Remuneration = currentRem.PK;
			baseSalary.GSI_EntitlementCode = "BAS";
			baseSalary.GSI_Value = 100000;
			baseSalary.GSI_Frequency = "DIL";

			var remEquity = factory.NewWithValidTestData<GlbStaffEntitlement>();
			remEquity.GSI_GSR_Remuneration = currentRem.PK;
			remEquity.GSI_EntitlementCode = "RME";
			remEquity.GSI_Value = 1000;

			var perfBonus = factory.NewWithValidTestData<GlbStaffEntitlement>();
			perfBonus.GSI_GSR_Remuneration = currentRem.PK;
			perfBonus.GSI_EntitlementCode = "PBE";
			perfBonus.GSI_Value = 200;

			var perfBonusCash = factory.NewWithValidTestData<GlbStaffEntitlement>();
			perfBonusCash.GSI_GSR_Remuneration = currentRem.PK;
			perfBonusCash.GSI_EntitlementCode = "PBC";
			perfBonusCash.GSI_Value = 300;

			var comAllowance = factory.NewWithValidTestData<GlbStaffEntitlement>();
			comAllowance.GSI_GSR_Remuneration = currentRem.PK;
			comAllowance.GSI_EntitlementCode = "COM";
			comAllowance.GSI_Value = 1400;

			var carAllowance = factory.NewWithValidTestData<GlbStaffEntitlement>();
			carAllowance.GSI_GSR_Remuneration = currentRem.PK;
			carAllowance.GSI_EntitlementCode = "CAR";
			carAllowance.GSI_Value = 1600;

			var wowAllowance = factory.NewWithValidTestData<GlbStaffEntitlement>();
			wowAllowance.GSI_GSR_Remuneration = currentRem.PK;
			wowAllowance.GSI_EntitlementCode = "WOW";
			wowAllowance.GSI_Value = 100;

			var famAllowance = factory.NewWithValidTestData<GlbStaffEntitlement>();
			famAllowance.GSI_GSR_Remuneration = currentRem.PK;
			famAllowance.GSI_EntitlementCode = "FAM";
			famAllowance.GSI_Value = 1700;

			var otherRem = factory.NewWithValidTestData<GlbStaffEntitlement>();
			otherRem.GSI_GSR_Remuneration = currentRem.PK;
			otherRem.GSI_EntitlementCode = "OTR";
			otherRem.GSI_Value = 1900;

			var carLease = factory.NewWithValidTestData<GlbStaffEntitlement>();
			carLease.GSI_GSR_Remuneration = currentRem.PK;
			carLease.GSI_EntitlementCode = "CRL";
			carLease.GSI_Value = 2000;

			var onCallAllowance = factory.NewWithValidTestData<GlbStaffEntitlement>();
			onCallAllowance.GSI_GSR_Remuneration = currentRem.PK;
			onCallAllowance.GSI_EntitlementCode = "ONC";
			onCallAllowance.GSI_Value = 2100;

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("DIL", sheetContent[5, 35].ToString());
				AssertContains("100000", sheetContent[5, 36].ToString());
				AssertContains("100000", sheetContent[5, 37].ToString());
				AssertContains("1000", sheetContent[5, 38].ToString());
				AssertContains("200", sheetContent[5, 39].ToString());
				AssertContains("300", sheetContent[5, 40].ToString());
				AssertContains("101500", sheetContent[5, 48].ToString());
				AssertContains("100", sheetContent[5, 49].ToString());
				AssertContains("1600", sheetContent[5, 50].ToString());
				AssertContains("2000", sheetContent[5, 51].ToString());
				AssertContains("1700", sheetContent[5, 52].ToString());
				AssertContains("2100", sheetContent[5, 53].ToString());
				AssertContains("1900", sheetContent[5, 55].ToString());
				AssertContains("110900", sheetContent[5, 56].ToString());
			}
		}

		public void TestIgnoreInactiveRemEntitlements()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var staffBasis = factory.NewWithValidTestData<GlbStaffWorkingBasis>();
			staffBasis.GSW_GS_Staff = staff.PK;
			staffBasis.GSW_WorkingBasis = "PER";
			staffBasis.GSW_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var oldRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			oldRem.GSR_GS_Staff = staff.PK;
			oldRem.GSR_RN_NKCountry = "AU";
			oldRem.GSR_RX_NKCurrency = "AUD";
			oldRem.GSR_EffectiveDate = new ZDateTimeOffset(2021, 5, 20);

			var oldBaseSalary = factory.NewWithValidTestData<GlbStaffEntitlement>();
			oldBaseSalary.GSI_GSR_Remuneration = oldRem.PK;
			oldBaseSalary.GSI_EntitlementCode = "BAS";
			oldBaseSalary.GSI_Value = 200000;
			oldBaseSalary.GSI_Frequency = "YRL";

			var currentRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			currentRem.GSR_GS_Staff = staff.PK;
			currentRem.GSR_RN_NKCountry = "AU";
			currentRem.GSR_RX_NKCurrency = "AUD";
			currentRem.GSR_EffectiveDate = new ZDateTimeOffset(2023, 5, 20);

			var baseSalary = factory.NewWithValidTestData<GlbStaffEntitlement>();
			baseSalary.GSI_GSR_Remuneration = currentRem.PK;
			baseSalary.GSI_EntitlementCode = "BAS";
			baseSalary.GSI_Value = 100000;
			baseSalary.GSI_Frequency = "DIL";

			factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateField)Report.FilterCollection["Report Date (Current On)"]).Value = new DateTime(2023, 6, 6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContains("DIL", sheetContent[5, 35].ToString());
				AssertContains("100000", sheetContent[5, 37].ToString());
				AssertNotContains("YRL", sheetContent[5, 35].ToString());
				AssertNotContains("200000", sheetContent[5, 37].ToString());
			}
		}
	}
}
