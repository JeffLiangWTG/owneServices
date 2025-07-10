using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public class JiraImporterProgressTracker : IJiraImporterProgressTracker
	{
		public void UpdateCurrentImportingProject(string projectName)
		{
			currentProjectName = projectName;
			currentProjectNumber++;

			currentIssueName = null;
			currentIssueNumber = 0;
			totalIssues = 0;

			UpdatePercentComplete();
			UpdateStatusMessage();
		}

		public void UpdateCurrentImportingIssue(string issueName)
		{
			currentIssueName = issueName;
			currentIssueNumber++;
			currentLowLevelItemActivityDescription = null;

			UpdateStatusMessage();
		}

		public void UpdateCurrentLowLevelActivity(string itemActivityDescription)
		{
			var truncated = new ZString(itemActivityDescription).SubstringSafe(0, 40);

			if (truncated != itemActivityDescription)
			{
				truncated += "...";
			}

			currentLowLevelItemActivityDescription = truncated;
			UpdateStatusMessage();
		}

		public void ClearCurrentLowLevelActivity()
		{
			currentLowLevelItemActivityDescription = null;
			UpdateStatusMessage();
		}

		public void SetTotalNumberOfProjects(int numberOfProjects)
		{
			totalProjects = numberOfProjects;
			currentProjectNumber = 0;
		}

		public void SetTotalNumberOfIssuesForThisProject(int numberOfIssues)
		{
			totalIssues = numberOfIssues;
			currentIssueNumber = 0;
		}

		public void ShowGenericMessage(string message)
		{
			ProgressUpdated?.Invoke(this, new JiraProgressUpdatedEventArgs(message, percentComplete));
		}

		void UpdateStatusMessage()
		{
			var projectString = string.IsNullOrEmpty(currentProjectName) ? null : ResString.GetMultilingualString("f8e2cc01-0433-4fe5-99c5-0bcb9a3b6c6e", "Importing Project {0} ({1} of {2})", currentProjectName, currentProjectNumber, totalProjects);
			var issuesString = string.IsNullOrEmpty(currentIssueName) ? null : ResString.GetMultilingualString("c9fff140-6f99-4126-b705-8d622f5ec98a", "Importing Issue {0} ({1} of {2})", currentIssueName, currentIssueNumber, totalIssues);

			var lines = new[] { projectString, issuesString, currentLowLevelItemActivityDescription }.WhereNotNull();
			var status = string.Join(System.Environment.NewLine, lines);

			ProgressUpdated?.Invoke(this, new JiraProgressUpdatedEventArgs(status, percentComplete));
		}

		public event EventHandler<JiraProgressUpdatedEventArgs> ProgressUpdated;

		void UpdatePercentComplete()
		{
			var decimalDone = Math.Max(currentProjectNumber - 1, 0) / (decimal)totalProjects;
			percentComplete = Math.Min((int)(decimalDone * 100), 100);
		}

		#region Fields

		int percentComplete;

		int totalProjects;
		int currentProjectNumber;
		string currentProjectName;

		int totalIssues;
		int currentIssueNumber;
		string currentIssueName;

		string currentLowLevelItemActivityDescription;

		#endregion
	}
}
