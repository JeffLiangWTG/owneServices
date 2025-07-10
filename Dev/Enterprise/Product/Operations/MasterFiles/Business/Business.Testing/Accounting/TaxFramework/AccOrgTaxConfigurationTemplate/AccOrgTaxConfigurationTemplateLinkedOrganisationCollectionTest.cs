using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccOrgTaxConfigurationTemplateLinkedOrganisationCollection))]
	sealed class AccOrgTaxConfigurationTemplateLinkedOrganisationCollectionTest_AR : AccOrgTaxConfigurationTemplateLinkedOrganisationCollectionTest
	{
		protected override AccOrgTaxConfigurationTemplate CreateAccOrgTaxConfigurationTemplate()
		{
			var result = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			result.OCT_Code = "TAR";
			result.OCT_IsReceivable = true;
			return result;
		}

		protected override void LinkTemplateToOrg(AccOrgTaxConfigurationTemplate template, OrgHeader org)
		{
			org.CompanyData.OB_OCT_ARTaxTemplate = template.PK;
		}

		protected override AccTaxConfiguration MakeTaxConfiguration()
			=> AccountingTestObjectCreator.CreateTaxConfiguration(LedgerTypes.AccountsReceivable);
	}

	[TestedType(typeof(AccOrgTaxConfigurationTemplateLinkedOrganisationCollection))]
	sealed class AccOrgTaxConfigurationTemplateLinkedOrganisationCollectionTest_AP : AccOrgTaxConfigurationTemplateLinkedOrganisationCollectionTest
	{
		protected override AccOrgTaxConfigurationTemplate CreateAccOrgTaxConfigurationTemplate()
		{
			var result = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			result.OCT_Code = "TAP";
			result.OCT_IsReceivable = false;
			return result;
		}

		protected override void LinkTemplateToOrg(AccOrgTaxConfigurationTemplate template, OrgHeader org)
		{
			org.CompanyData.OB_OCT_APTaxTemplate = template.PK;
		}

		protected override AccTaxConfiguration MakeTaxConfiguration()
			=> AccountingTestObjectCreator.CreateTaxConfiguration(LedgerTypes.AccountsPayable);
	}

	public abstract class AccOrgTaxConfigurationTemplateLinkedOrganisationCollectionTest : BusinessObjectCollectionTestCase
	{
		public AccOrgTaxConfigurationTemplateLinkedOrganisationCollectionTest()
		{
			AccOrgTaxConfigurationTemplate = CreateAccOrgTaxConfigurationTemplate();
		}

		protected AccOrgTaxConfigurationTemplateLinkedOrganisationCollection CreateTestObject()
			=> new AccOrgTaxConfigurationTemplateLinkedOrganisationCollection(AccOrgTaxConfigurationTemplate);

		protected AccOrgTaxConfigurationTemplate AccOrgTaxConfigurationTemplate { get; }

		protected abstract AccOrgTaxConfigurationTemplate CreateAccOrgTaxConfigurationTemplate();

		protected sealed override BusinessObjectCollection GetCollectionToTest()
			=> CreateTestObject();

		public void TestAttachAndDetach()
		{
			var testingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var collection = CreateTestObject();

			collection.Add(testingOrg);
			CombineAssertions("OrgHeader should be added to Collection.", () =>
			{
				AssertEquals(1, collection.Count);
				AssertEquals(testingOrg.PK, collection[0].PK);
			});
			Factory.Save();

			var reloadCollection = new AccOrgTaxConfigurationTemplateLinkedOrganisationCollection(
				new BusinessObjectFactory().Load<AccOrgTaxConfigurationTemplate>(AccOrgTaxConfigurationTemplate.PK)
			);
			CombineAssertions("Reloading result should be same even with New Factory.", () =>
			{
				reloadCollection.Load();
				AssertEquals(1, reloadCollection.Count);
				AssertEquals(testingOrg.PK, reloadCollection[0].PK);
			});

			CombineAssertions("OrgHeader should not be searched after being detached.", () =>
			{
				reloadCollection.Remove(testingOrg.PK);
				reloadCollection.Factory.Save();

				reloadCollection.Load();
				AssertEquals(0, reloadCollection.Count);
			});
		}

		public void TestWarnningCurrentTaxConfigurationTemplateIsLinked()
		{
			var testingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var existedTemplate = CreateAccOrgTaxConfigurationTemplate();
			existedTemplate.OCT_Code = "AAA";
			LinkTemplateToOrg(existedTemplate, testingOrg);
			Factory.Save();

			var collection = CreateTestObject();

			collection.Add(testingOrg);
			AssertHasRowWarning(testingOrg, $"Current Tax Configuration Template is {existedTemplate.OCT_Code}");

			collection.Remove(testingOrg);
			AssertNoRowWarningContaining(testingOrg, $"Current Tax Configuration Template is {existedTemplate.OCT_Code}");
		}

		protected abstract void LinkTemplateToOrg(AccOrgTaxConfigurationTemplate template, OrgHeader org);

		public void TestWarnningHasExistingTaxConfigurations()
		{
			var accTaxConfiguration = MakeTaxConfiguration();
			Factory.Save();

			var testingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var accOrgTaxConfiguration = AccountingTestObjectCreator.CreateOrgTaxConfiguration(accTaxConfiguration, testingOrg.CompanyData);
			accOrgTaxConfiguration.OTC_OCT = ZGuid.Empty;
			Factory.Save();

			var collection = CreateTestObject();

			collection.Add(testingOrg);
			AssertHasRowWarning(testingOrg, "This organization has one or more existing Tax Configurations.");

			collection.Remove(testingOrg);
			AssertNoRowWarningContaining(testingOrg, "This organization has one or more existing Tax Configurations.");
		}

		protected abstract AccTaxConfiguration MakeTaxConfiguration();

		protected AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;

		public void TestCheckOrgLinkedHasChanged()
		{
			var testingOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var collection = CreateTestObject();
			AssertEquals("PreCondition", false, testingOrg.CompanyData.IsInDatabase);
			AssertEquals("PreCondition", true, collection.CheckOrgLinkedHasChanged(testingOrg));
			Factory.Save();

			AssertEquals("PreCondition", true, testingOrg.CompanyData.IsInDatabase);
			CombineAssertions("When OrgHeader not attached to collection", () =>
			{
				collection.Add(testingOrg);
				AssertEquals(true, collection.CheckOrgLinkedHasChanged(testingOrg));
				collection.Remove(testingOrg);
				AssertEquals(false, collection.CheckOrgLinkedHasChanged(testingOrg));
			});

			collection.Add(testingOrg);
			Factory.Save();
			AssertEquals("PreCondition", false, collection.CheckOrgLinkedHasChanged(testingOrg));
			CombineAssertions("When OrgHeader has been attached to collection", () =>
			{
				collection.Remove(testingOrg);
				AssertEquals(true, collection.CheckOrgLinkedHasChanged(testingOrg));
				collection.Add(testingOrg);
				AssertEquals(false, collection.CheckOrgLinkedHasChanged(testingOrg));
			});
		}

		public void TestLoadWithFilter()
		{
			var collection = new DummyAccOrgTaxConfigurationTemplateLinkedOrganisationCollection(AccOrgTaxConfigurationTemplate);

			AssertEquals("PreCondition", 0, collection.CreateAdditionalFilterCalledCount);
			collection.Load(new ZQuery());
			AssertEquals("CreateAdditionalFilter will only be called once", 1, collection.CreateAdditionalFilterCalledCount);
		}

		public void TestLoadWithoutFilter()
		{
			var collection = new DummyAccOrgTaxConfigurationTemplateLinkedOrganisationCollection(AccOrgTaxConfigurationTemplate);

			AssertEquals("PreCondition", 0, collection.CreateAdditionalFilterCalledCount);
			collection.Load();
			AssertEquals("CreateAdditionalFilter will be called when calling Load with customized filter", 1, collection.CreateAdditionalFilterCalledCount);
		}

		sealed class DummyAccOrgTaxConfigurationTemplateLinkedOrganisationCollection : AccOrgTaxConfigurationTemplateLinkedOrganisationCollection
		{
			public DummyAccOrgTaxConfigurationTemplateLinkedOrganisationCollection(AccOrgTaxConfigurationTemplate accOrgTaxConfigurationTemplate) : base(accOrgTaxConfigurationTemplate)
			{
			}

			public int CreateAdditionalFilterCalledCount { get; private set; }

			protected override ZQuery CreateAdditionalFilter()
			{
				CreateAdditionalFilterCalledCount++;
				return base.CreateAdditionalFilter();
			}
		}

		public void TestSkipItemsValidation()
		{
			var testingOrg = Factory.NewWithValidTestData<OrgHeader>();
			CombineAssertions("PreCondition", () =>
			{
				testingOrg.OH_IsConsignee = false;
				testingOrg.OH_IsConsignor = false;
				testingOrg.OH_IsDebtor = false;
				testingOrg.OH_IsCreditor = false;
				testingOrg.OH_IsTempAccount = true;
				AssertHasErrors(testingOrg.OH_IsTempAccountInfo);
				testingOrg.OH_IsTempAccountInfo.ClearAllNotifications();
				AssertNoErrors(testingOrg.OH_IsTempAccountInfo);
				ErrorReporter.Instance.Clear();
			});

			var collection = CreateTestObject();
			collection.Add(testingOrg);
			AssertNoErrors("PreCondition", testingOrg.OH_CodeInfo);
			collection.RunPreSaveValidation();
			AssertNoErrors("Collection will not call items validation for performance issue,all items should be validated in each Edit form itself.", testingOrg.OH_CodeInfo);
		}

		public void TestOnlyClearResult()
		{
			var testingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var collection = CreateTestObject();

			collection.Add(testingOrg);
			CombineAssertions("OrgHeader should be added to Collection.", () =>
			{
				AssertEquals(1, collection.Count);
				AssertEquals(testingOrg.PK, collection[0].PK);
			});
			Factory.Save();

			var reloadCollection = new AccOrgTaxConfigurationTemplateLinkedOrganisationCollection(
				new BusinessObjectFactory().Load<AccOrgTaxConfigurationTemplate>(AccOrgTaxConfigurationTemplate.PK)
			);

			CombineAssertions("PreCondition", () =>
			{
				AssertEquals(0, reloadCollection.Count);

				reloadCollection.Load();
				AssertEquals(1, reloadCollection.Count);
			});

			CombineAssertions("OnlyClearResult should only do clear result and keep FK value.", () =>
			{
				reloadCollection.ClearResult();
				AssertEquals(0, reloadCollection.Count);

				reloadCollection.Factory.Save();

				reloadCollection.Load();
				AssertEquals(1, reloadCollection.Count);
			});
		}

		public void TestIHaveZQueryForZGridExcelExportQuery()
		{
			var testingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var collection = CreateTestObject();

			collection.Add(testingOrg);
			CombineAssertions("OrgHeader should be added to Collection.", () =>
			{
				AssertEquals(1, collection.Count);
				AssertEquals(testingOrg.PK, collection[0].PK);
			});
			Factory.Save();

			var reloadCollection = new AccOrgTaxConfigurationTemplateLinkedOrganisationCollection(
				new BusinessObjectFactory().Load<AccOrgTaxConfigurationTemplate>(AccOrgTaxConfigurationTemplate.PK)
			);
			IHaveZQueryForZGridExcelExport haveZQueryForZGridExcelExport = reloadCollection;
			CombineAssertions("PreCondition", () =>
			{
				AssertEquals(0, reloadCollection.Count);
				AssertEquals(0, new BusinessObjectFactory().Load<OrgHeader>(haveZQueryForZGridExcelExport.Query).Length);

				reloadCollection.Load();
				AssertEquals(1, reloadCollection.Count);
				AssertEquals(1, new BusinessObjectFactory().Load<OrgHeader>(haveZQueryForZGridExcelExport.Query).Length);
			});

			CombineAssertions("ClearResult should only do clear result and keep LastLoadedAdditionalFilter.", () =>
			{
				reloadCollection.ClearResult();
				AssertEquals(0, reloadCollection.Count);
				AssertEquals(1, new BusinessObjectFactory().Load<OrgHeader>(haveZQueryForZGridExcelExport.Query).Length);

				reloadCollection.Factory.Save();

				reloadCollection.Load();
				AssertEquals(1, reloadCollection.Count);
				AssertEquals(1, new BusinessObjectFactory().Load<OrgHeader>(haveZQueryForZGridExcelExport.Query).Length);
			});
		}

		public void TestGetTotalRowsCount()
		{
			var testingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var collection = CreateTestObject();

			collection.Add(testingOrg);
			CombineAssertions("OrgHeader should be added to Collection.", () =>
			{
				AssertEquals(1, collection.Count);
				AssertEquals(testingOrg.PK, collection[0].PK);
			});
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadCollection = new AccOrgTaxConfigurationTemplateLinkedOrganisationCollection(
				newFactory.Load<AccOrgTaxConfigurationTemplate>(AccOrgTaxConfigurationTemplate.PK)
			);
			AssertEquals("Result should be correct", 1, reloadCollection.GetTotalRowsCount());
			AssertEquals("Query TotalRowsCount should not cause selecting table.", 0, newFactory.TableSelects.FirstOrDefault(x => x.TableName == OrgCompanyDataSchema.Constants.TableName).Value);
		}
	}
}
