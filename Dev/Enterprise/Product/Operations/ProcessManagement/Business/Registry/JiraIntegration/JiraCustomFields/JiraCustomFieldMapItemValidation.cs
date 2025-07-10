using System;
using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	public class JiraCustomFieldMapItemValidation : ZValidation
	{
		readonly JiraCustomFieldMapItem parent;

		public JiraCustomFieldMapItemValidation(JiraCustomFieldMapItem parent) : base(parent)
		{
			this.parent = parent;
		}

		public override Type AutoValidationType => typeof(JiraCustomFieldMapItemValidation);

		public override void ValidateAll()
		{
			ValidateJiraEntityName();
			ValidateSelectionCriterionFieldName();
			ValidateSelectionCriterionFieldValue();
			ValidateCustomFieldId();
		}

		public void ValidateCustomFieldId()
		{
			ValidateCalculatedProperty(parent.CustomFieldIdInfo);
		}

		public void ValidateJiraEntityName()
		{
			ValidateCalculatedProperty(parent.JiraEntityNameInfo);
		}

		public void ValidateSelectionCriterionFieldName()
		{
			ValidateCalculatedProperty(parent.SelectionCriterionFieldNameInfo);
		}

		public void ValidateSelectionCriterionFieldValue()
		{
			ValidateCalculatedProperty(parent.SelectionCriterionFieldValueInfo);
		}

		protected void CheckCustomFieldId()
		{
			MandatoryValidation.CheckEntered(parent.CustomFieldIdInfo);
		}

		protected void CheckJiraEntityName()
		{
			MandatoryValidation.CheckEntered(parent.JiraEntityNameInfo);
		}

		protected void CheckSelectionCriterionFieldName()
		{
			MandatoryValidation.CheckEntered(parent.SelectionCriterionFieldNameInfo);
			ListValidation.ErrorIfInvalidCode(parent.SelectionCriterionFieldNameInfo);
		}

		protected void CheckSelectionCriterionFieldValue()
		{
			MandatoryValidation.CheckEntered(parent.SelectionCriterionFieldValueInfo);
		}
	}
}
