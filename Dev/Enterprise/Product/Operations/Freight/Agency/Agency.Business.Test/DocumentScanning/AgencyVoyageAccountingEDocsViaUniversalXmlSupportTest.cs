using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Test.DocumentScanning
{
	class AgencyVoyageAccountingEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadAgencyVoyageAccounting()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.CompanyName = "testCompany1";
			company1.GC_Code = "TC1";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.CompanyName = "testCompany2";
			company2.GC_Code = "TC2";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = "testBranch1";
			branch1.GB_Code = "TB1";
			branch1.GB_GC = company1.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_BranchName = "testBranch2";
			branch2.GB_Code = "TB2";
			branch2.GB_GC = company2.PK;

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "TD1";
			department.GE_Desc = "Test Department 1";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "XXX";

			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			jobVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var voyageAccount1 = Factory.New<VoyageAccount>();
				voyageAccount1.NA_JV = jobVoyage.PK;
				voyageAccount1.NA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				voyageAccount1.NA_JobNumber = "VA00000001";
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var loader = new AgencyVoyageAccountingEDocsViaUniversalXmlSupport();
				AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, "VA00000001"));

				var voyageAccount2 = Factory.New<VoyageAccount>();
				voyageAccount2.NA_JV = jobVoyage.PK;
				voyageAccount2.NA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				voyageAccount2.NA_JobNumber = "VA00000001";
				Factory.Save();

				AssertEquals(voyageAccount2.PK, loader.LoadBusinessObjectFromCode(Factory, "VA00000001")?.PK);
			}
		}

		public void TestTryLoadAgencyVoyageAccountingNotInDb()
		{
			var loader = new AgencyVoyageAccountingEDocsViaUniversalXmlSupport();
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, "VA99999999"));
		}
	}
}
