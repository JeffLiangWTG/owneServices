using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class ProjectCategoryMapItem : JiraEntityClassificationMapItem
	{
		#region Properties

		[ResourceStringData("ProjectCategoryMapItem.JiraEntityName", Caption = "Jira Project Category Name", ShortCaption = "Project Category", FullDescription = "The Name specified against the Jira Project Category.")]
		public override ZString JiraEntityName => base.JiraEntityName;

		[ResourceStringData("ProjectCategoryMapItem.SelectionCriterionFieldName", Caption = "Project Selection Criterion", ShortCaption = "Selection Criterion", FullDescription = "The field which will have the specified value set against it when imported Jira projects are found to have the specified Category.")]
		public override ZString SelectionCriterionFieldName => base.SelectionCriterionFieldName;

		[ResourceStringData("ProjectCategoryMapItem.SelectionCriterionFieldValue", Caption = "Selection Criterion Field Value", ShortCaption = "Field Value", FullDescription = "The value which will be set against the selection criterion field when imported Jira projects are found to have the specified Category.")]
		public override ZString SelectionCriterionFieldValue => base.SelectionCriterionFieldValue;

		#endregion

		#region JiraEntityClassificationMapItem Overrides

		protected override IEnumerable<Tuple<string, string>> GetSelectionCriteriaFieldNamesAndDescriptions()
		{
			yield return Tuple.Create(WorkProjectSchema.WKP_Type.Name, ProcessManagementRegistry.Instance.ProjectTypeLabel.Value.ToString());
			yield return Tuple.Create(WorkProjectSchema.WKP_SubType.Name, ProcessManagementRegistry.Instance.ProjectSubtypeLabel.Value.ToString());
			yield return Tuple.Create(WorkProjectSchema.WKP_Module.Name, ProcessManagementRegistry.Instance.ProjectModuleLabel.Value.ToString());
			yield return Tuple.Create(WorkProjectSchema.WKP_Priority.Name, ProcessManagementRegistry.Instance.ProjectPriorityLabel.Value.ToString());
		}

		protected override IEnumerable<string> GetSelectionCriteriaFieldNames()
		{
			yield return WorkProjectSchema.WKP_Type.Name;
			yield return WorkProjectSchema.WKP_SubType.Name;
			yield return WorkProjectSchema.WKP_Module.Name;
			yield return WorkProjectSchema.WKP_Priority.Name;
		}

		protected override JiraEntityClassificationMapItem CreateNewItemForClone()
		{
			return new ProjectCategoryMapItem();
		}

		#endregion
	}
}
