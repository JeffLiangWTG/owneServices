using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Company Staff Count Report")]
	public class CompanyStaffCountReportTemplateTest : TemplateTestCase
	{
		public void TestCompanyStaffCountReport_DepartureDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var employingStaff1 = Factory.NewWithValidTestData<GlbStaff>();

			var employingStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff2.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var employingStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff3.GS_DepartureDate = ZDateTime.Today.AddDays(1);

			var employingStaff4 = Factory.NewWithValidTestData<GlbStaff>();

			var employingStaff5 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff5.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var employingStaff6 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff6.GS_DepartureDate = ZDateTime.Today.AddDays(1);

			var employingStaff7 = Factory.NewWithValidTestData<GlbStaff>();

			var employingStaff8 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff8.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var employingStaff9 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff9.GS_DepartureDate = ZDateTime.Today.AddDays(1);

			var beneficiaryStaff1 = Factory.NewWithValidTestData<GlbStaff>();

			var beneficiaryStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff2.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var beneficiaryStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff3.GS_DepartureDate = ZDateTime.Today.AddDays(1);

			var beneficiaryStaff4 = Factory.NewWithValidTestData<GlbStaff>();

			var beneficiaryStaff5 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff5.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var beneficiaryStaff6 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff6.GS_DepartureDate = ZDateTime.Today.AddDays(1);

			var beneficiaryStaff7 = Factory.NewWithValidTestData<GlbStaff>();

			var beneficiaryStaff8 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff8.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var beneficiaryStaff9 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff9.GS_DepartureDate = ZDateTime.Today.AddDays(1);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			var branch4 = Factory.NewWithValidTestData<GlbBranch>();

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "GC1";
			company1.GC_Name = "GC Name 1";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "GC2";
			company2.GC_Name = "GC Name 2";

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "GC3";
			company3.GC_Name = "GC Name 3";

			var company4 = Factory.NewWithValidTestData<GlbCompany>();
			company4.GC_Code = "GC4";
			company4.GC_Name = "GC Name 4";

			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company2.PK;
			branch3.GB_GC = company2.PK;
			branch4.GB_GC = company3.PK;

			var employingEntity1 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity1.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity1.GHB_GS_Staff = employingStaff1.PK;
			employingEntity1.GHB_GB_Branch = branch1.PK;

			var employingEntity2 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity2.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity2.GHB_GS_Staff = employingStaff2.PK;
			employingEntity2.GHB_GB_Branch = branch1.PK;

			var employingEntity3 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity3.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity3.GHB_GS_Staff = employingStaff3.PK;
			employingEntity3.GHB_GB_Branch = branch1.PK;

			var employingEntity4 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity4.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity4.GHB_GS_Staff = employingStaff4.PK;
			employingEntity4.GHB_GB_Branch = branch2.PK;

			var employingEntity5 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity5.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity5.GHB_GS_Staff = employingStaff5.PK;
			employingEntity5.GHB_GB_Branch = branch2.PK;

			var employingEntity6 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity6.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity6.GHB_GS_Staff = employingStaff6.PK;
			employingEntity6.GHB_GB_Branch = branch2.PK;

			var employingEntity7 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity7.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity7.GHB_GS_Staff = employingStaff7.PK;
			employingEntity7.GHB_GB_Branch = branch3.PK;

			var employingEntity8 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity8.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity8.GHB_GS_Staff = employingStaff8.PK;
			employingEntity8.GHB_GB_Branch = branch3.PK;

			var employingEntity9 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity9.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity9.GHB_GS_Staff = employingStaff9.PK;
			employingEntity9.GHB_GB_Branch = branch3.PK;

			var beneficiaryEntity1 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity1.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity1.GBB_GS_Staff = beneficiaryStaff1.PK;
			beneficiaryEntity1.GBB_GB_Branch = branch1.PK;

			var beneficiaryEntity2 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity2.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity2.GBB_GS_Staff = beneficiaryStaff2.PK;
			beneficiaryEntity2.GBB_GB_Branch = branch1.PK;

			var beneficiaryEntity3 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity3.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity3.GBB_GS_Staff = beneficiaryStaff3.PK;
			beneficiaryEntity3.GBB_GB_Branch = branch1.PK;

			var beneficiaryEntity4 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity4.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity4.GBB_GS_Staff = beneficiaryStaff4.PK;
			beneficiaryEntity4.GBB_GB_Branch = branch2.PK;

			var beneficiaryEntity5 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity5.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity5.GBB_GS_Staff = beneficiaryStaff5.PK;
			beneficiaryEntity5.GBB_GB_Branch = branch2.PK;

			var beneficiaryEntity6 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity6.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity6.GBB_GS_Staff = beneficiaryStaff6.PK;
			beneficiaryEntity6.GBB_GB_Branch = branch2.PK;

			var beneficiaryEntity7 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity7.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity7.GBB_GS_Staff = beneficiaryStaff7.PK;
			beneficiaryEntity7.GBB_GB_Branch = branch4.PK;

			var beneficiaryEntity8 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity8.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity8.GBB_GS_Staff = beneficiaryStaff8.PK;
			beneficiaryEntity8.GBB_GB_Branch = branch4.PK;

			var beneficiaryEntity9 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity9.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity9.GBB_GS_Staff = beneficiaryStaff9.PK;
			beneficiaryEntity9.GBB_GB_Branch = branch4.PK;

			Factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertEquals("GC Name 1", sheetContent[3, 3].ToString());
				AssertEquals("2", sheetContent[3, 4].ToString());
				AssertEquals("2", sheetContent[3, 5].ToString());
				AssertEquals("GC Name 2", sheetContent[4, 3].ToString());
				AssertEquals("2", sheetContent[4, 4].ToString());
				AssertEquals("4", sheetContent[4, 5].ToString());
				AssertEquals("GC Name 3", sheetContent[5, 3].ToString());
				AssertEquals("2", sheetContent[5, 4].ToString());
				AssertEquals("0", sheetContent[5, 5].ToString());
			}
		}

		public void TestCompanyStaffCountReport_EffectiveDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var employingStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff1.GS_DepartureDate = ZDateTime.Today.AddDays(-9);

			var employingStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff2.GS_DepartureDate = ZDateTime.Today.AddDays(-9);

			var employingStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff3.GS_DepartureDate = ZDateTime.Today.AddDays(-9);

			var employingStaff4 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff4.GS_DepartureDate = ZDateTime.Today.AddDays(-9);

			var beneficiaryStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff1.GS_DepartureDate = ZDateTime.Today.AddDays(-9);

			var beneficiaryStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff2.GS_DepartureDate = ZDateTime.Today.AddDays(-9);

			var beneficiaryStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff3.GS_DepartureDate = ZDateTime.Today.AddDays(-9);

			var beneficiaryStaff4 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff4.GS_DepartureDate = ZDateTime.Today.AddDays(-9);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "GB1";
			branch1.GB_BranchName = "GB Name 1";

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "GB2";
			branch2.GB_BranchName = "GB Name 2";

			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_Code = "GB3";
			branch3.GB_BranchName = "GB Name 3";

			var branch4 = Factory.NewWithValidTestData<GlbBranch>();
			branch4.GB_Code = "GB4";
			branch4.GB_BranchName = "GB Name 4";

			var branch5 = Factory.NewWithValidTestData<GlbBranch>();
			branch5.GB_Code = "GB5";
			branch5.GB_BranchName = "GB Name 5";

			var branch6 = Factory.NewWithValidTestData<GlbBranch>();
			branch6.GB_Code = "GB6";
			branch6.GB_BranchName = "GB Name 6";

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "GC1";
			company1.GC_Name = "GC Name 1";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "GC2";
			company2.GC_Name = "GC Name 2";

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "GC3";
			company3.GC_Name = "GC Name 3";

			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company2.PK;
			branch3.GB_GC = company2.PK;
			branch4.GB_GC = company3.PK;
			branch5.GB_GC = company3.PK;
			branch6.GB_GC = company3.PK;

			var employingEntity1 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity1.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			employingEntity1.GHB_GS_Staff = employingStaff1.PK;
			employingEntity1.GHB_GB_Branch = branch1.PK;

			var employingEntity2 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity2.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-12);
			employingEntity2.GHB_GS_Staff = employingStaff1.PK;
			employingEntity2.GHB_GB_Branch = branch2.PK;

			var employingEntity3 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity3.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-9);
			employingEntity3.GHB_GS_Staff = employingStaff2.PK;
			employingEntity3.GHB_GB_Branch = branch3.PK;

			var employingEntity4 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity4.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			employingEntity4.GHB_GS_Staff = employingStaff2.PK;
			employingEntity4.GHB_GB_Branch = branch4.PK;

			var employingEntity5 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity5.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-9);
			employingEntity5.GHB_GS_Staff = employingStaff3.PK;
			employingEntity5.GHB_GB_Branch = branch5.PK;

			var employingEntity6 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity6.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-8);
			employingEntity6.GHB_GS_Staff = employingStaff3.PK;
			employingEntity6.GHB_GB_Branch = branch6.PK;

			var employingEntity7 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity7.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			employingEntity7.GHB_GS_Staff = employingStaff4.PK;
			employingEntity7.GHB_GB_Branch = branch4.PK;

			var beneficiaryEntity1 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity1.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			beneficiaryEntity1.GBB_GS_Staff = beneficiaryStaff1.PK;
			beneficiaryEntity1.GBB_GB_Branch = branch1.PK;

			var beneficiaryEntity2 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity2.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-12);
			beneficiaryEntity2.GBB_GS_Staff = beneficiaryStaff1.PK;
			beneficiaryEntity2.GBB_GB_Branch = branch2.PK;

			var beneficiaryEntity3 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity3.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-9);
			beneficiaryEntity3.GBB_GS_Staff = beneficiaryStaff2.PK;
			beneficiaryEntity3.GBB_GB_Branch = branch3.PK;

			var beneficiaryEntity4 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity4.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			beneficiaryEntity4.GBB_GS_Staff = beneficiaryStaff2.PK;
			beneficiaryEntity4.GBB_GB_Branch = branch4.PK;

			var beneficiaryEntity5 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity5.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-8);
			beneficiaryEntity5.GBB_GS_Staff = beneficiaryStaff3.PK;
			beneficiaryEntity5.GBB_GB_Branch = branch5.PK;

			var beneficiaryEntity6 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity6.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-9);
			beneficiaryEntity6.GBB_GS_Staff = beneficiaryStaff3.PK;
			beneficiaryEntity6.GBB_GB_Branch = branch6.PK;

			var beneficiaryEntity7 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity7.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			beneficiaryEntity7.GBB_GS_Staff = beneficiaryStaff3.PK;
			beneficiaryEntity7.GBB_GB_Branch = branch6.PK;

			Factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today.AddDays(-10);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertEquals("GC Name 1", sheetContent[3, 3].ToString());
				AssertEquals("1", sheetContent[3, 4].ToString());
				AssertEquals("1", sheetContent[3, 5].ToString());
				AssertEquals("GC Name 3", sheetContent[4, 3].ToString());
				AssertEquals("2", sheetContent[4, 4].ToString());
				AssertEquals("2", sheetContent[4, 5].ToString());
			}
		}
	}
}
