using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	abstract class UniversalActivityImplementationTestCase : TestCaseWithUniversalObjectFactory
	{
		protected BusinessObjectFactory BizoFactory => bizoFactory ?? (bizoFactory = new BusinessObjectFactory());
		BusinessObjectFactory bizoFactory;

		protected GlbBranch CreateBranch()
		{
			var branch = BizoFactory.New<GlbBranch>();
			branch.GB_Code = "BRA";
			branch.GB_BranchName = "Branch: that's right, I'm calling the BRANCH that.";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			return branch;
		}

		protected GlbDepartment CreateDepartment()
		{
			var department = BizoFactory.New<GlbDepartment>();
			department.GE_Code = "DEP";
			department.GE_Desc = "Department, NOT BRANCH!";

			return department;
		}

		protected GlbCompany CreateCompany()
		{
			var company = BizoFactory.New<GlbCompany>();
			company.GC_Code = "COM";
			company.GC_Name = "WiseTech Clobal";

			return company;
		}

		protected OrgContact CreateContact()
		{
			var contact = ProcessMgmtTestHelper.CreateOrganizationAndContact(BizoFactory, "Vandelay Industries");
			var address = contact.Header.MainAddress;
			address.Address1 = "123 Canada Street";
			address.Address2 = "Unit 123";
			address.City = "Canada City";
			address.State = "Nova Scotia";
			address.Postcode = "B0V 1A0";
			address.OA_RN_NKCountryCode = "CA";
			address.OA_Email = "some@email.com";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			BizoFactory.Save();

			return contact;
		}
	}
}
