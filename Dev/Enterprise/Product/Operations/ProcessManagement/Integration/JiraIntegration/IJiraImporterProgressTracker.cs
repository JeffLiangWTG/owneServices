using System;

namespace Enterprise.ProcessManagement.Integration
{
	public interface IJiraImporterProgressTracker
	{
		void UpdateCurrentImportingProject(string projectName);
		void UpdateCurrentImportingIssue(string issueName);
		void UpdateCurrentLowLevelActivity(string itemActivityDescription);
		void ClearCurrentLowLevelActivity();

		void SetTotalNumberOfProjects(int numberOfProjects);
		void SetTotalNumberOfIssuesForThisProject(int numberOfIssues);
		void ShowGenericMessage(string message);

		event EventHandler<JiraProgressUpdatedEventArgs> ProgressUpdated;
	}
}
