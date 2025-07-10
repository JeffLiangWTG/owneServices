using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationCollection<ConsolidatedDeclaration>))]
	class ConsolidatedDeclarationCollectionTest : ActiveBusinessObjectCollectionTestCase<ConsolidatedDeclarationCollection<ConsolidatedDeclaration>>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ConsolidatedDeclarationCollection<ConsolidatedDeclaration>(Factory, ZString.Empty));
		}

		public void TestFilteredByCompany()
		{
			var currentCompanyDeclaration = Factory.New<ConsolidatedDeclaration>();
			currentCompanyDeclaration.CRD_JobReferenceNumber = "111111";
			currentCompanyDeclaration.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			currentCompanyDeclaration.CRD_ApplicationCode = ConsolidatedDeclaration.ApplicationCodes.TSW;

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			var differentCompanyDeclaration = Factory.New<ConsolidatedDeclaration>();
			differentCompanyDeclaration.CRD_JobReferenceNumber = "222222";
			differentCompanyDeclaration.CRD_GB_Branch = branch.PK;
			differentCompanyDeclaration.CRD_ApplicationCode = ConsolidatedDeclaration.ApplicationCodes.TSW;

			CombineAssertions(() =>
			{
				var cusReconDeclarationCollection = new ConsolidatedDeclarationCollection<ConsolidatedDeclaration>(Factory, ConsolidatedDeclaration.ApplicationCodes.TSW);
				AssertEquals("Current Company", true, currentCompanyDeclaration.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
				AssertEquals("Different Company", false, differentCompanyDeclaration.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
			});
		}

		public void TestFilteredByApplicationCode()
		{
			var valid = Factory.New<ConsolidatedDeclaration>();
			valid.CRD_JobReferenceNumber = "111111";
			valid.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			valid.CRD_ApplicationCode = ConsolidatedDeclaration.ApplicationCodes.TSW;

			var invalid = Factory.New<ConsolidatedDeclaration>();
			invalid.CRD_JobReferenceNumber = "222222";
			invalid.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			invalid.CRD_ApplicationCode = "XXX";

			CombineAssertions(() =>
			{
				var cusReconDeclarationCollection = new ConsolidatedDeclarationCollection<ConsolidatedDeclaration>(Factory, ConsolidatedDeclaration.ApplicationCodes.TSW);
				AssertEquals("Valid", true, valid.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
				AssertEquals("Invalid", false, invalid.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
			});
		}

		protected override ConsolidatedDeclarationCollection<ConsolidatedDeclaration> GetCollectionToTest() => new ConsolidatedDeclarationCollection<ConsolidatedDeclaration>(Factory, ConsolidatedDeclaration.ApplicationCodes.TSW);
	}
}
