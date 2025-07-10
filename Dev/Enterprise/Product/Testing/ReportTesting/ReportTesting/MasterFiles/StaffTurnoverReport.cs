using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Staff Turnover Report")]
	public class StaffTurnoverReportTemplateTest : TemplateTestCase
	{
		public void TestMatchesEmploymentDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmploymentDate = new ZDateTime(2021, 11, 8);
			staff.GS_Code = "BOB";
			staff.GS_FullName = "Bob Belcher";

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 3].ToString();
				AssertContains("Bob Belcher", sheetContent);
			}
		}

		public void TestMatchesDepartureDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_DepartureDate = new ZDateTime(2021, 11, 8);
			staff.GS_Code = "JPP";
			staff.GS_FullName = "John F Kennedy";

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 3].ToString();
				AssertContains("John F Kennedy", sheetContent);
			}
		}

		public void TestStaffOutsideRangeArentMatched()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmploymentDate = new ZDateTime(2021, 11, 8);
			staff1.GS_Code = "EDN";
			staff1.GS_FullName = "Edward Norton";
			staff2.GS_EmploymentDate = new ZDateTime(2021, 12, 8);
			staff2.GS_Code = "BRD";
			staff2.GS_FullName = "Brad Pitt";

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 3].ToString();
				AssertContains("Edward Norton", sheetContent);
				AssertNotContains("Brad Pitt", sheetContent);
			}
		}

		public void TestMatchesLatestDRMLeader()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var yoda = Factory.NewWithValidTestData<GlbStaff>();
			yoda.GS_FullName = "Yoda";
			yoda.GS_Code = "YOD";

			var luke = Factory.NewWithValidTestData<GlbStaff>();
			luke.GS_EmploymentDate = new ZDateTime(2021, 11, 8);

			var manager = luke.Managers.AddNew();
			manager.GSM_GS_Manager = yoda.PK;
			manager.GSM_GS_Staff = luke.PK;
			manager.GSM_EffectiveDate = new ZDateTime(2021, 6, 1);
			manager.GSM_EndDate = new ZDateTime(2022, 1, 1);
			manager.GSM_ManagerType = "DRM";

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 10].ToString();
				AssertContains("Yoda", sheetContent);
			}
		}

		public void TestIgnoresNonDRMManagerTypes()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var yoda = Factory.NewWithValidTestData<GlbStaff>();
			yoda.GS_FullName = "Yoda";
			yoda.GS_Code = "YOD";

			var vader = Factory.NewWithValidTestData<GlbStaff>();
			vader.GS_FullName = "Anakin Skywalker";
			vader.GS_Code = "DVR";

			var luke = Factory.NewWithValidTestData<GlbStaff>();
			luke.GS_EmploymentDate = new ZDateTime(2021, 11, 8);

			var pplManager = luke.Managers.AddNew();
			var drmManager = luke.Managers.AddNew();

			pplManager.GSM_GS_Manager = yoda.PK;
			pplManager.GSM_GS_Staff = luke.PK;
			pplManager.GSM_EffectiveDate = new ZDateTime(2021, 6, 1);
			pplManager.GSM_EndDate = new ZDateTime(2022, 1, 1);
			pplManager.GSM_ManagerType = "PPL";

			drmManager.GSM_GS_Manager = vader.PK;
			drmManager.GSM_GS_Staff = luke.PK;
			drmManager.GSM_EffectiveDate = new ZDateTime(2021, 10, 1);
			drmManager.GSM_ManagerType = "DRM";

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 10].ToString();
				AssertNotContains("Yoda", sheetContent);
				AssertContains("Anakin Skywalker", sheetContent);
			}
		}

		public void TestManagerChangesMidwayThroughPeriod()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var quiGon = Factory.NewWithValidTestData<GlbStaff>();
			quiGon.GS_FullName = "Qui-Gon Jinn";
			quiGon.GS_Code = "QGJ";

			var obiWan = Factory.NewWithValidTestData<GlbStaff>();
			obiWan.GS_FullName = "Obi-Wan Kenobi";
			obiWan.GS_Code = "OWK";

			var anakin = Factory.NewWithValidTestData<GlbStaff>();
			anakin.GS_EmploymentDate = new ZDateTime(2021, 11, 8);

			var drmManager = anakin.Managers.AddNew();
			var newDrmManager = anakin.Managers.AddNew();

			drmManager.GSM_GS_Manager = quiGon.PK;
			drmManager.GSM_GS_Staff = anakin.PK;
			drmManager.GSM_EffectiveDate = new ZDateTime(2021, 6, 1);
			drmManager.GSM_EndDate = new ZDateTime(2021, 11, 20);
			drmManager.GSM_ManagerType = "DRM";

			newDrmManager.GSM_GS_Manager = obiWan.PK;
			newDrmManager.GSM_GS_Staff = anakin.PK;
			newDrmManager.GSM_EffectiveDate = new ZDateTime(2021, 11, 21);
			newDrmManager.GSM_ManagerType = "DRM";

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 10].ToString();
				AssertContains("Obi-Wan Kenobi", sheetContent);
				AssertNotContains("Qui-Gon Jinn", sheetContent);
			}
		}

		public void TestUsesCurrentBeneficiary()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Daniel Craig";
			staff.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			currentCompany.GC_Name = "James Bond Co.";

			var previousCompany = Factory.NewWithValidTestData<GlbCompany>();
			previousCompany.GC_Name = "Goldfinger Co.";

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "G07";

			var previousDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			previousDepartment.GE_Code = "PD7";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "B07";
			currentBranch.GB_GC = currentCompany.PK;

			var previousBranch = Factory.NewWithValidTestData<GlbBranch>();
			previousBranch.GB_Code = "PB7";
			previousBranch.GB_GC = previousCompany.PK;

			var currentBeneficiary = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			currentBeneficiary.GBB_GS_Staff = staff.PK;
			currentBeneficiary.GBB_GE_Department = currentDepartment.PK;
			currentBeneficiary.GBB_GB_Branch = currentBranch.PK;
			currentBeneficiary.GBB_EffectiveDate = new ZDateTimeOffset(2021, 11, 20);

			var previousBeneficiary = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			previousBeneficiary.GBB_GS_Staff = staff.PK;
			previousBeneficiary.GBB_GE_Department = previousDepartment.PK;
			previousBeneficiary.GBB_GB_Branch = previousBranch.PK;
			previousBeneficiary.GBB_EffectiveDate = new ZDateTimeOffset(2021, 11, 2);

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var departmentContent = excelInterface.WorkSheets[0][4, 11].ToString();
				AssertContains("G07", departmentContent);
				AssertNotContains("PG7", departmentContent);

				var branchContent = excelInterface.WorkSheets[0][4, 12].ToString();
				AssertContains("B07", branchContent);
				AssertNotContains("PB7", branchContent);

				var companyContent = excelInterface.WorkSheets[0][4, 14].ToString();
				AssertContains("James Bond Co.", companyContent);
				AssertNotContains("Goldfinger co.", companyContent);
			}
		}

		public void TestUsesCurrentEmployingEntity()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Sean Connery";
			staff.GS_DepartureDate = new ZDateTime(2021, 11, 20);

			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			currentCompany.GC_Name = "Goldfinger";

			var previousCompany = Factory.NewWithValidTestData<GlbCompany>();
			previousCompany.GC_Name = "Dr No.";

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "GE7";

			var previousDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			previousDepartment.GE_Code = "PEG";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "GB7";
			currentBranch.GB_GC = currentCompany.PK;

			var previousBranch = Factory.NewWithValidTestData<GlbBranch>();
			previousBranch.GB_Code = "PEB";
			previousBranch.GB_GC = previousCompany.PK;

			var currentEmployingEntity = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			currentEmployingEntity.GHB_GS_Staff = staff.PK;
			currentEmployingEntity.GHB_GE_Department = currentDepartment.PK;
			currentEmployingEntity.GHB_GB_Branch = currentBranch.PK;
			currentEmployingEntity.GHB_EffectiveDate = new ZDateTimeOffset(2021, 11, 25);

			var previousEmployingEntity = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			previousEmployingEntity.GHB_GS_Staff = staff.PK;
			previousEmployingEntity.GHB_GE_Department = previousDepartment.PK;
			previousEmployingEntity.GHB_GB_Branch = previousBranch.PK;
			previousEmployingEntity.GHB_EffectiveDate = new ZDateTimeOffset(2021, 11, 20);

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var departmentContent = excelInterface.WorkSheets[0][4, 15].ToString();
				AssertContains("GE7", departmentContent);
				AssertNotContains("PEG", departmentContent);

				var branchContent = excelInterface.WorkSheets[0][4, 17].ToString();
				AssertContains("GB7", branchContent);
				AssertNotContains("PEB", branchContent);

				var companyContent = excelInterface.WorkSheets[0][4, 20].ToString();
				AssertContains("Goldfinger", companyContent);
				AssertNotContains("Dr. No", companyContent);
			}
		}

		public void TestUseCurrentCostCentre()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";
			staff.GS_DepartureDate = new ZDateTime(2021, 11, 20);

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "MGM";

			var previousDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			previousDepartment.GE_Code = "PDD";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "BEL";
			currentBranch.GB_AccountingGroupCode = "ABC";

			var previousBranch = Factory.NewWithValidTestData<GlbBranch>();
			previousBranch.GB_Code = "PBB";
			previousBranch.GB_AccountingGroupCode = "PAG";

			var currentCostCentre = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			currentCostCentre.GSK_GS_Staff = staff.PK;
			currentCostCentre.GSK_GE_Department = currentDepartment.PK;
			currentCostCentre.GSK_GB_Branch = currentBranch.PK;
			currentCostCentre.GSK_StartDate = new ZDate(2021, 11, 25);

			var previousCostCentre = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			previousCostCentre.GSK_GS_Staff = staff.PK;
			previousCostCentre.GSK_GE_Department = previousDepartment.PK;
			previousCostCentre.GSK_GB_Branch = previousBranch.PK;
			previousCostCentre.GSK_StartDate = new ZDate(2021, 11, 20);

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var costCentreContent = excelInterface.WorkSheets[0][4, 21].ToString();
				AssertContains("MGM", costCentreContent);
				AssertNotContains("PDD", costCentreContent);

				var branchContent = excelInterface.WorkSheets[0][4, 22].ToString();
				AssertContains("ABC", branchContent);
				AssertNotContains("PAG", branchContent);
			}
		}

		public void TestMatchesCurrentTitleandJobFamily()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Xavier Camno";
			staff.GS_EmploymentDate = new ZDateTime(2021, 11, 2);

			var backend = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			backend.GEH_GS_Staff = staff.PK;
			backend.GEH_JobTitle = "Developer";
			backend.GEH_JobFamily = "DEV";
			backend.GEH_EffectiveDate = new ZDateTimeOffset(2021, 11, 2);

			var product = Factory.NewWithValidTestData<GlbEmploymentHistory>();
			product.GEH_GS_Staff = staff.PK;
			product.GEH_JobTitle = "Product";
			product.GEH_JobFamily = "PRD";
			product.GEH_EffectiveDate = new ZDateTimeOffset(2021, 11, 11);

			Factory.Save();

			((DateField)Report.FilterCollection["Report Date Start"]).Value = new ZDateTime(2021, 11, 1);
			((DateField)Report.FilterCollection["Report Date End"]).Value = new ZDateTime(2021, 11, 30);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var titleContent = excelInterface.WorkSheets[0][4, 7].ToString();
				AssertContains("Product", titleContent);
				AssertNotContains("Developer", titleContent);

				var familyContent = excelInterface.WorkSheets[0][4, 8].ToString();
				AssertContains("PRD", familyContent);
				AssertNotContains("DEV", familyContent);
			}
		}
	}
}
