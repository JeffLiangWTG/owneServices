using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Workflow.Business.Test
{
	class ReadOnlyProcessFieldChangeRuleTest : TestCaseWithFactory
	{
		public void TestReadOnlyProcessFieldChangeRuleWithFields()
		{
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

			Factory.Save();

			var actual = new ReadOnlyProcessFieldChangeRule(rule, (IEnumerable<ProcessFieldChangeRuleField>)rule.Fields);
			AssertEquals(rule.PFR_Description, actual.Description);
			AssertEquals(rule.PFR_GroupName, actual.GroupName);
			AssertEquals(rule.PFR_IsActive, actual.IsActive);
			AssertEquals(rule.PFR_ProcessType, actual.ProcessType);
			AssertEquals(rule.PFR_Reference, actual.Reference);
			AssertEquals(rule.PFR_SE_NKEvent, actual.SE_NKEvent);
			AssertEquals(rule.PK, actual.PK);

			int currentIndex = 0;
			foreach (var field in rule.Fields)
			{
				var actualField = actual.Fields.ElementAt(currentIndex);
				currentIndex++;

				AssertEquals(field.PFL_FieldName, actualField.FieldName);
				AssertEquals(field.FieldDisplayName, actualField.FieldDisplayName);
				AssertEquals(field.PFL_TableCode, actualField.TableCode);
				AssertEquals(field.PFL_PFR, actualField.PFR);
				AssertEquals(field.IsBlacklisted, actualField.IsBlacklisted);
			}
		}
	}
}
