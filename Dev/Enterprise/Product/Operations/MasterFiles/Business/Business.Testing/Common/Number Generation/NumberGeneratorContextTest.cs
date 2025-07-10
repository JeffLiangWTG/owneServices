using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NumberGeneratorContextTest : TestCaseWithFactory
	{
		public void TestParameterlessContext()
		{
			NumberGeneratorContext context = new NumberGeneratorContext();

			AssertEquals(GlbCompany.CurrentCompany.GC_Code, context.CompanyValue(Factory, (c) => c.GC_Code));
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, context.BranchValue(Factory, (b) => b.GB_Code));
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, context.DepartmentValue(Factory, (d) => d.GE_Code));
		}

		public void TestExplicitContext()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			department.GE_Code = "WWW";

			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "XXX";

			GlbBranch branch1 = company.Branches.AddNew();
			branch1.GB_Code = "YYY";

			GlbBranch branch2 = company.Branches.AddNew();
			branch2.GB_Code = "ZZZ";

			Factory.Save();

			NumberGeneratorContext context1 = new NumberGeneratorContext(branch1.GB_GC, branch1.PK, department.PK);
			NumberGeneratorContext context2 = new NumberGeneratorContext(branch2.GB_GC, branch2.PK, ZGuid.Empty);

			AssertEquals("XXX", context1.CompanyValue(Factory, (c) => c.GC_Code));
			AssertEquals("YYY", context1.BranchValue(Factory, (b) => b.GB_Code));
			AssertEquals("WWW", context1.DepartmentValue(Factory, (d) => d.GE_Code));

			AssertEquals("XXX", context2.CompanyValue(Factory, (c) => c.GC_Code));
			AssertEquals("ZZZ", context2.BranchValue(Factory, (b) => b.GB_Code));
			AssertEquals("", context2.DepartmentValue(Factory, (d) => d.GE_Code));
		}
	}
}
