using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbSecuritySortIndexCalculatorTest : TestCaseWithFactory
	{
		public void TestGlbSecuritySortIndexCalculator()
		{
			GlbSecurityCollection securityCollection = new GlbSecurityCollection(Factory);

			GlbGroup groupAAA = Factory.NewWithValidTestData<GlbGroup>();
			groupAAA.GG_Code = "AAA";

			GlbGroup groupZZZ = Factory.NewWithValidTestData<GlbGroup>();
			groupZZZ.GG_Code = "ZZZ";

			GlbSecurity aAASecurityRecordEmpty = Factory.New<GlbSecurity>();
			aAASecurityRecordEmpty.GU_GG = groupAAA.PK;
			securityCollection.Add(aAASecurityRecordEmpty);

			GlbSecurity aAASecurityRecordWithCompany = Factory.New<GlbSecurity>();
			aAASecurityRecordWithCompany.GU_GC = Env.CurrentCompany.PK;
			aAASecurityRecordWithCompany.GU_GG = groupAAA.PK;
			securityCollection.Add(aAASecurityRecordWithCompany);

			GlbSecurity zZZSecurityRecordEmpty = Factory.New<GlbSecurity>();
			zZZSecurityRecordEmpty.GU_GG = groupZZZ.PK;
			securityCollection.Add(zZZSecurityRecordEmpty);

			GlbSecurity zZZSecurityRecordWithCompany = Factory.New<GlbSecurity>();
			zZZSecurityRecordWithCompany.GU_GC = Env.CurrentCompany.PK;
			zZZSecurityRecordWithCompany.GU_GG = groupZZZ.PK;
			securityCollection.Add(zZZSecurityRecordWithCompany);

			GlbSecurity aAASecurityRecordWithBranch = Factory.New<GlbSecurity>();
			aAASecurityRecordWithBranch.GU_GB = Env.CurrentBranch.PK;
			aAASecurityRecordWithBranch.GU_GG = groupAAA.PK;
			securityCollection.Add(aAASecurityRecordWithBranch);

			GlbSecurity zZZSecurityRecordWithBranch = Factory.New<GlbSecurity>();
			zZZSecurityRecordWithBranch.GU_GB = Env.CurrentBranch.PK;
			zZZSecurityRecordWithBranch.GU_GG = groupZZZ.PK;
			securityCollection.Add(zZZSecurityRecordWithBranch);

			GlbSecurity zZZSecurityRecordWithCompanyAndDepartment = Factory.New<GlbSecurity>();
			zZZSecurityRecordWithCompanyAndDepartment.GU_GC = Env.CurrentCompany.PK;
			zZZSecurityRecordWithCompanyAndDepartment.GU_GE = Env.CurrentDepartment.PK;
			zZZSecurityRecordWithCompanyAndDepartment.GU_GG = groupZZZ.PK;
			securityCollection.Add(zZZSecurityRecordWithCompanyAndDepartment);

			GlbSecurity aAASecurityRecordWithCompanyAndDepartment = Factory.New<GlbSecurity>();
			aAASecurityRecordWithCompanyAndDepartment.GU_GC = Env.CurrentCompany.PK;
			aAASecurityRecordWithCompanyAndDepartment.GU_GE = Env.CurrentDepartment.PK;
			aAASecurityRecordWithCompanyAndDepartment.GU_GG = groupAAA.PK;
			securityCollection.Add(aAASecurityRecordWithCompanyAndDepartment);

			GlbSecurity zZZSecurityRecordWithBranchAndDepartment = Factory.New<GlbSecurity>();
			zZZSecurityRecordWithBranchAndDepartment.GU_GB = Env.CurrentBranch.PK;
			zZZSecurityRecordWithBranchAndDepartment.GU_GE = Env.CurrentDepartment.PK;
			zZZSecurityRecordWithBranchAndDepartment.GU_GG = groupZZZ.PK;
			securityCollection.Add(zZZSecurityRecordWithBranchAndDepartment);

			GlbSecurity aAASecurityRecordWithBranchAndDepartment = Factory.New<GlbSecurity>();
			aAASecurityRecordWithBranchAndDepartment.GU_GB = Env.CurrentBranch.PK;
			aAASecurityRecordWithBranchAndDepartment.GU_GE = Env.CurrentDepartment.PK;
			aAASecurityRecordWithBranchAndDepartment.GU_GG = groupAAA.PK;
			securityCollection.Add(aAASecurityRecordWithBranchAndDepartment);

			securityCollection.Sort(new GlbSecuritySortIndexCalculator());

			AssertEquals("Should be Empty Security Record for group AAA", aAASecurityRecordEmpty, securityCollection[0]);
			AssertEquals("Should be Empty Security Record for group ZZZ", zZZSecurityRecordEmpty, securityCollection[1]);
			AssertEquals("Should be Security Record with company for group AAA", aAASecurityRecordWithCompany, securityCollection[2]);
			AssertEquals("Should be Security Record with branch for group AAA", aAASecurityRecordWithBranch, securityCollection[3]);
			AssertEquals("Should be Security Record with company & department for group AAA", aAASecurityRecordWithCompanyAndDepartment, securityCollection[4]);
			AssertEquals("Should be Security Record with branch & department for group AAA", aAASecurityRecordWithBranchAndDepartment, securityCollection[5]);
			AssertEquals("Should be Security Record with company for group ZZZ", zZZSecurityRecordWithCompany, securityCollection[6]);
			AssertEquals("Should be Security Record with branch for group ZZZ", zZZSecurityRecordWithBranch, securityCollection[7]);
			AssertEquals("Should be Security Record with company & department for group ZZZ", zZZSecurityRecordWithCompanyAndDepartment, securityCollection[8]);
			AssertEquals("Should be Security Record with branch & department for group ZZZ", zZZSecurityRecordWithBranchAndDepartment, securityCollection[9]);
		}
	}
}
