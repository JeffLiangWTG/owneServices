using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Staff Changed Beneficiary Department Report")]
	public class StaffChangedBeneficiaryDepartmentReport : TemplateTestCase
	{
		public void TestGetStaffFields()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var filterDateTime = new ZDateTime(2021, 10, 15);
			var testStartDate = filterDateTime.AddDays(-28);

			for (int i = 0; i < 30; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmploymentDate = new ZDateTime(2020, 11, 8);
				staff.GS_Code = $"S{i.ToString("D2")}";
				staff.GS_FullName = $"Staff {i.ToString("D2")}";
				staff.GS_EmailAddress = $"staff{i.ToString("D2")}@email.com";

				var staffDepartmentOld = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
				staffDepartmentOld.GBB_GS_Staff = staff.PK;
				staffDepartmentOld.GBB_EffectiveDate = new ZDateTimeOffset(testStartDate.AddDays(i - 1));

				var staffDepartmentNew = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
				staffDepartmentNew.GBB_GS_Staff = staff.PK;
				staffDepartmentNew.GBB_EffectiveDate = new ZDateTimeOffset(testStartDate.AddDays(i));
			}

			Factory.Save();

			((DateField)Report.FilterCollection["Recent Date UTC"]).Value = filterDateTime;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var staffList = new List<(string code, string fullName, string email)>();

				int i = 4;
				while (!string.IsNullOrWhiteSpace(excelInterface.WorkSheets[0][i, 2].ToString()) ||
					!string.IsNullOrWhiteSpace(excelInterface.WorkSheets[0][i, 3].ToString()) ||
					!string.IsNullOrWhiteSpace(excelInterface.WorkSheets[0][i, 4].ToString()))
				{
					var code = excelInterface.WorkSheets[0][i, 2].ToString();
					var fullName = excelInterface.WorkSheets[0][i, 3].ToString();
					var email = excelInterface.WorkSheets[0][i, 4].ToString();

					staffList.Add((code, fullName, email));

					i++;
				}

				AssertEquals(28, staffList.Count);
				for (int j = 0; j < 28; j++)
				{
					AssertEquals($"S{(j + 1).ToString("D2")}", staffList[j].code);
					AssertEquals($"Staff {(j + 1).ToString("D2")}", staffList[j].fullName);
					AssertEquals($"staff{(j + 1).ToString("D2")}@email.com", staffList[j].email);
				}
			}
		}

		public void TestGetJobFamilies()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmploymentDate = new ZDateTime(2020, 11, 8);
			staff.GS_Code = "S01";
			staff.GS_FullName = "Staff 01";

			var filterDateTime = new ZDateTime(2021, 10, 15);
			var testStartDate = filterDateTime.AddDays(-28);

			for (int i = 0; i < 30; i++)
			{
				var staffDepartment = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
				staffDepartment.GBB_GS_Staff = staff.PK;
				staffDepartment.GBB_EffectiveDate = new ZDateTimeOffset(testStartDate.AddDays(i));

				var employmentHistory = Factory.NewWithValidTestData<GlbEmploymentHistory>();
				employmentHistory.GEH_GS_Staff = staff.PK;
				employmentHistory.GEH_EffectiveDate = new ZDateTimeOffset(testStartDate.AddDays(i));
				employmentHistory.GEH_JobFamily = $"F{i.ToString("D2")}";
			}

			Factory.Save();

			((DateField)Report.FilterCollection["Recent Date UTC"]).Value = filterDateTime;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var familyList = new List<(string oldFamily, string newFamily)>();

				int i = 4;
				while (!string.IsNullOrWhiteSpace(excelInterface.WorkSheets[0][i, 5].ToString()) ||
					!string.IsNullOrWhiteSpace(excelInterface.WorkSheets[0][i, 6].ToString()))
				{
					var oldFamily = excelInterface.WorkSheets[0][i, 5].ToString();
					var newFamily = excelInterface.WorkSheets[0][i, 6].ToString();

					familyList.Add((oldFamily, newFamily));

					i++;
				}

				AssertEquals(28, familyList.Count);
				for (int j = 0; j < 28; j++)
				{
					AssertEquals($"F{j.ToString("D2")}", familyList[j].oldFamily);
					AssertEquals($"F{(j + 1).ToString("D2")}", familyList[j].newFamily);
				}
			}
		}

		public void TestGetLineManagers()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmploymentDate = new ZDateTime(2020, 11, 8);
			staff.GS_Code = "S01";
			staff.GS_FullName = "Staff 01";

			var filterDateTime = new ZDateTime(2021, 10, 15);
			var testStartDate = filterDateTime.AddDays(-28);

			for (int i = 0; i < 30; i++)
			{
				var staffDepartment = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
				staffDepartment.GBB_GS_Staff = staff.PK;
				staffDepartment.GBB_EffectiveDate = new ZDateTimeOffset(testStartDate.AddDays(i));

				var manager = Factory.NewWithValidTestData<GlbStaff>();
				manager.GS_Code = $"M{i.ToString("D2")}";
				manager.GS_FullName = $"Manager {i.ToString("D2")}";

				var staffManager = Factory.NewWithValidTestData<GlbStaffManager>();
				staffManager.GSM_GS_Staff = staff.PK;
				staffManager.GSM_GS_Manager = manager.PK;
				staffManager.GSM_EffectiveDate = testStartDate.AddDays(i);
			}

			Factory.Save();

			((DateField)Report.FilterCollection["Recent Date UTC"]).Value = filterDateTime;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var managerList = new List<string>();

				int i = 4;
				while (!string.IsNullOrWhiteSpace(excelInterface.WorkSheets[0][i, 7].ToString()))
				{
					var manager = excelInterface.WorkSheets[0][i, 7].ToString();

					managerList.Add(manager);

					i++;
				}

				AssertEquals(28, managerList.Count);
				for (int j = 0; j < 28; j++)
				{
					AssertEquals($"Manager {(j + 1).ToString("D2")}", managerList[j]);
				}
			}
		}

		public void TestGetMoveDates()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmploymentDate = new ZDateTime(2020, 11, 8);
			staff.GS_Code = "S01";
			staff.GS_FullName = "Staff 01";

			var testStartDate = new ZDateTime(2021, 09, 01);
			var filterDateTime = new ZDateTime(2021, 10, 15);

			for (int i = 0; i < 60; i++)
			{
				var staffDepartment = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
				staffDepartment.GBB_GS_Staff = staff.PK;
				staffDepartment.GBB_EffectiveDate = new ZDateTimeOffset(testStartDate.AddDays(i));
			}

			Factory.Save();

			((DateField)Report.FilterCollection["Recent Date UTC"]).Value = filterDateTime;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var datelist = new List<DateTime>();

				int i = 4;
				while (!string.IsNullOrWhiteSpace(excelInterface.WorkSheets[0][i, 8].ToString()))
				{
					var dateString = double.Parse(excelInterface.WorkSheets[0][i, 8].ToString());
					var date = DateTime.FromOADate(dateString);

					datelist.Add(date);

					i++;
				}

				var expectedDays = Enumerable.Range(0, 28)
					.Select(offset => filterDateTime.ToDateTime().AddDays(-offset))
					.OrderBy(day => day)
					.ToArray();

				AssertArrayEqualsByElements(expectedDays, datelist.ToArray());
			}
		}

		public void TestNoRecordWithinTimeRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmploymentDate = new ZDateTime(2020, 11, 8);
			staff.GS_Code = "S01";
			staff.GS_FullName = "Staff 01";

			var filterDateTime = new ZDateTime(2021, 10, 15);
			var testLastRecordDate = filterDateTime.AddDays(-28);

			for (int i = 0; i < 5; i++)
			{
				var staffDepartment = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
				staffDepartment.GBB_GS_Staff = staff.PK;
				staffDepartment.GBB_EffectiveDate = new ZDateTimeOffset(testLastRecordDate.AddDays(-i));
			}

			Factory.Save();

			((DateField)Report.FilterCollection["Recent Date UTC"]).Value = filterDateTime;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var datelist = new List<DateTime>();

				int i = 4;
				while (!string.IsNullOrWhiteSpace(excelInterface.WorkSheets[0][i, 8].ToString()))
				{
					var dateString = double.Parse(excelInterface.WorkSheets[0][i, 8].ToString());
					var date = DateTime.FromOADate(dateString);

					datelist.Add(date);

					i++;
				}

				AssertEquals(0, datelist.Count);
			}
		}
	}
}
