using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfigurationCollection;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCashAdvanceDefaultingConfigurationCollection))]
	sealed class AccCashAdvanceDefaultingConfigurationCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Loading Collection for a given Level

		public void TestCompanyLevelCollectionReturnsCorrectSets()
		{
			var (companyPk, _, _, expectedCompanyConfig, _, _, _) = PrepareTestData();

			var companyLevelCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, companyPk);

			AssertEquals(AccCashAdvanceDefaultingLevel.Company, companyLevelCollection.Level);

			companyLevelCollection.Load();

			Assert(companyLevelCollection.IsLoaded);
			AssertEquals(1, companyLevelCollection.Count);

			var companyLevelConfigs = companyLevelCollection.Cast<AccCashAdvanceDefaultingConfiguration>().FirstOrDefault();

			AssertNotNull(companyLevelConfigs);
			AssertEquals(AccCashAdvanceDefaultingLevel.Company, companyLevelConfigs.Level);
			AssertEquals(expectedCompanyConfig, companyLevelConfigs.PK);

			AssertAddingNewObjectToCollection(companyLevelCollection, ZGuid.Empty);
		}

		public void TestBranchLevelCollectionReturnsCorrectSets()
		{
			var (companyPk, branchPk, _, expectedCompanyConfig, expectedBranchConfig, _, _) = PrepareTestData();

			var branchLevelCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, companyPk, branchPk);

			AssertEquals(AccCashAdvanceDefaultingLevel.Branch, branchLevelCollection.Level);

			branchLevelCollection.Load();

			Assert(branchLevelCollection.IsLoaded);
			AssertEquals(2, branchLevelCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { expectedCompanyConfig, expectedBranchConfig }, branchLevelCollection.Select(x => x.PK));

			AssertAddingNewObjectToCollection(branchLevelCollection, branchPk);
		}

		public void TestDebtorLevelCollectionReturnsCorrectSets()
		{
			var (companyPk, branchPk, organizationPk, expectedCompanyConfig, expectedBranchConfig, expectedDebtorLevelConfig, _) = PrepareTestData();

			var debtorLevelCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, companyPk, branchPk, organizationPk, LedgerTypes.AccountsReceivable);

			AssertEquals(AccCashAdvanceDefaultingLevel.Debtor, debtorLevelCollection.Level);

			debtorLevelCollection.Load();

			Assert(debtorLevelCollection.IsLoaded);
			AssertEquals(3, debtorLevelCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { expectedCompanyConfig, expectedBranchConfig, expectedDebtorLevelConfig }, debtorLevelCollection.Select(x => x.PK));

			AssertAddingNewObjectToCollection(debtorLevelCollection, organizationPk);
		}

		public void TestCreditorLevelCollectionReturnsCorrectSets()
		{
			var (companyPk, branchPk, organizationPk, expectedCompanyConfig, expectedBranchConfig, _, expectedCreditorLevelConfig) = PrepareTestData();

			var creditorLevelCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, companyPk, branchPk, organizationPk, LedgerTypes.AccountsPayable);

			AssertEquals(AccCashAdvanceDefaultingLevel.Creditor, creditorLevelCollection.Level);

			creditorLevelCollection.Load();

			Assert(creditorLevelCollection.IsLoaded);
			AssertEquals(3, creditorLevelCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { expectedCompanyConfig, expectedBranchConfig, expectedCreditorLevelConfig }, creditorLevelCollection.Select(x => x.PK));

			AssertAddingNewObjectToCollection(creditorLevelCollection, organizationPk);
		}

		void AssertAddingNewObjectToCollection(AccCashAdvanceDefaultingConfigurationCollection col, ZGuid parentPk)
		{
			var bizo = col.AddNew();
			AssertEquals("Level", col.Level, bizo.Level);
			if (col.Level == AccCashAdvanceDefaultingLevel.Company)
			{
				AssertEquals("CAC_ParentID", ZGuid.Empty, bizo.CAC_ParentId);
				AssertEquals("CAC_ParentTableCode", ZString.Empty, bizo.CAC_ParentTableCode);
			}
			else
			{
				AssertEquals("CAC_ParentID", parentPk, bizo.CAC_ParentId);
				if (col.Level == AccCashAdvanceDefaultingLevel.Branch)
				{
					AssertEquals("CAC_ParentTableCode", GlbBranchSchema.Constants.Prefix, bizo.CAC_ParentTableCode);
				}
				else if (col.Level == AccCashAdvanceDefaultingLevel.Debtor)
				{
					AssertEquals("CAC_ParentTableCode", OrgHeaderSchema.Constants.Prefix, bizo.CAC_ParentTableCode);
					AssertEquals("CAC_Ledger", LedgerTypes.AccountsReceivable, bizo.CAC_Ledger);
				}
				else if (col.Level == AccCashAdvanceDefaultingLevel.Creditor)
				{
					AssertEquals("CAC_ParentTableCode", OrgHeaderSchema.Constants.Prefix, bizo.CAC_ParentTableCode);
					AssertEquals("CAC_Ledger", LedgerTypes.AccountsPayable, bizo.CAC_Ledger);
				}
				else
				{
					AssertEquals("CAC_ParentTableCode", ZString.Empty, bizo.CAC_ParentTableCode);
				}
			}
		}

		#endregion

		public void TestCanNotCreateOrganizationLevelCollectionWithEmptyOrInvalidLedgerType()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			AssertExceptionThrown<ArgumentException>("Empty Ledger", () => new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, organization.PK, ZString.Empty));
			AssertExceptionThrown<ArgumentException>("Invalid Ledger", () => new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, organization.PK, LedgerTypes.CashBook));
		}

		public void TestOnChildDefaultingOptionChanged()
		{
			var companyLevelCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK);
			companyLevelCollection.OnChildDefaultingOptionChanged += CompanyLevelCollection_OnChildDefaultingOptionChanged;

			var changedConfigPk = ZGuid.Empty;
			var shouldHideChargeGroupAndChargeCodeGrids = false;

			var companyLevelConfig1 = CreateCashAdvanceConfig(GlbCompany.CurrentCompany.PK, ZGuid.Empty, defaultingOption: CashAdvanceDefaultingOption.All);
			var companyLevelConfig2 = CreateCashAdvanceConfig(GlbCompany.CurrentCompany.PK, ZGuid.Empty, ledger: LedgerTypes.AccountsPayable, defaultingOption: CashAdvanceDefaultingOption.All);
			companyLevelCollection.Add(companyLevelConfig1);
			companyLevelCollection.Add(companyLevelConfig2);

			AssertEquals(ZGuid.Empty, changedConfigPk);
			Assert(!shouldHideChargeGroupAndChargeCodeGrids);

			companyLevelConfig1.CAC_DefaultingOption = CashAdvanceDefaultingOption.None;

			AssertEquals(companyLevelConfig1.PK, changedConfigPk);
			Assert(shouldHideChargeGroupAndChargeCodeGrids);

			companyLevelConfig1.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

			AssertEquals(companyLevelConfig1.PK, changedConfigPk);
			Assert(!shouldHideChargeGroupAndChargeCodeGrids);

			companyLevelConfig1.CAC_DefaultingOption = CashAdvanceDefaultingOption.All;

			AssertEquals(companyLevelConfig1.PK, changedConfigPk);
			Assert(shouldHideChargeGroupAndChargeCodeGrids);

			companyLevelCollection.Remove(companyLevelConfig1);
			companyLevelConfig1.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

			AssertEquals(companyLevelConfig1.PK, changedConfigPk);
			Assert(shouldHideChargeGroupAndChargeCodeGrids);

			companyLevelConfig2.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

			AssertEquals(companyLevelConfig2.PK, changedConfigPk);
			Assert(!shouldHideChargeGroupAndChargeCodeGrids);

			var companyLevelConfig3 = (AccCashAdvanceDefaultingConfiguration)((IBindingList)companyLevelCollection).AddNew(); // uncomitted element
			companyLevelConfig3.CAC_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			companyLevelConfig3.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
			AssertEquals(CashAdvanceDefaultingOption.All, companyLevelConfig3.CAC_DefaultingOption);

			companyLevelConfig3.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

			AssertEquals(companyLevelConfig3.PK, changedConfigPk);
			Assert(!shouldHideChargeGroupAndChargeCodeGrids);

			companyLevelConfig3.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

			void CompanyLevelCollection_OnChildDefaultingOptionChanged(object sender, DefaultingOptionChangedEventArgs e)
			{
				changedConfigPk = e.ConfigPk;
				shouldHideChargeGroupAndChargeCodeGrids = e.ShouldHide;
			}
		}

		#region Helpers

		(ZGuid companyA, ZGuid branchA, ZGuid organizationA, ZGuid companyLevelConfig, ZGuid branchLevelConfig, ZGuid debtorLevelConfig, ZGuid creditorLevelConfig) PrepareTestData()
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			var branchA = Factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = companyA.PK;
			var organizationA = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var companyLevelConfig = CreateCashAdvanceConfigAndReturnPK(companyA.PK, ZGuid.Empty);

			var branchLevelConfig = CreateCashAdvanceConfigAndReturnPK(companyA.PK, branchA.PK, GlbBranchSchema.Constants.Prefix);

			var debtorLevelConfig = CreateCashAdvanceConfigAndReturnPK(companyA.PK, organizationA.PK, OrgHeaderSchema.Constants.Prefix, ledger: LedgerTypes.AccountsReceivable);
			var creditorLevelConfig = CreateCashAdvanceConfigAndReturnPK(companyA.PK, organizationA.PK, OrgHeaderSchema.Constants.Prefix, ledger: LedgerTypes.AccountsPayable);

			return (companyA.PK, branchA.PK, organizationA.PK, companyLevelConfig, branchLevelConfig, debtorLevelConfig, creditorLevelConfig);
		}

		AccCashAdvanceDefaultingConfiguration CreateCashAdvanceConfig(ZGuid parentCompanyPk, ZGuid parentPK, string parentTableCode = "", string ledger = "", string defaultingOption = "ALL")
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_GC = parentCompanyPk.IsEmpty ? GlbCompany.CurrentCompany.PK : parentCompanyPk;
			config.CAC_Ledger = ledger;
			config.CAC_ParentTableCode = parentTableCode;
			config.CAC_ParentId = parentPK;
			config.CAC_ServiceDirection = "IMP";
			config.CAC_TransportMode = "SEA";
			config.CAC_DefaultingOption = defaultingOption;
			return config;
		}

		ZGuid CreateCashAdvanceConfigAndReturnPK(ZGuid parentCompanyPk, ZGuid parentPK, string parentTableCode = "", string ledger = "", string defaultingOption = "ALL")
		{
			return CreateCashAdvanceConfig(parentCompanyPk, parentPK, parentTableCode, ledger, defaultingOption).PK;
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK);
		}

		#endregion
	}
}
