using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TemplateConditionsViewModelLookups : ZLookups
	{
		public TemplateConditionsViewModelLookups(TemplateConditionsViewModel viewModel)
			: base(viewModel)
		{
			Argument.NotNull(viewModel, nameof(viewModel));
			Argument.NotNull(viewModel.WorkflowItem, nameof(viewModel.WorkflowItem));

			workflowItem = viewModel.WorkflowItem;
			template = viewModel.Template;
		}

		readonly ITemplateConditionalWorkflowItem workflowItem;
		readonly ProcessTaskTemplate template;

		public CodeDescriptionPairList TemplateCondition1List
		{
			get { return template?.WorkflowDescriptor?.GetConditionList1(workflowItem) ?? new CodeDescriptionPairList(); }
		}

		public CodeDescriptionPairList TemplateCondition2List
		{
			get
			{
				var list = template?.WorkflowDescriptor?.GetConditionList2(workflowItem) ?? new CodeDescriptionPairList();

				if (list.Count > 0)
				{
					list.AddPair("", "");
				}

				list.AddPair(ProcessTasksLookups.UserDefinedCondition, ProcessTasksLookups.UserDefinedConditionDescription);

				if (WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.Value)
				{
					list.AddPair(ProcessTasksLookups.MacroCondition, ProcessTasksLookups.MacroConditionDescription);
				}
				
				return list;
			}
		}

		public CodeDescriptionPairList TemplateCondition2ValueList
		{
			get { return template?.WorkflowDescriptor?.GetConditionValueList2(workflowItem) ?? new CodeDescriptionPairList(); }
		}

		public IBusinessObjectCollection OriginCountries => CachedCountryCollection;
		public IBusinessObjectCollection DestinationCountries => CachedCountryCollection;

		RefCountryCollection CachedCountryCollection
		{
			get { return Factory.GetCachedValue(nameof(TemplateConditionsViewModelLookups) + ".RefCountryCollection", () => new RefCountryCollection(Factory)); }
		}
	}
}
