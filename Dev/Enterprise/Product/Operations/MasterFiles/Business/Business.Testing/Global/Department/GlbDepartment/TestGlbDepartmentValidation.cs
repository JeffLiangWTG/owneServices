using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestGlbDepartmentValidation : BusinessObjectValidationTestCase
	{
		public void TestGE_IsActive()
		{
			GlbDepartment[] departments = Factory.Load<GlbDepartment>(new ZQuery());
			for (int i = 1; i < departments.Length; i++)
			{
				departments[i].GE_IsActive = false;
			}
			Factory.Save();

			Assert("should pass", !departments[0].GE_IsActiveInfo.HasErrors());
			departments[0].GE_IsActive = false;
			Assert("should have error", departments[0].GE_IsActiveInfo.HasErrors());
		}

		public void TestCheckGE_Desc()
		{
			testGlbDepartment.GE_Desc = "";
			Assert("error: no description", testGlbDepartment.GE_DescInfo.HasErrors());
			testGlbDepartment.GE_Desc = "p";
			Assert("error: description length < 4", testGlbDepartment.GE_DescInfo.HasErrors());
			testGlbDepartment.GE_Desc = "pppp";
			Assert("should pass", !testGlbDepartment.GE_DescInfo.HasErrors());
		}

		public void TestCheckGE_GE()
		{
			testGlbDepartment.GE_GE = testGlbDepartment.PK;
			Assert("error: department is parent of itself", testGlbDepartment.GE_GEInfo.HasErrors());
		}

		public void TestCheckGE_Code()
		{
			testGlbDepartment.GE_Code = "";
			Assert("Property Info should have errors - " + testGlbDepartment.GE_Code, testGlbDepartment.GE_CodeInfo.HasErrors());
			testGlbDepartment.GE_Code = "COD";
			Assert("Property Info should not have errors - " + testGlbDepartment.GE_Code, !testGlbDepartment.GE_CodeInfo.HasErrors());
			testGlbDepartment.GE_Code = "C_D";
			Assert("Property Info should have errors - " + testGlbDepartment.GE_Code, testGlbDepartment.GE_CodeInfo.HasErrors());
			testGlbDepartment.GE_Code = "C1 ";
			Assert("Property Info should have errors - " + testGlbDepartment.GE_Code, testGlbDepartment.GE_CodeInfo.HasErrors());
			testGlbDepartment.GE_Code = "CO#";
			Assert("Property Info should have errors - " + testGlbDepartment.GE_Code, testGlbDepartment.GE_CodeInfo.HasErrors());
		}

		public void TestUniqueDepartmentCodes()
		{
			var queryForSystemDepts = new ZQuery(GlbDepartmentSchema.GE_SystemCode, true);
			queryForSystemDepts.MaximumRows = 1;

			var systemDepartments = Factory.Load<GlbDepartment>(queryForSystemDepts);

			testGlbDepartment.GE_Code = systemDepartments.First().GE_Code;
			AssertHasError(testGlbDepartment.GE_CodeInfo, "Code must be unique.");
		}

		public void TestUniqueDepartmentCodes_WhenProductivityWise_ShouldReportSpecialError()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var queryForSystemDepts = new ZQuery(GlbDepartmentSchema.GE_SystemCode, true);
			queryForSystemDepts.MaximumRows = 1;

			var systemDepartments = Factory.Load<GlbDepartment>(queryForSystemDepts);

			testGlbDepartment.GE_Code = systemDepartments.Single().GE_Code;
			AssertHasError(testGlbDepartment.GE_CodeInfo, "Code is reserved for system usage.");
		}

		BusinessObjectFactory testFactory;
		GlbDepartment testGlbDepartment;

		protected override void SetUp()
		{
			base.SetUp();
			testFactory = new BusinessObjectFactory();
			testGlbDepartment = testFactory.New<GlbDepartment>();
		}
	}
}
