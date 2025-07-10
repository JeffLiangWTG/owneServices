using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	class ProcessFieldChangeRuleStoreTest : TestCaseWithFactory
	{
		static void CompareRule(ProcessFieldChangeRule rule, IReadOnlyProcessFieldChangeRule actual)
		{
			AssertEquals(rule.PFR_Description, actual.Description);
			AssertEquals(rule.PFR_GroupName, actual.GroupName);
			AssertEquals(rule.PFR_IsActive, actual.IsActive);
			AssertEquals(rule.PFR_ProcessType, actual.ProcessType);
			AssertEquals(rule.PFR_Reference, actual.Reference);
			AssertEquals(rule.PFR_SE_NKEvent, actual.SE_NKEvent);
			AssertEquals(rule.PK, actual.PK);

			int currentIndex = 0;
			var expectFields = rule.Fields.OrderBy(x => x.PFL_FieldName);
			var actualFields = actual.Fields.OrderBy(x => x.FieldName);
			foreach (var field in expectFields)
			{
				var actualField = actualFields.ElementAt(currentIndex);
				currentIndex++;

				AssertEquals(field.PFL_FieldName, actualField.FieldName);
				AssertEquals(field.FieldDisplayName, actualField.FieldDisplayName);
				AssertEquals(field.PFL_TableCode, actualField.TableCode);
				AssertEquals(field.PFL_PFR, actualField.PFR);
				AssertEquals(field.IsBlacklisted, actualField.IsBlacklisted);
			}
		}

		public void TestGetActiveRulesByType()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = ("DUM", new DummyProcessFieldChangeRuleConfiguration());
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.PFR_Description = "TestGetActiveRulesByType_Description";

			var field1 = rule.Fields.AddNew();
			field1.PFL_FieldName = "Z0_AnotherDecimal";
			var field2 = rule.Fields.AddNew();
			field2.PFL_FieldName = "Z0_PK";
			var field3 = rule.Fields.AddNew();
			field3.PFL_FieldName = "Z0_Code";
			var field4 = rule.Fields.AddNew();
			field4.PFL_FieldName = "Z0_Decimal";

			Factory.Save();

			var ruleStore = new ProcessFieldChangeRuleStore();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };

			var actualRules2 = ruleStore.GetProcessFieldChangeRules(factory2, "DUM");

			AssertEquals("Fields number not in Backoutlist", 2, actualRules2.Count);

			CompareRule(rule, actualRules2[field1.PFL_FieldName][0]);
			CompareRule(rule, actualRules2[field4.PFL_FieldName][0]);
			// 1 hit per table for first call
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
			}, factory2);

			var actualRules3 = ruleStore.GetProcessFieldChangeRules(factory3, "DUM");

			AssertEquals("Fields number not in Backoutlist", 2, actualRules3.Count);

			CompareRule(rule, actualRules3[field1.PFL_FieldName][0]);
			CompareRule(rule, actualRules3[field4.PFL_FieldName][0]);
			// no hits for second call since cached
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 0 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 0 },
			}, factory3);
		}

		public void TestGetActiveRulesByType_Active()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = ("DUM", new DummyProcessFieldChangeRuleConfiguration());
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.PFR_Description = "TestGetActiveRulesByType_Description";

			var field1 = rule.Fields.AddNew();
			field1.PFL_FieldName = "Z0_AnotherDecimal";
			var field2 = rule.Fields.AddNew();
			field2.PFL_FieldName = "Z0_PK";
			var field3 = rule.Fields.AddNew();
			field3.PFL_FieldName = "Z0_Code";
			var field4 = rule.Fields.AddNew();
			field4.PFL_FieldName = "Z0_Decimal";

			var rule2 = Factory.New<ProcessFieldChangeRule>();
			rule2.PFR_ProcessType = "SHP";
			rule2.PFR_GroupName = "Group2";
			rule2.PFR_SE_NKEvent = "Z97";
			rule2.PFR_IsActive = false;

			var field_2 = rule2.Fields.AddNew();
			field_2.PFL_FieldName = "JS_RL_NKDestination";
			Factory.Save();

			var ruleStore = new ProcessFieldChangeRuleStore();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };

			var actualRules2 = ruleStore.GetProcessFieldChangeRules(factory2, "DUM");

			AssertEquals("Only one rule active", 1, actualRules2[field1.PFL_FieldName].Count);
			AssertEquals("Only one rule active", 1, actualRules2[field4.PFL_FieldName].Count);

			CompareRule(rule, actualRules2[field1.PFL_FieldName][0]);
			CompareRule(rule, actualRules2[field4.PFL_FieldName][0]);

			// 1 hit per table for first call
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
			}, factory2);

			var actualRules3 = ruleStore.GetProcessFieldChangeRules(factory3, "DUM");

			AssertEquals("Only one rule active", 1, actualRules3[field1.PFL_FieldName].Count);
			AssertEquals("Only one rule active", 1, actualRules3[field4.PFL_FieldName].Count);

			CompareRule(rule, actualRules2[field1.PFL_FieldName][0]);
			CompareRule(rule, actualRules2[field4.PFL_FieldName][0]);
			// no hits for second call since cached
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 0 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 0 },
			}, factory3);
		}

		public void TestGetActiveRulesByType_Expired()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = ("DUM", new DummyProcessFieldChangeRuleConfiguration());
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.PFR_Description = "Before_Update";

			var field1 = rule.Fields.AddNew();
			field1.PFL_FieldName = "Z0_AnotherDecimal";
			var field2 = rule.Fields.AddNew();
			field2.PFL_FieldName = "Z0_PK";
			var field3 = rule.Fields.AddNew();
			field3.PFL_FieldName = "Z0_Code";
			var field4 = rule.Fields.AddNew();
			field4.PFL_FieldName = "Z0_Decimal";

			Factory.Save();

			var ruleStore = new ProcessFieldChangeRuleStore();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };

			var actualRules2 = ruleStore.GetProcessFieldChangeRules(factory2, "DUM");

			CompareRule(rule, actualRules2[field1.PFL_FieldName][0]);
			CompareRule(rule, actualRules2[field4.PFL_FieldName][0]);

			// 1 hit per table for first call
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
			}, factory2);

			rule.PFR_Description = "Updated";
			Factory.Save();

			var actualRules3 = ruleStore.GetProcessFieldChangeRules(factory3, "DUM");

			AssertEquals("Description changed as cache reset", "Updated", actualRules3[field1.PFL_FieldName][0].Description);

			// 1 hits for second call since cache reset
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
			}, factory3);

			TestDateAttribute.AddSeconds(ObjectFactory.Get<IEntityFrameworkSettings>().UberFactoryTimeoutPeriod + 5);
			Factory.Save();

			actualRules3 = ruleStore.GetProcessFieldChangeRules(factory3, "DUM"); //get rules again

			// Add 1 hit per table after cache expired
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 2 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 2 },
			}, factory3);
		}

		public void TestGetActiveRulesByDataContext_EmptyResult()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = ("DUM", new DummyProcessFieldChangeRuleConfiguration());
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.PFR_Description = "Before_Update";

			var field1 = rule.Fields.AddNew();
			field1.PFL_FieldName = "Z0_AnotherDecimal";
			var field2 = rule.Fields.AddNew();
			field2.PFL_FieldName = "Z0_PK";
			var field3 = rule.Fields.AddNew();
			field3.PFL_FieldName = "Z0_Code";

			Factory.Save();

			var ruleStore = new ProcessFieldChangeRuleStore();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };

			var actualRules2 = ruleStore.GetProcessFieldChangeRules(factory2, "TEST");

			AssertEquals(0, actualRules2.Count);

			// 1 hit per table for first call
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
			}, factory2);

			var actualRules3 = ruleStore.GetProcessFieldChangeRules(factory3, "TEST");

			AssertEquals(0, actualRules3.Count);
			// no hits for second call since cached
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 0 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 0 },
			}, factory2);
		}
		public void TestGetActiveRulesByType_MultipleRule()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = ("DUM", new DummyProcessFieldChangeRuleConfiguration());
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.PFR_Description = "TestGetActiveRulesByType_Description";

			var field1 = rule.Fields.AddNew();
			field1.PFL_FieldName = "Z0_AnotherDecimal";
			var field2 = rule.Fields.AddNew();
			field2.PFL_FieldName = "Z0_PK";
			var field3 = rule.Fields.AddNew();
			field3.PFL_FieldName = "Z0_Code";
			var field4 = rule.Fields.AddNew();
			field4.PFL_FieldName = "Z0_Decimal";

			var rule2 = Factory.New<ProcessFieldChangeRule>();
			rule2.PFR_ProcessType = "DUM";
			rule2.PFR_GroupName = "Group2";
			rule2.PFR_SE_NKEvent = "Z01";
			rule2.PFR_Description = "TestGetActiveRulesByType_Description2";
			rule2.PFR_IsActive = true;

			var field1_2 = rule2.Fields.AddNew();
			field1_2.PFL_FieldName = "Z0_AnotherDecimal";
			var field2_2 = rule2.Fields.AddNew();
			field2_2.PFL_FieldName = "Z0_PK";
			var field3_2 = rule2.Fields.AddNew();
			field3_2.PFL_FieldName = "Z0_Code";
			var field4_2 = rule2.Fields.AddNew();
			field4_2.PFL_FieldName = "Z0_Decimal";

			Factory.Save();

			var ruleStore = new ProcessFieldChangeRuleStore();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };

			var actualRules2 = ruleStore.GetProcessFieldChangeRules(factory2, "DUM");

			AssertEquals("Two rules", 2, actualRules2[field1.PFL_FieldName].Count);
			AssertEquals("Two rules", 2, actualRules2[field4.PFL_FieldName].Count);

			var field1Rules_2 = actualRules2[field1.PFL_FieldName].OrderBy(x => x.GroupName).ToList();
			var field4Rules_2 = actualRules2[field4.PFL_FieldName].OrderBy(x => x.GroupName).ToList();

			CompareRule(rule, field1Rules_2[0]);
			CompareRule(rule, field4Rules_2[0]);
			CompareRule(rule2, field1Rules_2[1]);
			CompareRule(rule2, field4Rules_2[1]);
			// 1 hit per table for first call
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
			}, factory2);
			var actualRules3 = ruleStore.GetProcessFieldChangeRules(factory3, "DUM");

			AssertEquals("Two rules", 2, actualRules3[field1.PFL_FieldName].Count);
			AssertEquals("Two rules", 2, actualRules3[field4.PFL_FieldName].Count);

			var field1Rules_3 = actualRules3[field1.PFL_FieldName].OrderBy(x => x.GroupName).ToList();
			var field4Rules_3 = actualRules3[field4.PFL_FieldName].OrderBy(x => x.GroupName).ToList();

			CompareRule(rule, field1Rules_3[0]);
			CompareRule(rule, field4Rules_3[0]);
			CompareRule(rule2, field1Rules_3[1]);
			CompareRule(rule2, field4Rules_3[1]);
			// no hits for second call since cached
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 0 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 0 },
			}, factory3);
		}

		public void TestGetActiveRuleTables()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = ("DUM", new DummyProcessFieldChangeRuleConfiguration());
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.PFR_Description = "TestGetActiveRulesByType_Description";

			var field1 = rule.Fields.AddNew();
			field1.PFL_FieldName = "Z0_AnotherDecimal";
			var field2 = rule.Fields.AddNew();
			field2.PFL_FieldName = "Z0_PK";
			var field3 = rule.Fields.AddNew();
			field3.PFL_FieldName = "Z0_Code";

			var rule2 = Factory.New<ProcessFieldChangeRule>();
			rule2.PFR_ProcessType = "SHP";
			rule2.PFR_GroupName = "Group2";
			rule2.PFR_SE_NKEvent = "Z97";

			var field_2 = rule2.Fields.AddNew();
			field_2.PFL_FieldName = "JS_RL_NKDestination";
			Factory.Save();

			var ruleStore = new ProcessFieldChangeRuleStore();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };

			var actualTables2 = string.Join(", ", ruleStore.GetActiveRuleTables(factory2).OrderBy(x => x));

			AssertEquals("Two rules", "JS, Z0", actualTables2);

			// 1 hit per table for first call
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 1 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 1 },
			}, factory2);

			var actualTables3 = string.Join(", ", ruleStore.GetActiveRuleTables(factory2).OrderBy(x => x));

			AssertEquals("Two rules", "JS, Z0", actualTables3);

			// no hits for second call since cached
			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessFieldChangeRuleSchema.Constants.TableName, 0 },
				{ ProcessFieldChangeRuleFieldSchema.Constants.TableName, 0 },
			}, factory3);
		}
	}
}
