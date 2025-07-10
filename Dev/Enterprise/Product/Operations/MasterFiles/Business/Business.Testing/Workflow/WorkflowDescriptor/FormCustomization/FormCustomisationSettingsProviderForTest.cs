using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FormCustomisationSettingsProviderForTest : FormCustomisationSettingsProvider
	{
		public new bool UseCaching { get { return base.UseCaching; } }

		public Func<FormCustomisableElementCollection> GetDisplayTabsImplementation { get; set; }
		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			return GetDisplayTabsImplementation != null ? GetDisplayTabsImplementation() : base.GetDisplayTabs();
		}

		public Func<FormCustomisableElementCollection> GetDisplayFieldsImplementation { get; set; }
		protected override FormCustomisableElementCollection GetDisplayFields()
		{
			return GetDisplayFieldsImplementation != null ? GetDisplayFieldsImplementation() : base.GetDisplayFields();
		}

		public Func<string[]> GetPropertiesThatAffectWorkflowTemplateImplementation { get; set; }
		protected override string[] GetPropertiesThatAffectWorkflowTemplate()
		{
			return GetPropertiesThatAffectWorkflowTemplateImplementation != null ? GetPropertiesThatAffectWorkflowTemplateImplementation() : base.GetPropertiesThatAffectWorkflowTemplate();
		}
	}
}
