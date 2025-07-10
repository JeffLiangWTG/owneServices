using System;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Staff Changed Beneficiary Branch Department Report (Date Range)")]
	public class StaffChangedBeneficiaryBranchDepartmentDateRangeReport : TemplateTestCase
	{
		public void TestIncludeDepartmentChangeWithinRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var daniel = Factory.NewWithValidTestData<GlbStaff>();
			daniel.GS_FullName = "Daniel Craig";
			daniel.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var john = Factory.NewWithValidTestData<GlbStaff>();
			john.GS_FullName = "John Tester";
			john.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var company = Factory.NewWithValidTestData<GlbCompany>();

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "G07";
			currentDepartment.GE_Desc = "New Department";

			var previousDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			previousDepartment.GE_Code = "PD7";
			previousDepartment.GE_Desc = "Old Department";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "B07";
			currentBranch.GB_GC = company.PK;

			var currentBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			currentBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			currentBeneficiaryDaniel.GBB_GB_Branch = currentBranch.PK;
			currentBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 12, 12);

			var previousBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			previousBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			previousBeneficiaryDaniel.GBB_GE_Department = previousDepartment.PK;
			previousBeneficiaryDaniel.GBB_GB_Branch = currentBranch.PK;
			previousBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 11, 1);

			var currentBeneficiaryJohn = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBeneficiaryJohn.GBB_GS_Staff = john.PK;
			currentBeneficiaryJohn.GBB_GE_Department = currentDepartment.PK;
			currentBeneficiaryJohn.GBB_GB_Branch = currentBranch.PK;
			currentBeneficiaryJohn.GBB_EffectiveDate = new ZDateTimeOffset(2023, 6, 6);

			var previousBeneficiaryJohn = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			previousBeneficiaryJohn.GBB_GS_Staff = john.PK;
			previousBeneficiaryJohn.GBB_GE_Department = previousDepartment.PK;
			previousBeneficiaryJohn.GBB_GB_Branch = currentBranch.PK;
			previousBeneficiaryJohn.GBB_EffectiveDate = new ZDateTimeOffset(2022, 11, 1);

			Factory.Save();

			((DateField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTime(2022, 11, 11);
			((DateField)Report.FilterCollection["Report End Date"]).Value = new ZDateTime(2023, 1, 1);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertContains("Daniel Craig", sheetContent[4, 3].ToString());
				AssertContains("PD7 - Old Department", sheetContent[4, 7].ToString());
				AssertContains("G07 - New Department", sheetContent[4, 10].ToString());
				AssertEquals(new DateTime(2022, 12, 12), DateTime.FromOADate((double)sheetContent[4, 12]));

				AssertNotContains("John Tester", sheetContent.ToString());
			}
		}

		public void TestIncludesBranchChangeWithinRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var daniel = Factory.NewWithValidTestData<GlbStaff>();
			daniel.GS_FullName = "Daniel Craig";
			daniel.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var john = Factory.NewWithValidTestData<GlbStaff>();
			john.GS_FullName = "John Tester";
			john.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var company = Factory.NewWithValidTestData<GlbCompany>();

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "G07";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "B07";
			currentBranch.GB_GC = company.PK;
			currentBranch.GB_BranchName = "New Branch";

			var previousBranch = Factory.NewWithValidTestData<GlbBranch>();
			previousBranch.GB_Code = "C07";
			previousBranch.GB_GC = company.PK;
			previousBranch.GB_BranchName = "Old Branch";

			var currentBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			currentBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			currentBeneficiaryDaniel.GBB_GB_Branch = currentBranch.PK;
			currentBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 12, 12);

			var previousBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			previousBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			previousBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			previousBeneficiaryDaniel.GBB_GB_Branch = previousBranch.PK;
			previousBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 11, 1);

			var currentBeneficiaryJohn = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBeneficiaryJohn.GBB_GS_Staff = john.PK;
			currentBeneficiaryJohn.GBB_GE_Department = currentDepartment.PK;
			currentBeneficiaryJohn.GBB_GB_Branch = currentBranch.PK;
			currentBeneficiaryJohn.GBB_EffectiveDate = new ZDateTimeOffset(2023, 6, 6);

			var previousBeneficiaryJohn = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			previousBeneficiaryJohn.GBB_GS_Staff = john.PK;
			previousBeneficiaryJohn.GBB_GE_Department = currentDepartment.PK;
			previousBeneficiaryJohn.GBB_GB_Branch = previousBranch.PK;
			previousBeneficiaryJohn.GBB_EffectiveDate = new ZDateTimeOffset(2022, 11, 1);

			Factory.Save();

			((DateField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTime(2022, 11, 11);
			((DateField)Report.FilterCollection["Report End Date"]).Value = new ZDateTime(2023, 1, 1);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertContains("Daniel Craig", sheetContent[4, 3].ToString());
				AssertContains("C07 - Old Branch", sheetContent[4, 6].ToString());
				AssertContains("B07 - New Branch", sheetContent[4, 9].ToString());
				AssertEquals(new DateTime(2022, 12, 12), DateTime.FromOADate((double)sheetContent[4, 12]));

				AssertNotContains("John Tester", sheetContent.ToString());
			}
		}

		public void TestShowMostRecentJobFamilyHistoryWithinRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var daniel = Factory.NewWithValidTestData<GlbStaff>();
			daniel.GS_FullName = "Daniel Craig";
			daniel.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var company = Factory.NewWithValidTestData<GlbCompany>();

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "G07";
			currentDepartment.GE_Desc = "Branch 1";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "B07";
			currentBranch.GB_GC = company.PK;

			var previousBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "C07";
			currentBranch.GB_GC = company.PK;

			var currentBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			currentBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			currentBeneficiaryDaniel.GBB_GB_Branch = currentBranch.PK;
			currentBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 12, 12);

			var previousBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			previousBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			previousBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			previousBeneficiaryDaniel.GBB_GB_Branch = previousBranch.PK;
			previousBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 11, 1);

			var currentEmploymentHistory = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			currentEmploymentHistory.GEH_JobFamily = "DEV";
			currentEmploymentHistory.GEH_EffectiveDate = new ZDateTimeOffset(2022, 12, 30);
			currentEmploymentHistory.GEH_GS_Staff = daniel.PK;

			var previousEmploymentHistory = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			previousEmploymentHistory.GEH_JobFamily = "PRD";
			previousEmploymentHistory.GEH_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);
			previousEmploymentHistory.GEH_GS_Staff = daniel.PK;

			Factory.Save();

			((DateField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTime(2022, 11, 11);
			((DateField)Report.FilterCollection["Report End Date"]).Value = new ZDateTime(2023, 1, 1);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertContains("Daniel Craig", sheetContent[4, 3].ToString());
				AssertContains("PRD", sheetContent[4, 5].ToString());
				AssertContains("DEV", sheetContent[4, 8].ToString());
			}
		}

		public void TestOnlyIncludeBranchOrDepartmentChange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var daniel = Factory.NewWithValidTestData<GlbStaff>();
			daniel.GS_FullName = "Daniel Craig";
			daniel.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var company = Factory.NewWithValidTestData<GlbCompany>();

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "G07";
			currentDepartment.GE_Desc = "Branch 1";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "B07";
			currentBranch.GB_GC = company.PK;

			var currentBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			currentBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			currentBeneficiaryDaniel.GBB_GB_Branch = currentBranch.PK;
			currentBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2021, 12, 12);

			var previousBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			previousBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			previousBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			previousBeneficiaryDaniel.GBB_GB_Branch = currentBranch.PK;
			previousBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 12, 12);

			Factory.Save();

			((DateField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTime(2022, 11, 11);
			((DateField)Report.FilterCollection["Report End Date"]).Value = new ZDateTime(2023, 1, 1);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				AssertNotContains("Daniel Craig", sheetContent.ToString());
			}
		}

		public void TestGetMostRecentLineManager()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var daniel = Factory.NewWithValidTestData<GlbStaff>();
			daniel.GS_FullName = "Daniel Craig";
			daniel.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var company = Factory.NewWithValidTestData<GlbCompany>();

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "G07";
			currentDepartment.GE_Desc = "Branch 1";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "B07";
			currentBranch.GB_GC = company.PK;

			var previousBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "C07";
			currentBranch.GB_GC = company.PK;

			var currentBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			currentBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			currentBeneficiaryDaniel.GBB_GB_Branch = currentBranch.PK;
			currentBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 12, 12);

			var previousBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			previousBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			previousBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			previousBeneficiaryDaniel.GBB_GB_Branch = previousBranch.PK;
			previousBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 11, 1);

			var manager = Factory.NewWithValidTestData<GlbStaff>();
			manager.GS_Code = "MGR";
			manager.GS_FullName = "Skyfall Management";

			var staffManager = Factory.NewWithValidTestData<GlbStaffManager>();
			staffManager.GSM_GS_Staff = daniel.PK;
			staffManager.GSM_GS_Manager = manager.PK;
			staffManager.GSM_EffectiveDate = new ZDateTime(2022, 12, 12);

			Factory.Save();

			((DateField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTime(2022, 11, 11);
			((DateField)Report.FilterCollection["Report End Date"]).Value = new ZDateTime(2023, 1, 1);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertContains("Daniel Craig", sheetContent[4, 3].ToString());
				AssertContains("Skyfall Management", sheetContent[4, 11].ToString());
			}
		}

		public void TestGetMoveDates()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var daniel = Factory.NewWithValidTestData<GlbStaff>();
			daniel.GS_FullName = "Daniel Craig";
			daniel.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var company = Factory.NewWithValidTestData<GlbCompany>();

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "G07";
			currentDepartment.GE_Desc = "Branch 1";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "B07";
			currentBranch.GB_GC = company.PK;

			var previousBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "C07";
			currentBranch.GB_GC = company.PK;

			var currentBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			currentBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			currentBeneficiaryDaniel.GBB_GB_Branch = currentBranch.PK;
			currentBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 12, 12);

			var previousBeneficiaryDaniel = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			previousBeneficiaryDaniel.GBB_GS_Staff = daniel.PK;
			previousBeneficiaryDaniel.GBB_GE_Department = currentDepartment.PK;
			previousBeneficiaryDaniel.GBB_GB_Branch = previousBranch.PK;
			previousBeneficiaryDaniel.GBB_EffectiveDate = new ZDateTimeOffset(2022, 11, 1);

			Factory.Save();

			((DateField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTime(2022, 11, 11);
			((DateField)Report.FilterCollection["Report End Date"]).Value = new ZDateTime(2023, 1, 1);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertContains("Daniel Craig", sheetContent[4, 3].ToString());
				AssertEquals(new DateTime(2022, 12, 12), DateTime.FromOADate((double)sheetContent[4, 12]));
			}
		}
	}
}
