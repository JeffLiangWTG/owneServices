using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccOrgTaxConfigurationTemplateDataHelperTest : TestCaseWithFactory
	{
		#region Insert

		public void TestInsertTaxConfigurationsForOrgnization_Insert_AP()
		{
			AssertInsertTaxConfigurationsForOrgnization_InsertCore(false);
		}

		public void TestInsertTaxConfigurationsForOrgnization_Insert_AR()
		{
			AssertInsertTaxConfigurationsForOrgnization_InsertCore(true);
		}

		void AssertInsertTaxConfigurationsForOrgnization_InsertCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);
			var helper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper();

			var orgTaxConfigCom1_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigCom1);
			orgTaxConfigCom1_TP.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigBrn2);
			orgTaxConfigBrn2_TP.OTC_IsThresholdUsed = true;
			Factory.Save();

			var orgTaxList = new List<AccOrgTaxConfiguration>() { orgTaxConfigCom1_TP, orgTaxConfigBrn2_TP };
			var targetCollection = isReceivable ? companyDataA.AROrgTaxConfigurations :
												  (AccOrgTaxConfigurationCollectionByLedger)companyDataA.APOrgTaxConfigurations;

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 2, collection.Length);
			var configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true);
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false);

			helper.UpdateTaxConfigurationsForOrgnization(targetCollection, orgTaxList);
			Factory.Save();

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(4, collection.Length);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configCom1_ObA, true, false, true, "Org-A has new org tax config added");
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, false, "Org-A has new org tax config added");
		}

		public void TestInsertTaxConfigurationsForMultipleOrgnizations_Insert_AP()
		{
			AssertInsertTaxConfigurationsForMultipleOrgnizations_InsertCore(false);
		}

		public void TestInsertTaxConfigurationsForMultipleOrgnizations_Insert_AR()
		{
			AssertInsertTaxConfigurationsForMultipleOrgnizations_InsertCore(true);
		}

		void AssertInsertTaxConfigurationsForMultipleOrgnizations_InsertCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);
			var helper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper();

			var orgTaxConfigCom1_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigCom1);
			orgTaxConfigCom1_TP.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigBrn2);
			orgTaxConfigBrn2_TP.OTC_IsThresholdUsed = true;
			Factory.Save();

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 2, collection.Length);
			var configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true);
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false);

			helper.UpdateTaxConfigurationsForMultipleOrgnizations(template.PK.ToGuid());

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(6, collection.Length);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configCom1_ObA, true, false, true, "Org-A has new org tax config added");
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, false, "Org-A has new org tax config added");
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, true, "Org-B has new org tax config added");
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, true, false, "Org-B has new org tax config added");
		}

		#endregion

		#region Update

		public void TestUpdateTaxConfigurationsForOrgnization_Insert_AP()
		{
			AssertUpdateTaxConfigurationsForOrgnization_InsertCore(false);
		}

		public void TestUpdateTaxConfigurationsForOrgnization_Insert_AR()
		{
			AssertUpdateTaxConfigurationsForOrgnization_InsertCore(true);
		}

		void AssertUpdateTaxConfigurationsForOrgnization_InsertCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);
			var helper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper();

			var orgTaxConfigCom1_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataA);
			orgTaxConfigCom1_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigCom1_ObA.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataA);
			orgTaxConfigBrn2_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigBrn2_ObA.OTC_RecoverTax = true;

			var orgTaxConfigCom1_ObB = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataB);

			var orgTaxConfigCom1_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigCom1);
			orgTaxConfigCom1_TP.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigBrn2);
			orgTaxConfigBrn2_TP.OTC_IsThresholdUsed = true;
			Factory.Save();

			var orgTaxList = new List<AccOrgTaxConfiguration>() { orgTaxConfigCom1_TP, orgTaxConfigBrn2_TP };
			var targetCollection = isReceivable ? companyDataA.AROrgTaxConfigurations :
												  (AccOrgTaxConfigurationCollectionByLedger)companyDataA.APOrgTaxConfigurations;

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 5, collection.Length);
			var configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true);
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false);
			AssertOrgTaxConfigEquals(configCom1_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, false);

			helper.UpdateTaxConfigurationsForOrgnization(targetCollection, orgTaxList);
			Factory.Save();

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(5, collection.Length);
			configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configCom1_ObA, true, false, true, "Org-A has old org tax config updated");
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, false, "Org-A has old org tax config updated");
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, false, "Org-B is not affected");
		}

		public void TestUpdateTaxConfigurationsForMultipleOrgnizations_Insert_AP()
		{
			AssertUpdateTaxConfigurationsForMultipleOrgnizations_InsertCore(false);
		}

		public void TestUpdateTaxConfigurationsForMultipleOrgnizations_Insert_AR()
		{
			AssertUpdateTaxConfigurationsForMultipleOrgnizations_InsertCore(true);
		}

		void AssertUpdateTaxConfigurationsForMultipleOrgnizations_InsertCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);
			var helper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper();

			var orgTaxConfigCom1_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataA);
			orgTaxConfigCom1_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigCom1_ObA.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataA);
			orgTaxConfigBrn2_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigBrn2_ObA.OTC_RecoverTax = true;

			var orgTaxConfigCom1_ObB = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataB);
			orgTaxConfigCom1_ObB.OTC_IsActive = false;
			var orgTaxConfigBrn2_ObB = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataB);
			orgTaxConfigBrn2_ObB.OTC_IsActive = false;

			var orgTaxConfigCom1_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigCom1);
			orgTaxConfigCom1_TP.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigBrn2);
			orgTaxConfigBrn2_TP.OTC_IsThresholdUsed = true;
			Factory.Save();

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 6, collection.Length);
			var configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true);
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false);
			AssertOrgTaxConfigEquals(configCom1_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configCom1_ObB, false, false, false);
			AssertOrgTaxConfigEquals(configBrn2_ObB, false, false, false);

			helper.UpdateTaxConfigurationsForMultipleOrgnizations(template.PK.ToGuid());

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(6, collection.Length);
			configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configCom1_ObA, true, false, true, "Org-A has old org tax config updated");
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, false, "Org-A has old org tax config updated");
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, true, "Org-B has old org tax config updated");
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, true, false, "Org-B has old org tax config updated");
		}

		#endregion

		#region Deactive

		public void TestDeactiveTaxConfigurationsForOrgnization_Insert_AP()
		{
			AssertDeactiveTaxConfigurationsForOrgnization_InsertCore(false);
		}

		public void TestDeactiveTaxConfigurationsForOrgnization_Insert_AR()
		{
			AssertDeactiveTaxConfigurationsForOrgnization_InsertCore(true);
		}

		void AssertDeactiveTaxConfigurationsForOrgnization_InsertCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);
			var helper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper();

			var orgTaxConfigCom1_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataA);
			orgTaxConfigCom1_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigCom1_ObA.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataA);
			orgTaxConfigBrn2_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigBrn2_ObA.OTC_RecoverTax = true;

			var orgTaxConfigCom1_ObB = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataB);
			Factory.Save();

			var orgTaxList = new List<AccOrgTaxConfiguration>();
			var targetCollection = isReceivable ? companyDataA.AROrgTaxConfigurations :
												  (AccOrgTaxConfigurationCollectionByLedger)companyDataA.APOrgTaxConfigurations;

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 3, collection.Length);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, false);

			helper.UpdateTaxConfigurationsForOrgnization(targetCollection, orgTaxList);
			Factory.Save();

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(3, collection.Length);
			configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_ObA, false, true, true, "Org-A has old org tax config deactived");
			AssertOrgTaxConfigEquals(configBrn2_ObA, false, true, true, "Org-A has old org tax config deactived");
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, false, "Org-B is unchanged");
		}

		public void TestDeactiveTaxConfigurationsForMultipleOrgnizations_Insert_AP()
		{
			AssertDeactiveTaxConfigurationsForMultipleOrgnizations_InsertCore(false);
		}

		public void TestDeactiveTaxConfigurationsForMultipleOrgnizations_Insert_AR()
		{
			AssertDeactiveTaxConfigurationsForMultipleOrgnizations_InsertCore(true);
		}

		void AssertDeactiveTaxConfigurationsForMultipleOrgnizations_InsertCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);
			var helper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper();

			var orgTaxConfigCom1_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataA);
			orgTaxConfigCom1_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigCom1_ObA.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataA);
			orgTaxConfigBrn2_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigBrn2_ObA.OTC_RecoverTax = true;

			var orgTaxConfigCom1_ObB = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataB);
			var orgTaxConfigBrn2_ObB = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataB);
			Factory.Save();

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 4, collection.Length);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, false);
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, false, false);

			helper.UpdateTaxConfigurationsForMultipleOrgnizations(template.PK.ToGuid());

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(4, collection.Length);
			configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_ObA, false, true, true, "Org-A has old org tax config deactived");
			AssertOrgTaxConfigEquals(configBrn2_ObA, false, true, true, "Org-A has old org tax config deactived");
			AssertOrgTaxConfigEquals(configCom1_ObB, false, false, false, "Org-B has old org tax config deactived");
			AssertOrgTaxConfigEquals(configBrn2_ObB, false, false, false, "Org-B has old org tax config deactived");
		}

		#endregion

		#region Mixed Operation

		public void TestMixedOperationTaxConfigurationsForOrgnization_Insert_AP()
		{
			AssertMixedOperationTaxConfigurationsForOrgnization_InsertCore(false);
		}

		public void TestMixedOperationTaxConfigurationsForOrgnization_Insert_AR()
		{
			AssertMixedOperationTaxConfigurationsForOrgnization_InsertCore(true);
		}

		void AssertMixedOperationTaxConfigurationsForOrgnization_InsertCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);
			var helper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper();

			var orgTaxConfigCom1_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataA);
			orgTaxConfigCom1_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigCom1_ObA.OTC_RecoverTax = true;
			var orgTaxConfigBrn1_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn1, companyDataA);
			var orgTaxConfigBrn1_ObA_NonCurrentCompany = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn1, companyDataA_NonCurrentCompany);
			var orgTaxConfigBrn1_ObB = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn1, companyDataB);
			Factory.Save();

			var orgTaxConfigCom1_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigCom1);
			orgTaxConfigCom1_TP.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigBrn2);
			orgTaxConfigBrn2_TP.OTC_IsThresholdUsed = true;
			Factory.Save();

			var orgTaxList = new List<AccOrgTaxConfiguration>() { orgTaxConfigCom1_TP, orgTaxConfigBrn2_TP };
			var targetCollection = isReceivable ? companyDataA.AROrgTaxConfigurations :
												  (AccOrgTaxConfigurationCollectionByLedger)companyDataA.APOrgTaxConfigurations;

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 6, collection.Length);
			var configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn1_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn1_ObA_NonCurrentCompany = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataA_NonCurrentCompany.PK && x.OTC_OCT.IsEmpty);
			var configBrn1_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true);
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false);
			AssertOrgTaxConfigEquals(configCom1_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configBrn1_ObA, true, false, false);
			AssertOrgTaxConfigEquals(configBrn1_ObA_NonCurrentCompany, true, false, false);
			AssertOrgTaxConfigEquals(configBrn1_ObB, true, false, false);

			helper.UpdateTaxConfigurationsForOrgnization(targetCollection, orgTaxList);
			Factory.Save();

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(7, collection.Length);
			configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn1_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn1_ObA_NonCurrentCompany = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataA_NonCurrentCompany.PK && x.OTC_OCT.IsEmpty);
			configBrn1_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true, "Template config is not changed");
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false, "Template config is not changed");
			AssertOrgTaxConfigEquals(configCom1_ObA, true, false, true, "Org-A has old org tax config updated");
			AssertOrgTaxConfigEquals(configBrn1_ObA, false, false, false, "Org-A has old org tax config deactived");
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, false, "Org-A has new org tax config added");
			AssertOrgTaxConfigEquals(configBrn1_ObA_NonCurrentCompany, true, false, false, "Org-A non current company data is not affected");
			AssertOrgTaxConfigEquals(configBrn1_ObB, true, false, false, "Org-B is not affected");
		}

		public void TestMixedOperationTaxConfigurationsForMultipleOrgnizations_Insert_AP()
		{
			AssertMixedOperationTaxConfigurationsForMultipleOrgnizations_InsertCore(false);
		}

		public void TestMixedOperationTaxConfigurationsForMultipleOrgnizations_Insert_AR()
		{
			AssertMixedOperationTaxConfigurationsForMultipleOrgnizations_InsertCore(true);
		}

		void AssertMixedOperationTaxConfigurationsForMultipleOrgnizations_InsertCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);
			var helper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper();

			var orgTaxConfigCom1_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataA);
			orgTaxConfigCom1_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigCom1_ObA.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_ObB = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataB);
			var orgTaxConfigBrn1_ObA = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn1, companyDataA);
			var orgTaxConfigBrn1_ObB = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn1, companyDataB);
			var orgTaxConfigBrn1_ObB_NonCurrentCompany = TestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn1, companyDataB_NonCurrentCompany);
			Factory.Save();

			var orgTaxConfigCom1_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigCom1);
			orgTaxConfigCom1_TP.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_TP = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigBrn2);
			orgTaxConfigBrn2_TP.OTC_IsThresholdUsed = true;
			Factory.Save();

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 7, collection.Length);
			var configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn1_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn1_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn1_ObB_NonCurrentCompany = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB_NonCurrentCompany.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true);
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false);
			AssertOrgTaxConfigEquals(configCom1_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, false, false);
			AssertOrgTaxConfigEquals(configBrn1_ObA, true, false, false);
			AssertOrgTaxConfigEquals(configBrn1_ObB, true, false, false);
			AssertOrgTaxConfigEquals(configBrn1_ObB_NonCurrentCompany, true, false, false);

			helper.UpdateTaxConfigurationsForMultipleOrgnizations(template.PK.ToGuid());

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(9, collection.Length);
			configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn1_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn1_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn1_ObB_NonCurrentCompany = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB_NonCurrentCompany.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true, "Template config is unchanged");
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false, "Template config is unchanged");
			AssertOrgTaxConfigEquals(configCom1_ObA, true, false, true, "Org-A has old org tax config updated");
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, false, "Org-A has new org tax config added");
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, true, "Org-B has new org tax config added");
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, true, false, "Org-B has old org tax config updated");
			AssertOrgTaxConfigEquals(configBrn1_ObA, false, false, false, "Org-A has old org tax config deactived");
			AssertOrgTaxConfigEquals(configBrn1_ObB, false, false, false, "Org-B has old org tax config deactived");
			AssertOrgTaxConfigEquals(configBrn1_ObB_NonCurrentCompany, true, false, false, "Org-B non current company data is not affected");
		}

		#endregion

		#region Implementation

		void PrepareTestData(bool isReceivable)
		{
			var ledger = isReceivable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;

			var orgA = TestObjectCreator.CreateOrgHeader("OrgA", true, true);
			var orgB = TestObjectCreator.CreateOrgHeader("OrgB", true, true);
			companyDataA = orgA.CompanyData;
			companyDataB = orgB.CompanyData;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				companyDataA_NonCurrentCompany = orgA.CompanyData;
				companyDataB_NonCurrentCompany = orgB.CompanyData;
			}
			Factory.Save();

			var taxSystem1 = TestObjectCreator.CreateTaxSystem("TS1");
			var taxSystem2 = TestObjectCreator.CreateTaxSystem("TS2");
			var taxAuthority1 = TestObjectCreator.CreateTaxAuthority("TA1");
			var taxAuthority2 = TestObjectCreator.CreateTaxAuthority("TA2");

			taxConfigBrn1 = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority1, taxSystem1, ledger, true, "TestCodeA1", "TestDescA1");
			taxConfigCom1 = TestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority1, taxSystem1, ledger, true, "TestCodeA2", "TestDescA2");
			taxConfigBrn2 = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority2, taxSystem2, ledger, true, "TestCodeB1", "TestDescB1");
			taxConfigCom2 = TestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority2, taxSystem2, ledger, true, "TestCodeB2", "TestDescB2");
			Factory.Save();

			template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = isReceivable;
			Factory.Save();

			if (isReceivable)
			{
				companyDataA.OB_OCT_ARTaxTemplate = template.PK;
				companyDataA_NonCurrentCompany.OB_OCT_ARTaxTemplate = template.PK;
				companyDataB.OB_OCT_ARTaxTemplate = template.PK;
				companyDataB_NonCurrentCompany.OB_OCT_ARTaxTemplate = template.PK;
			}
			else
			{
				companyDataA.OB_OCT_APTaxTemplate = template.PK;
				companyDataA_NonCurrentCompany.OB_OCT_APTaxTemplate = template.PK;
				companyDataB.OB_OCT_APTaxTemplate = template.PK;
				companyDataB_NonCurrentCompany.OB_OCT_APTaxTemplate = template.PK;
			}
			Factory.Save();
		}

		void AssertOrgTaxConfigEquals(AccOrgTaxConfiguration expected, bool isActive, bool isThresholdUsed, bool isRecoverTax, string assertMsg = null)
		{
			var connector = assertMsg == null ? string.Empty : ", and ";
			CombineAssertions($"{assertMsg}{connector}value should match expected ones",
			() =>
			{
				AssertEquals("Threshold Used", isThresholdUsed, expected.OTC_IsThresholdUsed);
				AssertEquals("Active", isActive, expected.OTC_IsActive);
				AssertEquals("Recover Tax", isRecoverTax, expected.OTC_RecoverTax);
			});
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;

		OrgCompanyData companyDataA, companyDataB, companyDataA_NonCurrentCompany, companyDataB_NonCurrentCompany;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "The mehod TestObjectCreator.CreateTaxConfiguration for assigning taxConfigCom2 has side effects (creating database records), we should neithor remove the assign method nor remove the field. ")]
		AccTaxConfiguration taxConfigBrn1, taxConfigCom1, taxConfigBrn2, taxConfigCom2;
		AccOrgTaxConfigurationTemplate template;

		#endregion
	}
}
