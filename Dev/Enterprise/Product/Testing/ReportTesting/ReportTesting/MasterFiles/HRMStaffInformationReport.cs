using System;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("HRM Staff Information Report")]
	public class HRMStaffInformationTemplateTest : TemplateTestCase
	{
		public void TestIncludeOnlyStaffFromFilteredResiCountry()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var aus = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_FullName = "Bob Belcher";
			bob.GS_LoginName = "Bob";
			bob.GS_RN_NKCountryCode = "AU";

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_LoginName = "Steve";
			steve.GS_FullName = "Steve Aussie";
			steve.GS_RN_NKCountryCode = "NZ";

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((LookupField)Report.FilterCollection["Residency Country/Region"]).ZValue = aus.PK;
			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertNotContains("STV", sheetContent.ToString());
			}
		}

		public void TestIncludeOnlyDepartedStaffWithinRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_EmploymentDate = new ZDateTime(2021, 1, 1);
			bob.GS_DepartureDate = new ZDateTime(2024, 1, 1);

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_EmploymentDate = new ZDateTime(2021, 1, 1);
			steve.GS_DepartureDate = new ZDateTime(2023, 1, 1);

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "Departed";
			((DateTimeOffsetField)Report.FilterCollection["Report Range Start Date (Turnover report only)"]).Value = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "STV");
				AssertNotContains("BOB", sheetContent.ToString());
			}
		}

		public void TestIgnoreNonStaff()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_EmploymentDate = new ZDateTime(2021, 1, 1);

			var sysuser = Factory.NewWithValidTestData<GlbStaff>();
			sysuser.GS_Code = "CW1";
			sysuser.GS_IsSystemAccount = true;

			var robot = Factory.NewWithValidTestData<GlbStaff>();
			robot.GS_Code = "~RB";
			robot.GS_IsResource = true;

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertNotContains("CW1", sheetContent.ToString());
				AssertNotContains("~RB", sheetContent.ToString());
			}
		}

		public void TestIncludeOnlyEmployedStaffWithinRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_EmploymentDate = new ZDateTime(2023, 1, 1);

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_EmploymentDate = new ZDateTime(2021, 1, 1);

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "Employed";
			((DateTimeOffsetField)Report.FilterCollection["Report Range Start Date (Turnover report only)"]).Value = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertNotContains("STV", sheetContent.ToString());
			}
		}

		public void TestIncludeAllStaffTurnoverWithinRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_EmploymentDate = new ZDateTime(2023, 1, 1);

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_EmploymentDate = new ZDateTime(2021, 1, 1);
			steve.GS_DepartureDate = new ZDateTime(2023, 1, 1);

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All Turnover";
			((DateTimeOffsetField)Report.FilterCollection["Report Range Start Date (Turnover report only)"]).Value = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertContainsValue(sheetContent, "STV");
			}
		}

		public void TestIncludeOnlyActiveStaff()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_IsActive = true;

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_IsActive = false;

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "Active";
			((DateTimeOffsetField)Report.FilterCollection["Report Range Start Date (Turnover report only)"]).Value = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "BOB");
				AssertNotContains("STV", sheetContent.ToString());
			}
		}

		public void TestGetAgeAtReportDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_Birthdate = new ZDate(2000, 7, 7);

			var jeff = Factory.NewWithValidTestData<GlbStaff>();
			jeff.GS_Code = "JFF";
			jeff.GS_Birthdate = new ZDate(1990, 6, 6);

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_Birthdate = new ZDate(1980, 2, 29);

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "22");
				AssertContainsValue(sheetContent, "33");
				AssertContainsValue(sheetContent, "43");
			}
		}

		public void TestGetMostRecentEmploymentInformation()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var oldJob = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			oldJob.GEH_GS_Staff = bob.PK;
			oldJob.GEH_JobTitle = "Designer";
			oldJob.GEH_JobFamily = "DSG";
			oldJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			oldJob.GEH_IsApproved = true;

			var newJob = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			newJob.GEH_GS_Staff = bob.PK;
			newJob.GEH_JobTitle = "C Suite Developer";
			newJob.GEH_JobFamily = "EXE";
			newJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);
			newJob.GEH_IsApproved = true;

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "C Suite Developer", "EXE");
				AssertNotContainsValue(sheetContent, "C Suite Developer", "Designer", "DSG");
			}
		}

		public void TestGetCurrentTeam()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var hrm = Factory.NewWithValidTestData<GlbTeam>();
			hrm.GST_Code = "HRM";
			hrm.GST_Name = "Human Resource Management";
			hrm.GST_IsActive = true;

			var oldTeam = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			oldTeam.GET_GS_Staff = bob.PK;
			oldTeam.GET_EffectiveDate = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			oldTeam.GET_GST_NKTeamCode = "HRM";
			oldTeam.GET_IsApproved = true;

			var glow = Factory.NewWithValidTestData<GlbTeam>();
			glow.GST_Code = "GLW";
			glow.GST_Name = "GLOW";
			glow.GST_IsActive = true;

			var newTeam = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			newTeam.GET_GS_Staff = bob.PK;
			newTeam.GET_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			newTeam.GET_GST_NKTeamCode = "GLW";
			newTeam.GET_IsApproved = true;

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "GLOW");
				AssertNotContainsValue(sheetContent, "GLOW", "Human Resource Management");
			}
		}

		public void TestGetCurrentDRM()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var luke = Factory.NewWithValidTestData<GlbStaff>();
			luke.GS_Code = "LUK";
			luke.GS_FullName = "Luke Skywalker";

			var yoda = Factory.NewWithValidTestData<GlbStaff>();
			yoda.GS_Code = "YOD";
			yoda.GS_FullName = "Yoda";

			var vader = Factory.NewWithValidTestData<GlbStaff>();
			vader.GS_Code = "VAD";
			vader.GS_FullName = "Darth Vader";

			var obiwan = Factory.NewWithValidTestData<GlbStaff>();
			obiwan.GS_Code = "OB1";
			obiwan.GS_FullName = "Obi Wan Kenobi";

			var oldDRM = luke.Managers.AddNew();
			oldDRM.GSM_GS_Manager = obiwan.PK;
			oldDRM.GSM_GS_Staff = luke.PK;
			oldDRM.GSM_ManagerType = "DRM";
			oldDRM.GSM_EffectiveDate = new ZDateTime(2022, 1, 1);
			oldDRM.GSM_EndDate = new ZDateTime(2022, 12, 12);
			oldDRM.GSM_IsApproved = true;

			var ppl = luke.Managers.AddNew();
			ppl.GSM_GS_Manager = vader.PK;
			ppl.GSM_GS_Staff = luke.PK;
			ppl.GSM_ManagerType = "PPL";
			ppl.GSM_EffectiveDate = new ZDateTime(2022, 1, 1);
			ppl.GSM_IsApproved = true;

			var newDRM = luke.Managers.AddNew();
			newDRM.GSM_GS_Manager = yoda.PK;
			newDRM.GSM_GS_Staff = luke.PK;
			newDRM.GSM_ManagerType = "DRM";
			newDRM.GSM_EffectiveDate = new ZDateTime(2023, 1, 1);
			newDRM.GSM_IsApproved = true;

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "Yoda", "YOD");
				AssertNotContainsValue(sheetContent, "Yoda", "Obi Wan Kenobi", "OB1");
				AssertNotContainsValue(sheetContent, "Yoda", "Darth Vader", "VAD");
			}
		}

		public void TestGetCurrentHomeDepartmentAndBranch()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "GB7";
			currentBranch.GB_BranchName = "New Branch";

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "GE7";
			currentDepartment.GE_Desc = "New Department";

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_GB_HomeBranch = currentBranch.PK;
			bob.GS_GE_HomeDepartment = currentDepartment.PK;

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "GB7", "New Branch", "GE7", "New Department");
			}
		}

		public void TestGetCurrentEmployingEntity()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_DepartureDate = new ZDateTime(2023, 5, 1);

			var oldCompany = Factory.NewWithValidTestData<GlbCompany>();
			oldCompany.GC_Code = "GC6";
			oldCompany.GC_Name = "Old Company";

			var oldBranch = Factory.NewWithValidTestData<GlbBranch>();
			oldBranch.GB_Code = "GB6";
			oldBranch.GB_BranchName = "Old Branch";
			oldBranch.GB_GC = oldCompany.PK;

			var oldDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			oldDepartment.GE_Code = "GE6";
			oldDepartment.GE_Desc = "Old Department";

			var oldEmpEntity = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			oldEmpEntity.GHB_GS_Staff = bob.PK;
			oldEmpEntity.GHB_GB_Branch = oldBranch.PK;
			oldEmpEntity.GHB_GE_Department = oldDepartment.PK;
			oldEmpEntity.GHB_EffectiveDate = new ZDateTimeOffset(2022, 5, 1, 0, 0, 0, offset);

			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			currentCompany.GC_Code = "GC7";
			currentCompany.GC_Name = "New Company";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "GB7";
			currentBranch.GB_BranchName = "New Branch";
			currentBranch.GB_GC = currentCompany.PK;

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "GE7";
			currentDepartment.GE_Desc = "New Department";

			var currentEmpEntity = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			currentEmpEntity.GHB_GS_Staff = bob.PK;
			currentEmpEntity.GHB_GB_Branch = currentBranch.PK;
			currentEmpEntity.GHB_GE_Department = currentDepartment.PK;
			currentEmpEntity.GHB_EffectiveDate = new ZDateTimeOffset(2023, 5, 1, 0, 0, 0, offset);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "GB7", "New Branch", "GE7", "New Department", "GC7", "New Company");
				AssertNotContainsValue(sheetContent, "GB7", "GB6", "Old Branch", "GE6", "Old Department", "GC6", "Old Company");
			}
		}

		public void TestGetCurrentBeneficiaryEntity()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_DepartureDate = new ZDateTime(2023, 5, 1);

			var oldCompany = Factory.NewWithValidTestData<GlbCompany>();
			oldCompany.GC_Code = "GC6";
			oldCompany.GC_Name = "Old Company";

			var oldBranch = Factory.NewWithValidTestData<GlbBranch>();
			oldBranch.GB_Code = "GB6";
			oldBranch.GB_BranchName = "Old Branch";
			oldBranch.GB_GC = oldCompany.PK;

			var oldDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			oldDepartment.GE_Code = "GE6";
			oldDepartment.GE_Desc = "Old Department";

			var oldBenEntity = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			oldBenEntity.GBB_GS_Staff = bob.PK;
			oldBenEntity.GBB_GB_Branch = oldBranch.PK;
			oldBenEntity.GBB_GE_Department = oldDepartment.PK;
			oldBenEntity.GBB_EffectiveDate = new ZDateTimeOffset(2022, 5, 1, 0, 0, 0, offset);

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "GC7";
			newCompany.GC_Name = "New Company";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "GB7";
			currentBranch.GB_BranchName = "New Branch";
			currentBranch.GB_GC = newCompany.PK;

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "GE7";
			currentDepartment.GE_Desc = "New Department";

			var currentBenEntity = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBenEntity.GBB_GS_Staff = bob.PK;
			currentBenEntity.GBB_GB_Branch = currentBranch.PK;
			currentBenEntity.GBB_GE_Department = currentDepartment.PK;
			currentBenEntity.GBB_EffectiveDate = new ZDateTimeOffset(2023, 5, 1, 0, 0, 0, offset);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "GB7", "New Branch", "GE7", "New Department", "GC7", "New Company");
				AssertNotContainsValue(sheetContent, "GB7", "GB6", "Old Branch", "GE6", "Old Department", "GC6", "Old Company");
			}
		}

		public void TestGetAllCurrentCostCenters()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var oldBranch = Factory.NewWithValidTestData<GlbBranch>();
			oldBranch.GB_Code = "GB6";
			oldBranch.GB_BranchName = "Branch 6";

			var oldDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			oldDepartment.GE_Code = "TC1";
			oldDepartment.GE_Desc = "Old Cost Center";

			var oldCostCenter = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			oldCostCenter.GSK_GS_Staff = bob.PK;
			oldCostCenter.GSK_GB_Branch = oldBranch.PK;
			oldCostCenter.GSK_GE_Department = oldDepartment.PK;
			oldCostCenter.GSK_StartDate = new ZDate(2022, 1, 1);
			oldCostCenter.GSK_EndDate = new ZDate(2022, 12, 12);

			var currentBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch1.GB_Code = "GB7";
			currentBranch1.GB_BranchName = "Branch 7";

			var currentDepartment1 = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment1.GE_Code = "TC2";
			currentDepartment1.GE_Desc = "Cost Center 2";

			var currentBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch2.GB_Code = "GB8";
			currentBranch2.GB_BranchName = "Branch 8";

			var currentDepartment2 = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment2.GE_Code = "TC3";
			currentDepartment2.GE_Desc = "Cost Center 3";

			var currentCostCenter1 = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			currentCostCenter1.GSK_GS_Staff = bob.PK;
			currentCostCenter1.GSK_GB_Branch = currentBranch1.PK;
			currentCostCenter1.GSK_GE_Department = currentDepartment1.PK;
			currentCostCenter1.GSK_StartDate = new ZDate(2023, 1, 1);

			var currentCostCenter2 = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			currentCostCenter2.GSK_GS_Staff = bob.PK;
			currentCostCenter2.GSK_GB_Branch = currentBranch2.PK;
			currentCostCenter2.GSK_GE_Department = currentDepartment2.PK;
			currentCostCenter2.GSK_StartDate = new ZDate(2023, 3, 2);

			var currentDepartment3 = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment3.GE_Code = "TC4";

			var currentCostCenter3 = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			currentCostCenter3.GSK_GS_Staff = bob.PK;
			currentCostCenter3.GSK_GE_Department = currentDepartment3.PK;
			currentCostCenter3.GSK_StartDate = new ZDate(2023, 2, 2);

			var offset = new TimeSpan(10, 0, 0);

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "TC2 (GB7), TC4, TC3 (GB8)");
				AssertNotContains("TC1", sheetContent.ToString());
				AssertNotContains("GB6", sheetContent.ToString());
			}
		}

		public void TestRetrieveCurrentLocationSource()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var oldLocationSource = Factory.NewWithValidTestData<GlbEmploymentLocation>();
			oldLocationSource.GEL_GS_Staff = bob.PK;
			oldLocationSource.GEL_EffectiveDate = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			oldLocationSource.GEL_LocationSource = "OTH";

			var newLocationSource = Factory.NewWithValidTestData<GlbEmploymentLocation>();
			newLocationSource.GEL_GS_Staff = bob.PK;
			newLocationSource.GEL_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			newLocationSource.GEL_LocationSource = "WFH";

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "WFH");
				AssertNotContainsValue(sheetContent, "WFH", "OTH");
			}
		}

		public void TestRetrieveCurrentTimeZone()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var oldTimeZone = Factory.NewWithValidTestData<GlbStaffTimezone>();
			oldTimeZone.GSZ_GS_Staff = bob.PK;
			oldTimeZone.GSZ_EffectiveDate = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			oldTimeZone.GSZ_R3_NKTimeZoneSetName = "Australia/Sydney";

			var newTimeZone = Factory.NewWithValidTestData<GlbStaffTimezone>();
			newTimeZone.GSZ_GS_Staff = bob.PK;
			newTimeZone.GSZ_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			newTimeZone.GSZ_R3_NKTimeZoneSetName = "Australia/Perth";

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "Australia/Perth");
				AssertNotContainsValue(sheetContent, "Australia/Perth", "Australia/Sydney");
			}
		}

		public void TestRetrieveCurrentWorkPattern()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var oldWorkPattern = Factory.NewWithValidTestData<GlbWorkPattern>();
			oldWorkPattern.GWP_GS_Staff = bob.PK;
			oldWorkPattern.GWP_EffectiveDate = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			oldWorkPattern.GWP_StandardDuration = new ZDateTime(1901, 1, 2, 16, 0, 0);
			oldWorkPattern.GWP_IsApproved = true;

			var workTimeMonOld = Factory.NewWithValidTestData<GlbWorkTime>();
			workTimeMonOld.GW_ParentID = oldWorkPattern.PK;
			workTimeMonOld.GW_StartTime = new ZDateTime(1900, 1, 1, 9, 0, 0);
			workTimeMonOld.GW_EndTime = new ZDateTime(1900, 1, 1, 17, 0, 0);
			workTimeMonOld.GW_DayOfWeek = "MON";

			var newWorkPattern = Factory.NewWithValidTestData<GlbWorkPattern>();
			newWorkPattern.GWP_GS_Staff = bob.PK;
			newWorkPattern.GWP_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			newWorkPattern.GWP_StandardDuration = new ZDateTime(1901, 1, 2, 14, 0, 0);
			newWorkPattern.GWP_IsApproved = true;

			var workTimeMonNew = Factory.NewWithValidTestData<GlbWorkTime>();
			workTimeMonNew.GW_ParentID = newWorkPattern.PK;
			workTimeMonNew.GW_StartTime = new ZDateTime(1900, 1, 1, 9, 0, 0);
			workTimeMonNew.GW_EndTime = new ZDateTime(1900, 1, 1, 17, 0, 0);
			workTimeMonNew.GW_DayOfWeek = "MON";

			var workTimeTueNew = Factory.NewWithValidTestData<GlbWorkTime>();
			workTimeTueNew.GW_ParentID = newWorkPattern.PK;
			workTimeTueNew.GW_StartTime = new ZDateTime(1900, 1, 1, 9, 0, 0);
			workTimeTueNew.GW_EndTime = new ZDateTime(1900, 1, 1, 17, 0, 0);
			workTimeTueNew.GW_DayOfWeek = "TUE";

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "38:00", "16:00", "0.42");
				AssertNotContainsValue(sheetContent, "38:00", "40:00", "8:00", "0.20");
			}
		}

		public void TestIgnoreUnApprovedChangeRequests()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var offset = new TimeSpan(10, 0, 0);

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";

			var changeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();
			changeRequest.GCR_Status = "REQ";

			var currentJob = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			currentJob.GEH_GS_Staff = bob.PK;
			currentJob.GEH_JobTitle = "Developer";
			currentJob.GEH_JobFamily = "DEV";
			currentJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			currentJob.GEH_IsApproved = true;

			var newJob = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			newJob.GEH_GS_Staff = bob.PK;
			newJob.GEH_JobTitle = "C Suite Developer";
			newJob.GEH_JobFamily = "EXE";
			newJob.GEH_EffectiveDate = new ZDateTimeOffset(2023, 5, 5, 0, 0, 0, offset);
			newJob.GEH_IsApproved = false;
			newJob.GEH_GCR_ChangeRequest = changeRequest.PK;

			var yoda = Factory.NewWithValidTestData<GlbStaff>();
			yoda.GS_Code = "YOD";
			yoda.GS_FullName = "Yoda";

			var obiwan = Factory.NewWithValidTestData<GlbStaff>();
			obiwan.GS_Code = "OB1";
			obiwan.GS_FullName = "Obi Wan Kenobi";

			var currentDRM = bob.Managers.AddNew();
			currentDRM.GSM_GS_Manager = obiwan.PK;
			currentDRM.GSM_GS_Staff = bob.PK;
			currentDRM.GSM_ManagerType = "DRM";
			currentDRM.GSM_EffectiveDate = new ZDateTime(2022, 1, 1);
			currentDRM.GSM_IsApproved = true;

			var newDRM = bob.Managers.AddNew();
			newDRM.GSM_GS_Manager = yoda.PK;
			newDRM.GSM_GS_Staff = bob.PK;
			newDRM.GSM_ManagerType = "DRM";
			newDRM.GSM_EffectiveDate = new ZDateTime(2023, 5, 5);
			newDRM.GSM_IsApproved = false;
			newDRM.GSM_GCR_ChangeRequest = changeRequest.PK;

			var hrm = Factory.NewWithValidTestData<GlbTeam>();
			hrm.GST_Code = "HRM";
			hrm.GST_Name = "Human Resource Management";
			hrm.GST_IsActive = true;

			var currentTeam = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			currentTeam.GET_GS_Staff = bob.PK;
			currentTeam.GET_EffectiveDate = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			currentTeam.GET_GST_NKTeamCode = "HRM";
			currentTeam.GET_IsApproved = true;

			var glow = Factory.NewWithValidTestData<GlbTeam>();
			glow.GST_Code = "GLW";
			glow.GST_Name = "GLOW";
			glow.GST_IsActive = true;

			var newTeam = Factory.NewWithValidTestData<GlbEmploymentTeam>();
			newTeam.GET_GS_Staff = bob.PK;
			newTeam.GET_EffectiveDate = new ZDateTimeOffset(2023, 5, 5, 0, 0, 0, offset);
			newTeam.GET_GST_NKTeamCode = "GLW";
			newTeam.GET_IsApproved = false;
			newTeam.GET_GCR_ChangeRequest = changeRequest.PK;

			var currentWorkPattern = Factory.NewWithValidTestData<GlbWorkPattern>();
			currentWorkPattern.GWP_GS_Staff = bob.PK;
			currentWorkPattern.GWP_EffectiveDate = new ZDateTimeOffset(2022, 1, 1, 0, 0, 0, offset);
			currentWorkPattern.GWP_StandardDuration = new ZDateTime(1901, 1, 2, 16, 0, 0);
			currentWorkPattern.GWP_IsApproved = true;

			var newWorkPattern = Factory.NewWithValidTestData<GlbWorkPattern>();
			newWorkPattern.GWP_GS_Staff = bob.PK;
			newWorkPattern.GWP_EffectiveDate = new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, offset);
			newWorkPattern.GWP_StandardDuration = new ZDateTime(1900, 1, 2, 14, 0, 0);
			newWorkPattern.GWP_IsApproved = false;
			newWorkPattern.GWP_GCR_ChangeRequest = changeRequest.PK;

			Factory.Save();

			((MultipleChoice)Report.FilterCollection["Staff Status"]).ValueAsStringForSerialisation = "All";
			((DateTimeOffsetField)Report.FilterCollection["Report Date (Current On)"]).Value = new ZDateTimeOffset(2023, 6, 6, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertContainsValue(sheetContent, "Developer");
				AssertContainsValue(sheetContent, "OB1");
				AssertContainsValue(sheetContent, "Human Resource Management");
				AssertContainsValue(sheetContent, "40:00");
				AssertNotContainsValue(sheetContent, "Developer", "C Suite Developer");
				AssertNotContainsValue(sheetContent, "OB1", "YOD");
				AssertNotContainsValue(sheetContent, "Human Resource Management", "GLOW");
				AssertNotContainsValue(sheetContent, "40:00", "38:00");
			}
		}

		void GetIndex(string code, ExcelWorkSheet report, ref int codeColumn, ref int codeRow)
		{
			for (var row = 0; row < report.RowCount; row++)
			{
				for (var col = 0; col < report.ColumnCount; col++)
				{
					if (report[row, col].ToString().Contains(code))
					{
						codeRow = row;
						codeColumn = col;
						return;
					}
				}
			}
			Fail("Couldn't find " + code);
		}

		void AssertContainsValue(ExcelWorkSheet report, params object[] rowTuple)
		{
			int codeRow = -1, codeColumn = -1;
			GetIndex(rowTuple[0].ToString(), report, ref codeColumn, ref codeRow);

			CombineAssertions(() =>
			{
				for (var i = 0; i < rowTuple.Length; i++)
				{
					AssertContains(rowTuple[i].ToString(), report[codeRow, codeColumn + i].ToString());
				}
			});
		}

		void AssertNotContainsValue(ExcelWorkSheet report, params object[] rowTuple) // The first entered object needs to be the same used in AssertContainsValue to identify the Row/Column
		{
			int codeRow = -1, codeColumn = -1;
			GetIndex(rowTuple[0].ToString(), report, ref codeColumn, ref codeRow);

			CombineAssertions(() =>
			{
				for (var i = 0; i < rowTuple.Length - 1; i++)
				{
					AssertNotContains(rowTuple[i + 1].ToString(), report[codeRow, codeColumn + i].ToString());
				}
			});
		}
	}
	public class HRMStaffInformationReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.HRReports(); }
		}

		public override string MenuName
		{
			get { return "HRM Staff Information Report"; }
		}

		public override string Hint
		{
			get
			{
				return "This report shows the most recent staff records for team members at the selected Report Date.\r\nThe staff that are included in the report can be filtered using the Staff Status drop-down. If the selected Staff Status is All or Active, the Report Range Start Date should be left blank, as no date range is required.\r\nThe date range is only required for a staff turnover report (when Staff Status is Employed or Departed or All turnover). For a turnover report, the Report Range Start Date and Report Date can be used to specify a date range where staff employment or departure occurred.\r\nIrrespective of the Staff Status selection, the selected Report Date will always be used to get the most recent staff information.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new HRMStaffInformationTemplateTest();
		}
	}
}
