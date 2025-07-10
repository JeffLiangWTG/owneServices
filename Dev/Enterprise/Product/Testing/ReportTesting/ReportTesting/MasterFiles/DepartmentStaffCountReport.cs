using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Department Staff Count Report")]
	public class DepartmentStaffCountReportTemplateTest : TemplateTestCase
	{
		public void TestDepartmentStaffCountReport_DepartureDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var employingStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff1.GS_DepartureDate = ZDateTime.Today.AddDays(1);

			var employingStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff2.GS_DepartureDate = ZDateTime.Today.AddDays(2);

			var employingStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff3.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var employingStaff4 = Factory.NewWithValidTestData<GlbStaff>();

			var employingStaff5 = Factory.NewWithValidTestData<GlbStaff>();
			employingStaff5.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var beneficiaryStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff1.GS_DepartureDate = ZDateTime.Today.AddDays(1);

			var beneficiaryStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff2.GS_DepartureDate = ZDateTime.Today.AddDays(2);

			var beneficiaryStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff3.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var beneficiaryStaff4 = Factory.NewWithValidTestData<GlbStaff>();

			var beneficiaryStaff5 = Factory.NewWithValidTestData<GlbStaff>();
			beneficiaryStaff5.GS_DepartureDate = ZDateTime.Today.AddDays(-1);

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "GE1";
			department1.GE_Desc = "GE Name 1";

			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "GE2";
			department2.GE_Desc = "GE Name 2";

			var department3 = Factory.NewWithValidTestData<GlbDepartment>();
			department3.GE_Code = "GE3";
			department3.GE_Desc = "GE Name 3";

			var department4 = Factory.NewWithValidTestData<GlbDepartment>();
			department4.GE_Code = "GE4";
			department4.GE_Desc = "GE Name 4";

			var employingEntity1 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity1.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity1.GHB_GS_Staff = employingStaff1.PK;
			employingEntity1.GHB_GE_Department = department1.PK;

			var employingEntity2 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity2.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity2.GHB_GS_Staff = employingStaff2.PK;
			employingEntity2.GHB_GE_Department = department1.PK;

			var employingEntity3 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity3.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity3.GHB_GS_Staff = employingStaff3.PK;
			employingEntity3.GHB_GE_Department = department1.PK;

			var employingEntity4 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity4.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity4.GHB_GS_Staff = employingStaff4.PK;
			employingEntity4.GHB_GE_Department = department2.PK;

			var employingEntity5 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity5.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			employingEntity5.GHB_GS_Staff = employingStaff5.PK;
			employingEntity5.GHB_GE_Department = department3.PK;

			var beneficiaryEntity1 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity1.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity1.GBB_GS_Staff = beneficiaryStaff3.PK;
			beneficiaryEntity1.GBB_GE_Department = department2.PK;

			var beneficiaryEntity2 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity2.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity2.GBB_GS_Staff = beneficiaryStaff4.PK;
			beneficiaryEntity2.GBB_GE_Department = department3.PK;

			var beneficiaryEntity3 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity3.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity3.GBB_GS_Staff = beneficiaryStaff1.PK;
			beneficiaryEntity3.GBB_GE_Department = department4.PK;

			var beneficiaryEntity4 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity4.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity4.GBB_GS_Staff = beneficiaryStaff2.PK;
			beneficiaryEntity4.GBB_GE_Department = department4.PK;

			var beneficiaryEntity5 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity5.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-1);
			beneficiaryEntity5.GBB_GS_Staff = beneficiaryStaff5.PK;
			beneficiaryEntity5.GBB_GE_Department = department4.PK;

			Factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertEquals("GE Name 1", sheetContent[42, 3].ToString());
				AssertEquals("0", sheetContent[42, 4].ToString());
				AssertEquals("2", sheetContent[42, 5].ToString());
				AssertEquals("GE Name 2", sheetContent[43, 3].ToString());
				AssertEquals("0", sheetContent[43, 4].ToString());
				AssertEquals("1", sheetContent[43, 5].ToString());
				AssertEquals("GE Name 3", sheetContent[44, 3].ToString());
				AssertEquals("1", sheetContent[44, 4].ToString());
				AssertEquals("0", sheetContent[44, 5].ToString());
				AssertEquals("GE Name 4", sheetContent[45, 3].ToString());
				AssertEquals("2", sheetContent[45, 4].ToString());
				AssertEquals("0", sheetContent[45, 5].ToString());
			}
		}

		public void TestDepartmentStaffCountReport_EffectiveDate()
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

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "GE1";
			department1.GE_Desc = "GE Name 1";

			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "GE2";
			department2.GE_Desc = "GE Name 2";

			var department3 = Factory.NewWithValidTestData<GlbDepartment>();
			department3.GE_Code = "GE3";
			department3.GE_Desc = "GE Name 3";

			var department4 = Factory.NewWithValidTestData<GlbDepartment>();
			department4.GE_Code = "GE4";
			department4.GE_Desc = "GE Name 4";

			var department5 = Factory.NewWithValidTestData<GlbDepartment>();
			department5.GE_Code = "GE5";
			department5.GE_Desc = "GE Name 5";

			var department6 = Factory.NewWithValidTestData<GlbDepartment>();
			department6.GE_Code = "GE6";
			department6.GE_Desc = "GE Name 6";

			var employingEntity1 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity1.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			employingEntity1.GHB_GS_Staff = employingStaff1.PK;
			employingEntity1.GHB_GE_Department = department1.PK;

			var employingEntity2 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity2.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-12);
			employingEntity2.GHB_GS_Staff = employingStaff1.PK;
			employingEntity2.GHB_GE_Department = department2.PK;

			var employingEntity3 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity3.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-9);
			employingEntity3.GHB_GS_Staff = employingStaff2.PK;
			employingEntity3.GHB_GE_Department = department3.PK;

			var employingEntity4 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity4.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			employingEntity4.GHB_GS_Staff = employingStaff2.PK;
			employingEntity4.GHB_GE_Department = department4.PK;

			var employingEntity5 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity5.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-9);
			employingEntity5.GHB_GS_Staff = employingStaff3.PK;
			employingEntity5.GHB_GE_Department = department5.PK;

			var employingEntity6 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity6.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-8);
			employingEntity6.GHB_GS_Staff = employingStaff3.PK;
			employingEntity6.GHB_GE_Department = department6.PK;

			var employingEntity7 = Factory.NewWithValidTestData<GlbEmployingBranchDepartment>();
			employingEntity7.GHB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			employingEntity7.GHB_GS_Staff = employingStaff4.PK;
			employingEntity7.GHB_GE_Department = department4.PK;

			var beneficiaryEntity1 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity1.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			beneficiaryEntity1.GBB_GS_Staff = beneficiaryStaff1.PK;
			beneficiaryEntity1.GBB_GE_Department = department1.PK;

			var beneficiaryEntity2 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity2.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-12);
			beneficiaryEntity2.GBB_GS_Staff = beneficiaryStaff1.PK;
			beneficiaryEntity2.GBB_GE_Department = department2.PK;

			var beneficiaryEntity3 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity3.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-9);
			beneficiaryEntity3.GBB_GS_Staff = beneficiaryStaff2.PK;
			beneficiaryEntity3.GBB_GE_Department = department3.PK;

			var beneficiaryEntity4 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity4.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			beneficiaryEntity4.GBB_GS_Staff = beneficiaryStaff2.PK;
			beneficiaryEntity4.GBB_GE_Department = department4.PK;

			var beneficiaryEntity5 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity5.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-8);
			beneficiaryEntity5.GBB_GS_Staff = beneficiaryStaff3.PK;
			beneficiaryEntity5.GBB_GE_Department = department5.PK;

			var beneficiaryEntity6 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity6.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-9);
			beneficiaryEntity6.GBB_GS_Staff = beneficiaryStaff3.PK;
			beneficiaryEntity6.GBB_GE_Department = department6.PK;

			var beneficiaryEntity7 = Factory.NewWithValidTestData<GlbBeneficiaryBranchDepartment>();
			beneficiaryEntity7.GBB_EffectiveDate = ZDateTimeOffset.Today.AddDays(-11);
			beneficiaryEntity7.GBB_GS_Staff = beneficiaryStaff3.PK;
			beneficiaryEntity7.GBB_GE_Department = department6.PK;

			Factory.Save();

			((DateField)Report.FilterCollection["Effective Date"]).Value = ZDateTime.Today.AddDays(-10);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertEquals("GE Name 1", sheetContent[42, 3].ToString());
				AssertEquals("1", sheetContent[42, 4].ToString());
				AssertEquals("1", sheetContent[42, 5].ToString());
				AssertEquals("GE Name 2", sheetContent[43, 3].ToString());
				AssertEquals("0", sheetContent[43, 4].ToString());
				AssertEquals("0", sheetContent[43, 5].ToString());
				AssertEquals("GE Name 3", sheetContent[44, 3].ToString());
				AssertEquals("0", sheetContent[44, 4].ToString());
				AssertEquals("0", sheetContent[44, 5].ToString());
				AssertEquals("GE Name 4", sheetContent[45, 3].ToString());
				AssertEquals("1", sheetContent[45, 4].ToString());
				AssertEquals("2", sheetContent[45, 5].ToString());
				AssertEquals("GE Name 5", sheetContent[46, 3].ToString());
				AssertEquals("0", sheetContent[46, 4].ToString());
				AssertEquals("0", sheetContent[46, 5].ToString());
				AssertEquals("GE Name 6", sheetContent[47, 3].ToString());
				AssertEquals("1", sheetContent[47, 4].ToString());
				AssertEquals("0", sheetContent[47, 5].ToString());
			}
		}
	}
}
