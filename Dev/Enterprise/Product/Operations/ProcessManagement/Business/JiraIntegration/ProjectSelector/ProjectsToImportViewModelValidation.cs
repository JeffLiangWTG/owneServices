using System;
using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectsToImportViewModelValidation : ZValidation
	{
		public ProjectsToImportViewModelValidation(ProjectsToImportViewModel parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ProjectsToImportViewModel parent;

		public override Type AutoValidationType => typeof(ProjectsToImportViewModelValidation);

		public override void ValidateAll()
		{
			ValidateJiraUserName();
			ValidateJiraAuthToken();
			ValidateJiraSystemCode();
		}

		internal void ValidateJiraUserName()
		{
			ValidateCalculatedProperty(parent.JiraUserNameInfo);
		}

		protected void CheckJiraUserName()
		{
			MandatoryValidation.CheckEntered(parent.JiraUserNameInfo);
		}

		internal void ValidateJiraAuthToken()
		{
			ValidateCalculatedProperty(parent.JiraAuthTokenInfo);
		}

		protected void CheckJiraAuthToken()
		{
			MandatoryValidation.CheckEntered(parent.JiraAuthTokenInfo);
		}

		public void ValidateJiraSystemCode()
		{
			ValidateCalculatedProperty(parent.JiraSystemCodeInfo);
		}

		protected void CheckJiraSystemCode()
		{
			MandatoryValidation.CheckEntered(parent.JiraSystemCodeInfo);
			ListValidation.ErrorIfInvalidCode(parent.JiraSystemCodeInfo);
		}
	}
}
