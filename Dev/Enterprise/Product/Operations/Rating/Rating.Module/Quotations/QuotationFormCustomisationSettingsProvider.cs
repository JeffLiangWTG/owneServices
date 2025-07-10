using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.Module.Quotations
{
	public class QuotationFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		public QuotationFormCustomisationSettingsProvider(QuotationWorkflowDescriptor parentWorkflowDescriptor)
		{
			Argument.NotNull(parentWorkflowDescriptor, "parentWorkflowDescriptor");

			ParentWorkflowDescriptor = parentWorkflowDescriptor;
		}

		public QuotationWorkflowDescriptor ParentWorkflowDescriptor { get; private set; }

		class TabNames
		{
			public const string CustomFieldsTabPage = "CustomFieldsTabPage";
		}

		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			FormCustomisableElementCollection tabs = new FormCustomisableElementCollection();

			tabs.SuspendValidation();

			ZString currentTemplateType = WorkflowDescriptors.QuotationWorkflowDescriptorCode;

			tabs.Add(ResString.GetMultilingualString("6ccec03e-ea31-47c5-a17f-f27d01dabf1b", "Custom Fields"), TabNames.CustomFieldsTabPage, true);

			tabs.ResumeValidation();

			return tabs;
		}

		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[] { Quote.Schema.TH_OH };
		}

		protected override string[] GetPropertiesThatAffectWorkflowTemplate()
		{
			return new[] { ProcessTaskTemplate.Schema.P0_OH_Client };
		}
	}
}
