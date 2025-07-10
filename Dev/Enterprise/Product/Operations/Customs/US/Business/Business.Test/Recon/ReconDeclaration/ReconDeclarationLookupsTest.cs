using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconDeclarationLookupsTest : TestCaseWithFactory
	{
		public void TestPaymentTypeList()
		{
			Assert(!lookups.US_PaymentTypeList.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			Assert(!lookups.US_PaymentTypeList.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			Assert(!lookups.US_PaymentTypeList.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));
			AssertEquals("sorted", PaymentTypeList.Codes.IndividualBasis, lookups.US_PaymentTypeList[0].Code);
		}

		public void TestSuretyCodeList()
		{
			OrgHeader imp = Factory.New<OrgHeader>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			reconDec.JE_OH_Importer = imp.PK;
			OrgHeaderWrapper iORWrapper = reconDec.IORWrapper;
			ReconDeclarationLookups lookups = reconDec.Lookups;
			AssertEquals(0, lookups.SuretyCodeList.Count);
			CusBondDetail bondDetail1 = iORWrapper.BondDetails.AddNew();
			bondDetail1.PW_SuretyCode = "791";
			var list = lookups.SuretyCodeList;
			AssertEquals(1, list.Count);
			AssertEquals(true, list.ContainsCode("791"));
			CusBondDetail bondDetail2 = iORWrapper.BondDetails.AddNew();
			list = lookups.SuretyCodeList;
			AssertEquals(1, list.Count);
			AssertEquals(true, list.ContainsCode("791"));
			bondDetail2.PW_SuretyCode = "798";
			list = lookups.SuretyCodeList;
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode("791"));
			AssertEquals(true, list.ContainsCode("798"));
		}

		public void TestTeamNoList()
		{
			AssertNotNull(lookups.US_TeamNoList);
		}

		public void TestReconPortList()
		{
			AssertNotNull(lookups.SchDPortList);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(GlbStaffCollection), lookups.CusAgents.GetType());
			AssertEquals(typeof(CusStatementHeaderCollection), lookups.StatementList.GetType());
			AssertEquals(typeof(RefServiceLevelCollection), lookups.ServiceLevels.GetType());
		}

		public void TestUS_YesNoList()
		{
			AssertEquals(typeof(YesNoDefaultList), lookups.US_YesNoList.GetType());
			Assert("Should contain No", lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.No));
			Assert("Should contain Yes", lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.Yes));
			Assert("Should not contain Default", !lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.Default));
		}

		public void TestUS_IssueCodeList()
		{
			AssertEquals(typeof(ReconIssueCodeList), lookups.US_IssueCodeList.GetType());
			Assert("List should not contain 'Not Applicable' (NA) code when used in Recon Declaration", !lookups.US_IssueCodeList.ContainsCode(ReconIssueCodeList.Codes.NotApplicable));
		}

		ReconDeclarationLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			lookups = new ReconDeclarationLookups(reconDec, declaration);
		}
	}
}
