using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class IssueTypeMapItem : JiraEntityClassificationMapItem
	{
		#region Properties

		[ResourceStringData("IssueTypeMapItem.JiraEntityName", Caption = "Jira Issue Type Name", ShortCaption = "Issue Type", FullDescription = "The Name specified against the Jira Issue Type.")]
		public override ZString JiraEntityName => base.JiraEntityName;

		[ResourceStringData("IssueTypeMapItem.SelectionCriterionFieldName", Caption = "Work Item Selection Criterion", ShortCaption = "Selection Criterion", FullDescription = "The field which will have the specified value set against it when imported Jira issues are found to have the specified Issue Type.")]
		public override ZString SelectionCriterionFieldName => base.SelectionCriterionFieldName;

		[ResourceStringData("IssueTypeMapItem.SelectionCriterionFieldValue", Caption = "Selection Criterion Field Value", ShortCaption = "Field Value", FullDescription = "The value which will be set against the selection criterion field when imported Jira issues are found to have the specified Issue Type.")]
		public override ZString SelectionCriterionFieldValue => base.SelectionCriterionFieldValue;

		#endregion

		#region JiraEntityClassificationMapItem Overrides

		protected override IEnumerable<Tuple<string, string>> GetSelectionCriteriaFieldNamesAndDescriptions()
		{
			yield return Tuple.Create(WorkItemSchema.WKI_WorkItemType.Name, ProcessManagementRegistry.Instance.WorkItemTypeLabel.Value.ToString());
			yield return Tuple.Create(WorkItemSchema.WKI_WorkItemArea.Name, ProcessManagementRegistry.Instance.WorkItemAreaLabel.Value.ToString());
			yield return Tuple.Create(WorkItemSchema.WKI_ActivityType.Name, ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel.Value.ToString());
			yield return Tuple.Create(WorkItemSchema.WKI_ActivitySubtype.Name, ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel.Value.ToString());
			yield return Tuple.Create(WorkItemSchema.WKI_Priority.Name, ProcessManagementRegistry.Instance.WorkItemPriorityLabel.Value.ToString());
		}

		protected override IEnumerable<string> GetSelectionCriteriaFieldNames()
		{
			yield return WorkItemSchema.WKI_WorkItemType.Name;
			yield return WorkItemSchema.WKI_WorkItemArea.Name;
			yield return WorkItemSchema.WKI_ActivityType.Name;
			yield return WorkItemSchema.WKI_ActivitySubtype.Name;
			yield return WorkItemSchema.WKI_Priority.Name;
		}

		protected override JiraEntityClassificationMapItem CreateNewItemForClone()
		{
			return new IssueTypeMapItem();
		}

		#endregion
	}
}
