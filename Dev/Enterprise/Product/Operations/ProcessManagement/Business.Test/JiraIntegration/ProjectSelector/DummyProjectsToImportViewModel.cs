using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class DummyProjectsToImportViewModel : ProjectsToImportViewModel
	{
		public DummyProjectsToImportViewModel(BusinessObjectFactory factory, string jiraApiVersion)
			: base(factory)
		{
			JiraApiVersion = jiraApiVersion;
		}

		public ProjectImportReport CustomImportResult { get; set; }
		public JiraEntityImporter ImporterToImportWith { get; set; }

		protected override ProjectImportReport ImportJiraContentAndReportResultCore(IJiraImporterProgressTracker progressTracker)
		{
			if (CustomImportResult != null)
			{
				return CustomImportResult;
			}
			else
			{
				return base.ImportJiraContentAndReportResultCore(progressTracker);
			}
		}

		public override bool CheckJiraApiVersion(JiraWebHelper jiraWebHelper)
		{
			return true;
		}

		protected override JiraEntityImporter GetImporter(IJiraImporterProgressTracker progressTracker)
		{
			return ImporterToImportWith ?? base.GetImporter(progressTracker);
		}
	}
}
