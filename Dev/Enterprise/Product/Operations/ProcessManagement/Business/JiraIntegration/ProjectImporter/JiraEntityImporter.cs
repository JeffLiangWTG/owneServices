using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class JiraEntityImporter
	{
		public JiraEntityImporter(ProjectsToImportViewModel viewModel, IJiraImporterProgressTracker progressTracker)
		{
			this.viewModel = viewModel;
			this.ProgressTracker = progressTracker;
		}

		protected IJiraImporterProgressTracker ProgressTracker { get; }
		readonly ProjectsToImportViewModel viewModel;

		ProjectKeyViewModelCollection ProjectCodes => viewModel.SpecificProjectsToImport;

		#region Import

		public JiraResult Import(JiraCredentials credentials)
		{
			return ImportCore(credentials);
		}

		protected virtual JiraResult ImportCore(JiraCredentials credentials)
		{
			var latestResult = new JiraResult(JiraResponseStatus.Success, "");
			var wasDataSaved = false;
			ProgressTracker.ShowGenericMessage(Res.GetString("55ABC78E-4765-406A-8266-AB129CF416F8", "Beginning download..."));

			try
			{
				var importedProjects = ImportJiraProjects(credentials, latestResult);

				if (importedProjects.Any())
				{
					ProgressTracker.SetTotalNumberOfProjects(importedProjects.Length);

					var involvedStaff = new Dictionary<string, JiraUser>();

					foreach (var importedJiraProject in importedProjects.OrderBy(x => x.Code))
					{
						ProgressTracker.UpdateCurrentImportingProject(importedJiraProject.Code);

						var convertedProject = JiraToCargoWiseDecoder.GetProjectFromJiraProject(importedJiraProject, Factory);

						if (!importedJiraProject.ProjectLeadID.IsNullOrEmpty())
						{
							importedJiraProject.ProjectLeadEmail = GetJiraUserFromStaffID(credentials, involvedStaff, importedJiraProject.ProjectLeadID).Email;
							convertedProject.WKP_GS_NKProjectManager = GetStaffCodeFromEmailAddress(importedJiraProject.ProjectLeadEmail);
						}

						TrySetClient(importedJiraProject, convertedProject);
						ExternalEntityLinkHelper.CreateLink(importedJiraProject, convertedProject, viewModel.JiraSystemCode);

						SaveImportFactory(createNewFactoryAfterSave: false);
						wasDataSaved = true;

						if (viewModel.ShouldImportIssues)
						{
							ImportIssuesForProjectInBatches(credentials, importedJiraProject, convertedProject);
							FactoryProvider.CreateNewWithoutSave();
						}
					}
				}
			}
			catch (JiraRequestException ex)
			{
				latestResult = ex.ResultThatCausedException;
				latestResult.WasDataSaved = wasDataSaved;
			}

			return latestResult;
		}

		#region Project Client

		void TrySetClient(JiraProject importedJiraProject, Project convertedProject)
		{
			if (!AreClientPKsSet)
			{
				InitialiseClientPKsOnce(importedJiraProject);
			}

			convertedProject.WKP_OA_ClientAddress = clientAddressPk;
			convertedProject.WKP_OC_Contact = clientContactPk;
		}

		void InitialiseClientPKsOnce(JiraProject importedJiraProject)
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			clientAddressPk = orgProxy.MainAddress.PK;

			OrgContact client = null;

			if (!string.IsNullOrEmpty(importedJiraProject.ProjectLeadEmail))
			{
				client = Factory.LoadTop1<OrgContact>(GetNewBaseClientQuery(orgProxy).AddToFilter(OrgContactSchema.OC_Email, importedJiraProject.ProjectLeadEmail));
			}

			if (client == null)
			{
				var currentUserEmail = Env.CurrentUser.EmailAddress;

				if (!string.IsNullOrEmpty(currentUserEmail))
				{
					client = Factory.LoadTop1<OrgContact>(GetNewBaseClientQuery(orgProxy).AddToFilter(OrgContactSchema.OC_Email, currentUserEmail));
				}
			}

			if (client == null)
			{
				var baseQuery = GetNewBaseClientQuery(orgProxy);
				baseQuery.OrderBy = OrgContactSchema.OC_ContactName.Name;
				client = Factory.LoadTop1<OrgContact>(baseQuery);
			}

			if (client != null)
			{
				clientContactPk = client.PK;
			}
			else
			{
				var message = ResString.GetMultilingualString("b1bb0d4d-8691-45f6-a4dd-4c5faf0c4f28",
					"Could not find a contact for the Project Client because the Organization Proxy [{0}] for the current company does not have any contacts. Please add a contact for the Organization and then try the import again.",
					orgProxy.NameAndCode);

				throw new JiraRequestException(new JiraResult(JiraResponseStatus.NoProjectClient, message));
			}
		}

		static ZQuery GetNewBaseClientQuery(OrgHeader orgProxy)
		{
			return new ZQuery(OrgContactSchema.OC_OH, orgProxy.PK);
		}

		bool AreClientPKsSet => !clientAddressPk.IsEmpty && !clientContactPk.IsEmpty;

		ZGuid clientAddressPk = ZGuid.Empty;
		ZGuid clientContactPk = ZGuid.Empty;

		#endregion

		#endregion

		#region Project Request Methods

		JiraProject[] ImportJiraProjects(JiraCredentials credentials, JiraResult result)
		{
			var projectJSONResponse = viewModel.ShouldImportAllProjects
				? RequestAllProjects(credentials)
				: RequestSomeProjects(credentials);

			var projects = JiraToCargoWiseDecoder.DecodeProjectJSONArray(projectJSONResponse, result);

			return ExternalEntityLinkHelper.FilterOutPreviouslyLinkedEntities(projects, viewModel.JiraSystemCode, Factory).ToArray();
		}

		protected virtual string RequestAllProjects(JiraCredentials credentials)
		{
			var query = new AllProjectsJiraQuery();

			return RequestFromJiraService(credentials, query, includesUserTypedText: false);
		}

		string RequestSomeProjects(JiraCredentials credentials)
		{
			var responseArray = new StringBuilder("[");
			var projectCodes = ProjectCodes.Select(vm => vm.ProjectKey).ToArray();

			foreach (var projectCode in projectCodes)
			{
				responseArray.Append(RequestOneProject(credentials, projectCode));

				if (!projectCodes.Last().Equals(projectCode))
				{
					responseArray.Append(",");
				}
			}

			responseArray.Append("]");

			return responseArray.ToString();
		}

		string RequestOneProject(JiraCredentials credentials, string projectCode)
		{
			var query = new OneProjectJiraQuery(projectCode);

			return RequestFromJiraService(credentials, query, includesUserTypedText: true); // individual projects are typed out by the user
		}

		#endregion

		#region Issue Request Methods

		void ImportIssuesForProjectInBatches(JiraCredentials credentials, JiraProject importedJiraProject, Project convertedProject)
		{
			var workItemCreationBatchSize = ProcessManagementRegistry.Instance.JiraIssueImportBatchSize.Value;
			var importedJiraIssuesForThisProject = ImportJiraIssuesForOneProject(credentials, importedJiraProject.Code);

			if (importedJiraIssuesForThisProject.Any())
			{
				ProgressTracker.SetTotalNumberOfIssuesForThisProject(importedJiraIssuesForThisProject.Length);

				foreach (var issueBatch in importedJiraIssuesForThisProject.OrderBy(x => x.Code).Batch(workItemCreationBatchSize))
				{
					ConvertOneBatchOfIssuesIntoWorkItems(issueBatch.ToArray(), credentials, convertedProject);
				}
			}
		}

		void ConvertOneBatchOfIssuesIntoWorkItems(JiraIssue[] issueBatch, JiraCredentials credentials, Project convertedProject)
		{
			convertedProject = Factory.ImportFromAnotherFactorySafe(convertedProject);
			var importedWorkItemsByCreatorEmail = new DictionaryOfLists<string, WorkItem>();

			foreach (var jiraIssue in issueBatch)
			{
				ProgressTracker.UpdateCurrentImportingIssue(jiraIssue.Code);

				var workItem = JiraToCargoWiseDecoder.GetWorkItemFromJiraIssue(jiraIssue, Factory);

				jiraIssue.WorkItem = workItem;
				convertedProject.RelatedItems.Add(workItem);

				var email = jiraIssue.Creator?.Email;

				if (email != null && !importedWorkItemsByCreatorEmail.ContainsKey(email))
				{
					importedWorkItemsByCreatorEmail.AddValues(email, workItem);
				}

				ExternalEntityLinkHelper.CreateLink(jiraIssue, workItem, viewModel.JiraSystemCode);
			}

			ImportIssueDocuments(credentials, issueBatch);
			var emailsForWhichToFetchStaffCodes = importedWorkItemsByCreatorEmail.Keys.Where(e => !StaffCodesByEmailAddress.ContainsKey(e)).ToArray();

			AddFetchHintsForAllStaffByEmail(emailsForWhichToFetchStaffCodes);

			foreach (var kvp in importedWorkItemsByCreatorEmail)
			{
				var staffCode = GetStaffCodeFromEmailAddress(kvp.Key);

				if (!string.IsNullOrEmpty(staffCode))
				{
					foreach (var workItem in kvp.Value)
					{
						workItem.WKI_SystemCreateUser = staffCode;
					}
				}
			}

			SaveImportFactory(createNewFactoryAfterSave: false);

			var didAddEConversationMessages = ImportCommentsIntoEConversationForBatchOfIssues(issueBatch);

			if (didAddEConversationMessages)
			{
				SaveImportFactory(createNewFactoryAfterSave: false);
			}
		}

		bool ImportCommentsIntoEConversationForBatchOfIssues(JiraIssue[] issueBatch)
		{
			var validComments = issueBatch.SelectMany(x => x.Comments).Where(x => x.IsValid);
			var authors = validComments.Select(comment => comment.Author).WhereNotNull();
			var distinctUsersAddresses = authors.Select(x => x.Email).Distinct().ToArray();

			AddFetchHintsForAllStaffByEmail(distinctUsersAddresses);
			var staff = LoadStaff(distinctUsersAddresses);
			var didAddComments = false;

			foreach (var issue in issueBatch)
			{
				foreach (var comment in issue.Comments.Where(x => x.IsValid))
				{
					AddEConversationMessageForComment(issue.WorkItem, comment, staff);
					didAddComments = true;
				}
			}

			return didAddComments;
		}

		void AddEConversationMessageForComment(WorkItem workItem, JiraComment comment, GlbStaff[] staffRecords)
		{
			var conversation = workItem.Conversation;
			var emailAddress = comment.Author?.Email;
			var staff = staffRecords.SingleOrDefault(x => !string.IsNullOrEmpty(emailAddress) && x.GS_EmailAddress == emailAddress) ?? UnknownStaff;
			var participant = conversation.Participants.SingleOrDefault(x => x.JCP_ParticipantID == staff.PK) ?? conversation.Participants.AddNewParticipant(staff);

			var message = conversation.Messages.AddNew(participant, comment.Body);
			message.JCM_PostedTimeUtc = comment.CreatedTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System-defined constant")]
		GlbStaff UnknownStaff => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "~UK");

		JiraIssue[] ImportJiraIssuesForOneProject(JiraCredentials credentials, string projectCode)
		{
			var issuesDownloadedSoFar = new List<JiraIssue>();
			JiraIssue[] issuesForThisBatch;
			var batchNumber = 0;

			do
			{
				UpdateProgressTrackerWithIssueBatchDownloadDetails(issuesDownloadedSoFar.Count);

				var projectIssuesJSON = RequestProjectIssues(credentials, projectCode, batchNumber);
				issuesForThisBatch = JiraToCargoWiseDecoder.DecodeIssueJSONArray(projectIssuesJSON);
				issuesDownloadedSoFar.AddRange(issuesForThisBatch);
				batchNumber++;
			} while (issuesForThisBatch.Length == MaxJiraDownloadBatchSize);

			return ExternalEntityLinkHelper.FilterOutPreviouslyLinkedEntities(issuesDownloadedSoFar, viewModel.JiraSystemCode, Factory).ToArray();
		}

		protected virtual int MaxJiraDownloadBatchSize => ManyIssuesJiraQuery.JiraDownloadBatchSize;

		void UpdateProgressTrackerWithIssueBatchDownloadDetails(int numberDownloadedSoFar)
		{
			var status = numberDownloadedSoFar == 0
				? ResString.GetMultilingualString("67cb2423-34d1-4570-83fc-a08c43cdb5ae", "Downloading Issues...")
				: ResString.GetMultilingualString("962e06bd-a78e-4d8a-b65d-50d156cdad1d", "Downloading Issues... ({0} received)", numberDownloadedSoFar
				);

			ProgressTracker.UpdateCurrentLowLevelActivity(status);
		}

		protected virtual string RequestProjectIssues(JiraCredentials credentials, string projectCode, int batchNumber)
		{
			var query = new ManyIssuesJiraQuery(projectCode, batchNumber, alreadyAssembledIssueStatusQuery: viewModel.FormattedIssueStatusQuery, customIssueQuery: viewModel.IssueJiraQueryLanguageStatement);
			var includesCustomQuery = !viewModel.IssueJiraQueryLanguageStatement.IsEmpty;

			return RequestFromJiraService(credentials, query, includesUserTypedText: includesCustomQuery); // custom queries are user-provider, otherwise this is entirely us
		}

		#endregion

		#region Import Staff Methods

		protected virtual string RequestUser(JiraCredentials credentials, string userAccountID)
		{
			var query = new OneUserJiraQuery(userAccountID);

			return RequestFromJiraService(credentials, query, includesUserTypedText: false);
		}

		JiraUser GetJiraUserFromStaffID(JiraCredentials credentials, Dictionary<string, JiraUser> involvedStaff, string staffID)
		{
			if (!involvedStaff.TryGetValue(staffID, out var instantiatedStaff))
			{
				var projectLeadResponse = RequestUser(credentials, staffID);
				instantiatedStaff = JiraToCargoWiseDecoder.GetUserFromJSON(projectLeadResponse);

				if (!string.IsNullOrEmpty(instantiatedStaff?.Email))
				{
					involvedStaff.Add(staffID, instantiatedStaff);
				}
			}

			return instantiatedStaff;
		}

		string GetStaffCodeFromEmailAddress(string emailAddress)
		{
			if (!StaffCodesByEmailAddress.ContainsKey(emailAddress))
			{
				var staff = LoadOneStaff(emailAddress);

				StaffCodesByEmailAddress[emailAddress] = staff?.GS_Code;
			}

			return StaffCodesByEmailAddress[emailAddress];
		}

		GlbStaff LoadOneStaff(string emailAddress)
		{
			return Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_EmailAddress, emailAddress));
		}

		GlbStaff[] LoadStaff(string[] emailAddresses)
		{
			return Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_EmailAddress, emailAddresses));
		}

		Dictionary<string, string> StaffCodesByEmailAddress { get; } = new Dictionary<string, string>();

		#endregion

		#region Issue Documents

		void ImportIssueDocuments(JiraCredentials credentials, JiraIssue[] issueBatch)
		{
			var importedDocs = false;
			var importedAttachments = false;
			var docFactory = new DbBackendDocumentFactory(Factory);
			var attachmentDecoder = new JiraAttachmentDecoder(docFactory, credentials, ProgressTracker);

			AddFetchHintsForDocFactoryImport(docFactory, issueBatch.Select(issue => issue.WorkItem).ToArray());

			foreach (var jiraIssue in issueBatch)
			{
				var workItem = jiraIssue.WorkItem;
				var importedDescriptionDoc = ImportIssueDescriptionDoc(jiraIssue, workItem, docFactory);

				if (viewModel.ShouldImportIssueAttachments && jiraIssue.Attachments.Any())
				{
					importedAttachments = ImportIssueAttachmentsIntoEDocs(jiraIssue, workItem, attachmentDecoder);
				}

				importedDocs = importedDescriptionDoc || importedAttachments || importedDocs;
			}

			if (importedAttachments)
			{
				ProgressTracker.ClearCurrentLowLevelActivity();
			}

			if (importedDocs)
			{
				docFactory.Save();
			}
		}

		bool ImportIssueDescriptionDoc(JiraIssue jiraIssue, WorkItem workItem, IDocumentFactory docFactory)
		{
			bool addedDescriptionDoc = false;
			string descriptionToGiveWorkItem;

			if (jiraIssue.DescriptionIsPlaintext)
			{
				descriptionToGiveWorkItem = jiraIssue.Description;
			}
			else
			{
				var descriptionAttachment = new JiraAttachment(jiraIssue.Code + " Description.html", jiraIssue.Description);
				var decoder = GetDescriptionDecoder(docFactory);

				decoder.ProcessDocumentsAndAddToWorkItem(workItem, new[] { descriptionAttachment }.ToList());

				descriptionToGiveWorkItem = decoder.DocumentDescription;
				addedDescriptionDoc = true;
			}

			JiraToCargoWiseDecoder.SetWorkItemDescription(workItem, descriptionToGiveWorkItem);

			return addedDescriptionDoc;
		}

		protected virtual bool ImportIssueAttachmentsIntoEDocs(JiraIssue issue, WorkItem workItem, JiraAttachmentDecoder decoder)
		{
			decoder.ProcessDocumentsAndAddToWorkItem(workItem, issue.Attachments);

			return true;
		}

		protected virtual JiraDescriptionDecoder GetDescriptionDecoder(IDocumentFactory factory)
		{
			return new JiraDescriptionDecoder(factory);
		}

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get => FactoryProvider.Current;
		}

		BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (factoryProvider == null)
				{
					factoryProvider = new BusinessObjectFactoryProvider(new BusinessObjectFactory { NameForDebugging = "Jira Integration Factory" });
				}

				return factoryProvider;
			}
		}
		BusinessObjectFactoryProvider factoryProvider;

		protected virtual void SaveImportFactory(bool createNewFactoryAfterSave)
		{
			if (createNewFactoryAfterSave)
			{
				FactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
			}
			else
			{
				FactoryProvider.SaveCurrentAndUpdateRecordCounts();
			}
		}

		void AddFetchHintsForAllStaffByEmail(string[] allEmailAddresses)
		{
			Factory.AddFetchHint(GlbStaffSchema.Instance, new ZQuery(GlbStaffSchema.GS_EmailAddress, allEmailAddresses));
		}

		void AddFetchHintsForDocFactoryImport(DbBackendDocumentFactory factory, IEnumerable<WorkItem> workItemBatch)
		{
			foreach (var workItem in workItemBatch)
			{
				factory.AddFetchHint(StorageMainSchema.SM_ParentFK, workItem.PK);
			}
		}

		#endregion

		#region Helper methods

		protected virtual string RequestFromJiraService(JiraCredentials credentials, JiraQuery query, bool includesUserTypedText)
		{
			var response = new JiraWebHelper().SendJiraRequestAndGetResponse(viewModel.SelectedUri, credentials, query, includesUserTypedText, viewModel.JiraApiVersion);

			if (!response.Status.Equals(JiraResponseStatus.Success))
			{
				if (includesUserTypedText && query is ManyIssuesJiraQuery)
				{
					response.Status = JiraResponseStatus.InvalidRequestWithCustomQuery;
				}

				throw new JiraRequestException(response);
			}

			return response.Response;
		}

		#endregion
	}
}
