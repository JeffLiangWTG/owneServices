using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class HtmlCommissionAgreementConflictEmailDocumentParserTest : TestCaseWithFactory
	{
		#region Parse

		[ExpectNoExceptions]
		public void TestParse()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var parser = GetNewDocumentParser(emailCreator);
			var actual = parser.Parse(emailCreator, "");
		}

		public void TestParse_SpecificAgreementIDHyperlink()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var parser = GetNewDocumentParser(emailCreator);
			var actual = parser.Parse(emailCreator, "(*SpecificAgreementIDHyperlink*)");
			AssertStartsWith("SpecificAgreementIDHyperlink", @"<a href=""edient:Command=ShowEditForm&ControllerID=OrgCommissionAgreement&BusinessEntityPK=" + SpecificAgreement.PK + @"&VersionNumber=" + new EnterpriseInformationRetriever().VersionNumber + @"&Hash=", actual);
			AssertEndsWith("SpecificAgreementIDHyperlink", @">O0001001#1</a>", actual);
		}

		public void TestParse_GenericAgreementIDHyperlink()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var parser = GetNewDocumentParser(emailCreator);
			var actual = parser.Parse(emailCreator, "(*GenericAgreementIDHyperlink*)");
			AssertStartsWith("SpecificAgreementIDHyperlink", @"<a href=""edient:Command=ShowEditForm&ControllerID=OrgCommissionAgreement&BusinessEntityPK=" + GenericAgreement.PK + @"&VersionNumber=" + new EnterpriseInformationRetriever().VersionNumber + @"&Hash=", actual);
			AssertEndsWith("SpecificAgreementIDHyperlink", @">O0001001#2</a>", actual);
		}

		public void TestParse_SpecificAgreementItems()
		{
			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var conflicts = new[] { new CommissionAgreementItemConflict(SpecificAgreementItemEntHos, GenericAgreementItemAll), new CommissionAgreementItemConflict(SpecificAgreementItemEntOdmPav, GenericAgreementItemAll) };
				var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, conflicts);
				var parser = GetNewDocumentParser(emailCreator);
				var actual = parser.Parse(emailCreator, "(*SpecificAgreementItems*)");
				AssertMultilineASCIIEquals("SpecificAgreementItems", "<ul><li>ENT &gt; HOS &gt; ALL</li><li>ENT &gt; ODM &gt; PAV</li></ul>", actual);
			}
		}

		public void TestParse_SpecificAgreementItems_WithConditions()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();

			var agreementItem_XXX_XXX_AUSYD_UAIEV = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "XXX", "XXX", "AUSYD", "UAIEV");
			var agreementItem_XXX_XXX_AUSYD_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "XXX", "XXX", "AUSYD", "");
			var agreementItem_YYY_YYY_USLAX_GBLON = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "YYY", "YYY", "USLAX", "GBLON");
			var agreementItem_YYY_YYY_USLAX_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "YYY", "YYY", "AUSYD", "");

			var conflicts = new[]
			{
				new CommissionAgreementItemConflict(agreementItem_XXX_XXX_AUSYD_UAIEV, GenericAgreementItemAll),
				new CommissionAgreementItemConflict(agreementItem_YYY_YYY_USLAX_GBLON, GenericAgreementItemAll)
			};

			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, conflicts);
			var parser = GetNewDocumentParser(emailCreator);
			var actual = parser.Parse(emailCreator, "(*SpecificAgreementItems*)");
			AssertMultilineASCIIEquals("SpecificAgreementItems",
				@"<ul><li>XXX | AUSYD &gt; ALL | XXX 
 XXX | AUSYD &gt; UAIEV | XXX</li><li>YYY | AUSYD &gt; ALL | YYY 
 YYY | USLAX &gt; GBLON | YYY</li></ul>
", actual);
		}

		#endregion

		#region Implementation
		
		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			Staff = Factory.NewWithValidTestData<GlbStaff>();
			Staff.GS_Code = "ADL";
			Staff.GS_FullName = "Andrew";
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.OH_FullName = "WiseTech Global";

			Opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Opportunity.P8_OpportunityID = "O0001001";
			SpecificAgreement = Opportunity.ApprovedCommissionAgreements.AddNew();
			SpecificAgreement.FillWithValidTestData();
			SpecificAgreement.CA0_Name = "#1";
			SpecificAgreement.ProductItems.DeleteAll();

			var specificAgreementItemEnt =
				SpecificAgreement.ProductItems.AddNew(true, "ENT");

			SpecificAgreementItemEntHos =
				specificAgreementItemEnt.ChildServiceItems.AddNew(true, "HOS");

			SpecificAgreementItemEntOdmPav =
				specificAgreementItemEnt.ChildServiceItems.AddNew(true, "ODM")
				.ChildSubModuleItems.AddNew(true, "PAV");

			GenericAgreement = Opportunity.ApprovedCommissionAgreements.AddNew();
			GenericAgreement.FillWithValidTestData();
			GenericAgreement.CA0_Name = "#2";
			GenericAgreement.CA0_OH_Customer = Org.PK;
			GenericAgreement.ProductItems.DeleteAll();

			GenericAgreementItemAll = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(GenericAgreement, OrgCommissionAgreementItemLookups.AllProductsCode, OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);

			GenericAgreementRecipient1 = GenericAgreement.Recipients.AddNew();
			GenericAgreementRecipient1.CAR_GS_NKStaff = "ADL";
			GenericAgreementRecipient2 = GenericAgreement.Recipients.AddNew();
			GenericAgreementRecipient2.CAR_OH_Party = Org.PK;

			Factory.Save();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}

		GlbStaff Staff;
		OrgHeader Org;
		OrgOpportunity Opportunity;
		OrgCommissionAgreement SpecificAgreement;
		OrgCommissionAgreementItem SpecificAgreementItemEntHos;
		OrgCommissionAgreementItem SpecificAgreementItemEntOdmPav;
		OrgCommissionAgreement GenericAgreement;
		OrgCommissionAgreementItem GenericAgreementItemAll;
		OrgCommissionAgreementRecipient GenericAgreementRecipient1;
		OrgCommissionAgreementRecipient GenericAgreementRecipient2;

		HtmlCommissionAgreementConflictEmailDocumentParser GetNewDocumentParser(CommissionAgreementConflictEmailCreator agreementOverlapEmailCreator)
		{
			return new HtmlCommissionAgreementConflictEmailDocumentParser(Factory);
		}

		#endregion
	}
}
