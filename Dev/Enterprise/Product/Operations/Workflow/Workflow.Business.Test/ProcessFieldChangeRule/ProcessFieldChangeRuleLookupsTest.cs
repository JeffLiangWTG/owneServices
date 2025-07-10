using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test
{
	internal class ProcessFieldChangeRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWorkflowTypes()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			var types = new WorkflowDescriptorList();
			foreach (var type in types.GetAllCodes())
			{
				AssertEquals(true, rule.Lookups.Types.ContainsCode(type));
			}
		}

		public void TestFields_MatchesSchema()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = (WorkflowDescriptors.DummyWorkflowDescriptorCode, new DummyProcessFieldChangeRuleConfiguration());
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;

			foreach (var column in DummyBizoSchema.All)
			{
				if (!(column.IsPKColumn || column == DummyBizoSchema.Z0_Code || column == DummyBizoSchema.Z0_IsValid || column == DummyBizoSchema.Z0_AddInfo || column == DummyBizoSchema.Z0_NAddInfo))//blacklisted columns
				{
					AssertEquals(true, rule.Lookups.Fields.ContainsCode(column.Name));
				}
			}
		}

		public void TestEvents_CustomisableOnly()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			var events = rule.Lookups.Events;

			foreach (var e in Events.Customizables)
			{
				AssertEquals(true, events.ContainsCode(e.Code));
			}

			AssertEquals(events.Count, Events.Customizables.Count);
		}
	}
}
