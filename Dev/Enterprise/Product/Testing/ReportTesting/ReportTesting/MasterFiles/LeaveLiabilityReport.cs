using System;
using System.IO;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions.LeaveEngine;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.HRM.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Tasks.DbSecurityAdmin;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Leave Liability Report")]
	[UseSnapshotProtection(true)]
	public class LeaveLiabilityReportTemplateTest : TemplateTestCase
	{
		void SetReportFilters(ZGuid countryPK, Decimal oncost, ZDateTime accrualDate, ZDateTime priorAccrualDate, ZString leaveType, bool requireFinanceReport)
		{
			((LookupField)Report.FilterCollection["Country/Region"]).ZValue = countryPK;
			((NumberField)Report.FilterCollection["Oncost"]).Value = oncost;
			((DateField)Report.FilterCollection["Snapshot Accrual Date"]).Value = accrualDate;
			((DateField)Report.FilterCollection["Snapshot Prior Accrual Date"]).Value = priorAccrualDate;
			((CodeListMultipleChoice)Report.FilterCollection["Leave Type"]).Value = leaveType;
			((OptionGroup)Report.FilterCollection["Report Type"]).BindableBooleanItems[0].BoolValue = requireFinanceReport;
		}

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

		public void TestStaffBelongsToPolicyOfReportCountry()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_FullName = "Bob Belcher";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var ausANNLeave = Factory.NewWithValidTestData<HrlBenefit>();
			ausANNLeave.LPB_LLP_Policy = ausPolicy.PK;
			ausANNLeave.LPB_LeaveType = "ANN";

			var bobPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			bobPolicy.LLS_GS_Staff = bob.PK;
			bobPolicy.LLS_LLP_Policy = ausPolicy.PK;
			bobPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);

			var archer = Factory.NewWithValidTestData<GlbStaff>();
			archer.GS_Code = "ARC";
			archer.GS_FullName = "Archer West";

			var nzPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			nzPolicy.LLP_RN_NKCountry = "NZ";
			nzPolicy.LLP_Name = "NZ Leave";

			var nzANNLeave = Factory.NewWithValidTestData<HrlBenefit>();
			nzANNLeave.LPB_LLP_Policy = nzPolicy.PK;
			nzANNLeave.LPB_LeaveType = "ANN";

			var arcPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			arcPolicy.LLS_GS_Staff = archer.PK;
			arcPolicy.LLS_LLP_Policy = nzPolicy.PK;
			arcPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 5), new ZDate(2022, 5, 5), "ANN", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 3].ToString();
				AssertContains("Bob Belcher", sheetContent);
				AssertNotContains("Archer West", sheetContent);
			}
		}

		public void TestOnlyMatchAccrualStaffPolicyWithinDateRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_FullName = "Bob Belcher";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var nzPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			nzPolicy.LLP_RN_NKCountry = "AU";
			nzPolicy.LLP_Name = "NZ Leave";

			var bobPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			bobPolicy.LLS_GS_Staff = bob.PK;
			bobPolicy.LLS_LLP_Policy = ausPolicy.PK;
			bobPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);

			var bobPolicy2 = Factory.NewWithValidTestData<HrlStaffPolicy>();
			bobPolicy2.LLS_GS_Staff = bob.PK;
			bobPolicy2.LLS_LLP_Policy = nzPolicy.PK;
			bobPolicy2.LLS_EffectiveDate = new ZDateTimeOffset(2023, 6, 6);

			var ausRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			ausRun.LLT_GS_Staff = bob.PK;
			ausRun.LLT_LeaveType = "ANN";
			ausRun.LLT_Accrual = new ZDateTimeOffset(2023, 5, 1);
			ausRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var nzRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			nzRun.LLT_GS_Staff = bob.PK;
			nzRun.LLT_LeaveType = "ANN";
			nzRun.LLT_Accrual = new ZDateTimeOffset(2023, 7, 1);
			nzRun.LLT_TransactionType = BalanceTransactionTypes.FinanceReprocessing;

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 5), new ZDate(2022, 5, 5), "ANN", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 7].ToString();
				AssertContains("AUS Leave", sheetContent);
				AssertNotContains("NZ Leave", sheetContent);

				var countryContent = excelInterface.WorkSheets[0][4, 8].ToString();
				AssertContains("AU", countryContent);
				AssertNotContains("NZ", countryContent);
			}
		}

		public void TestUsesMostRecentBusinessUnitAtAccrualDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Sean Connery";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var staffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			currentCompany.GC_Name = "Goldfinger";
			currentCompany.GC_Code = "007";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "GB7";
			currentBranch.GB_GC = currentCompany.PK;

			var currentEmployingEntity = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			currentEmployingEntity.GHB_GS_Staff = staff.PK;
			currentEmployingEntity.GHB_GB_Branch = currentBranch.PK;
			currentEmployingEntity.GHB_EffectiveDate = new ZDateTimeOffset(2023, 5, 1);

			var previousCompany = Factory.NewWithValidTestData<GlbCompany>();
			previousCompany.GC_Name = "Dr No.";
			previousCompany.GC_Code = "DLZ";

			var previousBranch = Factory.NewWithValidTestData<GlbBranch>();
			previousBranch.GB_Code = "PEB";
			previousBranch.GB_GC = previousCompany.PK;

			var previousEmployingEntity = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			previousEmployingEntity.GHB_GS_Staff = staff.PK;
			previousEmployingEntity.GHB_GB_Branch = previousBranch.PK;
			previousEmployingEntity.GHB_EffectiveDate = new ZDateTimeOffset(2021, 11, 20);

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 5), new ZDate(2022, 5, 5), "ANN", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 4].ToString();
				AssertContains("007 - Goldfinger", sheetContent);
				AssertNotContains("DLZ - Dr. No", sheetContent);
			}
		}

		public void TestUseMostRecentCostCentre()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var staffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "11C";
			currentDepartment.GE_Desc = "Rate Management";

			var currentCostCentre = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			currentCostCentre.GSK_GS_Staff = staff.PK;
			currentCostCentre.GSK_GE_Department = currentDepartment.PK;
			currentCostCentre.GSK_StartDate = new ZDate(2023, 5, 1);

			var previousDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			previousDepartment.GE_Code = "10C";
			previousDepartment.GE_Desc = "Customer Service";

			var previousCostCentre = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			previousCostCentre.GSK_GS_Staff = staff.PK;
			previousCostCentre.GSK_GE_Department = previousDepartment.PK;
			previousCostCentre.GSK_StartDate = new ZDate(2022, 1, 1);

			Factory.Save();
			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 1), new ZDate(2022, 5, 5), "ANN", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 5].ToString();
				AssertContains("11C - Rate Management", sheetContent);
				AssertNotContains("10C - Customer Service", sheetContent);
			}
		}

		public void TestGetMostRecentAccrualDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var staffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var recentRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			recentRun.LLT_GS_Staff = staff.PK;
			recentRun.LLT_LeaveType = "ANN";
			recentRun.LLT_Accrual = new ZDateTimeOffset(2023, 5, 1);
			recentRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var priorRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			priorRun.LLT_GS_Staff = staff.PK;
			priorRun.LLT_LeaveType = "ANN";
			priorRun.LLT_Accrual = new ZDateTimeOffset(2022, 5, 1);
			priorRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var oldRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			oldRun.LLT_GS_Staff = staff.PK;
			oldRun.LLT_LeaveType = "ANN";
			oldRun.LLT_Accrual = new ZDateTimeOffset(2021, 5, 1);
			oldRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 1), new ZDate(2022, 5, 5), "ANN", false);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 6].ToString();
				AssertContains("5/1/2023 12:00:00 AM +10:00", sheetContent);
				AssertNotContains("5/1/2022 12:00:00 AM +10:00", sheetContent);
				AssertNotContains("5/1/2021 12:00:00 AM +10:00", sheetContent);
			}
		}

		public void TestStaffRemPackageMatchesAccrualDate()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var ausPolicy = factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var staffPolicy = factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var prevRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			prevRem.GSR_GS_Staff = staff.PK;
			prevRem.GSR_FullTimeEquivalent = 1.00000000;
			prevRem.GSR_RN_NKCountry = "AU";
			prevRem.GSR_RX_NKCurrency = "EUR";
			prevRem.GSR_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);
			prevRem.GSR_LeaveLiabilityHourlyRate = 20.50;

			var accrualRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			accrualRem.GSR_GS_Staff = staff.PK;
			accrualRem.GSR_FullTimeEquivalent = 1.00000000;
			accrualRem.GSR_RN_NKCountry = "AU";
			accrualRem.GSR_RX_NKCurrency = "AUD";
			accrualRem.GSR_EffectiveDate = new ZDateTimeOffset(2023, 5, 1);
			accrualRem.GSR_LeaveLiabilityHourlyRate = 25.50;

			var currentRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			currentRem.GSR_GS_Staff = staff.PK;
			currentRem.GSR_FullTimeEquivalent = 1.00000000;
			currentRem.GSR_RN_NKCountry = "AU";
			currentRem.GSR_RX_NKCurrency = "AUD";
			currentRem.GSR_EffectiveDate = new ZDateTimeOffset(2023, 5, 20);
			currentRem.GSR_LeaveLiabilityHourlyRate = 30.50;

			var recentRun = factory.NewWithValidTestData<HrlBalanceTransaction>();
			recentRun.LLT_GS_Staff = staff.PK;
			recentRun.LLT_LeaveType = "ANN";
			recentRun.LLT_Accrual = new ZDateTimeOffset(2023, 5, 1);
			recentRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var priorRun = factory.NewWithValidTestData<HrlBalanceTransaction>();
			priorRun.LLT_GS_Staff = staff.PK;
			priorRun.LLT_LeaveType = "ANN";
			priorRun.LLT_Accrual = new ZDateTimeOffset(2022, 5, 1);
			priorRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 1), new ZDate(2022, 5, 5), "ANN", false);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var rateSheetContent = excelInterface.WorkSheets[0][4, 10].ToString();
				AssertContains("25.5", rateSheetContent);
				AssertNotContains("20.5", rateSheetContent);
				AssertNotContains("30.5", rateSheetContent);

				var currSheetContent = excelInterface.WorkSheets[0][4, 9].ToString();
				AssertContains("AUD", currSheetContent);
				AssertNotContains("EUR", currSheetContent);
			}
		}

		public void TestProcessBalanceCalculatedForReportLeaveType()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var staffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var balanceANN = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceANN.LLT_GS_Staff = staff.PK;
			balanceANN.LLT_LeaveType = "ANN";
			balanceANN.LLT_Accrual = new ZDateTimeOffset(2023, 5, 5);
			balanceANN.LLT_DeltaValueHours = 20;
			balanceANN.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var balanceANNPrior = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceANNPrior.LLT_GS_Staff = staff.PK;
			balanceANNPrior.LLT_LeaveType = "SIC";
			balanceANNPrior.LLT_Accrual = new ZDateTimeOffset(2022, 5, 5);
			balanceANNPrior.LLT_DeltaValueHours = 20;
			balanceANNPrior.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var balanceSIC = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceSIC.LLT_GS_Staff = staff.PK;
			balanceSIC.LLT_LeaveType = "ANN";
			balanceSIC.LLT_Accrual = new ZDateTimeOffset(2023, 5, 5);
			balanceSIC.LLT_DeltaValueHours = 30;
			balanceSIC.LLT_TransactionType = BalanceTransactionTypes.FinanceReprocessing;

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 6, 5), new ZDate(2022, 6, 5), "ANN", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 11].ToString();
				AssertContains("30", sheetContent);
				AssertNotContains("20", sheetContent);
				AssertNotContains("40", sheetContent);
			}
		}

		public void TestGetPriorAccrualDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var ausANNLeave = Factory.NewWithValidTestData<HrlBenefit>();
			ausANNLeave.LPB_LLP_Policy = ausPolicy.PK;
			ausANNLeave.LPB_LeaveType = "ANN";

			var staffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var recentRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			recentRun.LLT_GS_Staff = staff.PK;
			recentRun.LLT_LeaveType = "ANN";
			recentRun.LLT_Accrual = new ZDateTimeOffset(2023, 5, 1);
			recentRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var priorRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			priorRun.LLT_GS_Staff = staff.PK;
			priorRun.LLT_LeaveType = "ANN";
			priorRun.LLT_Accrual = new ZDateTimeOffset(2022, 5, 1);
			priorRun.LLT_TransactionType = BalanceTransactionTypes.FinanceReprocessing;

			var oldRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			oldRun.LLT_GS_Staff = staff.PK;
			oldRun.LLT_LeaveType = "ANN";
			oldRun.LLT_Accrual = new ZDateTimeOffset(2021, 5, 1);
			oldRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 2), new ZDate(2022, 5, 2), "ANN", false);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 17].ToString();
				AssertContains("5/1/2021 12:00:00 AM +10:00", sheetContent);
				AssertNotContains("5/1/2022 12:00:00 AM +10:00", sheetContent);
				AssertNotContains("5/1/2023 12:00:00 AM +10:00", sheetContent);
			}
		}

		public void TestUsesMostRecentBusinessUnitAtPriorAccrualDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Sean Connery";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var ausANNLeave = Factory.NewWithValidTestData<HrlBenefit>();
			ausANNLeave.LPB_LLP_Policy = ausPolicy.PK;
			ausANNLeave.LPB_LeaveType = "ANN";

			var staffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			currentCompany.GC_Name = "Goldfinger";
			currentCompany.GC_Code = "007";

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_Code = "GB7";
			currentBranch.GB_GC = currentCompany.PK;

			var currentEmployingEntity = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			currentEmployingEntity.GHB_GS_Staff = staff.PK;
			currentEmployingEntity.GHB_GB_Branch = currentBranch.PK;
			currentEmployingEntity.GHB_EffectiveDate = new ZDateTimeOffset(2023, 5, 1);

			var recentRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			recentRun.LLT_GS_Staff = staff.PK;
			recentRun.LLT_LeaveType = "ANN";
			recentRun.LLT_Accrual = new ZDateTimeOffset(2023, 5, 1);
			recentRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var previousCompany = Factory.NewWithValidTestData<GlbCompany>();
			previousCompany.GC_Name = "Dr No";
			previousCompany.GC_Code = "DLZ";

			var previousBranch = Factory.NewWithValidTestData<GlbBranch>();
			previousBranch.GB_Code = "PEB";
			previousBranch.GB_GC = previousCompany.PK;

			var previousEmployingEntity = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			previousEmployingEntity.GHB_GS_Staff = staff.PK;
			previousEmployingEntity.GHB_GB_Branch = previousBranch.PK;
			previousEmployingEntity.GHB_EffectiveDate = new ZDateTimeOffset(2021, 11, 20);

			var priorRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			priorRun.LLT_GS_Staff = staff.PK;
			priorRun.LLT_LeaveType = "ANN";
			priorRun.LLT_Accrual = new ZDateTimeOffset(2022, 1, 1);
			priorRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 1), new ZDate(2022, 5, 5), "ANN", false);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 15].ToString();
				AssertContains("DLZ - Dr No", sheetContent);
				AssertNotContains("007 - Goldfinger", sheetContent);
			}
		}

		public void TestUsePriorCostCentre()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var staffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			currentDepartment.GE_Code = "11C";
			currentDepartment.GE_Desc = "Rate Management";

			var currentCostCentre = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			currentCostCentre.GSK_GS_Staff = staff.PK;
			currentCostCentre.GSK_GE_Department = currentDepartment.PK;
			currentCostCentre.GSK_StartDate = new ZDate(2023, 5, 1);

			var recentRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			recentRun.LLT_GS_Staff = staff.PK;
			recentRun.LLT_LeaveType = "ANN";
			recentRun.LLT_Accrual = new ZDateTimeOffset(2023, 5, 1);
			recentRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var previousDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			previousDepartment.GE_Code = "10C";
			previousDepartment.GE_Desc = "Customer Service";

			var previousCostCentre = Factory.NewWithValidTestData<GlbStaffCostCentre>();
			previousCostCentre.GSK_GS_Staff = staff.PK;
			previousCostCentre.GSK_GE_Department = previousDepartment.PK;
			previousCostCentre.GSK_StartDate = new ZDate(2022, 1, 1);

			var priorRun = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			priorRun.LLT_GS_Staff = staff.PK;
			priorRun.LLT_LeaveType = "ANN";
			priorRun.LLT_Accrual = new ZDateTimeOffset(2022, 1, 1);
			priorRun.LLT_TransactionType = BalanceTransactionTypes.Processing;

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 1), new ZDate(2022, 5, 5), "ANN", false);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 16].ToString();
				AssertNotContains("11C - Rate Management", sheetContent);
				AssertContains("10C - Customer Service", sheetContent);
			}
		}

		public void TestStaffRemPackageMatchesPriorAccrualDate()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var ausPolicy = factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var staffPolicy = factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var prevRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			prevRem.GSR_GS_Staff = staff.PK;
			prevRem.GSR_FullTimeEquivalent = 1.00000000;
			prevRem.GSR_RN_NKCountry = "AU";
			prevRem.GSR_RX_NKCurrency = "EUR";
			prevRem.GSR_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);
			prevRem.GSR_LeaveLiabilityHourlyRate = 20.50;

			var accrualRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			accrualRem.GSR_GS_Staff = staff.PK;
			accrualRem.GSR_FullTimeEquivalent = 1.00000000;
			accrualRem.GSR_RN_NKCountry = "AU";
			accrualRem.GSR_RX_NKCurrency = "AUD";
			accrualRem.GSR_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);
			accrualRem.GSR_LeaveLiabilityHourlyRate = 25.50;

			var currentRem = factory.NewWithValidTestData<GlbStaffRemuneration>();
			currentRem.GSR_GS_Staff = staff.PK;
			currentRem.GSR_FullTimeEquivalent = 1.00000000;
			currentRem.GSR_RN_NKCountry = "AU";
			currentRem.GSR_RX_NKCurrency = "AUD";
			currentRem.GSR_EffectiveDate = new ZDateTimeOffset(2023, 5, 20);
			currentRem.GSR_LeaveLiabilityHourlyRate = 30.50;

			var recentRun = factory.NewWithValidTestData<HrlBalanceTransaction>();
			recentRun.LLT_GS_Staff = staff.PK;
			recentRun.LLT_LeaveType = "ANN";
			recentRun.LLT_Accrual = new ZDateTimeOffset(2023, 1, 1);
			recentRun.LLT_TransactionType = BalanceTransactionTypes.FinanceProcessing;

			var priorRun = factory.NewWithValidTestData<HrlBalanceTransaction>();
			priorRun.LLT_GS_Staff = staff.PK;
			priorRun.LLT_LeaveType = "ANN";
			priorRun.LLT_Accrual = new ZDateTimeOffset(2022, 1, 1);
			priorRun.LLT_TransactionType = BalanceTransactionTypes.FinanceReprocessing;

			factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 5, 5), new ZDate(2022, 5, 5), "ANN", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var rateSheetContent = excelInterface.WorkSheets[0][4, 21].ToString();
				AssertContains("20.5", rateSheetContent);
				AssertNotContains("25.5", rateSheetContent);
				AssertNotContains("30.5", rateSheetContent);

				var currSheetContent = excelInterface.WorkSheets[0][4, 20].ToString();
				AssertContains("EUR", currSheetContent);
				AssertNotContains("AUD", currSheetContent);
			}
		}

		public void TestPreviousBalanceCalculatedForReportLeaveType()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var staffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			staffPolicy.LLS_GS_Staff = staff.PK;
			staffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			staffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 1, 1);

			var balanceANN = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceANN.LLT_GS_Staff = staff.PK;
			balanceANN.LLT_LeaveType = "ANN";
			balanceANN.LLT_Accrual = new ZDateTimeOffset(2023, 5, 5);
			balanceANN.LLT_DeltaValueHours = 40;
			balanceANN.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var balanceANNPrior = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceANNPrior.LLT_GS_Staff = staff.PK;
			balanceANNPrior.LLT_LeaveType = "ANN";
			balanceANNPrior.LLT_Accrual = new ZDateTimeOffset(2022, 5, 5);
			balanceANNPrior.LLT_DeltaValueHours = 30;
			balanceANNPrior.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var balanceSIC = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceSIC.LLT_GS_Staff = staff.PK;
			balanceSIC.LLT_LeaveType = "SIC";
			balanceSIC.LLT_Accrual = new ZDateTimeOffset(2022, 5, 5);
			balanceSIC.LLT_DeltaValueHours = 50;
			balanceSIC.LLT_TransactionType = BalanceTransactionTypes.Processing;

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 6, 5), new ZDate(2022, 5, 5), "ANN", false);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0][4, 22].ToString();
				AssertContains("30", sheetContent);
				AssertNotContains("60", sheetContent);
			}
		}

		public void TestGetPriorStaffPolicyWithinDateRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Danny Ocean";

			var ausPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			ausPolicy.LLP_RN_NKCountry = "AU";
			ausPolicy.LLP_Name = "AUS Leave";

			var ausStaffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			ausStaffPolicy.LLS_GS_Staff = staff.PK;
			ausStaffPolicy.LLS_LLP_Policy = ausPolicy.PK;
			ausStaffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2023, 1, 1);

			var nzPolicy = Factory.NewWithValidTestData<HrlPolicy>();
			nzPolicy.LLP_RN_NKCountry = "NZ";
			nzPolicy.LLP_Name = "NZ Leave";

			var priorStaffPolicy = Factory.NewWithValidTestData<HrlStaffPolicy>();
			priorStaffPolicy.LLS_GS_Staff = staff.PK;
			priorStaffPolicy.LLS_LLP_Policy = nzPolicy.PK;
			priorStaffPolicy.LLS_EffectiveDate = new ZDateTimeOffset(2022, 3, 1);

			var balanceANN = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceANN.LLT_GS_Staff = staff.PK;
			balanceANN.LLT_LeaveType = "ANN";
			balanceANN.LLT_Accrual = new ZDateTimeOffset(2023, 5, 5);
			balanceANN.LLT_DeltaValueHours = 40;
			balanceANN.LLT_TransactionType = BalanceTransactionTypes.Processing;

			var balanceANNPrior = Factory.NewWithValidTestData<HrlBalanceTransaction>();
			balanceANNPrior.LLT_GS_Staff = staff.PK;
			balanceANNPrior.LLT_LeaveType = "ANN";
			balanceANNPrior.LLT_Accrual = new ZDateTimeOffset(2022, 5, 5);
			balanceANNPrior.LLT_DeltaValueHours = 30;
			balanceANNPrior.LLT_TransactionType = BalanceTransactionTypes.Reprocessing;

			Factory.Save();

			SetReportFilters(country.PK, 15m, new ZDate(2023, 6, 5), new ZDate(2022, 5, 5), "ANN", false);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var nameContent = excelInterface.WorkSheets[0][4, 18].ToString();
				AssertContains("NZ Leave", nameContent);
				AssertNotContains("AUS Leave", nameContent);

				var countryContent = excelInterface.WorkSheets[0][4, 19].ToString();
				AssertContains("NZ", countryContent);
				AssertNotContains("AUS", countryContent);
			}
		}
	}
}
