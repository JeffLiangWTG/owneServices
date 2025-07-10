using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(ProcessTemplateValidationFilterStripBusinessObject))]
	sealed class ProcessTemplateValidationFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region StatusAndFlagsFilters

		public void TestActionSourceFilter()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "BRK";
			var rule1 = processTaskTemplate.ProcessTemplateValidations.AddNew();
			rule1.P0V_Description = "Rule 1";
			rule1.P0V_Message = "Test Rule 1";
			var action1 = rule1.ProcessTemplateValidationActions.AddNew();
			action1.P0A_ActionSource = "AAA";

			var rule2 = processTaskTemplate.ProcessTemplateValidations.AddNew();
			rule2.P0V_Description = "Rule 2";
			rule2.P0V_Message = "Test Rule 2";
			var action2 = rule2.ProcessTemplateValidationActions.AddNew();
			action2.P0A_ActionSource = "BBB";

			var processTaskTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var rule3 = processTaskTemplate2.ProcessTemplateValidations.AddNew();
			rule3.P0V_Description = "Rule 3";
			rule3.P0V_Message = "Test Rule 3";
			var action3 = rule3.ProcessTemplateValidationActions.AddNew();
			action3.P0A_ActionSource = "AAA";

			Factory.Save();

			var filterBO = new ProcessTemplateValidationFilterStripBusinessObject();
			var actionSourceFilter = (ModuleTextFilter)filterBO["Action Source"];
			actionSourceFilter.IsActive = true;
			actionSourceFilter.Property = "AAA";

			AssertNull(actionSourceFilter.List);

			var rules = new ProcessTemplateValidationCollection(Factory);
			rules.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { "Rule 1", "Rule 3" }, rules.Select(x => x.P0V_Description));

			filterBO.FilterInProcessTaskTemplates = new ProcessTaskTemplate[] { processTaskTemplate };

			AssertContainsExactElementsInAnyOrder(processTaskTemplate.Lookups.ProcessTemplateValidationActionSourceList.GetAllCodes().Distinct(), ((CodeDescriptionPairList)actionSourceFilter.List).GetAllCodes());

			rules.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { "Rule 1" }, rules.Select(x => x.P0V_Description));
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProcessTemplateValidationFilterStripBusinessObject();
		}

		#endregion
	}
}
