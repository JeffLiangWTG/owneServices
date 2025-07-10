using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessFieldChangeRule))]
	public class ProcessFieldChangeRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFieldsCollection()
		{
			var rule = Factory.NewWithValidTestData<ProcessFieldChangeRule>();
			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_TEST";
			AssertEquals(rule.PK, field.PFL_PFR);

			Factory.Save();

			var reloadRule = new BusinessObjectFactory().Load<ProcessFieldChangeRule>(rule.PK);
			AssertEquals(1, reloadRule.Fields.Count);
			AssertEquals(rule.Fields.First().PK, reloadRule.Fields.First().PK);
		}

		public void TestProcessFieldChangeRulesAreCached()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			rule.Fields.AddNew().PFL_FieldName = "Z0_AnotherDecimal";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			factory.GetProcessFieldChangeRules("DUM");
			AssertEquals(1, factory.GetTableHitCount(ProcessFieldChangeRuleSchema.Constants.TableName));
			AssertEquals(1, factory.GetTableHitCount(ProcessFieldChangeRuleFieldSchema.Constants.TableName));

			var newFactory = new BusinessObjectFactory();
			newFactory.GetProcessFieldChangeRules("DUM");
			AssertEquals(0, newFactory.GetTableHitCount(ProcessFieldChangeRuleSchema.Constants.TableName));
			AssertEquals(0, newFactory.GetTableHitCount(ProcessFieldChangeRuleFieldSchema.Constants.TableName));
		}

		public void TestProcessFieldChangeRulesAreCachedWithEmptyTables()
		{
			Factory.GetProcessFieldChangeRules("SHP");
			AssertEquals(1, Factory.GetTableHitCount(ProcessFieldChangeRuleSchema.Constants.TableName));
			//the store will not query ProcessFieldChangeRuleField if no ProcessFieldChangeRule found
			//AssertEquals(1, Factory.GetTableHitCount(ProcessFieldChangeRuleFieldSchema.Constants.TableName));
			AssertEquals(0, Factory.GetTableHitCount(ProcessFieldChangeRuleFieldSchema.Constants.TableName));

			var newFactory = new BusinessObjectFactory();
			newFactory.GetProcessFieldChangeRules("SHP");
			AssertEquals(0, newFactory.GetTableHitCount(ProcessFieldChangeRuleSchema.Constants.TableName));
			AssertEquals(0, newFactory.GetTableHitCount(ProcessFieldChangeRuleFieldSchema.Constants.TableName));
		}

		public void TestProcessFieldChangeRulesCacheIgnoresNonActiveRules()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			rule.Fields.AddNew().PFL_FieldName = "Z0_AnotherDecimal";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			AssertEquals(2, Factory.GetProcessFieldChangeRules("DUM").Count);

			rule.PFR_IsActive = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertEquals(0, newFactory.GetProcessFieldChangeRules("DUM").Count);
		}

		public void TestProcessFieldChangeRulesCacheIgnoresBlackListedFields()
		{
			//CASE: When fields are added to the blacklist after they have been included in a rule already
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = ("DUM", new DummyProcessFieldChangeRuleConfiguration());
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			var field1 = rule.Fields.AddNew();
			field1.PFL_FieldName = "Z0_AnotherDecimal";
			var field2 = rule.Fields.AddNew();
			field2.PFL_FieldName = "Z0_PK";
			var field3 = rule.Fields.AddNew();
			field3.PFL_FieldName = "Z0_Code";

			AssertEquals(false, field1.IsBlacklisted);
			AssertEquals(true, field2.IsBlacklisted);
			AssertEquals(true, field3.IsBlacklisted);
			Factory.Save();

			AssertEquals(1, Factory.GetProcessFieldChangeRules("DUM").Count);
		}

		public void TestFieldCorllectionIsChildEditable()
		{
			var rule = Factory.NewWithValidTestData<ProcessFieldChangeRule>();
			var fields = rule.Fields;
			AssertEquals(true, rule.IsRegisteredEditableChildObject(fields));
		}

		[TestDate(2020, 01, 01)]
		public void TestFieldEditUpdatesAuditProperties()
		{
			var rule = Factory.NewWithValidTestData<ProcessFieldChangeRule>();
			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_TEST";

			Factory.Save();

			TestDateAttribute.AddDays(1);

			AssertEquals(new ZDateTime(2020, 1, 1), rule.PFR_SystemLastEditTimeUtc);

			field.PFL_FieldName = "Z0_NEW";
			Factory.Save();

			AssertEquals(new ZDateTime(2020, 1, 2), rule.PFR_SystemLastEditTimeUtc);
		}

		public void TestDelete()
		{
			var rule = Factory.NewWithValidTestData<ProcessFieldChangeRule>();
			var field1 = rule.Fields.AddNew();
			field1.PFL_FieldName = "Z0_TEST";
			var field2 = rule.Fields.AddNew();
			field2.PFL_FieldName = "Z0_TES2";
			Factory.Save();

			rule.Delete();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertNull("Parent Deleted", newFactory.Load<ProcessFieldChangeRule>(rule.PK));
			AssertNull("Children Deleted", newFactory.Load<ProcessFieldChangeRuleField>(field1.PK));
			AssertNull("Children Deleted", newFactory.Load<ProcessFieldChangeRuleField>(field2.PK));
		}
		public void TestProcessFieldChangeRulesCacheCleared()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			rule.Fields.AddNew().PFL_FieldName = "Z0_AnotherDecimal";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			factory.GetProcessFieldChangeRules("DUM");
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
			}, factory);

			newFactory.GetProcessFieldChangeRules("DUM");
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 0 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 0 },
			}, newFactory);

			rule.PFR_Description = "Test Clear Cache";
			Factory.Save();
			factory2.GetProcessFieldChangeRules("DUM");
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
			}, factory2);

			var store = ObjectFactory.Get<IProcessFieldChangeRuleStore>();
			var mockStore = new Mock<IProcessFieldChangeRuleStore>(MockBehavior.Strict);

			mockStore.Setup(x => x.ClearCache()).Callback(() => store.ClearCache()).Verifiable(Times.Once, "IProcessFieldChangeRuleStore.ClearCache() should be called in Factory.Save().");
			mockStore.Setup(x => x.GetProcessFieldChangeRules(It.IsAny<BusinessObjectFactory>(), It.IsAny<string>())).Returns((BusinessObjectFactory factory, string processType) => store.GetProcessFieldChangeRules(factory, processType));

			using (ObjectFactory.Substitute(mockStore.Object))
			{
				rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";
				Factory.Save();
				factory3.GetProcessFieldChangeRules("DUM");

				mockStore.VerifyAll();

				AssertDbHits(new Dictionary<string, int>
				{
					{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
					{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
				}, factory3);
			}
		}
	}
}
