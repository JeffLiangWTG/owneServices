using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocCommissionAgreementConflictEmailCreatorTest : TestCaseWithFactory
	{
		#region Document Fields

		public void TestSpecificAgreementID()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var docWrapper = GetDocumentWrapper(emailCreator);
			AssertEquals("O0001001#1", docWrapper.SpecificAgreementID);
		}

		public void TestSpecificAgreementIDHyperlink()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var docWrapper = GetDocumentWrapper(emailCreator);
			AssertEquals("O0001001#1", docWrapper.SpecificAgreementIDHyperlink);
		}

		public void TestSpecificAgreementCreateUser()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var docWrapper = GetDocumentWrapper(emailCreator);
			using (Env.SetTemporaryUserContext(Staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Andrew (ADL)", docWrapper.SpecificAgreementCreateUser);
			}
		}

		public void TestSpecificAgreementItems()
		{
			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var conflicts = new[] { new CommissionAgreementItemConflict(SpecificAgreementItemEntHos, GenericAgreementItemAll), new CommissionAgreementItemConflict(SpecificAgreementItemEntOdmPav, GenericAgreementItemAll) };
				var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, conflicts);
				var docWrapper = GetDocumentWrapper(emailCreator);

				AssertMultilineASCIIEquals("SpecificAgreementItems",
	@"   ENT > HOS > ALL
   ENT > ODM > PAV", docWrapper.SpecificAgreementItems);
			}
		}

		public void TestSpecificAgreementItems_WithConditions()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();

			var agreementItem_XXX_XXX_AUSYD_UAIEV = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "XXX", "XXX", "AUSYD", "UAIEV");
			var agreementItem_XXX_XXX_AUSYD_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "XXX", "XXX", "AUSYD", "");
			var agreementItem_YYY_YYY_USLAX_GBLON = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "YYY", "YYY", "USLAX", "GBLON");
			var agreementItem_YYY_YYY_USLAX_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "YYY", "YYY", "AUSYD", "");

			var conflicts = new[] {
				new CommissionAgreementItemConflict(agreementItem_XXX_XXX_AUSYD_UAIEV, agreementItem_XXX_XXX_AUSYD_ALL),
				new CommissionAgreementItemConflict(agreementItem_YYY_YYY_USLAX_GBLON, agreementItem_YYY_YYY_USLAX_ALL) };

			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, agreement, GenericAgreement, conflicts);
			var docWrapper = GetDocumentWrapper(emailCreator);

			AssertMultilineASCIIEquals("SpecificAgreementItems",
@"   XXX | AUSYD > ALL | XXX 
   XXX | AUSYD > UAIEV | XXX
   YYY | AUSYD > ALL | YYY 
   YYY | USLAX > GBLON | YYY", docWrapper.SpecificAgreementItems);
		}

		public void TestGenericAgreementID()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var docWrapper = GetDocumentWrapper(emailCreator);
			AssertEquals("O0001001#2", docWrapper.GenericAgreementID);
		}

		public void TestGenericAgreementIDHyperlink()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var docWrapper = GetDocumentWrapper(emailCreator);
			AssertEquals("O0001001#2", docWrapper.GenericAgreementIDHyperlink);
		}

		public void TestGenericAgreementCustomer()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var docWrapper = GetDocumentWrapper(emailCreator);
			AssertEquals("WiseTech Global", docWrapper.GenericAgreementCustomer);
		}

		public void TestRecipientName()
		{
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, SpecificAgreement, GenericAgreement, System.Array.Empty<ICommissionAgreementConflict>());
			var docWrapper = GetDocumentWrapper(emailCreator);
			AssertEquals("", docWrapper.RecipientName);

			emailCreator.RecipientName = "Andrew";
			AssertEquals("Andrew", docWrapper.RecipientName);

			emailCreator.RecipientName = "WiseTech Global";
			AssertEquals("WiseTech Global", docWrapper.RecipientName);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

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

			Factory.Save();
		}

		GlbStaff Staff;
		OrgHeader Org;
		OrgOpportunity Opportunity;
		OrgCommissionAgreement SpecificAgreement;
		OrgCommissionAgreementItem SpecificAgreementItemEntHos;
		OrgCommissionAgreementItem SpecificAgreementItemEntOdmPav;
		OrgCommissionAgreement GenericAgreement;
		OrgCommissionAgreementItem GenericAgreementItemAll;

		DocCommissionAgreementConflictEmailCreator GetDocumentWrapper(CommissionAgreementConflictEmailCreator agreementOverlapEmailCreator)
		{
			return DocCommissionAgreementConflictEmailCreator.New(agreementOverlapEmailCreator, Factory);
		}

		#endregion
	}
}
