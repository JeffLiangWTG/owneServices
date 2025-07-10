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
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Cost Centers Report")]
	[UseSnapshotProtection(skipTransaction: true)]
	public class CostCentersReportReportTemplateTest : TemplateTestCase
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

		public struct CreateRecordParameters
		{
			public bool expectedIsActive { get; set; }
			public string expectedStaffCode { get; set; }
			public string expectedStaffFullName { get; set; }
			public ZDate expectedEmploymentDate { get; set; }
			public string expectedCurrentJobTitle { get; set; }
			public string expectedCurrentTeam { get; set; }
			public string expectedCurrentWorkingBasis { get; set; }
			public ZDate expectedDepartureDate { get; set; }
			public ZDate expectedStartDateCostCenter { get; set; }
			public ZDate expectedEndDateCostCenter { get; set; }
			public string expectedCostCenterCode { get; set; }
			public string expectedCostCenterName { get; set; }
			public string expectedBranchCostCenter { get; set; }
			public string expectedIsCurrentCostCenter { get; set; }
			public ZDateTimeOffset expectedPackageEffectiveDateCurrentRemuneration { get; set; }
			public ZDecimal expectedFTECurrentRemuneration { get; set; }
			public int expectedValueMostRecentSalary { get; set; }
			public int expectedValueMostRecentWaysofWorkingAllowance { get; set; }
			public string expectedCountryCurrentRemuneration { get; set; }
			public string expectedCountryCodeResidence { get; set; }
			public string expectedCountryNameResidence { get; set; }
			public string expectedCurrencyCurrentRemuneration { get; set; }
			public string expectedFrequencyMostRecentSalary { get; set; }
			public string expectedFrequencyMostRecentWaysofWorkingAllowance { get; set; }
			public string expectedCompanyCurrentBeneficiaryEntity { get; set; }
			public string expectedBranchCurrentBeneficiaryEntity { get; set; }
			public string expectedDepartmentCurrentBeneficiaryEntity { get; set; }
			public string expectedCompanyCurrentEmployingEntity { get; set; }
			public string expectedBranchCurrentEmployingEntity { get; set; }
			public string expectedDepartmentCurrentEmployingEntity { get; set; }
			public ZDateTimeOffset beneficiaryBranchDepartmentEffectiveDate { get; set; }
			public ZDateTimeOffset homeBranchDepartmentEffectiveDate { get; set; }
			public ZDateTimeOffset employmentHistoryEffectiveDate { get; set; }
			public ZDateTime expectedEffectiveDateMostRecentPeopleLeader { get; set; }
			public string expectedManagerMostRecentPeopleLeader { get; set; }
		}

		public void createRecord(BusinessObjectFactory factory,
						 CreateRecordParameters createRecordParameters)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = createRecordParameters.expectedIsActive;
			staff.GS_Code = createRecordParameters.expectedStaffCode;
			staff.GS_FullName = createRecordParameters.expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = createRecordParameters.expectedStartDateCostCenter;
			costCenter.GSK_EndDate = createRecordParameters.expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = createRecordParameters.expectedCostCenterCode;
			department1.GE_Desc = createRecordParameters.expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = createRecordParameters.expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = branch1.PK;

			var staffremuneration = factory.NewWithValidTestData<GlbStaffRemuneration>();
			staffremuneration.GSR_GS_Staff = staff.PK;
			staffremuneration.GSR_EffectiveDate = createRecordParameters.expectedPackageEffectiveDateCurrentRemuneration;
			staffremuneration.GSR_FullTimeEquivalent = createRecordParameters.expectedFTECurrentRemuneration;

			var staffentitlementbas = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementbas.GSI_EntitlementCode = "BAS";
			staffentitlementbas.GSI_GSR_Remuneration = staffremuneration.PK;
			staffentitlementbas.GSI_Value = createRecordParameters.expectedValueMostRecentSalary;
			staffentitlementbas.GSI_Frequency = createRecordParameters.expectedFrequencyMostRecentSalary;

			var staffentitlementwow = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementwow.GSI_EntitlementCode = "WOW";
			staffentitlementwow.GSI_GSR_Remuneration = staffremuneration.PK;
			staffentitlementwow.GSI_Value = createRecordParameters.expectedValueMostRecentWaysofWorkingAllowance;
			staffentitlementwow.GSI_Frequency = createRecordParameters.expectedFrequencyMostRecentWaysofWorkingAllowance;

			var beneficiarybranchdepartment = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiarybranchdepartment.GBB_EffectiveDate = createRecordParameters.beneficiaryBranchDepartmentEffectiveDate;
			beneficiarybranchdepartment.GBB_GS_Staff = staff.PK;

			var branch2 = factory.NewWithValidTestData<GlbBranch>();
			beneficiarybranchdepartment.GBB_GB_Branch = branch2.PK;
			branch2.GB_BranchName = createRecordParameters.expectedBranchCurrentBeneficiaryEntity;

			var company1 = factory.NewWithValidTestData<GlbCompany>();
			branch2.GB_GC = company1.PK;
			company1.GC_Name = createRecordParameters.expectedCompanyCurrentBeneficiaryEntity;

			var department2 = factory.NewWithValidTestData<GlbDepartment>();
			beneficiarybranchdepartment.GBB_GE_Department = department2.PK;
			department2.GE_Desc = createRecordParameters.expectedDepartmentCurrentBeneficiaryEntity;

			var homeBranchdepartment = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			homeBranchdepartment.GHB_EffectiveDate = createRecordParameters.homeBranchDepartmentEffectiveDate;
			homeBranchdepartment.GHB_GS_Staff = staff.PK;

			var branch3 = factory.NewWithValidTestData<GlbBranch>();
			homeBranchdepartment.GHB_GB_Branch = branch3.PK;
			branch3.GB_BranchName = createRecordParameters.expectedBranchCurrentEmployingEntity;

			var company2 = factory.NewWithValidTestData<GlbCompany>();
			branch3.GB_GC = company2.PK;
			company2.GC_Name = createRecordParameters.expectedCompanyCurrentEmployingEntity;

			var department3 = factory.NewWithValidTestData<GlbDepartment>();
			homeBranchdepartment.GHB_GE_Department = department3.PK;
			department3.GE_Desc = createRecordParameters.expectedDepartmentCurrentEmployingEntity;

			staffremuneration.GSR_RN_NKCountry = createRecordParameters.expectedCountryCurrentRemuneration;
			staff.GS_RN_NKCountryCode = createRecordParameters.expectedCountryCodeResidence;

			var refCountry = factory.NewWithValidTestData<RefCountry>();
			refCountry.RN_Code = staff.GS_RN_NKCountryCode;
			refCountry.RN_Desc = createRecordParameters.expectedCountryNameResidence;

			staffremuneration.GSR_RX_NKCurrency = createRecordParameters.expectedCurrencyCurrentRemuneration;

			staff.GS_EmploymentDate = createRecordParameters.expectedEmploymentDate;

			var employmentHistory = factory.NewWithValidTestData<GlbEmploymentHistory>();
			employmentHistory.GEH_EffectiveDate = createRecordParameters.employmentHistoryEffectiveDate;
			employmentHistory.GEH_GS_Staff = staff.PK;
			employmentHistory.GEH_JobTitle = createRecordParameters.expectedCurrentJobTitle;

			var employmentTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			employmentTeam.GET_GS_Staff = staff.PK;
			employmentTeam.GET_GST_NKTeamCode = createRecordParameters.expectedCurrentTeam;

			staff.GS_EmploymentBasis = createRecordParameters.expectedCurrentWorkingBasis;
			staff.GS_DepartureDate = createRecordParameters.expectedDepartureDate;

			var staffManager = factory.NewWithValidTestData<GlbStaffManager>();
			staffManager.GSM_GS_Staff = staff.PK;
			staffManager.GSM_EffectiveDate = createRecordParameters.expectedEffectiveDateMostRecentPeopleLeader;

			var manager = factory.NewWithValidTestData<GlbStaff>();
			staffManager.GSM_GS_Manager = manager.PK;
			manager.GS_FullName = createRecordParameters.expectedManagerMostRecentPeopleLeader;
		}

		public struct RecordCheckParameters
		{
			public bool expectedIsActive;
			public string expectedStaffCode;
			public string expectedStaffFullName;
			public string expectedStartDateCostCenter;
			public string expectedEndDateCostCenter;
			public string expectedIsCurrentCostCenter;
			public string expectedCostCenterCode;
			public string expectedCostCenterName;
			public string expectedBranchCostCenter;
			public string expectedPackageEffectiveDateCurrentRemuneration;
			public string expectedFTECurrentRemuneration;
			public string expectedValueMostRecentSalary;
			public string expectedValueMostRecentWaysofWorkingAllowance;
			public string expectedCompanyCurrentBeneficiaryEntity;
			public string expectedBranchCurrentBeneficiaryEntity;
			public string expectedDepartmentCurrentBeneficiaryEntity;
			public string expectedCompanyCurrentEmployingEntity;
			public string expectedBranchCurrentEmployingEntity;
			public string expectedDepartmentCurrentEmployingEntity;
			public string expectedCountryCurrentRemuneration;
			public string expectedCountryCodeResidence;
			public string expectedCountryNameResidence;
			public string expectedCurrencyCurrentRemuneration;
			public string expectedFrequencyMostRecentSalary;
			public string expectedFrequencyMostRecentWaysofWorkingAllowance;
			public string expectedEmploymentDate;
			public string expectedCurrentJobTitle;
			public string expectedCurrentTeam;
			public string expectedCurrentWorkingBasis;
			public string expectedDepartureDate;
			public string expectedEffectiveDateMostRecentPeopleLeader;
			public string expectedManagerMostRecentPeopleLeader;
		}

		public void CheckRecord(ExcelWorkSheet sheetContent,
								int row,
								RecordCheckParameters recordCheckParameters)
		{
			AssertEquals(recordCheckParameters.expectedIsActive ? "Y" : "N", sheetContent[row, 2].ToString());
			AssertEquals(recordCheckParameters.expectedStaffCode, sheetContent[row, 3].ToString());
			AssertEquals(recordCheckParameters.expectedStaffFullName, sheetContent[row, 4].ToString());
			AssertEquals(recordCheckParameters.expectedStartDateCostCenter, sheetContent[row, 5].ToString());
			AssertEquals(recordCheckParameters.expectedEndDateCostCenter, sheetContent[row, 6].ToString());
			AssertEquals(recordCheckParameters.expectedIsCurrentCostCenter, sheetContent[row, 7].ToString());
			AssertEquals(recordCheckParameters.expectedCostCenterCode, sheetContent[row, 8].ToString());
			AssertEquals(recordCheckParameters.expectedCostCenterName, sheetContent[row, 9].ToString());
			AssertEquals(recordCheckParameters.expectedBranchCostCenter, sheetContent[row, 10].ToString());
			AssertEquals(recordCheckParameters.expectedPackageEffectiveDateCurrentRemuneration, sheetContent[row, 11].ToString());
			AssertEquals(recordCheckParameters.expectedFTECurrentRemuneration, sheetContent[row, 12].ToString());
			AssertEquals(recordCheckParameters.expectedValueMostRecentSalary, sheetContent[row, 13].ToString());
			AssertEquals(recordCheckParameters.expectedValueMostRecentWaysofWorkingAllowance, sheetContent[row, 14].ToString());
			AssertEquals(recordCheckParameters.expectedCompanyCurrentBeneficiaryEntity, sheetContent[row, 15].ToString());
			AssertEquals(recordCheckParameters.expectedBranchCurrentBeneficiaryEntity, sheetContent[row, 16].ToString());
			AssertEquals(recordCheckParameters.expectedDepartmentCurrentBeneficiaryEntity, sheetContent[row, 17].ToString());
			AssertEquals(recordCheckParameters.expectedCompanyCurrentEmployingEntity, sheetContent[row, 18].ToString());
			AssertEquals(recordCheckParameters.expectedBranchCurrentEmployingEntity, sheetContent[row, 19].ToString());
			AssertEquals(recordCheckParameters.expectedDepartmentCurrentEmployingEntity, sheetContent[row, 20].ToString());
			AssertEquals(recordCheckParameters.expectedCountryCurrentRemuneration, sheetContent[row, 21].ToString());
			AssertEquals(recordCheckParameters.expectedCountryCodeResidence, sheetContent[row, 22].ToString());
			AssertEquals(recordCheckParameters.expectedCountryNameResidence, sheetContent[row, 23].ToString());
			AssertEquals(recordCheckParameters.expectedCurrencyCurrentRemuneration, sheetContent[row, 24].ToString());
			AssertEquals(recordCheckParameters.expectedFrequencyMostRecentSalary, sheetContent[row, 25].ToString());
			AssertEquals(recordCheckParameters.expectedFrequencyMostRecentWaysofWorkingAllowance, sheetContent[row, 26].ToString());
			AssertEquals(recordCheckParameters.expectedEmploymentDate, sheetContent[row, 27].ToString());
			AssertEquals(recordCheckParameters.expectedCurrentJobTitle, sheetContent[row, 28].ToString());
			AssertEquals(recordCheckParameters.expectedCurrentTeam, sheetContent[row, 29].ToString());
			AssertEquals(recordCheckParameters.expectedCurrentWorkingBasis, sheetContent[row, 30].ToString());
			AssertEquals(recordCheckParameters.expectedDepartureDate, sheetContent[row, 31].ToString());
			AssertEquals(recordCheckParameters.expectedEffectiveDateMostRecentPeopleLeader, sheetContent[row, 32].ToString());
			AssertEquals(recordCheckParameters.expectedManagerMostRecentPeopleLeader, sheetContent[row, 33].ToString());
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_AllValues()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			CreateRecordParameters createRecordParameters = new CreateRecordParameters
			{
				expectedIsActive = true,
				expectedStaffCode = "SC1",
				expectedStaffFullName = "Staff 1 Name",
				expectedEmploymentDate = (ZDate)ZDateTime.Today.AddDays(-11),
				expectedCurrentJobTitle = "Job Title 1",
				expectedCurrentTeam = "TN1",
				expectedCurrentWorkingBasis = "EB1",
				expectedDepartureDate = (ZDate)ZDateTime.Today.AddDays(20),
				expectedStartDateCostCenter = ZDate.Today.AddDays(-10),
				expectedEndDateCostCenter = ZDate.Today.AddDays(3),
				expectedCostCenterCode = "DP1",
				expectedCostCenterName = "Department 1",
				expectedBranchCostCenter = "Branch 1",
				expectedIsCurrentCostCenter = "Yes",
				expectedPackageEffectiveDateCurrentRemuneration = ZDateTimeOffset.Today.AddDays(-9),
				expectedFTECurrentRemuneration = new ZDecimal(1.1),
				expectedValueMostRecentSalary = 9,
				expectedValueMostRecentWaysofWorkingAllowance = 8,
				expectedCountryCurrentRemuneration = "C1",
				expectedCountryCodeResidence = "C2",
				expectedCountryNameResidence = "Country Desc",
				expectedCurrencyCurrentRemuneration = "AU1",
				expectedFrequencyMostRecentSalary = "YRL",
				expectedFrequencyMostRecentWaysofWorkingAllowance = "YRL",
				expectedCompanyCurrentBeneficiaryEntity = "Company 1 Name",
				expectedBranchCurrentBeneficiaryEntity = "Branch 2 Name",
				expectedDepartmentCurrentBeneficiaryEntity = "Department 2 Desc",
				expectedCompanyCurrentEmployingEntity = "Company 2 Name",
				expectedBranchCurrentEmployingEntity = "Branch 3 Name",
				expectedDepartmentCurrentEmployingEntity = "Department 3 Desc",
				beneficiaryBranchDepartmentEffectiveDate = ZDateTimeOffset.Today.AddDays(-10),
				homeBranchDepartmentEffectiveDate = ZDateTimeOffset.Today.AddDays(-10),
				employmentHistoryEffectiveDate = ZDateTimeOffset.Today.AddDays(-10),
				expectedEffectiveDateMostRecentPeopleLeader = ZDateTime.Today.AddDays(-5),
				expectedManagerMostRecentPeopleLeader = "Manager 1 Name"
			};

			createRecord(factory,
				 createRecordParameters);

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = createRecordParameters.expectedIsActive,
					expectedStaffCode = createRecordParameters.expectedStaffCode,
					expectedStaffFullName = createRecordParameters.expectedStaffFullName,
					expectedStartDateCostCenter = createRecordParameters.expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = createRecordParameters.expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = createRecordParameters.expectedIsCurrentCostCenter,
					expectedCostCenterCode = createRecordParameters.expectedCostCenterCode,
					expectedCostCenterName = createRecordParameters.expectedCostCenterName,
					expectedBranchCostCenter = createRecordParameters.expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = createRecordParameters.expectedPackageEffectiveDateCurrentRemuneration.ToString("yyyy-MM-dd"),
					expectedFTECurrentRemuneration = string.Format("{0:0.##}", createRecordParameters.expectedFTECurrentRemuneration),
					expectedValueMostRecentSalary = createRecordParameters.expectedValueMostRecentSalary.ToString(),
					expectedValueMostRecentWaysofWorkingAllowance = createRecordParameters.expectedValueMostRecentWaysofWorkingAllowance.ToString(),
					expectedCompanyCurrentBeneficiaryEntity = createRecordParameters.expectedCompanyCurrentBeneficiaryEntity,
					expectedBranchCurrentBeneficiaryEntity = createRecordParameters.expectedBranchCurrentBeneficiaryEntity,
					expectedDepartmentCurrentBeneficiaryEntity = createRecordParameters.expectedDepartmentCurrentBeneficiaryEntity,
					expectedCompanyCurrentEmployingEntity = createRecordParameters.expectedCompanyCurrentEmployingEntity,
					expectedBranchCurrentEmployingEntity = createRecordParameters.expectedBranchCurrentEmployingEntity,
					expectedDepartmentCurrentEmployingEntity = createRecordParameters.expectedDepartmentCurrentEmployingEntity,
					expectedCountryCurrentRemuneration = createRecordParameters.expectedCountryCurrentRemuneration,
					expectedCountryCodeResidence = createRecordParameters.expectedCountryCodeResidence,
					expectedCountryNameResidence = createRecordParameters.expectedCountryNameResidence,
					expectedCurrencyCurrentRemuneration = createRecordParameters.expectedCurrencyCurrentRemuneration,
					expectedFrequencyMostRecentSalary = createRecordParameters.expectedFrequencyMostRecentSalary,
					expectedFrequencyMostRecentWaysofWorkingAllowance = createRecordParameters.expectedFrequencyMostRecentWaysofWorkingAllowance,
					expectedEmploymentDate = createRecordParameters.expectedEmploymentDate.ToString("yyyy-MM-dd"),
					expectedCurrentJobTitle = createRecordParameters.expectedCurrentJobTitle,
					expectedCurrentTeam = createRecordParameters.expectedCurrentTeam,
					expectedCurrentWorkingBasis = createRecordParameters.expectedCurrentWorkingBasis,
					expectedDepartureDate = createRecordParameters.expectedDepartureDate.ToString("yyyy-MM-dd"),
					expectedEffectiveDateMostRecentPeopleLeader = createRecordParameters.expectedEffectiveDateMostRecentPeopleLeader.ToString("yyyy-MM-dd"),
					expectedManagerMostRecentPeopleLeader = createRecordParameters.expectedManagerMostRecentPeopleLeader
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_MultipleRemuneration()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(3);
			var expectedIsCurrentCostCenter = "Yes";
			var expectedCostCenterCode = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedBranchCostCenter = "Branch 1";
			var expectedPackageEffectiveDateCurrentRemuneration = ZDateTimeOffset.Today.AddDays(-9);
			var expectedFTECurrentRemuneration = new ZDecimal(1.1);
			var expectedValueMostRecentSalary = 9;
			var expectedValueMostRecentWaysofWorkingAllowance = 8;
			var expectedFrequencyMostRecentSalary = "YRL";
			var expectedFrequencyMostRecentWaysofWorkingAllowance = "YRL";
			var expectedCountryCurrentRemuneration = "C1";
			var expectedCurrencyCurrentRemuneration = "AU1";
			var expectedCountryCodeResidence = "AB";

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = expectedCostCenterCode;
			department1.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = branch1.PK;

			var staffremuneration = factory.NewWithValidTestData<GlbStaffRemuneration>();
			staffremuneration.GSR_GS_Staff = staff.PK;
			staffremuneration.GSR_EffectiveDate = expectedPackageEffectiveDateCurrentRemuneration;
			staffremuneration.GSR_FullTimeEquivalent = expectedFTECurrentRemuneration;
			staffremuneration.GSR_RN_NKCountry = expectedCountryCurrentRemuneration;
			staffremuneration.GSR_RX_NKCurrency = expectedCurrencyCurrentRemuneration;

			var staffremuneration_old = factory.NewWithValidTestData<GlbStaffRemuneration>();
			staffremuneration_old.GSR_GS_Staff = staff.PK;
			staffremuneration_old.GSR_EffectiveDate = expectedPackageEffectiveDateCurrentRemuneration.AddDays(-10);
			staffremuneration_old.GSR_FullTimeEquivalent = new ZDecimal(1.5);

			var staffentitlementbas = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementbas.GSI_EntitlementCode = "BAS";
			staffentitlementbas.GSI_GSR_Remuneration = staffremuneration.PK;
			staffentitlementbas.GSI_Value = expectedValueMostRecentSalary;
			staffentitlementbas.GSI_Frequency = expectedFrequencyMostRecentSalary;

			var staffentitlementbas_oldStaffRemuneration = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementbas_oldStaffRemuneration.GSI_EntitlementCode = "BAS";
			staffentitlementbas_oldStaffRemuneration.GSI_GSR_Remuneration = staffremuneration_old.PK;
			staffentitlementbas_oldStaffRemuneration.GSI_Value = expectedValueMostRecentSalary + 1;
			staffentitlementbas_oldStaffRemuneration.GSI_Frequency = "WKL";

			var staffentitlementwow = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementwow.GSI_EntitlementCode = "WOW";
			staffentitlementwow.GSI_GSR_Remuneration = staffremuneration.PK;
			staffentitlementwow.GSI_Value = expectedValueMostRecentWaysofWorkingAllowance;
			staffentitlementwow.GSI_Frequency = expectedFrequencyMostRecentWaysofWorkingAllowance;

			var staffentitlementwow_oldStaffRemuneration = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementwow_oldStaffRemuneration.GSI_EntitlementCode = "WOW";
			staffentitlementwow_oldStaffRemuneration.GSI_GSR_Remuneration = staffremuneration_old.PK;
			staffentitlementwow_oldStaffRemuneration.GSI_Value = expectedValueMostRecentWaysofWorkingAllowance + 1;
			staffentitlementwow_oldStaffRemuneration.GSI_Frequency = "WKL";

			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = expectedPackageEffectiveDateCurrentRemuneration.ToString("yyyy-MM-dd"),
					expectedFTECurrentRemuneration = string.Format("{0:0.##}", expectedFTECurrentRemuneration),
					expectedValueMostRecentSalary = expectedValueMostRecentSalary.ToString(),
					expectedValueMostRecentWaysofWorkingAllowance = expectedValueMostRecentWaysofWorkingAllowance.ToString(),
					expectedCompanyCurrentBeneficiaryEntity = String.Empty,
					expectedBranchCurrentBeneficiaryEntity = String.Empty,
					expectedDepartmentCurrentBeneficiaryEntity = String.Empty,
					expectedCompanyCurrentEmployingEntity = String.Empty,
					expectedBranchCurrentEmployingEntity = String.Empty,
					expectedDepartmentCurrentEmployingEntity = String.Empty,
					expectedCountryCurrentRemuneration = expectedCountryCurrentRemuneration,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = String.Empty,
					expectedCurrencyCurrentRemuneration = expectedCurrencyCurrentRemuneration,
					expectedFrequencyMostRecentSalary = expectedFrequencyMostRecentSalary,
					expectedFrequencyMostRecentWaysofWorkingAllowance = expectedFrequencyMostRecentWaysofWorkingAllowance,
					expectedEmploymentDate = String.Empty,
					expectedCurrentJobTitle = String.Empty,
					expectedCurrentTeam = String.Empty,
					expectedCurrentWorkingBasis = String.Empty,
					expectedDepartureDate = String.Empty,
					expectedEffectiveDateMostRecentPeopleLeader = String.Empty,
					expectedManagerMostRecentPeopleLeader = String.Empty
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_MultipleBeneficiaryEntites()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(3);
			var expectedIsCurrentCostCenter = "Yes";
			var expectedCostCenterCode = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedBranchCostCenter = "Branch 1";
			var expectedCompanyCurrentBeneficiaryEntity = "Company 1 Name";
			var expectedBranchCurrentBeneficiaryEntity = "Branch 2 Name";
			var expectedDepartmentCurrentBeneficiaryEntity = "Department 2 Desc";
			var expectedCountryCodeResidence = "C2";

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = expectedCostCenterCode;
			department1.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = branch1.PK;

			var beneficiarybranchdepartment = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiarybranchdepartment.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-3);
			beneficiarybranchdepartment.GBB_GS_Staff = staff.PK;

			var branch2 = factory.NewWithValidTestData<GlbBranch>();
			beneficiarybranchdepartment.GBB_GB_Branch = branch2.PK;
			branch2.GB_BranchName = expectedBranchCurrentBeneficiaryEntity;

			var company1 = factory.NewWithValidTestData<GlbCompany>();
			branch2.GB_GC = company1.PK;
			company1.GC_Name = expectedCompanyCurrentBeneficiaryEntity;

			var department2 = factory.NewWithValidTestData<GlbDepartment>();
			beneficiarybranchdepartment.GBB_GE_Department = department2.PK;
			department2.GE_Desc = expectedDepartmentCurrentBeneficiaryEntity;

			var beneficiarybranchdepartment_old = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiarybranchdepartment_old.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			beneficiarybranchdepartment_old.GBB_GS_Staff = staff.PK;

			var branch2_old = factory.NewWithValidTestData<GlbBranch>();
			beneficiarybranchdepartment_old.GBB_GB_Branch = branch2_old.PK;
			branch2_old.GB_BranchName = "XXX";

			var company1_old = factory.NewWithValidTestData<GlbCompany>();
			branch2_old.GB_GC = company1_old.PK;
			company1_old.GC_Name = "XXX";

			var department2_old = factory.NewWithValidTestData<GlbDepartment>();
			beneficiarybranchdepartment_old.GBB_GE_Department = department2_old.PK;
			department2_old.GE_Desc = "XXX";

			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = String.Empty,
					expectedFTECurrentRemuneration = String.Empty,
					expectedValueMostRecentSalary = String.Empty,
					expectedValueMostRecentWaysofWorkingAllowance = String.Empty,
					expectedCompanyCurrentBeneficiaryEntity = expectedCompanyCurrentBeneficiaryEntity,
					expectedBranchCurrentBeneficiaryEntity = expectedBranchCurrentBeneficiaryEntity,
					expectedDepartmentCurrentBeneficiaryEntity = expectedDepartmentCurrentBeneficiaryEntity,
					expectedCompanyCurrentEmployingEntity = String.Empty,
					expectedBranchCurrentEmployingEntity = String.Empty,
					expectedDepartmentCurrentEmployingEntity = String.Empty,
					expectedCountryCurrentRemuneration = String.Empty,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = String.Empty,
					expectedCurrencyCurrentRemuneration = String.Empty,
					expectedFrequencyMostRecentSalary = String.Empty,
					expectedFrequencyMostRecentWaysofWorkingAllowance = String.Empty,
					expectedEmploymentDate = String.Empty,
					expectedCurrentJobTitle = String.Empty,
					expectedCurrentTeam = String.Empty,
					expectedCurrentWorkingBasis = String.Empty,
					expectedDepartureDate = String.Empty,
					expectedEffectiveDateMostRecentPeopleLeader = String.Empty,
					expectedManagerMostRecentPeopleLeader = String.Empty
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_MultipleHomeEntities()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(3);
			var expectedCostCenterCode = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedBranchCostCenter = "Branch 1";
			var expectedCompanyCurrentEmployingEntity = "Company 2 Name";
			var expectedBranchCurrentEmployingEntity = "Branch 3 Name";
			var expectedDepartmentCurrentEmployingEntity = "Department 3 Desc";
			var expectedCountryCodeResidence = "C2";
			var expectedIsCurrentCostCenter = "Yes";

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = expectedCostCenterCode;
			department1.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = branch1.PK;

			var homeBranchdepartment = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			homeBranchdepartment.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-3);
			homeBranchdepartment.GHB_GS_Staff = staff.PK;

			var branch3 = factory.NewWithValidTestData<GlbBranch>();
			homeBranchdepartment.GHB_GB_Branch = branch3.PK;
			branch3.GB_BranchName = expectedBranchCurrentEmployingEntity;

			var company2 = factory.NewWithValidTestData<GlbCompany>();
			branch3.GB_GC = company2.PK;
			company2.GC_Name = expectedCompanyCurrentEmployingEntity;

			var department3 = factory.NewWithValidTestData<GlbDepartment>();
			homeBranchdepartment.GHB_GE_Department = department3.PK;
			department3.GE_Desc = expectedDepartmentCurrentEmployingEntity;

			var homeBranchdepartment_old = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			homeBranchdepartment_old.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			homeBranchdepartment_old.GHB_GS_Staff = staff.PK;

			var branch3_old = factory.NewWithValidTestData<GlbBranch>();
			homeBranchdepartment_old.GHB_GB_Branch = branch3_old.PK;
			branch3_old.GB_BranchName = "XXX";

			var company2_old = factory.NewWithValidTestData<GlbCompany>();
			branch3_old.GB_GC = company2_old.PK;
			company2_old.GC_Name = "XXX";

			var department3_old = factory.NewWithValidTestData<GlbDepartment>();
			homeBranchdepartment_old.GHB_GE_Department = department3_old.PK;
			department3_old.GE_Desc = "XXX";

			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = String.Empty,
					expectedFTECurrentRemuneration = String.Empty,
					expectedValueMostRecentSalary = String.Empty,
					expectedValueMostRecentWaysofWorkingAllowance = String.Empty,
					expectedCompanyCurrentBeneficiaryEntity = String.Empty,
					expectedBranchCurrentBeneficiaryEntity = String.Empty,
					expectedDepartmentCurrentBeneficiaryEntity = String.Empty,
					expectedCompanyCurrentEmployingEntity = expectedCompanyCurrentEmployingEntity,
					expectedBranchCurrentEmployingEntity = expectedBranchCurrentEmployingEntity,
					expectedDepartmentCurrentEmployingEntity = expectedDepartmentCurrentEmployingEntity,
					expectedCountryCurrentRemuneration = String.Empty,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = String.Empty,
					expectedCurrencyCurrentRemuneration = String.Empty,
					expectedFrequencyMostRecentSalary = String.Empty,
					expectedFrequencyMostRecentWaysofWorkingAllowance = String.Empty,
					expectedEmploymentDate = String.Empty,
					expectedCurrentJobTitle = String.Empty,
					expectedCurrentTeam = String.Empty,
					expectedCurrentWorkingBasis = String.Empty,
					expectedDepartureDate = String.Empty,
					expectedEffectiveDateMostRecentPeopleLeader = String.Empty,
					expectedManagerMostRecentPeopleLeader = String.Empty
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_MultipleEmploymentHistories()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(3);
			var expectedIsCurrentCostCenter = "Yes";
			var expectedCostCenterCode = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedBranchCostCenter = "Branch 1";
			var expectedCountryCodeResidence = "C2";
			var expectedCurrentJobTitle = "Job Title 1";

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = expectedCostCenterCode;
			department1.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = branch1.PK;

			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			var employmentHistory = factory.NewWithValidTestData<GlbEmploymentHistory>();
			employmentHistory.GEH_EffectiveDate = ZDateTimeOffset.Today.AddDays(-3);
			employmentHistory.GEH_GS_Staff = staff.PK;
			employmentHistory.GEH_JobTitle = expectedCurrentJobTitle;

			var employmentHistory_old = factory.NewWithValidTestData<GlbEmploymentHistory>();
			employmentHistory_old.GEH_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			employmentHistory_old.GEH_GS_Staff = staff.PK;
			employmentHistory_old.GEH_JobTitle = "XXX";

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = String.Empty,
					expectedFTECurrentRemuneration = String.Empty,
					expectedValueMostRecentSalary = String.Empty,
					expectedValueMostRecentWaysofWorkingAllowance = String.Empty,
					expectedCompanyCurrentBeneficiaryEntity = String.Empty,
					expectedBranchCurrentBeneficiaryEntity = String.Empty,
					expectedDepartmentCurrentBeneficiaryEntity = String.Empty,
					expectedCompanyCurrentEmployingEntity = String.Empty,
					expectedBranchCurrentEmployingEntity = String.Empty,
					expectedDepartmentCurrentEmployingEntity = String.Empty,
					expectedCountryCurrentRemuneration = String.Empty,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = String.Empty,
					expectedCurrencyCurrentRemuneration = String.Empty,
					expectedFrequencyMostRecentSalary = String.Empty,
					expectedFrequencyMostRecentWaysofWorkingAllowance = String.Empty,
					expectedEmploymentDate = String.Empty,
					expectedCurrentJobTitle = expectedCurrentJobTitle,
					expectedCurrentTeam = String.Empty,
					expectedCurrentWorkingBasis = String.Empty,
					expectedDepartureDate = String.Empty,
					expectedEffectiveDateMostRecentPeopleLeader = String.Empty,
					expectedManagerMostRecentPeopleLeader = String.Empty
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_MultipleEmploymentTeams()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(3);
			var expectedIsCurrentCostCenter = "Yes";
			var expectedCostCenterCode = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedBranchCostCenter = "Branch 1";
			var expectedCountryCodeResidence = "C2";
			var expectedCurrentTeam = "TN1";

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = expectedCostCenterCode;
			department1.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = branch1.PK;

			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			var employmentTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			employmentTeam.GET_EffectiveDate = ZDateTimeOffset.Today.AddDays(-3);
			employmentTeam.GET_GS_Staff = staff.PK;
			employmentTeam.GET_GST_NKTeamCode = expectedCurrentTeam;

			var employmentTeam_old = factory.NewWithValidTestData<GlbEmploymentTeam>();
			employmentTeam_old.GET_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			employmentTeam_old.GET_GS_Staff = staff.PK;
			employmentTeam_old.GET_GST_NKTeamCode = "XXX";

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = String.Empty,
					expectedFTECurrentRemuneration = String.Empty,
					expectedValueMostRecentSalary = String.Empty,
					expectedValueMostRecentWaysofWorkingAllowance = String.Empty,
					expectedCompanyCurrentBeneficiaryEntity = String.Empty,
					expectedBranchCurrentBeneficiaryEntity = String.Empty,
					expectedDepartmentCurrentBeneficiaryEntity = String.Empty,
					expectedCompanyCurrentEmployingEntity = String.Empty,
					expectedBranchCurrentEmployingEntity = String.Empty,
					expectedDepartmentCurrentEmployingEntity = String.Empty,
					expectedCountryCurrentRemuneration = String.Empty,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = String.Empty,
					expectedCurrencyCurrentRemuneration = String.Empty,
					expectedFrequencyMostRecentSalary = String.Empty,
					expectedFrequencyMostRecentWaysofWorkingAllowance = String.Empty,
					expectedEmploymentDate = String.Empty,
					expectedCurrentJobTitle = String.Empty,
					expectedCurrentTeam = expectedCurrentTeam,
					expectedCurrentWorkingBasis = String.Empty,
					expectedDepartureDate = String.Empty,
					expectedEffectiveDateMostRecentPeopleLeader = String.Empty,
					expectedManagerMostRecentPeopleLeader = String.Empty
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_MultipleStaffManagers()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(3);
			var expectedIsCurrentCostCenter = "Yes";
			var expectedCostCenterCode = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedBranchCostCenter = "Branch 1";
			var expectedCountryCodeResidence = "C2";
			var expectedEffectiveDateMostRecentPeopleLeader = ZDateTime.Today.AddDays(-5);
			var expectedManagerMostRecentPeopleLeader = "Manager 1 Name";

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = expectedCostCenterCode;
			department1.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = branch1.PK;

			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			var staffManager = factory.NewWithValidTestData<GlbStaffManager>();
			staffManager.GSM_GS_Staff = staff.PK;
			staffManager.GSM_EffectiveDate = expectedEffectiveDateMostRecentPeopleLeader;

			var manager = factory.NewWithValidTestData<GlbStaff>();
			staffManager.GSM_GS_Manager = manager.PK;
			manager.GS_FullName = expectedManagerMostRecentPeopleLeader;

			var staffManager_old = factory.NewWithValidTestData<GlbStaffManager>();
			staffManager_old.GSM_GS_Staff = staff.PK;
			staffManager_old.GSM_EffectiveDate = expectedEffectiveDateMostRecentPeopleLeader.AddDays(-10);

			var manager_old = factory.NewWithValidTestData<GlbStaff>();
			staffManager_old.GSM_GS_Manager = manager_old.PK;
			manager_old.GS_FullName = "XXX";

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = String.Empty,
					expectedFTECurrentRemuneration = String.Empty,
					expectedValueMostRecentSalary = String.Empty,
					expectedValueMostRecentWaysofWorkingAllowance = String.Empty,
					expectedCompanyCurrentBeneficiaryEntity = String.Empty,
					expectedBranchCurrentBeneficiaryEntity = String.Empty,
					expectedDepartmentCurrentBeneficiaryEntity = String.Empty,
					expectedCompanyCurrentEmployingEntity = String.Empty,
					expectedBranchCurrentEmployingEntity = String.Empty,
					expectedDepartmentCurrentEmployingEntity = String.Empty,
					expectedCountryCurrentRemuneration = String.Empty,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = String.Empty,
					expectedCurrencyCurrentRemuneration = String.Empty,
					expectedFrequencyMostRecentSalary = String.Empty,
					expectedFrequencyMostRecentWaysofWorkingAllowance = String.Empty,
					expectedEmploymentDate = String.Empty,
					expectedCurrentJobTitle = String.Empty,
					expectedCurrentTeam = String.Empty,
					expectedCurrentWorkingBasis = String.Empty,
					expectedDepartureDate = String.Empty,
					expectedEffectiveDateMostRecentPeopleLeader = expectedEffectiveDateMostRecentPeopleLeader.ToString("yyyy-MM-dd"),
					expectedManagerMostRecentPeopleLeader = expectedManagerMostRecentPeopleLeader
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_MultiCostCenters()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedStartDateCostCenter_2 = ZDate.Today.AddDays(-20);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(3);
			var expectedEndDateCostCenter_2 = ZDate.Today.AddDays(30);
			var expectedIsCurrentCostCenter = "Yes";
			var expectedCostCenterCode = "DP2";
			var expectedCostCenterCode_2 = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedCostCenterName_2 = "Department 2";
			var expectedBranchCostCenter = "Branch 1";
			var expectedBranchCostCenter_2 = "Branch 2";
			var expectedPackageEffectiveDateCurrentRemuneration = ZDateTimeOffset.Today.AddDays(-9);
			var expectedFTECurrentRemuneration = new ZDecimal(1.1);
			var expectedValueMostRecentSalary = 9;
			var expectedValueMostRecentWaysofWorkingAllowance = 8;
			var expectedCompanyCurrentBeneficiaryEntity = "Company 1 Name";
			var expectedBranchCurrentBeneficiaryEntity = "Branch 2 Name";
			var expectedDepartmentCurrentBeneficiaryEntity = "Department 2 Desc";
			var expectedCompanyCurrentEmployingEntity = "Company 2 Name";
			var expectedBranchCurrentEmployingEntity = "Branch 3 Name";
			var expectedDepartmentCurrentEmployingEntity = "Department 3 Desc";
			var expectedCountryCurrentRemuneration = "C1";
			var expectedCountryCodeResidence = "C2";
			var expectedCountryNameResidence = "Country Desc";
			var expectedCurrencyCurrentRemuneration = "AU1";
			var expectedFrequencyMostRecentSalary = "YRL";
			var expectedFrequencyMostRecentWaysofWorkingAllowance = "YRL";
			var expectedEmploymentDate = ZDateTime.Today.AddDays(-11);
			var expectedCurrentJobTitle = "Job Title 1";
			var expectedCurrentTeam = "TN1";
			var expectedCurrentWorkingBasis = "EB1";
			var expectedDepartureDate = ZDateTime.Today.AddDays(20);
			var expectedEffectiveDateMostRecentPeopleLeader = ZDateTime.Today.AddDays(-5);
			var expectedManagerMostRecentPeopleLeader = "Manager 1 Name";

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var costCenterDepartment = factory.NewWithValidTestData<GlbDepartment>();
			costCenterDepartment.GE_Code = expectedCostCenterCode;
			costCenterDepartment.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = costCenterDepartment.PK;

			var costCenterBranch = factory.NewWithValidTestData<GlbBranch>();
			costCenterBranch.GB_BranchName = expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = costCenterBranch.PK;

			var costCenter_2 = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter_2.GSK_StartDate = expectedStartDateCostCenter_2;
			costCenter_2.GSK_EndDate = expectedEndDateCostCenter_2;
			costCenter_2.GSK_GS_Staff = staff.PK;

			var costCenterDepartment_2 = factory.NewWithValidTestData<GlbDepartment>();
			costCenterDepartment_2.GE_Code = expectedCostCenterCode_2;
			costCenterDepartment_2.GE_Desc = expectedCostCenterName_2;
			costCenter_2.GSK_GE_Department = costCenterDepartment_2.PK;

			var costCenterBranch_2 = factory.NewWithValidTestData<GlbBranch>();
			costCenterBranch_2.GB_BranchName = expectedBranchCostCenter_2;
			costCenter_2.GSK_GB_Branch = costCenterBranch_2.PK;

			var staffremuneration = factory.NewWithValidTestData<GlbStaffRemuneration>();
			staffremuneration.GSR_GS_Staff = staff.PK;
			staffremuneration.GSR_EffectiveDate = expectedPackageEffectiveDateCurrentRemuneration;
			staffremuneration.GSR_FullTimeEquivalent = expectedFTECurrentRemuneration;

			var staffentitlementbas = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementbas.GSI_EntitlementCode = "BAS";
			staffentitlementbas.GSI_GSR_Remuneration = staffremuneration.PK;
			staffentitlementbas.GSI_Value = expectedValueMostRecentSalary;
			staffentitlementbas.GSI_Frequency = expectedFrequencyMostRecentSalary;

			var staffentitlementwow = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementwow.GSI_EntitlementCode = "WOW";
			staffentitlementwow.GSI_GSR_Remuneration = staffremuneration.PK;
			staffentitlementwow.GSI_Value = expectedValueMostRecentWaysofWorkingAllowance;
			staffentitlementwow.GSI_Frequency = expectedFrequencyMostRecentWaysofWorkingAllowance;

			var beneficiarybranchdepartment = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiarybranchdepartment.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			beneficiarybranchdepartment.GBB_GS_Staff = staff.PK;

			var branch2 = factory.NewWithValidTestData<GlbBranch>();
			beneficiarybranchdepartment.GBB_GB_Branch = branch2.PK;
			branch2.GB_BranchName = expectedBranchCurrentBeneficiaryEntity;

			var company1 = factory.NewWithValidTestData<GlbCompany>();
			branch2.GB_GC = company1.PK;
			company1.GC_Name = expectedCompanyCurrentBeneficiaryEntity;

			var department2 = factory.NewWithValidTestData<GlbDepartment>();
			beneficiarybranchdepartment.GBB_GE_Department = department2.PK;
			department2.GE_Desc = expectedDepartmentCurrentBeneficiaryEntity;

			var homeBranchdepartment = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			homeBranchdepartment.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			homeBranchdepartment.GHB_GS_Staff = staff.PK;

			var branch3 = factory.NewWithValidTestData<GlbBranch>();
			homeBranchdepartment.GHB_GB_Branch = branch3.PK;
			branch3.GB_BranchName = expectedBranchCurrentEmployingEntity;

			var company2 = factory.NewWithValidTestData<GlbCompany>();
			branch3.GB_GC = company2.PK;
			company2.GC_Name = expectedCompanyCurrentEmployingEntity;

			var department3 = factory.NewWithValidTestData<GlbDepartment>();
			homeBranchdepartment.GHB_GE_Department = department3.PK;
			department3.GE_Desc = expectedDepartmentCurrentEmployingEntity;

			staffremuneration.GSR_RN_NKCountry = expectedCountryCurrentRemuneration;
			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			var refCountry = factory.NewWithValidTestData<RefCountry>();
			refCountry.RN_Code = staff.GS_RN_NKCountryCode;
			refCountry.RN_Desc = expectedCountryNameResidence;

			staffremuneration.GSR_RX_NKCurrency = expectedCurrencyCurrentRemuneration;

			staff.GS_EmploymentDate = expectedEmploymentDate;

			var employmentHistory = factory.NewWithValidTestData<GlbEmploymentHistory>();
			employmentHistory.GEH_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			employmentHistory.GEH_GS_Staff = staff.PK;
			employmentHistory.GEH_JobTitle = expectedCurrentJobTitle;

			var employmentTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			employmentTeam.GET_GS_Staff = staff.PK;
			employmentTeam.GET_GST_NKTeamCode = expectedCurrentTeam;

			staff.GS_EmploymentBasis = expectedCurrentWorkingBasis;
			staff.GS_DepartureDate = expectedDepartureDate;

			var staffManager = factory.NewWithValidTestData<GlbStaffManager>();
			staffManager.GSM_GS_Staff = staff.PK;
			staffManager.GSM_EffectiveDate = expectedEffectiveDateMostRecentPeopleLeader;

			var manager = factory.NewWithValidTestData<GlbStaff>();
			staffManager.GSM_GS_Manager = manager.PK;
			manager.GS_FullName = expectedManagerMostRecentPeopleLeader;

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = expectedPackageEffectiveDateCurrentRemuneration.ToString("yyyy-MM-dd"),
					expectedFTECurrentRemuneration = string.Format("{0:0.##}", expectedFTECurrentRemuneration),
					expectedValueMostRecentSalary = expectedValueMostRecentSalary.ToString(),
					expectedValueMostRecentWaysofWorkingAllowance = expectedValueMostRecentWaysofWorkingAllowance.ToString(),
					expectedCompanyCurrentBeneficiaryEntity = expectedCompanyCurrentBeneficiaryEntity,
					expectedBranchCurrentBeneficiaryEntity = expectedBranchCurrentBeneficiaryEntity,
					expectedDepartmentCurrentBeneficiaryEntity = expectedDepartmentCurrentBeneficiaryEntity,
					expectedCompanyCurrentEmployingEntity = expectedCompanyCurrentEmployingEntity,
					expectedBranchCurrentEmployingEntity = expectedBranchCurrentEmployingEntity,
					expectedDepartmentCurrentEmployingEntity = expectedDepartmentCurrentEmployingEntity,
					expectedCountryCurrentRemuneration = expectedCountryCurrentRemuneration,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = expectedCountryNameResidence,
					expectedCurrencyCurrentRemuneration = expectedCurrencyCurrentRemuneration,
					expectedFrequencyMostRecentSalary = expectedFrequencyMostRecentSalary,
					expectedFrequencyMostRecentWaysofWorkingAllowance = expectedFrequencyMostRecentWaysofWorkingAllowance,
					expectedEmploymentDate = expectedEmploymentDate.ToString("yyyy-MM-dd"),
					expectedCurrentJobTitle = expectedCurrentJobTitle,
					expectedCurrentTeam = expectedCurrentTeam,
					expectedCurrentWorkingBasis = expectedCurrentWorkingBasis,
					expectedDepartureDate = expectedDepartureDate.ToString("yyyy-MM-dd"),
					expectedEffectiveDateMostRecentPeopleLeader = expectedEffectiveDateMostRecentPeopleLeader.ToString("yyyy-MM-dd"),
					expectedManagerMostRecentPeopleLeader = expectedManagerMostRecentPeopleLeader
				};

				CheckRecord(sheetContent: sheetContent,
							row: 4,
							recordCheckParameters: recordCheckParameters
				);

				var recordCheckParameters2 = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter_2.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter_2.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode_2,
					expectedCostCenterName = expectedCostCenterName_2,
					expectedBranchCostCenter = expectedBranchCostCenter_2,
					expectedPackageEffectiveDateCurrentRemuneration = expectedPackageEffectiveDateCurrentRemuneration.ToString("yyyy-MM-dd"),
					expectedFTECurrentRemuneration = string.Format("{0:0.##}", expectedFTECurrentRemuneration),
					expectedValueMostRecentSalary = expectedValueMostRecentSalary.ToString(),
					expectedValueMostRecentWaysofWorkingAllowance = expectedValueMostRecentWaysofWorkingAllowance.ToString(),
					expectedCompanyCurrentBeneficiaryEntity = expectedCompanyCurrentBeneficiaryEntity,
					expectedBranchCurrentBeneficiaryEntity = expectedBranchCurrentBeneficiaryEntity,
					expectedDepartmentCurrentBeneficiaryEntity = expectedDepartmentCurrentBeneficiaryEntity,
					expectedCompanyCurrentEmployingEntity = expectedCompanyCurrentEmployingEntity,
					expectedBranchCurrentEmployingEntity = expectedBranchCurrentEmployingEntity,
					expectedDepartmentCurrentEmployingEntity = expectedDepartmentCurrentEmployingEntity,
					expectedCountryCurrentRemuneration = expectedCountryCurrentRemuneration,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = expectedCountryNameResidence,
					expectedCurrencyCurrentRemuneration = expectedCurrencyCurrentRemuneration,
					expectedFrequencyMostRecentSalary = expectedFrequencyMostRecentSalary,
					expectedFrequencyMostRecentWaysofWorkingAllowance = expectedFrequencyMostRecentWaysofWorkingAllowance,
					expectedEmploymentDate = expectedEmploymentDate.ToString("yyyy-MM-dd"),
					expectedCurrentJobTitle = expectedCurrentJobTitle,
					expectedCurrentTeam = expectedCurrentTeam,
					expectedCurrentWorkingBasis = expectedCurrentWorkingBasis,
					expectedDepartureDate = expectedDepartureDate.ToString("yyyy-MM-dd"),
					expectedEffectiveDateMostRecentPeopleLeader = expectedEffectiveDateMostRecentPeopleLeader.ToString("yyyy-MM-dd"),
					expectedManagerMostRecentPeopleLeader = expectedManagerMostRecentPeopleLeader
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters2
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_NoCurrent()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-20);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedIsCurrentCostCenter = "No";
			var expectedCostCenterCode = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedBranchCostCenter = "Branch 1";
			var expectedPackageEffectiveDateCurrentRemuneration = ZDateTimeOffset.Today.AddDays(-9);
			var expectedFTECurrentRemuneration = new ZDecimal(1.1);
			var expectedValueMostRecentSalary = 9;
			var expectedValueMostRecentWaysofWorkingAllowance = 8;
			var expectedCompanyCurrentBeneficiaryEntity = "Company 1 Name";
			var expectedBranchCurrentBeneficiaryEntity = "Branch 2 Name";
			var expectedDepartmentCurrentBeneficiaryEntity = "Department 2 Desc";
			var expectedCompanyCurrentEmployingEntity = "Company 2 Name";
			var expectedBranchCurrentEmployingEntity = "Branch 3 Name";
			var expectedDepartmentCurrentEmployingEntity = "Department 3 Desc";
			var expectedCountryCurrentRemuneration = "C1";
			var expectedCountryCodeResidence = "C2";
			var expectedCountryNameResidence = "Country Desc";
			var expectedCurrencyCurrentRemuneration = "AU1";
			var expectedFrequencyMostRecentSalary = "YRL";
			var expectedFrequencyMostRecentWaysofWorkingAllowance = "YRL";
			var expectedEmploymentDate = ZDateTime.Today.AddDays(-11);
			var expectedCurrentJobTitle = "Job Title 1";
			var expectedCurrentTeam = "TN1";
			var expectedCurrentWorkingBasis = "EB1";
			var expectedDepartureDate = ZDateTime.Today.AddDays(20);
			var expectedEffectiveDateMostRecentPeopleLeader = ZDateTime.Today.AddDays(-5);
			var expectedManagerMostRecentPeopleLeader = "Manager 1 Name";

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = expectedCostCenterCode;
			department1.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = branch1.PK;

			var staffremuneration = factory.NewWithValidTestData<GlbStaffRemuneration>();
			staffremuneration.GSR_GS_Staff = staff.PK;
			staffremuneration.GSR_EffectiveDate = expectedPackageEffectiveDateCurrentRemuneration;
			staffremuneration.GSR_FullTimeEquivalent = expectedFTECurrentRemuneration;

			var staffentitlementbas = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementbas.GSI_EntitlementCode = "BAS";
			staffentitlementbas.GSI_GSR_Remuneration = staffremuneration.PK;
			staffentitlementbas.GSI_Value = expectedValueMostRecentSalary;
			staffentitlementbas.GSI_Frequency = expectedFrequencyMostRecentSalary;

			var staffentitlementwow = factory.NewWithValidTestData<GlbStaffEntitlement>();
			staffentitlementwow.GSI_EntitlementCode = "WOW";
			staffentitlementwow.GSI_GSR_Remuneration = staffremuneration.PK;
			staffentitlementwow.GSI_Value = expectedValueMostRecentWaysofWorkingAllowance;
			staffentitlementwow.GSI_Frequency = expectedFrequencyMostRecentWaysofWorkingAllowance;

			var beneficiarybranchdepartment = factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiarybranchdepartment.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			beneficiarybranchdepartment.GBB_GS_Staff = staff.PK;

			var branch2 = factory.NewWithValidTestData<GlbBranch>();
			beneficiarybranchdepartment.GBB_GB_Branch = branch2.PK;
			branch2.GB_BranchName = expectedBranchCurrentBeneficiaryEntity;

			var company1 = factory.NewWithValidTestData<GlbCompany>();
			branch2.GB_GC = company1.PK;
			company1.GC_Name = expectedCompanyCurrentBeneficiaryEntity;

			var department2 = factory.NewWithValidTestData<GlbDepartment>();
			beneficiarybranchdepartment.GBB_GE_Department = department2.PK;
			department2.GE_Desc = expectedDepartmentCurrentBeneficiaryEntity;

			var homeBranchdepartment = factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			homeBranchdepartment.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			homeBranchdepartment.GHB_GS_Staff = staff.PK;

			var branch3 = factory.NewWithValidTestData<GlbBranch>();
			homeBranchdepartment.GHB_GB_Branch = branch3.PK;
			branch3.GB_BranchName = expectedBranchCurrentEmployingEntity;

			var company2 = factory.NewWithValidTestData<GlbCompany>();
			branch3.GB_GC = company2.PK;
			company2.GC_Name = expectedCompanyCurrentEmployingEntity;

			var department3 = factory.NewWithValidTestData<GlbDepartment>();
			homeBranchdepartment.GHB_GE_Department = department3.PK;
			department3.GE_Desc = expectedDepartmentCurrentEmployingEntity;

			staffremuneration.GSR_RN_NKCountry = expectedCountryCurrentRemuneration;
			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			var refCountry = factory.NewWithValidTestData<RefCountry>();
			refCountry.RN_Code = staff.GS_RN_NKCountryCode;
			refCountry.RN_Desc = expectedCountryNameResidence;

			staffremuneration.GSR_RX_NKCurrency = expectedCurrencyCurrentRemuneration;

			staff.GS_EmploymentDate = expectedEmploymentDate;

			var employmentHistory = factory.NewWithValidTestData<GlbEmploymentHistory>();
			employmentHistory.GEH_EffectiveDate = ZDateTimeOffset.Today.AddDays(-10);
			employmentHistory.GEH_GS_Staff = staff.PK;
			employmentHistory.GEH_JobTitle = expectedCurrentJobTitle;

			var employmentTeam = factory.NewWithValidTestData<GlbEmploymentTeam>();
			employmentTeam.GET_GS_Staff = staff.PK;
			employmentTeam.GET_GST_NKTeamCode = expectedCurrentTeam;

			staff.GS_EmploymentBasis = expectedCurrentWorkingBasis;
			staff.GS_DepartureDate = expectedDepartureDate;

			var staffManager = factory.NewWithValidTestData<GlbStaffManager>();
			staffManager.GSM_GS_Staff = staff.PK;
			staffManager.GSM_EffectiveDate = expectedEffectiveDateMostRecentPeopleLeader;

			var manager = factory.NewWithValidTestData<GlbStaff>();
			staffManager.GSM_GS_Manager = manager.PK;
			manager.GS_FullName = expectedManagerMostRecentPeopleLeader;

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = expectedPackageEffectiveDateCurrentRemuneration.ToString("yyyy-MM-dd"),
					expectedFTECurrentRemuneration = string.Format("{0:0.##}", expectedFTECurrentRemuneration),
					expectedValueMostRecentSalary = expectedValueMostRecentSalary.ToString(),
					expectedValueMostRecentWaysofWorkingAllowance = expectedValueMostRecentWaysofWorkingAllowance.ToString(),
					expectedCompanyCurrentBeneficiaryEntity = expectedCompanyCurrentBeneficiaryEntity,
					expectedBranchCurrentBeneficiaryEntity = expectedBranchCurrentBeneficiaryEntity,
					expectedDepartmentCurrentBeneficiaryEntity = expectedDepartmentCurrentBeneficiaryEntity,
					expectedCompanyCurrentEmployingEntity = expectedCompanyCurrentEmployingEntity,
					expectedBranchCurrentEmployingEntity = expectedBranchCurrentEmployingEntity,
					expectedDepartmentCurrentEmployingEntity = expectedDepartmentCurrentEmployingEntity,
					expectedCountryCurrentRemuneration = expectedCountryCurrentRemuneration,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = expectedCountryNameResidence,
					expectedCurrencyCurrentRemuneration = expectedCurrencyCurrentRemuneration,
					expectedFrequencyMostRecentSalary = expectedFrequencyMostRecentSalary,
					expectedFrequencyMostRecentWaysofWorkingAllowance = expectedFrequencyMostRecentWaysofWorkingAllowance,
					expectedEmploymentDate = expectedEmploymentDate.ToString("yyyy-MM-dd"),
					expectedCurrentJobTitle = expectedCurrentJobTitle,
					expectedCurrentTeam = expectedCurrentTeam,
					expectedCurrentWorkingBasis = expectedCurrentWorkingBasis,
					expectedDepartureDate = expectedDepartureDate.ToString("yyyy-MM-dd"),
					expectedEffectiveDateMostRecentPeopleLeader = expectedEffectiveDateMostRecentPeopleLeader.ToString("yyyy-MM-dd"),
					expectedManagerMostRecentPeopleLeader = expectedManagerMostRecentPeopleLeader
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_MissingValue()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(3);
			var expectedIsCurrentCostCenter = "Yes";
			var expectedCostCenterCode = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedBranchCostCenter = "Branch 1";
			var expectedCountryCodeResidence = "C2";
			var expectedEmploymentDate = ZDateTime.Today.AddDays(-11);
			var expectedCurrentWorkingBasis = "EB1";
			var expectedDepartureDate = ZDateTime.Today.AddDays(20);

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = expectedCostCenterCode;
			department1.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = expectedBranchCostCenter;
			costCenter.GSK_GB_Branch = branch1.PK;

			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			staff.GS_EmploymentDate = expectedEmploymentDate;

			staff.GS_EmploymentBasis = expectedCurrentWorkingBasis;
			staff.GS_DepartureDate = expectedDepartureDate;

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = expectedBranchCostCenter,
					expectedPackageEffectiveDateCurrentRemuneration = string.Empty,
					expectedFTECurrentRemuneration = string.Empty,
					expectedValueMostRecentSalary = string.Empty,
					expectedValueMostRecentWaysofWorkingAllowance = string.Empty,
					expectedCompanyCurrentBeneficiaryEntity = string.Empty,
					expectedBranchCurrentBeneficiaryEntity = string.Empty,
					expectedDepartmentCurrentBeneficiaryEntity = string.Empty,
					expectedCompanyCurrentEmployingEntity = string.Empty,
					expectedBranchCurrentEmployingEntity = string.Empty,
					expectedDepartmentCurrentEmployingEntity = string.Empty,
					expectedCountryCurrentRemuneration = string.Empty,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = string.Empty,
					expectedCurrencyCurrentRemuneration = string.Empty,
					expectedFrequencyMostRecentSalary = string.Empty,
					expectedFrequencyMostRecentWaysofWorkingAllowance = string.Empty,
					expectedEmploymentDate = expectedEmploymentDate.ToString("yyyy-MM-dd"),
					expectedCurrentJobTitle = string.Empty,
					expectedCurrentTeam = string.Empty,
					expectedCurrentWorkingBasis = expectedCurrentWorkingBasis,
					expectedDepartureDate = expectedDepartureDate.ToString("yyyy-MM-dd"),
					expectedEffectiveDateMostRecentPeopleLeader = string.Empty,
					expectedManagerMostRecentPeopleLeader = string.Empty
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}

		[TestDate(2021, 12, 20)]
		public void TestCostCentersReport_NoBranch()
		{
			var factory = new BusinessObjectFactory(connectionThatCanAccessHrmSchema);
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var expectedIsActive = true;
			var expectedStaffCode = "SC1";
			var expectedStaffFullName = "Staff 1 Name";
			var expectedStartDateCostCenter = ZDate.Today.AddDays(-10);
			var expectedEndDateCostCenter = ZDate.Today.AddDays(3);
			var expectedIsCurrentCostCenter = "Yes";
			var expectedCostCenterCode = "DP1";
			var expectedCostCenterName = "Department 1";
			var expectedCountryCodeResidence = "C2";
			var expectedEmploymentDate = ZDateTime.Today.AddDays(-11);
			var expectedCurrentWorkingBasis = "EB1";
			var expectedDepartureDate = ZDateTime.Today.AddDays(20);

			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_IsActive = expectedIsActive;
			staff.GS_Code = expectedStaffCode;
			staff.GS_FullName = expectedStaffFullName;

			var costCenter = factory.NewWithValidTestData<GlbStaffCostCentre>();
			costCenter.GSK_StartDate = expectedStartDateCostCenter;
			costCenter.GSK_EndDate = expectedEndDateCostCenter;
			costCenter.GSK_GS_Staff = staff.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = expectedCostCenterCode;
			department1.GE_Desc = expectedCostCenterName;
			costCenter.GSK_GE_Department = department1.PK;

			staff.GS_RN_NKCountryCode = expectedCountryCodeResidence;

			staff.GS_EmploymentDate = expectedEmploymentDate;

			staff.GS_EmploymentBasis = expectedCurrentWorkingBasis;
			staff.GS_DepartureDate = expectedDepartureDate;

			factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				var recordCheckParameters = new RecordCheckParameters
				{
					expectedIsActive = expectedIsActive,
					expectedStaffCode = expectedStaffCode,
					expectedStaffFullName = expectedStaffFullName,
					expectedStartDateCostCenter = expectedStartDateCostCenter.ToString("yyyy-MM-dd"),
					expectedEndDateCostCenter = expectedEndDateCostCenter.ToString("yyyy-MM-dd"),
					expectedIsCurrentCostCenter = expectedIsCurrentCostCenter,
					expectedCostCenterCode = expectedCostCenterCode,
					expectedCostCenterName = expectedCostCenterName,
					expectedBranchCostCenter = string.Empty,
					expectedPackageEffectiveDateCurrentRemuneration = string.Empty,
					expectedFTECurrentRemuneration = string.Empty,
					expectedValueMostRecentSalary = string.Empty,
					expectedValueMostRecentWaysofWorkingAllowance = string.Empty,
					expectedCompanyCurrentBeneficiaryEntity = string.Empty,
					expectedBranchCurrentBeneficiaryEntity = string.Empty,
					expectedDepartmentCurrentBeneficiaryEntity = string.Empty,
					expectedCompanyCurrentEmployingEntity = string.Empty,
					expectedBranchCurrentEmployingEntity = string.Empty,
					expectedDepartmentCurrentEmployingEntity = string.Empty,
					expectedCountryCurrentRemuneration = string.Empty,
					expectedCountryCodeResidence = expectedCountryCodeResidence,
					expectedCountryNameResidence = string.Empty,
					expectedCurrencyCurrentRemuneration = string.Empty,
					expectedFrequencyMostRecentSalary = string.Empty,
					expectedFrequencyMostRecentWaysofWorkingAllowance = string.Empty,
					expectedEmploymentDate = expectedEmploymentDate.ToString("yyyy-MM-dd"),
					expectedCurrentJobTitle = string.Empty,
					expectedCurrentTeam = string.Empty,
					expectedCurrentWorkingBasis = expectedCurrentWorkingBasis,
					expectedDepartureDate = expectedDepartureDate.ToString("yyyy-MM-dd"),
					expectedEffectiveDateMostRecentPeopleLeader = string.Empty,
					expectedManagerMostRecentPeopleLeader = string.Empty
				};

				CheckRecord(sheetContent: sheetContent,
							row: 3,
							recordCheckParameters: recordCheckParameters
					);
			}
		}
	}
}
