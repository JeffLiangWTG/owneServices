using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconDeclarationCollection<CusReconDeclaration>))]
	class CusReconDeclarationCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconDeclarationCollection<CusReconDeclaration>>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CusReconDeclarationCollection<CusReconDeclaration>(Factory, ZString.Empty));
		}

		public void TestFilteredByCompany()
		{
			var currentCompanyDeclaration = Factory.New<CusReconDeclaration>();
			currentCompanyDeclaration.CRD_JobReferenceNumber = "111111";
			currentCompanyDeclaration.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			currentCompanyDeclaration.CRD_ApplicationCode = CusReconDeclarationApplicationCodeList.Codes.CLS;

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			var differentCompanyDeclaration = Factory.New<CusReconDeclaration>();
			differentCompanyDeclaration.CRD_JobReferenceNumber = "222222";
			differentCompanyDeclaration.CRD_GB_Branch = branch.PK;
			differentCompanyDeclaration.CRD_ApplicationCode = CusReconDeclarationApplicationCodeList.Codes.CLS;

			CombineAssertions(() =>
			{
				var cusReconDeclarationCollection = new CusReconDeclarationCollection<CusReconDeclaration>(Factory, CusReconDeclarationApplicationCodeList.Codes.CLS);
				AssertEquals("Current Company", true, currentCompanyDeclaration.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
				AssertEquals("Different Company", false, differentCompanyDeclaration.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
			});
		}

		public void TestFilteredByApplicationCode()
		{
			var valid = Factory.New<CusReconDeclaration>();
			valid.CRD_JobReferenceNumber = "111111";
			valid.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			valid.CRD_ApplicationCode = CusReconDeclarationApplicationCodeList.Codes.CLS;

			var invalid = Factory.New<CusReconDeclaration>();
			invalid.CRD_JobReferenceNumber = "222222";
			invalid.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			invalid.CRD_ApplicationCode = "XXX";

			CombineAssertions(() =>
			{
				var cusReconDeclarationCollection = new CusReconDeclarationCollection<CusReconDeclaration>(Factory, CusReconDeclarationApplicationCodeList.Codes.CLS);
				AssertEquals("Valid", true, valid.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
				AssertEquals("Invalid", false, invalid.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
			});
		}

		protected override CusReconDeclarationCollection<CusReconDeclaration> GetCollectionToTest() => new CusReconDeclarationCollection<CusReconDeclaration>(Factory, CusReconDeclarationApplicationCodeList.Codes.CLS);
	}
}
