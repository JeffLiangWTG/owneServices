using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business.Test
{
	#region Base Dummy Jira Entity Importer

	public abstract class DummyJiraEntityImporterBase : JiraEntityImporter
	{
		protected DummyJiraEntityImporterBase(ProjectsToImportViewModel viewModel, IJiraImporterProgressTracker progressTracker)
			: base(viewModel, progressTracker)
		{
		}

		public string AllProjectsJSONString { get; set; } = JiraIntegrationTestHelper.JSONForOneProject;
		public string OneUserJSONString { get; set; } = JiraIntegrationTestHelper.JSONForOneUser;
		public string ManyIssuesJSONString { get; set; } = JiraIntegrationTestHelper.JSONForOneIssue;

		protected override string RequestAllProjects(JiraCredentials credentials)
		{
			if (AllProjectsJSONString != null)
			{
				return SimulateRequestFromJiraService(AllProjectsJSONString);
			}

			return base.RequestAllProjects(credentials);
		}

		protected override string RequestUser(JiraCredentials credentials, string userAccountID)
		{
			if (OneUserJSONString != null)
			{
				return SimulateRequestFromJiraService(OneUserJSONString);
			}

			return base.RequestUser(credentials, userAccountID);
		}

		protected override string RequestFromJiraService(JiraCredentials credentials, JiraQuery query, bool includesCustomQuery)
		{
			NumberOfWebServiceRequestsExecutedOrSimulated++;

			return base.RequestFromJiraService(credentials, query, includesCustomQuery);
		}

		protected override void SaveImportFactory(bool createNewFactoryAfterSave)
		{
			base.SaveImportFactory(createNewFactoryAfterSave);

			NumberOfSaves++;
		}

		public int NumberOfSaves { get; private set; }

		public int NumberOfWebServiceRequestsExecutedOrSimulated { get; protected set; }

		protected string SimulateRequestFromJiraService(string overriddenResponse)
		{
			NumberOfWebServiceRequestsExecutedOrSimulated++;

			return overriddenResponse;
		}
	}

	#endregion

	#region Dummy Jira Entity Importer

	public class DummyJiraEntityImporter : DummyJiraEntityImporterBase
	{
		public DummyJiraEntityImporter(ProjectsToImportViewModel viewModel, IJiraImporterProgressTracker progressTracker = null)
			: base(viewModel, progressTracker ?? new JiraIntegrationTestHelper.DummyProgressTracker())
		{
		}

		public DummyJiraEntityImporter(BusinessObjectFactory factory)
			: this(new ProjectsToImportViewModel(factory) { ShouldImportAllProjects = true })
		{
		}

		public bool NeverActuallyImport { get; set; }
		public JiraResult ImportResult { get; set; }
		public Func<JiraAttachment, byte[]> IssueAttachmentContentGetter { get; set; }
		public Func<IDocumentFactory, JiraDescriptionDecoder> DescriptionDecoderToUse { get; set; }

		protected override JiraResult ImportCore(JiraCredentials credentials)
		{
			if (!NeverActuallyImport)
			{
				return base.ImportCore(credentials);
			}
			else
			{
				return ImportResult ?? new JiraResult(JiraResponseStatus.Success, "");
			}
		}

		protected override string RequestProjectIssues(JiraCredentials credentials, string projectCode, int batchNumber)
		{
			if (batchNumber == 0)
			{
				if (ManyIssuesJSONString != null)
				{
					return SimulateRequestFromJiraService(ManyIssuesJSONString);
				}

				return base.RequestProjectIssues(credentials, projectCode, batchNumber);
			}

			return JiraIntegrationTestHelper.JSONForNothing;
		}

		protected override JiraDescriptionDecoder GetDescriptionDecoder(IDocumentFactory factory)
		{
			if (DescriptionDecoderToUse != null)
			{
				return DescriptionDecoderToUse(factory);
			}
			else
			{
				return base.GetDescriptionDecoder(factory);
			}
		}

		protected override bool ImportIssueAttachmentsIntoEDocs(JiraIssue issue, WorkItem importedWorkItem, JiraAttachmentDecoder decoder)
		{
			if (IssueAttachmentContentGetter != null)
			{
				var dummyDocDecoder = new JiraAttachmentDecoderForTest(decoder.BatchDocFactory, decoder.Credentials, this.ProgressTracker)
				{
					IssueAttachmentContentGetter = IssueAttachmentContentGetter
				};

				dummyDocDecoder.ProcessDocumentsAndAddToWorkItem(importedWorkItem, issue.Attachments);

				return true;
			}
			else
			{
				return base.ImportIssueAttachmentsIntoEDocs(issue, importedWorkItem, decoder);
			}
		}
	}

	#endregion

	#region Dummy Jira Entity Importer with Multiple Issue Batch Results

	public class DummyJiraEntityImporterWithMultipleIssueBatchJsonResultsFromJira : DummyJiraEntityImporterBase
	{
		public DummyJiraEntityImporterWithMultipleIssueBatchJsonResultsFromJira(ProjectsToImportViewModel viewModel, IJiraImporterProgressTracker progressTracker = null)
			: base(viewModel, progressTracker)
		{
		}

		public DummyJiraEntityImporterWithMultipleIssueBatchJsonResultsFromJira(BusinessObjectFactory factory)
			: this(new ProjectsToImportViewModel(factory) { ShouldImportAllProjects = true }, new JiraIntegrationTestHelper.DummyProgressTracker())
		{
		}

		public void AddJsonToReturnForBatchOfIssueRequests(string jsonForNextBatch)
		{
			jsonToReturnForEachBatchOfIssueRequests.Add(jsonForNextBatch);
		}

		readonly List<string> jsonToReturnForEachBatchOfIssueRequests = new List<string>();

		protected override string RequestProjectIssues(JiraCredentials credentials, string projectCode, int batchNumber)
		{
			return SimulateRequestFromJiraService(jsonToReturnForEachBatchOfIssueRequests[batchNumber]);
		}

		protected override int MaxJiraDownloadBatchSize => maxJiraDownloadBatchSizeOverridden ?? base.MaxJiraDownloadBatchSize;

		int? maxJiraDownloadBatchSizeOverridden;

		public void OverrideMaxJiraDownloadBatchSize(int size)
		{
			maxJiraDownloadBatchSizeOverridden = size;
		}
	}

	#endregion
}
