using System;
using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	public class JiraEntityClassificationMapItemValidation : ZValidation
	{
		public JiraEntityClassificationMapItemValidation(JiraEntityClassificationMapItem parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly JiraEntityClassificationMapItem parent;

		public override Type AutoValidationType => typeof(JiraEntityClassificationMapItemValidation);

		public override void ValidateAll()
		{
			ValidateJiraEntityName();
			ValidateSelectionCriterionFieldName();
			ValidateSelectionCriterionFieldValue();
		}

		public void ValidateJiraEntityName()
		{
			ValidateCalculatedProperty(parent.JiraEntityNameInfo);
		}

		protected void CheckJiraEntityName()
		{
			MandatoryValidation.CheckEntered(parent.JiraEntityNameInfo);
		}

		public void ValidateSelectionCriterionFieldName()
		{
			ValidateCalculatedProperty(parent.SelectionCriterionFieldNameInfo);
		}

		protected void CheckSelectionCriterionFieldName()
		{
			MandatoryValidation.CheckEntered(parent.SelectionCriterionFieldNameInfo);
			ListValidation.ErrorIfInvalidCode(parent.SelectionCriterionFieldNameInfo);
		}

		public void ValidateSelectionCriterionFieldValue()
		{
			ValidateCalculatedProperty(parent.SelectionCriterionFieldValueInfo);
		}

		protected void CheckSelectionCriterionFieldValue()
		{
			MandatoryValidation.CheckEntered(parent.SelectionCriterionFieldValueInfo);
		}
	}
}
