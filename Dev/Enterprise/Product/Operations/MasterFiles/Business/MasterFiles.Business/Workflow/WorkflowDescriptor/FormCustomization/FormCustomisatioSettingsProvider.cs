using System.Collections;

namespace Enterprise.MasterFiles.Business
{
	public class FormCustomisationSettingsProvider
	{
		public FormCustomisableElementCollection DisplayTabs
		{
			get { return UseCaching ? (displayTabs ?? (displayTabs = GetDisplayTabs())) : GetDisplayTabs(); }
		}
		FormCustomisableElementCollection displayTabs;

		protected virtual FormCustomisableElementCollection GetDisplayTabs()
		{
			return new FormCustomisableElementCollection(null);
		}

		public FormCustomisableElementCollection DisplayFields
		{
			get { return UseCaching ? (displayFields ?? (displayFields = GetDisplayFields())) : GetDisplayFields(); }
		}
		FormCustomisableElementCollection displayFields;

		protected virtual FormCustomisableElementCollection GetDisplayFields()
		{
			return new FormCustomisableElementCollection(null);
		}

		public virtual TabPlacementProhibition[] TabPlacementProhibitions
		{
			get { return System.Array.Empty<TabPlacementProhibition>(); }
		}

		public bool IsTabPlacementAllowed(string tabPageName, string tabPlacement)
		{
			foreach (TabPlacementProhibition prohibition in TabPlacementProhibitions)
			{
				if (prohibition.TabName == tabPageName)
				{
					return !(((IList)prohibition.ProhibitedPlacements).Contains(tabPlacement));
				}
			}

			return true;
		}

		public struct TabPlacementProhibition
		{
			public TabPlacementProhibition(string tabName, params string[] prohibitedPlacements)
			{
				this.TabName = tabName;
				this.ProhibitedPlacements = prohibitedPlacements;
			}

			public readonly string TabName;
			public readonly string[] ProhibitedPlacements;
		}

		public string[] PropertiesThatAffectWorkflow
		{
			get { return propertiesThatAffectWorkflow ?? (propertiesThatAffectWorkflow = GetPropertiesThatAffectWorkflow()); }
		}
		string[] propertiesThatAffectWorkflow;

		protected virtual string[] GetPropertiesThatAffectWorkflow()
		{
			return System.Array.Empty<string>();
		}

		public string[] PropertiesThatAffectWorkflowTemplate
		{
			get { return propertiesThatAffectWorkflowTemplate ?? (propertiesThatAffectWorkflowTemplate = GetPropertiesThatAffectWorkflowTemplate()); }
		}
		string[] propertiesThatAffectWorkflowTemplate;

		protected virtual string[] GetPropertiesThatAffectWorkflowTemplate()
		{
			return System.Array.Empty<string>();
		}

		protected bool UseCaching
		{
			get { return PropertiesThatAffectWorkflowTemplate == null || PropertiesThatAffectWorkflowTemplate.Length == 0; }
		}
	}
}
