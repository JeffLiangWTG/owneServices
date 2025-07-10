using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	[DebuggerDisplay("Id: {CustomFieldId} Data: {JiraEntityName}")]
	public class JiraCustomFieldMapItem : RegistryBusinessObjectTemplate<JiraCustomFieldMapItemValidation>
	{
		#region Properties

		#region JiraEntityName

		ZString jiraEntityName;

		[ResourceStringData("JiraCustomFieldMapItem.JiraEntityName", Caption = "Jira Custom Field Data", ShortCaption = "Custom Field Data", FullDescription = "The data to be mapped into WI data.")]
		public virtual ZString JiraEntityName
		{
			get => jiraEntityName;
			set
			{
				SetNonPersistentPropertyValue(JiraEntityNameInfo, ref jiraEntityName, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateJiraEntityName();
				}
			}
		}

		public ZPropertyInfo JiraEntityNameInfo => GetZPropertyInfo(nameof(JiraEntityName));

		#endregion JiraEntityName

		#region SelectionCriterionFieldName

		ZString selectionCriterionFieldName;

		CodeDescriptionPairList selectionCriterionFieldNames;

		IEnumerable<string> selectionCriterionFields;

		[ResourceStringData("JiraCustomFieldMapItem.SelectionCriterionFieldName", Caption = "Work Item Selection Criterion", ShortCaption = "Selection Criterion", FullDescription = "The field which will have the specified value set against it when imported Jira issues are found to have the specified Issue Type.")]
		[List(nameof(SelectionCriterionFieldNames))]
		public virtual ZString SelectionCriterionFieldName
		{
			get => selectionCriterionFieldName;
			set
			{
				SetNonPersistentPropertyValue(SelectionCriterionFieldNameInfo, ref selectionCriterionFieldName, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateSelectionCriterionFieldName();
				}
			}
		}

		public ZPropertyInfo SelectionCriterionFieldNameInfo => GetZPropertyInfo(nameof(SelectionCriterionFieldName));

		public ICodeDescriptionPairList SelectionCriterionFieldNames
		{
			get
			{
				if (selectionCriterionFieldNames == null)
				{
					selectionCriterionFieldNames = new CodeDescriptionPairList();

					foreach (var tuple in GetSelectionCriteriaFieldNamesAndDescriptions())
					{
						selectionCriterionFieldNames.AddPair(tuple.Item1, tuple.Item2);
					}
				}

				return selectionCriterionFieldNames;
			}
		}

		public IEnumerable<string> SelectionCriterionFields => selectionCriterionFields ?? (selectionCriterionFields = ((CodeDescriptionPairList)SelectionCriterionFieldNames).GetAllCodes());

		#endregion SelectionCriterionFieldName

		#region SelectionCriterionFieldValue

		ZString selectionCriterionFieldValue;

		[MaxLength(3)]
		[ResourceStringData("JiraCustomFieldMapItem.SelectionCriterionFieldValue", Caption = "Selection Criterion Field Value", ShortCaption = "Field Value", FullDescription = "The value which will be set against the selection criterion field when imported Jira issues are found to have the specified Issue Type.")]
		public virtual ZString SelectionCriterionFieldValue
		{
			get => selectionCriterionFieldValue;
			set
			{
				SetNonPersistentPropertyValue(SelectionCriterionFieldValueInfo, ref selectionCriterionFieldValue, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateSelectionCriterionFieldValue();
				}
			}
		}

		public ZPropertyInfo SelectionCriterionFieldValueInfo => GetZPropertyInfo(nameof(SelectionCriterionFieldValue));

		#endregion SelectionCriterionFieldValue

		#region Custom Field Id

		ZString customFieldId;

		[ResourceStringData("JiraCustomFieldMapItem.CustomFieldId", Caption = "Custom Field Id", ShortCaption = "Field Id", FullDescription = "The Jira custom field id.")]
		public virtual ZString CustomFieldId
		{
			get => customFieldId;
			set
			{
				SetNonPersistentPropertyValue(CustomFieldIdInfo, ref customFieldId, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomFieldId();
				}
			}
		}

		public ZPropertyInfo CustomFieldIdInfo => GetZPropertyInfo(nameof(CustomFieldId));

		#endregion Custom Field Id

		#endregion Properties

		#region JiraEntityClassificationMapItem Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var item = CreateNewItemForClone();

			item.JiraEntityName = JiraEntityName;
			item.SelectionCriterionFieldName = SelectionCriterionFieldName;
			item.SelectionCriterionFieldValue = SelectionCriterionFieldValue;
			item.CustomFieldId = CustomFieldId;
			return item;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(nameof(JiraEntityName), JiraEntityName);
			writer.WriteElementString(nameof(SelectionCriterionFieldName), SelectionCriterionFieldName);
			writer.WriteElementString(nameof(SelectionCriterionFieldValue), SelectionCriterionFieldValue);
			writer.WriteElementString(nameof(CustomFieldId), CustomFieldId);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			JiraEntityName = reader.ReadElementString(nameof(JiraEntityName));
			SelectionCriterionFieldName = reader.ReadElementString(nameof(SelectionCriterionFieldName));
			SelectionCriterionFieldValue = reader.ReadElementString(nameof(SelectionCriterionFieldValue));
			CustomFieldId = reader.ReadElementString(nameof(CustomFieldId));
		}

		protected override JiraCustomFieldMapItemValidation GetNewValidation()
			=> new JiraCustomFieldMapItemValidation(this);

		protected IEnumerable<Tuple<string, string>> GetSelectionCriteriaFieldNamesAndDescriptions()
		{
			yield return Tuple.Create(WorkItemSchema.WKI_WorkItemType.Name, ProcessManagementRegistry.Instance.WorkItemTypeLabel.Value.ToString());
			yield return Tuple.Create(WorkItemSchema.WKI_WorkItemArea.Name, ProcessManagementRegistry.Instance.WorkItemAreaLabel.Value.ToString());
			yield return Tuple.Create(WorkItemSchema.WKI_ActivityType.Name, ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel.Value.ToString());
			yield return Tuple.Create(WorkItemSchema.WKI_ActivitySubtype.Name, ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel.Value.ToString());
			yield return Tuple.Create(WorkItemSchema.WKI_Priority.Name, ProcessManagementRegistry.Instance.WorkItemPriorityLabel.Value.ToString());
		}

		protected IEnumerable<string> GetSelectionCriteriaFieldNames()
		{
			yield return WorkItemSchema.WKI_WorkItemType.Name;
			yield return WorkItemSchema.WKI_WorkItemArea.Name;
			yield return WorkItemSchema.WKI_ActivityType.Name;
			yield return WorkItemSchema.WKI_ActivitySubtype.Name;
			yield return WorkItemSchema.WKI_Priority.Name;
		}

		protected JiraCustomFieldMapItem CreateNewItemForClone()
		{
			return new JiraCustomFieldMapItem();
		}

		#endregion JiraEntityClassificationMapItem Overrides
	}
}
