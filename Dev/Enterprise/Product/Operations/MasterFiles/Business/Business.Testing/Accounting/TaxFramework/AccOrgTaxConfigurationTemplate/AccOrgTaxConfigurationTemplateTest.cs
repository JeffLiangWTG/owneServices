using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccOrgTaxConfigurationTemplate))]
	sealed class AccOrgTaxConfigurationTemplateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(AccOrgTaxConfigurationTemplate)));
		}

		public void TestSetDefaultValues()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var templateForNonCurrentCompany = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
				AssertEquals(company.PK, templateForNonCurrentCompany.OCT_GC_Company);
			}
		}

		public void TestHumanReadableName()
		{
			var arTemplate = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			arTemplate.OCT_Code = "TAR";
			arTemplate.OCT_IsReceivable = true;
			AssertEquals("Receivables Organizations Template", arTemplate.HumanReadableName);

			var apTemplate = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			apTemplate.OCT_Code = "TAP";
			apTemplate.OCT_IsReceivable = false;
			AssertEquals("Payables Organizations Template", apTemplate.HumanReadableName);
		}

		public void TestOCT_IsReceivableIsReadonly()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			AssertEquals(true, template.OCT_IsReceivableInfo.ReadOnly);
		}

		public void TestOCT_IsPayableIsReadonly()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			AssertEquals(true, template.OCT_IsPayableInfo.ReadOnly);
		}

		public void TestIDocManagerSupportProvider()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			var docManagerSupport = template as IDocManagerSupport;
			AssertNotNull("IDocManagerSupport must be implemented", docManagerSupport);
			AssertEquals("DocManagerCode must be TaxConfigurationTemplate code.", Core.Constants.DocManagerCodes.TaxConfigurationTemplate, docManagerSupport.DocManagerInfo.DocManagerCode);
		}

		public void TestIsAutoLogged()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			Assert(template.IsAutoAdminBusinessObjectLoggerEnabled);

			AssertEquals("Expect that no log exists.", 0, template.Logs.GetAllLogs().Count);
			Factory.Save();
			AssertEquals("Expect that only one log exists.", 1, template.Logs.GetAllLogs().Count);
			AssertEquals("Expect that log is added.", true, template.Logs.GetAllLogs()[0].IsInDatabase);
		}

		public void TestTemplateType()
		{
			var arTemplate = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			arTemplate.OCT_IsReceivable = true;
			AssertEquals(false, arTemplate.OCT_IsPayable);
			AssertEquals("A/R - Receivables Organizations Template", arTemplate.TemplateType);

			var apTemplate = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			apTemplate.OCT_IsReceivable = false;
			AssertEquals(true, apTemplate.OCT_IsPayable);
			AssertEquals("A/P - Payables Organizations Template", apTemplate.TemplateType);
		}

		public void TestTemplateClone()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_Code = "Test Code";
			template.OCT_Description = "Test Description";
			template.OCT_IsActive = false;
			template.OCT_IsReceivable = false;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			template.OCT_GC_Company = company.PK;
			template.AccOrgTaxConfigurations.AddNew();

			AssertEquals("Precondition", true, template.SupportsClone());
			AssertEquals(1, template.AccOrgTaxConfigurations.Count);

			var copy = (AccOrgTaxConfigurationTemplate)template.Clone();
			AssertEquals("Test Code", copy.OCT_Code);
			AssertEquals("Test Description", copy.OCT_Description);
			AssertEquals(false, copy.OCT_IsActive);
			AssertEquals(false, copy.OCT_IsReceivable);
			AssertEquals(company.PK, copy.OCT_GC_Company);
			AssertEquals("Should be shallow clone", 0, copy.AccOrgTaxConfigurations.Count);
		}

		public void TestGetLedger()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = true;
			AssertEquals(LedgerTypes.AccountsReceivable, template.Ledger);

			template.OCT_IsReceivable = false;
			AssertEquals(LedgerTypes.AccountsPayable, template.Ledger);
		}

		public void TestSetOCT_IsReceivable()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = true;
			template.AccOrgTaxConfigurations.AddNew();
			template.AccOrgTaxConfigurations.AddNew();
			AssertEquals("Pre-condition", 2, template.AccOrgTaxConfigurations.Count);

			template.OCT_IsReceivable = true;
			AssertEquals("Should NOT delete tax configs when value has no changes", 2, template.AccOrgTaxConfigurations.Count);

			template.OCT_IsReceivable = false;
			AssertEquals("Should delete all tax configs when value changed", 0, template.AccOrgTaxConfigurations.Count);
		}

		public void TestFindBoxCollection()
		{
			var templateAR = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAR.OCT_IsReceivable = true;
			AssertEquals("Receivable", true, templateAR.FindBoxCollection is DebtorCollection);

			var templateAP = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAP.OCT_IsReceivable = false;
			AssertEquals("Payable", true, templateAP.FindBoxCollection is CreditorCollection);
		}

		#region Org Tax Configuration Collection

		public void TestReceivableAccOrgTaxConfigurations()
		{
			AssertAccOrgTaxConfigurationsCore(true);
		}

		public void TestPayableAccOrgTaxConfigurations()
		{
			AssertAccOrgTaxConfigurationsCore(false);
		}

		void AssertAccOrgTaxConfigurationsCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);

			if (isReceivable)
			{
				var orgConfig1 = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigARBranchSystem1);
				var orgConfig2 = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigARCompanySystem2);
				Factory.Save();

				AssertEquals("After filling configs for receivable template", 2, template.AccOrgTaxConfigurations.Count);
				AssertArrayEqualsByElements(new ZGuid[] { orgConfig1.PK, orgConfig2.PK }, template.AccOrgTaxConfigurations.Select(x => x.PK).ToArray());
			}
			else
			{
				var orgConfig3 = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigAPCompanySystem1);
				Factory.Save();

				AssertEquals("After filling configs for payable template", 1, template.AccOrgTaxConfigurations.Count);
				AssertArrayEqualsByElements(new ZGuid[] { orgConfig3.PK }, template.AccOrgTaxConfigurations.Select(x => x.PK).ToArray());
			}
		}

		#endregion

		#region Copy Template

		public void TestCopyReceivableTemplate()
		{
			AssertCopyTemplateCore(false);
		}

		public void TestCopyPayableTemplate()
		{
			AssertCopyTemplateCore(true);
		}

		public void TestWarnDeveloperIfNewColumnsIsNotInTemplateCopy()
		{
			var msg = @"If add/remove DB Column of bizO AccOrgTaxConfiguration or AccOrgTaxConfigurationTemplate, please update
 * AccOrgTaxConfigurationTemplate -> TemplateCopy method, and
 * AccOrgTaxConfigurationTemplateTest -> AssertCopyTemplateCore test method, and
 * Relative CopyPropertyList.";

			var propertyList = typeof(AccOrgTaxConfigurationTemplateSchema.Constants).GetAllPublicConstantValues().Where(x => x.StartsWith($"{AccOrgTaxConfigurationTemplateSchema.Constants.Prefix}_"));
			var expectedList = TemplateShouldCopyPropertyList.Concat(TemplateCouldSkipCopyPropertyList);
			AssertContainsExactElementsInAnyOrder(msg, expectedList, propertyList);

			propertyList = typeof(AccOrgTaxConfigurationSchema.Constants).GetAllPublicConstantValues().Where(x => x.StartsWith($"{AccOrgTaxConfigurationSchema.Constants.Prefix}_"));
			expectedList = OrgTaxConfigShouldCopyPropertyList.Concat(OrgTaxConfigCouldSkipCopyPropertyList);
			AssertContainsExactElementsInAnyOrder(msg, expectedList, propertyList);
		}

		void AssertCopyTemplateCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);

			var orgHeader1 = TestObjectCreator.CreateOrgHeader("OH1", true, true);
			var orgHeader2 = TestObjectCreator.CreateOrgHeader("OH2", true, true);
			Factory.Save();

			template.OCT_Code = "TEST 01";
			template.OCT_Description = "TEST DESC 01";

			AccOrgTaxConfiguration orgConfig1, orgConfig2;

			if (isReceivable)
			{
				orgConfig1 = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigARBranchSystem1);
				orgConfig2 = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigARCompanySystem2);
			}
			else
			{
				orgConfig1 = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigAPBranchSystem1);
				orgConfig2 = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigAPCompanySystem2);
			}

			orgConfig1.OTC_IsThresholdUsed = true;
			orgConfig1.OTC_RecoverTax = true;
			orgConfig1.OTC_IsActive = true;
			orgConfig2.OTC_IsThresholdUsed = false;
			orgConfig2.OTC_RecoverTax = false;
			orgConfig2.OTC_IsActive = false;

			TestObjectCreator.LinkOrgHeaderWithTaxConfigurationTemplate(template, orgHeader1);
			TestObjectCreator.LinkOrgHeaderWithTaxConfigurationTemplate(template, orgHeader2);

			taxConfigARBranchSystem1.ETC_RecoveryMethod = TaxRecoveryMethods.NoRecovery.Code;
			Factory.Save();

			AssertEquals("Pre-condition", 2, template.AccOrgTaxConfigurations.Count);
			AssertEquals(2, template.LinkedOrganisations.Count);
			AssertHasWarnings("Has 'Tax Recovery will not be activated' warning", template.AccOrgTaxConfigurations[0].OTC_RecoverTaxInfo);

			var target = (AccOrgTaxConfigurationTemplate)template.TemplateCopy();

			foreach (var propertyName in TemplateShouldCopyPropertyList)
			{
				AssertEquals(propertyName, template[propertyName], target[propertyName]);
			}

			AssertEquals("Should copy org tax configs", 2, target.AccOrgTaxConfigurations.Count);
			var targetConfig1 = target.AccOrgTaxConfigurations.Single(x => x.OTC_ETC == orgConfig1.OTC_ETC);
			var targetConfig2 = target.AccOrgTaxConfigurations.Single(x => x.OTC_ETC == orgConfig2.OTC_ETC);
			foreach (var propertyName in OrgTaxConfigShouldCopyPropertyList)
			{
				AssertEquals(propertyName, orgConfig1[propertyName], targetConfig1[propertyName]);
				AssertEquals(propertyName, orgConfig2[propertyName], targetConfig2[propertyName]);
			}
			AssertEquals(target.PK, targetConfig1.OTC_OCT);
			AssertEquals(target.PK, targetConfig2.OTC_OCT);
			AssertEquals("Source is unchanged", true, orgConfig1.OTC_IsThresholdUsed);
			AssertEquals(false, orgConfig2.OTC_IsThresholdUsed);
			AssertEquals(isReceivable ? taxConfigARBranchSystem1.PK : taxConfigAPBranchSystem1.PK, orgConfig1.OTC_ETC);
			AssertEquals(isReceivable ? taxConfigARCompanySystem2.PK : taxConfigAPCompanySystem2.PK, orgConfig2.OTC_ETC);
			AssertEquals("Should NOT copy linked orgs", 0, target.LinkedOrganisations.Count);
			AssertNoWarnings("Validation was suspended during template copy", target.AccOrgTaxConfigurations[0].OTC_RecoverTaxInfo);
			AssertNoWarnings(target.AccOrgTaxConfigurations[1].OTC_RecoverTaxInfo);
		}

		readonly string[] TemplateShouldCopyPropertyList = new string[] { "OCT_Code", "OCT_Description", "OCT_GC_Company", "OCT_IsActive", "OCT_IsReceivable" };
		readonly string[] TemplateCouldSkipCopyPropertyList = new string[] { "OCT_PK", "OCT_SystemCreateTimeUtc", "OCT_SystemCreateUser", "OCT_SystemLastEditTimeUtc", "OCT_SystemLastEditUser", "OCT_IsValid" };
		readonly string[] OrgTaxConfigShouldCopyPropertyList = new string[] { "OTC_ETC", "OTC_IsActive", "OTC_IsThresholdUsed", "OTC_OB", "OTC_RecoverTax" };
		readonly string[] OrgTaxConfigCouldSkipCopyPropertyList = new string[] { "OTC_PK", "OTC_SystemCreateTimeUtc", "OTC_SystemCreateUser", "OTC_SystemLastEditTimeUtc", "OTC_SystemLastEditUser", "OTC_OCT" };

		#endregion

		#region Overrides and Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory);
			((AccOrgTaxConfigurationTemplate)result).AccOrgTaxConfigurations.ToList().ForEach(x => x.OTC_OB = ZGuid.Empty);
			return result;
		}

		void PrepareTestData(bool isReceivable)
		{
			var taxSystem1 = TestObjectCreator.CreateTaxSystem("TS1");
			var taxAuthority1 = TestObjectCreator.CreateTaxAuthority("TA1");
			var taxSystem2 = TestObjectCreator.CreateTaxSystem("TS2");
			var taxAuthority2 = TestObjectCreator.CreateTaxAuthority("TA2");

			taxConfigARCompanySystem1 = TestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority1, taxSystem1, LedgerTypes.AccountsReceivable);
			taxConfigARBranchSystem1 = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority1, taxSystem1, LedgerTypes.AccountsReceivable);
			taxConfigARCompanySystem2 = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority2, taxSystem2, LedgerTypes.AccountsReceivable);
			taxConfigAPCompanySystem1 = TestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority1, taxSystem1, LedgerTypes.AccountsPayable);
			taxConfigAPBranchSystem1 = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority1, taxSystem1, LedgerTypes.AccountsPayable);
			taxConfigAPCompanySystem2 = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority2, taxSystem2, LedgerTypes.AccountsPayable);

			taxConfigARCompanySystem1.ETC_Code = "TS1";
			taxConfigARBranchSystem1.ETC_Code = "TS2";
			taxConfigARCompanySystem2.ETC_Code = "TS3";
			taxConfigAPCompanySystem1.ETC_Code = "TS4";
			taxConfigAPBranchSystem1.ETC_Code = "TS5";
			taxConfigAPCompanySystem2.ETC_Code = "TS6";
			Factory.Save();

			template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = isReceivable;
			Factory.Save();

			AssertEquals("Pre-condition", 0, template.AccOrgTaxConfigurations.Count);
			AssertEquals(0, template.LinkedOrganisations.Count);
		}

		AccTaxConfiguration taxConfigARCompanySystem1, taxConfigARBranchSystem1, taxConfigARCompanySystem2, taxConfigAPCompanySystem1, taxConfigAPBranchSystem1, taxConfigAPCompanySystem2;
		AccOrgTaxConfigurationTemplate template;

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;

		#endregion

		public void TestAccOrgTaxConfigurationTemplateLinkedOrganisations()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			Factory.Save();
			AssertEquals("PreCondition", 0, template.LinkedOrganisations.Count);
			AssertEquals("PreCondition", false, template.HasChanges);

			var orgAdded = Factory.NewWithValidTestData<OrgHeader>();
			template.LinkedOrganisations.Add(orgAdded);
			CombineAssertions("OrgHeader should be added to LinkedOrganisations.", () =>
			{
				AssertEquals(1, template.LinkedOrganisations.Count);
				AssertEquals(orgAdded.PK, template.LinkedOrganisations[0].PK);
			});
			AssertEquals("LinkedOrganisations HasChange should be applied to Parent", true, template.HasChanges);

			Factory.Save();
			CombineAssertions("Reloading result should be same even with New Factory.", () =>
			{
				var templateReload = new BusinessObjectFactory().Load<AccOrgTaxConfigurationTemplate>(template.PK);
				AssertEquals("collection will not load content at first time.", 0, templateReload.LinkedOrganisations.Count);
				AssertEquals("we could get total rows count via GetTotalRowsCount", 1, templateReload.LinkedOrganisations.GetTotalRowsCount());

				templateReload.LinkedOrganisations.Load();
				AssertEquals("collection will return result after calling Load", 1, templateReload.LinkedOrganisations.Count);
				AssertEquals(orgAdded.PK, templateReload.LinkedOrganisations[0].PK);
			});
		}

		public void TestDelete()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = true;
			Factory.Save();

			var orgTaxConfiguration = template.AccOrgTaxConfigurations.AddNew();
			orgTaxConfiguration.OTC_ETC = taxConfiguration.PK;
			var testingOrg = Factory.NewWithValidTestData<OrgHeader>();
			template.LinkedOrganisations.Add(testingOrg);
			AssertEquals("PreCondition", template.PK, testingOrg.CompanyData.OB_OCT_ARTaxTemplate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertEquals("PreCondition", 0, newFactory.GetTableHitCount(AccOrgTaxConfigurationTemplateSchema.Constants.TableName));
			var rowFactory = ((IBusinessObjectFactoryInternals)newFactory).RowFactory;
			newFactory.Load<AccOrgTaxConfigurationTemplate>(template.PK).Delete();

			CombineAssertions("template should be deleted.", () =>
			{
				AssertEquals(null, newFactory.Load<AccOrgTaxConfigurationTemplate>(template.PK));
				AssertEquals(null, newFactory.Load<AccOrgTaxConfigurationTemplateItem>(orgTaxConfiguration.PK));
			});

			CombineAssertions("linked FK should be reset, and it will not cause selecting table.", () =>
			{
				AssertEquals(0, newFactory.TableSelects.FirstOrDefault(x => x.TableName == OrgCompanyDataSchema.Constants.TableName).Value);

				var orgCopmanyData = newFactory.Load<OrgHeader>(testingOrg.PK).CompanyData;
				AssertEquals(ZGuid.Empty, orgCopmanyData.OB_OCT_ARTaxTemplate);
				var tableSelect = newFactory.TableSelects.FirstOrDefault(x => x.TableName == OrgCompanyDataSchema.Constants.TableName);
				AssertEquals(1, tableSelect.Value);
			});
		}
	}
}
