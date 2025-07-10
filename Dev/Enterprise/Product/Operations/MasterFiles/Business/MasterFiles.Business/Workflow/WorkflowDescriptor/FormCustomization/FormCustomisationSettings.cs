using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class FormCustomisationSettings : IFormCustomisationSettings
	{
		#region Construction

		public FormCustomisationSettings(ProcessTaskTemplate template)
		{
			Argument.NotNull(template, "template");

			Template = template;

			if (CheckIfTemplateStillInDatabase() && !Template.P0_FormState.IsEmpty)
			{
				try
				{
					RetrieveState();
				}
				catch (CustomisationSettingsDeserializeException)
				{
					//P0_FormState is unreadable - can't even be viewed/edited/reset on Workflow Template form. opt to reset it since there's nothing user can be asked to do
					Template.P0_FormState = ZBlob.Empty;
				}
			}

			UpdateFieldsFromWorkflowDescriptor();

			HookProcessTaskTemplateProperties(true);
		}

		public readonly ProcessTaskTemplate Template;

		#endregion

		void HookProcessTaskTemplateProperties(bool hook)
		{
			if (Template.WorkflowDescriptor == null || Template.WorkflowDescriptor.FormCustomisationSettings == null)
			{
				return;
			}

			string[] properties = Template.WorkflowDescriptor.FormCustomisationSettings.PropertiesThatAffectWorkflowTemplate;

			foreach (string propertyName in properties)
			{
				Template.ZPropertyInfoHash[propertyName].ValueChanged -= Refresh;

				if (hook)
				{
					Template.ZPropertyInfoHash[propertyName].ValueChanged += Refresh;
				}
			}
		}

		#region Field Synchronisation

		public void Reset()
		{
			HookProcessTaskTemplateProperties(false);

			ResetCore();

			RebuildViews();

			HookProcessTaskTemplateProperties(true);
		}

		protected virtual void ResetCore()
		{
			DisplayTabs.RemoveAndDeleteAll();
			DisplayFields.RemoveAndDeleteAll();

			UpdateFieldsFromWorkflowDescriptor();

			DisplayTabs.RunPreSaveValidation();
			DisplayFields.RunPreSaveValidation();
		}

		void Refresh(object sender, EventArgs args)
		{
			UpdateFieldsFromWorkflowDescriptor();

			RebuildViews();
		}

		void RebuildViews()
		{
			if (displayTabsView != null)
			{
				displayTabsView.Rebuild();
			}

			if (displayFieldsView != null)
			{
				displayFieldsView.Rebuild();
			}
		}

		void UpdateFieldsFromWorkflowDescriptor()
		{
			var workflowDescriptor = Template.WorkflowDescriptor;
			if (workflowDescriptor != null && workflowDescriptor.FormCustomisationSettings != null)
			{
				UpdateFields(DisplayTabs, workflowDescriptor.FormCustomisationSettings.DisplayTabs);
				UpdateFields(DisplayFields, workflowDescriptor.FormCustomisationSettings.DisplayFields);
			}
		}

		public bool AllFieldsAreOnDefaultTabs()
		{
			if (allFieldsAreOnDefaultTabs != null)
			{
				return allFieldsAreOnDefaultTabs.Value;
			}

			var workflowDescriptor = Template.WorkflowDescriptor;
			if (workflowDescriptor != null && workflowDescriptor.FormCustomisationSettings != null)
			{
				var defaultFields = workflowDescriptor.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>();
				foreach (var field in DisplayFields.Cast<FormCustomisableElement>())
				{
					var defaultField = defaultFields.FirstOrDefault(x => x.ElementName == field.ElementName);
					if (defaultField == null)
					{
						//may have been removed from the defaults without being removed from existing templates -
						//e.g. see WI00110509 (commit d6324f0c) which removed AdditionalContacts but it's still in P0_PK 06492193-26ba-4b77-aaa0-348f923e429f
						continue;
					}
					if (defaultField.DisplayTabCode != field.DisplayTabCode)
					{
						allFieldsAreOnDefaultTabs = false;
						return false;
					}
				}
				allFieldsAreOnDefaultTabs = true;
				return true;
			}
			else
			{
				throw new InvalidOperationException("no default to compare to");
			}
		}
		bool? allFieldsAreOnDefaultTabs;

		public void UpdateFieldsFrom(FormCustomisationSettings formCustomisationSettings)
		{
			UpdateFields(DisplayTabs, formCustomisationSettings.DisplayTabs);
			UpdateFields(DisplayFields, formCustomisationSettings.DisplayFields);
		}

		void UpdateFields(FormCustomisableElementCollection actualElements, FormCustomisableElementCollection descriptorElements)
		{
			using (Template.SuspendSettingHasChangesIncludingChildren())
			{
				foreach (FormCustomisableElement element in actualElements)
				{
					using (element.SuspendSettingHasChanges())
					{
						element.IsInTemplate = descriptorElements.GetElement(element.ElementName) != null;
					}
				}

				foreach (FormCustomisableElement element in descriptorElements)
				{
					using (element.SuspendSettingHasChanges())
					{
						var displayElement = GetCustomizableElement(actualElements, element);
						using (displayElement.SuspendSettingHasChanges())
						{
							if (!displayElement.IsPersistedInDatabase)
							{
								displayElement.CopyValues(element);
							}
							else
							{
								displayElement.CanContainOtherElements = element.CanContainOtherElements;
								displayElement.IsAvailableFunction = element.IsAvailableFunction;
							}
							displayElement.ElementDescription = element.ElementDescription;
							displayElement.ElementDescriptionMultilingual = element.ElementDescriptionMultilingual;
							displayElement.ElementGroup = element.ElementGroup;
							displayElement.ElementGroupMultilingual = element.ElementGroupMultilingual;
							displayElement.Validation.ValidateIsInTemplate();
						}
					}
				}
			}
		}

		static FormCustomisableElement GetCustomizableElement(FormCustomisableElementCollection actualElements, FormCustomisableElement element)
		{
			FormCustomisableElement displayElement = actualElements.GetElement(element.ElementName);
			if (displayElement == null)
			{
				displayElement = new FormCustomisableElement();
				actualElements.Add(displayElement);
			}

			return displayElement;
		}

		#endregion

		#region Settings

		public FormCustomisableElementCollection DisplayFields
		{
			get
			{
				if (displayFields == null)
				{
					displayFields = new FormCustomisableElementCollection(this);
					Template.RegisterEditableChildObject(displayFields);
					if (Template.P0_IsSystem)
					{
						displayFields.SetReadOnlyIncludingChildren(true);
					}
				}
				return displayFields;
			}
		}
		FormCustomisableElementCollection displayFields;

		public FormCustomisableElementCollectionView DisplayFieldsView
		{
			get { return displayFieldsView ?? (displayFieldsView = GetNewView(DisplayFields)); }
		}
		FormCustomisableElementCollectionView displayFieldsView;

		public FormCustomisableElementCollection DisplayTabs
		{
			get
			{
				if (displayTabs == null)
				{
					displayTabs = new FormCustomisableElementCollection(this);
					Template.RegisterEditableChildObject(displayTabs);
					if (Template.P0_IsSystem)
					{
						displayTabs.SetReadOnlyIncludingChildren(true);
					}
				}
				return displayTabs;
			}
		}
		FormCustomisableElementCollection displayTabs;

		public FormCustomisableElementCollectionView DisplayTabsView
		{
			get { return displayTabsView ?? (displayTabsView = GetNewView(DisplayTabs)); }
		}
		FormCustomisableElementCollectionView displayTabsView;

		protected FormCustomisableElementCollectionView GetNewView(FormCustomisableElementCollection collection)
		{
			return new FormCustomisableElementCollectionView(collection);
		}

		#endregion

		#region Xml Serialization

		void RetrieveState()
		{
			FormCustomisationSettingsStorage storage = FormCustomisationSettingsStorageSerializer.Deserialize(Template.P0_FormState);

			if (storage != null)
			{
				DisplayTabs.Hydrate(storage.Tab);
				DisplayFields.Hydrate(storage.Field);
			}
		}

		bool CheckIfTemplateStillInDatabase()
		{
			return new BusinessObjectFactory().Load<ProcessTaskTemplate>(Template.PK) != null;
		}

		public void SaveState()
		{
			FormCustomisationSettingsStorage storage = new FormCustomisationSettingsStorage();

			DisplayTabs.DeHydrate(storage.Tab);
			DisplayFields.DeHydrate(storage.Field);
			var state = FormCustomisationSettingsStorageSerializer.Serialize(storage);

			if (state != Template.P0_FormState)
			{
				Template.P0_FormState = state;
			}
		}

		#endregion

		#region IFormCustomisationSettings Members

		TabPlacement IFormCustomisationSettings.GetTabPlacement(string elementName)
		{
			return DisplayFields.GetTabPlacement(elementName);
		}

		bool? IFormCustomisationSettings.IsElementVisible(string elementName, ElementType elementType)
		{
			bool? result = null;

			switch (elementType)
			{
				case ElementType.Tab:
					result = DisplayTabs.IsElementVisible(elementName);
					break;
				case ElementType.Field:
					result = DisplayFields.IsElementVisible(elementName);
					break;
			}

			return result;
		}

		string[] IFormCustomisationSettings.CustomisableTabPageNames
		{
			get
			{
				List<string> tabs = new List<string>();
				foreach (FormCustomisableElement tab in DisplayTabs)
				{
					if (tab.CanContainOtherElements)
					{
						tabs.Add(tab.ElementName);
					}
				}
				return tabs.ToArray();
			}
		}

		#endregion
	}
}
